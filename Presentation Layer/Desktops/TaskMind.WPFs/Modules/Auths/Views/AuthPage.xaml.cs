using System.Windows;
using System.Windows.Controls;
using TaskMind.WPFs.Modules.Auths.ViewModels;

namespace TaskMind.WPFs.Modules.Auths.Views
{
    /// <summary>
    /// Interaction logic for AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
            DataContext = new AuthNavigationVM();
        }
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }

}
