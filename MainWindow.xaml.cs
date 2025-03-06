using NBRB_WPF_App.Models;
using NBRB_WPF_App.ViewModels;
using System;
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
            //Rate R = e.Row.Item as Rate;
            //MessageBox.Show($"{R.Cur_ID} {R.Cur_Name} {R.Date} {R.Cur_OfficialRate} {R.Cur_Abbreviation}");
            var viewModel = DataContext as CurrencyViewModel;
            viewModel?.DataGrid_CellEditEnding();
        }
    }
}