using System.Reflection;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
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

      return serviceCollection;
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
      MainView mainView = _serviceProvider.GetRequiredService<MainView>();
      mainView.Show();
    }
  }
}