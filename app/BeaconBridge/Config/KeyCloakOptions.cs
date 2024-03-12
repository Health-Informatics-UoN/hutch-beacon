using System.Net;

namespace BeaconBridge.Config;

public class KeyCloakOptions(ILogger logger)
{

  public string Authority { get; set; }

  public string BaseUrl { get; set; }
  public string ClientId { get; set; }
  public string ClientSecret { get; set; }
  public string RemoteSignOutPath { get; set; }
  public string SignedOutRedirectUri { get; set; }
  public string TokenExpiredAddress { get; set; }
  public bool Proxy { get; set; }

  public string BypassProxy { get; set; }

  public string ProxyAddressURL { get; set; }
  public string TokenRefreshSeconds { get; set; }


  public string RedirectURL { get; set; }

  public bool UseRedirectURL { get; set; }

  public string MetadataAddress { get; set; }
  public string? ValidAudiences { get; set; }
  public string Server { get; set; }
  public string Realm { get; set; }
  public bool AutoTrustKeycloakCert { get; set; }
  public string ValidIssuer { get; set; }

  public string ValidAudience { get; set; }

  public HttpClientHandler getProxyHandler {
    get
    {
      logger.LogInformation("getProxyHandler ProxyAddressURL > {ProxyAddressUrl} Proxy > {Proxy} ", ProxyAddressURL, Proxy);
      HttpClientHandler handler = new HttpClientHandler
      {
        Proxy = string.IsNullOrWhiteSpace(ProxyAddressURL)? null : new WebProxy(ProxyAddressURL,true), // Replace with your proxy server URL
        UseProxy = Proxy
      };
      return handler;
    }
  }

}
