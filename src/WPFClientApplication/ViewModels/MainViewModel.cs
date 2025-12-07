using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WPFClientApplication.ViewModels
{
  public class MainViewModel : ObservableObject
  {
    private readonly IPublicClientApplication _publicClientApplication;
    public ICommand LoginCommand { get; private set; }

    public MainViewModel(IPublicClientApplication publicClientApplication)
    {
      _publicClientApplication = publicClientApplication;

      LoginCommand = new AsyncRelayCommand(Login);
    }

    private async Task Login()
    {
      AuthenticationResult authenticationResult = await _publicClientApplication.AcquireTokenInteractive(new string[] { "basic" })
        .WithLoginHint("user")
        .ExecuteAsync();

      JsonWebTokenHandler tokenHandler = new JsonWebTokenHandler();
      JsonWebToken token = tokenHandler.ReadJsonWebToken(authenticationResult.AccessToken);
      string allClaims = string.Join(Environment.NewLine, token.Claims.Select(c => $"Claim Type: {c.Type}, Value: {c.Value}"));
      MessageBox.Show(allClaims);
    }
  }
}