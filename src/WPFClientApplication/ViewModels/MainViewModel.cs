using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Identity.Client;

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
    }
  }
}