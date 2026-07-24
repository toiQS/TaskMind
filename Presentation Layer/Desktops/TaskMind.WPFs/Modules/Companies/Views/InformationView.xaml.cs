using System.Windows.Controls;
using TaskMind.WPFs.Modules.Companies.ViewModels;

namespace TaskMind.WPFs.Modules.Companies.Views
{
    /// <summary>
    /// Interaction logic for InformationView.xaml
    /// </summary>
    public partial class InformationView : UserControl
    {
        public InformationView()
        {
            InitializeComponent();
            DataContext = new InformationVM();
        }
    }
}
