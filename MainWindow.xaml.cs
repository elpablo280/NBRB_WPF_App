using NBRB_WPF_App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NBRB_WPF_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new CurrencyViewModel();
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var viewModel = DataContext as CurrencyViewModel;
            viewModel?.DataGrid_CellEditEnding();
        }
    }
}