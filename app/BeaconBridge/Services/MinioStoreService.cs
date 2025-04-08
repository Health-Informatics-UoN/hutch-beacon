using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Flurl;
using Flurl.Http;
using BeaconBridge.Config;
using BeaconBridge.Models;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace BeaconBridge.Services;

public class MinioStoreServiceFactory(
  IOptions<MinioOptions> defaultOptions,
  IServiceProvider services,
  IOptionsSnapshot<OpenIdOptions> identityOptions,
  ILogger<MinioStoreServiceFactory> logger,
  OpenIdIdentityService identity)
{
  private readonly OpenIdOptions _identityOptions = identityOptions.Get(OpenIdOptions.Submission);

  /// <summary>
  /// The Default Options for a Minio Store, as configured.
  /// </summary>
  public MinioOptions DefaultOptions { get; } = defaultOptions.Value;

  private IMinioClient GetClient(MinioOptions options, string? sessionToken)
  {
    var clientBuilder = new MinioClient()
      .WithEndpoint(options.Host)
      .WithCredentials(options.AccessKey, options.SecretKey)
      .WithSSL(options.Secure);

    if (sessionToken is not null)
      clientBuilder.WithSessionToken(sessionToken);

    return clientBuilder.Build();
  }

  /// <summary>
  /// Get temporary Minio access credentials via a client access token
  /// </summary>
  /// <param name="minioBaseUrl">The base url for the minio server - i.e. a scheme (http(s)) + the configured host</param>
  /// <param name="token">The client's Access token</param>
  /// <returns>Temporary access key and secret key for use with Minio</returns>
  private async Task<(string accessKey, string secretKey, string sessionToken)> GetTemporaryCredentials(
    string minioBaseUrl, string token)
  {
    var url = minioBaseUrl
      .SetQueryParams(new
      {
        Action = "AssumeRoleWithClientGrants",
        Version = "2011-06-15", // AWS stipulates this version for this endpoint...
        DurationSeconds = 604800 // we ask for the max (7 days) - the credentials issued may be shorter
      })
      .SetQueryParam("Token", token, true);

    try
    {
      var response = await url.PostAsync().ReceiveString();

      return ParseAssumeRoleResponse(response);
    }
    catch (FlurlHttpException e)
    {
      logger.LogError("S3 STS AssumeRole Request failed: {ResponseBody}", await e.GetResponseStringAsync());
      throw;
    }
  }

  /// <summary>
  /// Parse the XML response from an STS AssumeRole request to extract the desired details.
  /// </summary>
  /// <param name="response">The XML response from an STS AssumeRole request as a string.</param>
  /// <returns>Access Token and Secret Key from the response.</returns>
  private static (string accessKey, string secretKey, string sessionToken) ParseAssumeRoleResponse(string response)
  {
    var xml = XElement.Parse(response);
    var accessKey = xml.Descendants().Single(x => x.Name.LocalName == "AccessKeyId").Value;
    var secretKey = xml.Descendants().Single(x => x.Name.LocalName == "SecretAccessKey").Value;
    var sessionToken = xml.Descendants().Single(x => x.Name.LocalName == "SessionToken").Value;

    return (accessKey, secretKey, sessionToken);
  }

  /// <summary>
  /// Combine provided options with default fallbacks where necessary
  /// </summary>
  /// <param name="options">The provided options</param>
  /// <returns>
  /// A complete options object built from those provided,
  /// falling back on pre-configured defaults.
  /// </returns>
  private MinioOptions MergeOptionsWithDefaults(MinioOptions? options = null)
  {
    var mergedOptions = new MinioOptions
    {
      Host = string.IsNullOrWhiteSpace(options?.Host)
        ? DefaultOptions.Host
        : options.Host,
      AccessKey = string.IsNullOrWhiteSpace(options?.AccessKey)
        ? DefaultOptions.AccessKey
        : options.AccessKey,
      SecretKey = string.IsNullOrWhiteSpace(options?.SecretKey)
        ? DefaultOptions.SecretKey
        : options.SecretKey,
      Secure = options?.Secure ?? DefaultOptions.Secure,
      Bucket = string.IsNullOrWhiteSpace(options?.Bucket)
        ? DefaultOptions.Bucket
        : options.Bucket,
    };

    return mergedOptions;
  }

  /// <summary>
  /// Create a new instance of MinioStoreService configured with the provided options.
  /// </summary>
  /// <returns>A <see cref="MinioStoreService"/> instance configured with the provided options.</returns>
  public async Task<MinioStoreService> Create(MinioOptions? options = null)
  {
    var mergedOptions = MergeOptionsWithDefaults(options);

    string? sessionToken = null;

    logger.LogInformation(
      "Attempting to retrieve credentials via OIDC");

    // Get an OIDC token
    var (token, _, _) = await identity.RequestUserTokens(_identityOptions);

    // Get MinIO STS credentials with the user's identity token
    // https://min.io/docs/minio/linux/developers/security-token-service/AssumeRoleWithWebIdentity.html#minio-sts-assumerolewithwebidentity
    // or with a client access token // NOTE: this seems to be unfinished; it's not in the docs site and gives 400 Bad Request on a real server
    // https://github.com/minio/minio/blob/master/docs/sts/client-grants.md
    var (accessKey, secretKey, returnedSessionToken) = await GetTemporaryCredentials(
      $"{(mergedOptions.Secure ? "https" : "http")}://{mergedOptions.Host}",
      token);

    // set the credentials to those from the STS response
    mergedOptions.AccessKey = accessKey;
    mergedOptions.SecretKey = secretKey;
    sessionToken = returnedSessionToken;


    return new MinioStoreService(
      services.GetRequiredService<ILogger<MinioStoreService>>(),
      mergedOptions,
      GetClient(mergedOptions, sessionToken));
  }
}

public class MinioStoreService(
  ILogger<MinioStoreService> logger,
  MinioOptions options,
  IMinioClient minio)
{
  /// <summary>
  /// Check if a given S3 bucket exists.
  /// </summary>
  /// <returns><c>true</c> if the bucket exists, else <c>false</c>.</returns>
  public async Task<bool> StoreExists()
  {
    var args = new BucketExistsArgs().WithBucket(options.Bucket);
    return await minio.BucketExistsAsync(args);
  }

  /// <summary>
  /// Upload a file to an S3 bucket.
  /// </summary>
  /// <param name="filePath">The path of the file to be uploaded.</param>
  /// <param name="zipFile">Zip file as byte array.</param>
  /// <exception cref="BucketNotFoundException">Thrown when the given bucket doesn't exists.</exception>
  /// <exception cref="MinioException">Thrown when any other error related to MinIO occurs.</exception>
  /// <exception cref="FileNotFoundException">Thrown when the file to be uploaded does not exist.</exception>
  public async Task WriteToStore(string filePath, byte[] zipFile)
  {
    if (!await StoreExists())
      throw new BucketNotFoundException(options.Bucket, $"No such bucket: {options.Bucket}");

    var objectName = Path.GetFileName(filePath);
    var putObjectArgs = new PutObjectArgs()
      .WithBucket(options.Bucket)
      .WithObject(objectName)
      .WithStreamData(new MemoryStream(zipFile))
      .WithObjectSize(zipFile.Length);

    logger.LogDebug("Uploading '{TargetObject} to {Bucket}...", filePath, options.Bucket);
    await minio.PutObjectAsync(putObjectArgs);
    logger.LogDebug("Successfully uploaded {TargetObject} to {Bucket}", filePath, options.Bucket);
  }

  /// <summary>
  /// Get the pre-signed download URL to an object in MinIO.
  /// </summary>
  /// <param name="objectName">The name of the object to download.</param>
  /// <returns>The object's download URL.</returns>
  public async Task<string> GetObjectDownloadUrl(string objectName)
  {
    try
    {
      var args = new PresignedGetObjectArgs().WithBucket(options.Bucket).WithObject(objectName)
        .WithExpiry(60 * 60 * 24);
      var url = await minio.PresignedGetObjectAsync(args: args);
      return url;
    }
    catch (MinioException)
    {
      logger.LogError("Unable to get Pre-signed Object URL");
      throw;
    }
  }

  /// <summary>
  /// Get an object from an S3 store.
  /// </summary>
  /// <param name="objectName">The name of the object to retrieve.</param>
  /// <param name="destination">The destination on disk to save the object.</param>
  /// <exception cref="BucketNotFoundException">The configured bucket does not exist.</exception>
  public async Task<byte[]> GetFromStore(string objectName)
  {
    if (!await StoreExists())
      throw new BucketNotFoundException(options.Bucket, $"No such bucket: {options.Bucket}");
    var memoryStream = new MemoryStream();
    var getObjectArgs = new GetObjectArgs()
      .WithBucket(options.Bucket)
      .WithObject(objectName)
      .WithCallbackStream(stream => { stream.CopyToAsync(memoryStream); });

    logger.LogInformation("Downloading {FileName} from {Bucket}", objectName, options.Bucket);

    await minio.GetObjectAsync(getObjectArgs);
    byte[] byteArray = memoryStream.ToArray();
    logger.LogInformation("Successfully downloaded {FileName} from {Bucket}", objectName,
      options.Bucket);
    return byteArray;
  }

  /// <summary>
  /// Determine if an object exists in the configured bucket
  /// </summary>
  /// <param name="objectName">The object to look for.</param>
  /// <returns><c>true</c> if the object is in the bucket, else <c>false</c>.</returns>
  public bool ObjectIsInStore(string objectName)
  {
    var args = new ListObjectsArgs().WithBucket(options.Bucket);

    var results = minio.ListObjectsAsync(args).Any(x => x.Key == objectName);
    foreach (var b in results)
    {
      if (b) return true;
    }

    return false;
  }
}
