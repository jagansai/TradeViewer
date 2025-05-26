using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace TradeViewer
{

    public partial class MainWindow : Window
    {
        private String _fileName = String.Empty;
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        public MainWindow()
        {
            InitializeComponent();
            SetCurrentHoldingPeriodInfo(StockToTradeGroup.HOLDING_PERIOD);
        }

        private void LoadCsvButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog() { Filter = "CSV Files|*.csv" };
            var result = ofd.ShowDialog();
            if (result == false)
            {
                treeviewList.Items.Clear();
            }

            _fileName = ofd.FileName;
            loadTrades();

        }

        // All the logic for UI is in this method.

        private void loadTrades()
        {
            if (_fileName == String.Empty)
            {
                return;
            }


            var viewModel = (MainViewModel)DataContext;
            int hold_days = 0;
            hold_days = GetHoldingPeriod();
            var stocks = this.Search_Stcok.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower());
            IEnumerable<StockToTradeGroup> records = viewModel.LoadCsvData(_fileName, hold_days);

            // if we have data, then 
            if (records.Any())
            {
                if (stocks.Any())
                {
                    records = GetStocks(records, stocks);
                }
                if (Action.SelectedItem != null && Action.SelectedItem is ComboBoxItem comboBoxItem)
                {
                    records = FilterByStockAction(records, comboBoxItem.Content.ToString());
                }
            }
            treeviewList.ItemsSource = records.Select(x =>
            {

                var temp = new StockToTradeGroup
                {
                    CanSellDescr = StockToTradeGroup.calculateCanSell(x.Trades, x.HeldFor, hold_days),
                    StockName = x.StockName,
                    HeldFor = x.HeldFor,
                    Trades = x.Trades,
                    HoldingPeriod = hold_days
                };
                temp.CanSell = temp.GetActionColor();
                return temp;
            });
        }

        private IEnumerable<StockToTradeGroup> GetStocks(IEnumerable<StockToTradeGroup> records, IEnumerable<String> stocks)
        {
            return records.Where(x => stocks.Any(stock => x.StockName.ToLower().Contains(stock))).DistinctBy(x => x.StockName);
        }

        private IEnumerable<StockToTradeGroup> FilterByStockAction(IEnumerable<StockToTradeGroup> records, String stockAction)
        {
            if (stockAction.Equals(TradeViewerConstants.STOCK_ACTION_RESET)) return records;

            return records.Where(x => x.CanSellDescr.Equals(stockAction));
        }

        private void Search_Stock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Search_Stcok.Text.Length >= 3)
            {
                loadTrades();
            }
            else if (this.Search_Stcok.Text.Length == 0)
            {
                loadTrades();
            }
        }

        private void Holding_Period_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Holding_Period.Text.Length >= 2)
            {
                loadTrades();
            }
            else if (this.Holding_Period.Text.Length == 0)
            {
                loadTrades();
            }
        }

        private void Action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadTrades();
        }

        private void Holding_Period_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        public int GetHoldingPeriod()
        {
            int hold_days = 0;
            Int32.TryParse(this.Holding_Period.Text, out hold_days);
            int result = hold_days == 0 ? StockToTradeGroup.HOLDING_PERIOD : hold_days;
            SetCurrentHoldingPeriodInfo(result);
            return result;
        }

        private void SetCurrentHoldingPeriodInfo(int hold_days)
        {
            if (hold_days == 0)
            {
                CurrentHoldingPeriodInfo.Content = "Default Holding Period: " + StockToTradeGroup.HOLDING_PERIOD;
            }
            else
            {
                CurrentHoldingPeriodInfo.Content = "Current Holding Period: " + hold_days;
            }
        }
    }
}
