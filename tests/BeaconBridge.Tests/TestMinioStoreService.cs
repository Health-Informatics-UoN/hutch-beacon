using BeaconBridge.Config;
using BeaconBridge.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BeaconBridge.Tests;

public class TestMinioStoreService
{
  [Fact]
  public void GetObjectDownloadUrl_Returns_CorrectUrl()
  {
    // Arrange
    var logger = new Mock<ILogger<MinioService>>();
    var options = Options.Create(new MinioOptions
    {
      AccessKey = "Access",
      SecretKey = "Secret",
      Secure = false,
      Bucket = "test-bucket",
      Host = "localhost:9000"
    });
    var service = new MinioService(logger.Object, options);
    var objectName = "test.crate.zip";
    var expectedUrl = "http://localhost:9000/browser/test-bucket/" + objectName;

    // Act
    var actualUrl = service.GetObjectDownloadUrl(objectName);

    // Assert
    Assert.Equal(actualUrl, expectedUrl);
  }
}
