using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
