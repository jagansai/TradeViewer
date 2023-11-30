using CsvHelper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TradeViewer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : Window
    {
        private String fileName = String.Empty;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "CSV Files|*.csv" };
            var result = ofd.ShowDialog();
            if (result == false)
            {
                treeviewList.Items.Clear();
            }

            fileName = ofd.FileName;
            var viewModel = (MainViewModel)DataContext;
            var hold_days_text = this.Holding_Period.Text;
            int hold_days = 0;
            if ( hold_days_text.Length >= 2 && Int32.TryParse(hold_days_text, out hold_days) )
            {
                var records = viewModel.LoadCsvData(fileName, hold_days);
                treeviewList.ItemsSource = records;
            }
            else
            {
                var records = viewModel.LoadCsvData(fileName, 0);
                treeviewList.ItemsSource = records;
            }
            
        }

        private void Search_Stcok_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ( this.Search_Stcok.Text.Length >= 3 )
            {
                var viewModel = (MainViewModel)DataContext;
                var stocks = this.Search_Stcok.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select(x => x.ToLower());
                treeviewList.ItemsSource = viewModel.Records.Where(x => stocks.Any( stock => x.StockName.ToLower().Contains(stock))).DistinctBy(x => x.StockName);
            }
            if ( this.Search_Stcok.Text.Length == 0 ) 
            {
                var viewModel = (MainViewModel)DataContext;                
                treeviewList.ItemsSource = viewModel.Records;
            }
        }

        private void Holding_Period_TextChanged( object sender, TextChangedEventArgs e)
        {
            var hold_days_text = this.Holding_Period.Text;
            int hold_days = 0;
            if ( hold_days_text.Length >= 2 && Int32.TryParse(hold_days_text, out hold_days ) )
            {
                
            }
        }
    }
}
