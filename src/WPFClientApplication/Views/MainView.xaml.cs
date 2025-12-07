using System.Windows;
using WPFClientApplication.ViewModels;

namespace WPFClientApplication.Views
{
  /// <summary>
  /// Interaction logic for MainView.xaml
  /// </summary>
  public partial class MainView : Window
  {
    public MainView(MainViewModel dataContext)
    {
      InitializeComponent();
      DataContext = dataContext;
    }
  }
}