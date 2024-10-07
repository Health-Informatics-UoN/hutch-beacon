namespace BeaconBridge.Config;

public class EgressOpenIdOptions
{
  /// <summary>
  /// Egress Realm base URL for API interactions with an OpenID Connect (OIDC) compliant
  /// Identity Provider (IdP), such as Keycloak.
  /// </summary>
  public string OpenIdBaseUrl { get; set; } = string.Empty;

  /// <summary>
  /// Egress Client ID for an OIDC IdP
  /// </summary>
  public string ClientId { get; set; } = string.Empty;

  /// <summary>
  /// Egress Client Secret for an OIDC IdP (if the IdP's Client config requires it)
  /// </summary>
  public string ClientSecret { get; set; } = string.Empty;
  
  /// <summary>
  /// Username for a User in the IdP for BeaconBridge to act on behalf of.
  /// </summary>
  ///  TODO in future it should be possible to omit this and use the Client Credentials flow?
  public string Username { get; set; } = string.Empty;

  /// <summary>
  /// Password for a User in the IdP for BeaconBridge to act on behalf of.
  /// </summary>
  ///  TODO in future it should be possible to omit this and use the Client Credentials flow?
  public string Password { get; set; } = string.Empty;
}
