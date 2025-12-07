using System.Net.Http.Headers;
using System.Text;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;
using Newtonsoft.Json;

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
        .WithLoginHint("admin")
        .ExecuteAsync();

      using (HttpClient httpClient = new HttpClient())
      {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        httpClient.BaseAddress = new Uri(@"https://localhost:8443");

        //create realm
        HttpResponseMessage createRealmResponse = await httpClient.PostAsync("/admin/realms",
          new StringContent($@"{{""realm"":""{realmName}"",""enabled"":true}}", Encoding.UTF8, "application/json"));
        createRealmResponse.EnsureSuccessStatusCode();

        HttpResponseMessage getRealmResponse = await httpClient.GetAsync($"/admin/realms/{realmName}");
        getRealmResponse.EnsureSuccessStatusCode();
        dynamic realm = JsonConvert.DeserializeObject<dynamic>(await getRealmResponse.Content.ReadAsStringAsync());
        string realmId = realm.id;

        //add realm-role 'app-user'
        HttpResponseMessage createAppUserRealmRoleResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/roles",
          new StringContent(@"{""name"":""app-user"",""description"":"""",""attributes"":{}}", Encoding.UTF8, "application/json"));
        createAppUserRealmRoleResponse.EnsureSuccessStatusCode();

        HttpResponseMessage getAppUserRoleResponse = await httpClient.GetAsync($"/admin/realms/{realmName}/roles/app-user");
        getAppUserRoleResponse.EnsureSuccessStatusCode();
        dynamic appUserRole = JsonConvert.DeserializeObject<dynamic>(await getAppUserRoleResponse.Content.ReadAsStringAsync());
        string appUserRoleId = appUserRole.id;

        //add realm-role 'app-admin'
        HttpResponseMessage createAppAdminRealmRoleResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/roles",
          new StringContent(@"{""name"":""app-admin"",""description"":"""",""attributes"":{}}", Encoding.UTF8, "application/json"));
        createAppAdminRealmRoleResponse.EnsureSuccessStatusCode();

        HttpResponseMessage getAppAdminRoleResponse = await httpClient.GetAsync($"/admin/realms/{realmName}/roles/app-admin");
        getAppAdminRoleResponse.EnsureSuccessStatusCode();
        dynamic appAdminRole = JsonConvert.DeserializeObject<dynamic>(await getAppAdminRoleResponse.Content.ReadAsStringAsync());
        string appAdminRoleId = appAdminRole.id;

        //create client 'wpf-client'
        HttpResponseMessage createClientResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/clients",
          new StringContent($@"{{""protocol"":""openid-connect"",""clientId"":""{clientName}"",""name"":"""",""description"":"""",""publicClient"":true,""authorizationServicesEnabled"":false,""serviceAccountsEnabled"":false,""implicitFlowEnabled"":false,""directAccessGrantsEnabled"":false,""standardFlowEnabled"":true,""frontchannelLogout"":true,""attributes"":{{""saml_idp_initiated_sso_url_name"":"""",""standard.token.exchange.enabled"":false,""oauth2.device.authorization.grant.enabled"":false,""oidc.ciba.grant.enabled"":false,""pkce.code.challenge.method"":""S256"",""dpop.bound.access.tokens"":""false"",""post.logout.redirect.uris"":""-""}},""alwaysDisplayInConsole"":false,""rootUrl"":""http://localhost"",""baseUrl"":""http://localhost"",""redirectUris"":[""http://localhost:*"",""http://localhost""]}}",
            Encoding.UTF8, "application/json"));
        createClientResponse.EnsureSuccessStatusCode();

        string userId = null;
        string adminUserId = null;

        //'user' account setup
        {
          //create user 'user'
          HttpResponseMessage createUserResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/users",
            new StringContent(@"{""requiredActions"":[],""emailVerified"":false,""username"":""user"",""email"":""user@test.com"",""firstName"":""first"",""lastName"":""last"",""groups"":[],""attributes"":{},""enabled"":true}",
              Encoding.UTF8, "application/json"));
          createUserResponse.EnsureSuccessStatusCode();

          //get the generated user id
          HttpResponseMessage getUserResponse = await httpClient.GetAsync($"/admin/realms/{realmName}/users?username=user");
          getUserResponse.EnsureSuccessStatusCode();
          dynamic users = JsonConvert.DeserializeObject<dynamic>(await getUserResponse.Content.ReadAsStringAsync());
          userId = users[0].id;

          //set user password
          HttpResponseMessage setUserPasswordResponse = await httpClient.PutAsync($"/admin/realms/{realmName}/users/{userId}/reset-password",
            new StringContent(@"{""temporary"":false,""type"":""password"",""value"":""user""}",
              Encoding.UTF8, "application/json"));
          setUserPasswordResponse.EnsureSuccessStatusCode();

          //add to 'app-user' role
          HttpResponseMessage addToAppUserRoleResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/users/{userId}/role-mappings/realm",
            new StringContent($@"[{{""id"":""{appUserRoleId}"",""name"":""app-user"",""description"":"""",""composite"":false,""clientRole"":false,""containerId"":""{realmId}""}}]",
              Encoding.UTF8, "application/json"));
          addToAppUserRoleResponse.EnsureSuccessStatusCode();
        }

        //'adminuser' account setup
        {
          //create user 'adminuser'
          HttpResponseMessage createUserResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/users",
            new StringContent(@"{""requiredActions"":[],""emailVerified"":false,""username"":""adminuser"",""email"":""adminuser@test.com"",""firstName"":""first"",""lastName"":""last"",""groups"":[],""attributes"":{},""enabled"":true}",
              Encoding.UTF8, "application/json"));
          createUserResponse.EnsureSuccessStatusCode();

          //get the generated user id
          HttpResponseMessage getUserResponse = await httpClient.GetAsync($"/admin/realms/{realmName}/users?username=adminuser");
          getUserResponse.EnsureSuccessStatusCode();
          dynamic users = JsonConvert.DeserializeObject<dynamic>(await getUserResponse.Content.ReadAsStringAsync());
          adminUserId = users[0].id;

          //set user password
          HttpResponseMessage setUserPasswordResponse = await httpClient.PutAsync($"/admin/realms/{realmName}/users/{adminUserId}/reset-password",
            new StringContent(@"{""temporary"":false,""type"":""password"",""value"":""adminuser""}",
              Encoding.UTF8, "application/json"));
          setUserPasswordResponse.EnsureSuccessStatusCode();

          //add to 'app-user' role
          HttpResponseMessage addToAppUserRoleResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/users/{adminUserId}/role-mappings/realm",
            new StringContent($@"[{{""id"":""{appUserRoleId}"",""name"":""app-user"",""description"":"""",""composite"":false,""clientRole"":false,""containerId"":""{realmId}""}}]",
              Encoding.UTF8, "application/json"));
          addToAppUserRoleResponse.EnsureSuccessStatusCode();

          //add to 'app-admin' role
          HttpResponseMessage addToAppAdminRoleResponse = await httpClient.PostAsync($"/admin/realms/{realmName}/users/{adminUserId}/role-mappings/realm",
            new StringContent($@"[{{""id"":""{appAdminRoleId}"",""name"":""app-admin"",""description"":"""",""composite"":false,""clientRole"":false,""containerId"":""{realmId}""}}]",
              Encoding.UTF8, "application/json"));
          addToAppAdminRoleResponse.EnsureSuccessStatusCode();
        }
      }
    }
  }
}