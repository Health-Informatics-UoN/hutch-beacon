using BeaconBridge.Config;
using BeaconBridge.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Moq;
using Xunit;

namespace BeaconBridge.Tests;

public class TestMinioStoreService
{
  private readonly Mock<ILogger<MinioStoreService>> _logger = new();
  private readonly Mock<IMinioClient> _minio = new();


  [Fact]
  public async Task GetObjectDownloadUrl_Returns_CorrectUrl()
  {
    // Arrange
    var options = Options.Create(new MinioOptions
    {
      AccessKey = "Access",
      SecretKey = "Secret",
      Secure = false,
      Bucket = "test-bucket",
      Host = "localhost:9000"
    });
    var service = new MinioStoreService(_logger.Object, options.Value, _minio.Object);

    _minio.Setup(m => m.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>()))
      .ReturnsAsync("http://localhost:9000/download/testFile.zip");

    var objectName = "testFile.zip";
    var expectedUrl = "http://localhost:9000/download/testFile.zip";
    // Act
    var actualUrl = await service.GetObjectDownloadUrl(objectName);

    // Assert
    Assert.Equal(expectedUrl, actualUrl);
  }
}
