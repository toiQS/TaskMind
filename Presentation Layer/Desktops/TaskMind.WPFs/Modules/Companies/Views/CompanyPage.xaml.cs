using System.Windows;
using System.Windows.Controls;

namespace TaskMind.WPFs.Modules.Companies.Views
{
    /// <summary>
    /// Interaction logic for CompanyPage.xaml
    /// </summary>
    public partial class CompanyPage : Page
    {
        public CompanyPage()
        {
            InitializeComponent();
        }

        /// <summary>Xác nhận trước khi thoát khỏi toàn bộ ứng dụng.</summary>
        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn thoát khỏi hệ thống?",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }
    }
}