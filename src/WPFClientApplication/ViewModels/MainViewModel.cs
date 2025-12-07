using System.Net.Http;
using System.Net.Http.Headers;
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
    private HttpClient _apiClient = null;
    private const string ApiBaseAddress = "https://localhost:7053";

    public ICommand LoginCommand { get; private set; }
    public ICommand GetWeatherForecastCommand { get; private set; }

    public MainViewModel(IPublicClientApplication publicClientApplication)
    {
      _publicClientApplication = publicClientApplication;

      LoginCommand = new AsyncRelayCommand(Login);
      GetWeatherForecastCommand = new AsyncRelayCommand(GetWeatherForecast);
    }

    private async Task Login()
    {
      try
      {
        AuthenticationResult authenticationResult = await _publicClientApplication.AcquireTokenInteractive(new string[] { "basic" })
        .WithLoginHint("user")
        .ExecuteAsync();

        _apiClient = new HttpClient();
        _apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);
        _apiClient.BaseAddress = new Uri(ApiBaseAddress);

        JsonWebTokenHandler tokenHandler = new JsonWebTokenHandler();
        JsonWebToken token = tokenHandler.ReadJsonWebToken(authenticationResult.AccessToken);
        MessageBox.Show($"Token retrieved successfully, claims:{Environment.NewLine}{string.Join(Environment.NewLine, token.Claims.Select(c => $"Claim Type: {c.Type}, Value: {c.Value}"))}");
      }
      catch(Exception ex)
      {
        MessageBox.Show($"An error occurred.  {ex}");
      }
    }

    private async Task GetWeatherForecast()
    {
      try
      {
        HttpResponseMessage weatherForecaseResponse = await _apiClient.GetAsync("/WeatherForecast");
        weatherForecaseResponse.EnsureSuccessStatusCode();

        MessageBox.Show($"Weeather forecast response: {(await weatherForecaseResponse.Content.ReadAsStringAsync())}");
      }
      catch(Exception ex)
      {
        MessageBox.Show($"An error occurred.  {ex}");
      }
    }
  }
}