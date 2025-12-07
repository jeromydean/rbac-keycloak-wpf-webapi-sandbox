using System.Net.Http.Headers;
using System.Text;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;

namespace KeycloakConfigurationUtility
{
  internal class Program
  {
    static async Task Main(string[] args)
    {
      string realmName = "rbac-application";
      string clientName = "wpf-client";

      IPublicClientApplication app = PublicClientApplicationBuilder.Create("security-admin-console")
        .WithExperimentalFeatures()//required for .WithOidcAuthority
        .WithOidcAuthority("https://localhost:8443/realms/master")
        .WithDefaultRedirectUri()//http://localhost is used for the webview
        .WithWindowsEmbeddedBrowserSupport()
        .Build();

      AuthenticationResult authenticationResult = await app.AcquireTokenInteractive(new string[] { "basic" })
        .ExecuteAsync();

      using (HttpClient httpClient = new HttpClient())
      {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        httpClient.BaseAddress = new Uri(@"https://localhost:8443");

        //create realm
        HttpResponseMessage createRealmResponse = await httpClient.PostAsync("/admin/realms",
          new StringContent($@"{{""realm"":""{realmName}"",""enabled"":true}}", Encoding.UTF8, "application/json"));
        createRealmResponse.EnsureSuccessStatusCode();

        //create client 'wpf-client'
        HttpResponseMessage createClientResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/clients/",
          new StringContent($@"{{""protocol"":""openid-connect"",""clientId"":""{clientName}"",""name"":"""",""description"":"""",""publicClient"":true,""authorizationServicesEnabled"":false,""serviceAccountsEnabled"":false,""implicitFlowEnabled"":false,""directAccessGrantsEnabled"":false,""standardFlowEnabled"":true,""frontchannelLogout"":true,""attributes"":{{""saml_idp_initiated_sso_url_name"":"""",""standard.token.exchange.enabled"":false,""oauth2.device.authorization.grant.enabled"":false,""oidc.ciba.grant.enabled"":false,""pkce.code.challenge.method"":""S256"",""dpop.bound.access.tokens"":""false"",""post.logout.redirect.uris"":""-""}},""alwaysDisplayInConsole"":false,""rootUrl"":""http://localhost"",""baseUrl"":""http://localhost"",""redirectUris"":[""http://localhost:*"",""http://localhost""]}}",
            Encoding.UTF8, "application/json"));
        createClientResponse.EnsureSuccessStatusCode();
      }
    }
  }
}