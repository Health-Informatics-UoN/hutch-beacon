using System.Reactive.Linq;
using BeaconBridge.Config;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace BeaconBridge.Services;

public class MinioService
{
  private readonly ILogger<MinioService> _logger;
  private readonly IMinioClient _minioClient;
  private readonly MinioOptions _options;

  public MinioService(ILogger<MinioService> logger, IOptions<MinioOptions> options)
  {
    _logger = logger;
    _options = options.Value;
    _minioClient = new MinioClient()
      .WithEndpoint(_options.Host)
      .WithCredentials(_options.AccessKey, _options.SecretKey)
      .WithSSL(_options.Secure)
      .Build();
  }

  /// <summary>
  /// Check if a given S3 bucket exists.
  /// </summary>
  /// <returns><c>true</c> if the bucket exists, else <c>false</c>.</returns>
  public async Task<bool> StoreExists()
  {
    var args = new BucketExistsArgs().WithBucket(_options.Bucket);
    return await _minioClient.BucketExistsAsync(args);
  }

  /// <summary>
  /// Upload a file to an S3 bucket.
  /// </summary>
  /// <param name="filePath">The path of the file to be uploaded.</param>
  /// <exception cref="BucketNotFoundException">Thrown when the given bucket doesn't exists.</exception>
  /// <exception cref="MinioException">Thrown when any other error related to MinIO occurs.</exception>
  /// <exception cref="FileNotFoundException">Thrown when the file to be uploaded does not exist.</exception>
  public async Task WriteToStore(string filePath)
  {
    if (!await StoreExists())
      throw new BucketNotFoundException(_options.Bucket, $"No such bucket: {_options.Bucket}");

    if (!File.Exists(filePath)) throw new FileNotFoundException();

    var objectName = Path.GetFileName(filePath);
    var putObjectArgs = new PutObjectArgs()
      .WithBucket(_options.Bucket)
      .WithFileName(filePath)
      .WithObject(objectName);

    _logger.LogInformation("Uploading {ObjectName} to {Bucket}...", objectName, _options.Bucket);
    await _minioClient.PutObjectAsync(putObjectArgs);
    _logger.LogInformation("Successfully uploaded {ObjectName} to {Bucket}", objectName, _options.Bucket);
  }

  /// <summary>
  /// Get an object from an S3 store.
  /// </summary>
  /// <param name="objectName">The name of the object to retrieve.</param>
  /// <param name="destination">The destination on disk to save the object.</param>
  /// <exception cref="BucketNotFoundException">The configured bucket does not exist.</exception>
  public async Task GetFromStore(string objectName, string destination)
  {
    if (!await StoreExists())
      throw new BucketNotFoundException(_options.Bucket, $"No such bucket: {_options.Bucket}");

    var getObjectArgs = new GetObjectArgs()
      .WithBucket(_options.Bucket)
      .WithFile(destination)
      .WithObject(objectName);

    _logger.LogInformation("Downloading {FileName} from {Bucket}", objectName, _options.Bucket);
    await _minioClient.GetObjectAsync(getObjectArgs);
    _logger.LogInformation("Successfully downloaded {FileName} from {Bucket} to {Destination}", objectName,
      _options.Bucket, destination);
  }

  /// <summary>
  /// Get the download URL to an object in MinIO.
  /// </summary>
  /// <param name="objectName">The name of the object to download.</param>
  /// <returns>The object's download URL.</returns>
  public string GetObjectDownloadUrl(string objectName)
  {
    try
    {
      var args = new PresignedGetObjectArgs().WithBucket(_options.Bucket).WithObject(objectName)
        .WithExpiry(60 * 60 * 24);
      var url = _minioClient.PresignedGetObjectAsync(args: args);
      return url.Result;
    }
    catch(MinioException)
    {
      _logger.LogError("Unable to get Pre-signed Object URL");
      throw;
    }
    
  }

  /// <summary>
  /// Determine if an object exists in the configured bucket
  /// </summary>
  /// <param name="objectName">The object to look for.</param>
  /// <returns><c>true</c> if the object is in the bucket, else <c>false</c>.</returns>
  public bool ObjectIsInStore(string objectName)
  {
    var args = new ListObjectsArgs().WithBucket(_options.Bucket);

    var results = _minioClient.ListObjectsAsync(args).Any(x => x.Key == objectName);
    foreach (var b in results)
    {
      if (b) return true;
    }

    return false;
  }

  /// <summary>
  /// Get an enumerator of objects in the configured bucket sorted by <c>LastModifiedTime</c>
  /// from most to least recent.
  /// </summary>
  /// <returns>An enumerator of objects in the configured bucket.</returns>
  public IEnumerable<Item> GetObjectsInBucket()
  {
    var args = new ListObjectsArgs().WithBucket(_options.Bucket);

    return _minioClient.ListObjectsAsync(args).ToEnumerable().OrderBy(x => x.LastModifiedDateTime).Reverse();
  }
}
