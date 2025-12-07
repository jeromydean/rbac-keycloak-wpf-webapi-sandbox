using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;
using WPFClientApplication.Views;

namespace WPFClientApplication
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
      _serviceProvider = ConfigureServices().BuildServiceProvider();
    }

    private ServiceCollection ConfigureServices()
    {
      ServiceCollection serviceCollection = new ServiceCollection();

      //register all views
      foreach(Type viewType in Assembly.GetExecutingAssembly().GetTypes().Where(t =>
        t.Namespace == "WPFClientApplication.Views"
        && typeof(Window).IsAssignableFrom(t)))
      {
        serviceCollection.AddTransient(viewType);
      }

      //register all view models
      foreach (Type viewModelType in Assembly.GetExecutingAssembly().GetTypes().Where(t =>
        t.Namespace == "WPFClientApplication.ViewModels"
        && typeof(ObservableObject).IsAssignableFrom(t)))
      {
        serviceCollection.AddTransient(viewModelType);
      }

      serviceCollection.AddSingleton<IPublicClientApplication>(sp =>
      {
        return PublicClientApplicationBuilder.Create("wpf-client")
          .WithExperimentalFeatures()//required for .WithOidcAuthority

          //https://localhost:8443/realms/rbac-application/protocol/saml/descriptor
          //https://localhost:8443/realms/rbac-application/.well-known/openid-configuration
          .WithOidcAuthority("https://localhost:8443/realms/rbac-application")
          .WithDefaultRedirectUri()//http://localhost is used for the webview
          .WithWindowsEmbeddedBrowserSupport()
          .Build();
      });

      return serviceCollection;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
      MainView mainView = _serviceProvider.GetRequiredService<MainView>();
      mainView.Show();
    }
  }
}