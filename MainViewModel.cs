using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace TradeViewer
{
    public class TradeViewerConstants
    {
        public static readonly string Buy = "Buy";
        public static readonly string Sell = "Sell";
        public static readonly string SOLD = "Sold";
        public static readonly string CAN_SELL = "Can Sell";
        public static readonly string CANNOT_SELL = "Cannot Sell";
        public static readonly string STOCK_ACTION_RESET = "Reset";
        
    }

    public class TradeData
    {        
        public string Date { get; set; }

        public string Stock { get; set; }
        public string Action { get; set; }
    }

    public class EnrichedTradeData : TradeData
    {
        public string HeldFor { get; set; }
    }

    public class StockToTradeGroup
    {
        private static readonly SolidColorBrush GREEN = new SolidColorBrush(Colors.Green);
        private static readonly SolidColorBrush RED = new SolidColorBrush(Colors.Red);
        public static readonly int HOLDING_PERIOD = 60;
        private int _holding_Period = 0;
        private SolidColorBrush _actionColor = RED;

        public string StockName { get; set; }
        public int HeldFor
        {
            get; set;
        }

        public int HoldingPeriod
        {
            set
            {
                _holding_Period = value;
            }
            get
            {
                return _holding_Period > 0 ? _holding_Period : HOLDING_PERIOD;
            }
        }


        public SolidColorBrush CanSell
        {
            get
            {
                return _actionColor;
            }
            set
            {
                _actionColor = GetActionColor();                
            }
        }

        public SolidColorBrush GetActionColor()
        {
            if (CanSellDescr.Equals(TradeViewerConstants.SOLD))
            {
                return RED;
            }

            return HeldFor >= HoldingPeriod ? GREEN : RED;

        }


        public String CanSellDescr
        {
            get; set;
        }

        public IEnumerable<EnrichedTradeData> Trades { get; set; }

        public static int calculateHeldFor( IEnumerable<TradeData> trades )
        {
            if (trades.Count() != 0)
            {
                return DateTimeOffset.Now.Subtract(DateTimeOffset.ParseExact(trades.First().Date, "dd-MMM-yyyy", CultureInfo.InvariantCulture)).Days;
            }
            return 0;
        }

        public static string calculateCanSell( IEnumerable<TradeData> trades, int heldForDays, int holding_period )
        {
            if (trades.Count() != 0)
            {
                return trades.First().Action.Equals(TradeViewerConstants.Sell) ? TradeViewerConstants.SOLD
                                                                      : heldForDays >= holding_period ? TradeViewerConstants.CAN_SELL : TradeViewerConstants.CANNOT_SELL;
            }
            return heldForDays >= holding_period ? TradeViewerConstants.CAN_SELL : TradeViewerConstants.CANNOT_SELL;
        }
    }


    public class MainViewModel
    {
        private ObservableCollection<StockToTradeGroup> records = new ObservableCollection<StockToTradeGroup>();

        public ObservableCollection<StockToTradeGroup> Records
        {
            get
            {
                return records;
            }
        }

        public ObservableCollection<StockToTradeGroup> LoadCsvData(string csvFilePath, int holding_period )
        {
            records.Clear();
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {

                var groupData = from trade in csv.GetRecords<TradeData>()
                                group trade by trade.Stock into stockGroup
                                select new { StockName = stockGroup.Key, Trades = EnrichTrades(stockGroup) };

                foreach (var data in groupData)
                {
                    StockToTradeGroup stockToTradeGroup = EnrichStackToTradeGroup(ref holding_period, data.StockName, data.Trades);
                    records.Add(stockToTradeGroup);
                }
            }
            return records;
        }

        private static StockToTradeGroup EnrichStackToTradeGroup(ref int holding_period, String stockName, List<TradeData> trades)
        {
            List<EnrichedTradeData> enrichedTradeDatas = new List<EnrichedTradeData>();

            EnrichedTradeData? temp = null;

            foreach (var trade in trades)
            {
                if (temp != null && temp.HeldFor == null)
                {
                    var tempDate = DateTimeOffset.ParseExact(temp.Date, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    var tradeDate = DateTimeOffset.ParseExact(trade.Date, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                    temp.HeldFor = tempDate.Subtract(tradeDate).Days.ToString();
                }
                var enrichedTrade = new EnrichedTradeData { Stock = trade.Stock, Action = trade.Action, Date = trade.Date };
                enrichedTradeDatas.Add(enrichedTrade);
                if (trade.Action.Equals(TradeViewerConstants.Sell))
                {
                    temp = enrichedTrade;
                }
            }
            int heldForDays = StockToTradeGroup.calculateHeldFor(enrichedTradeDatas);
            holding_period = holding_period != 0 ? holding_period : StockToTradeGroup.HOLDING_PERIOD;
            var stockToTradeGroup = new StockToTradeGroup()
            {
                StockName = stockName,
                Trades = enrichedTradeDatas,
                HeldFor = heldForDays,
                CanSellDescr = StockToTradeGroup.calculateCanSell(enrichedTradeDatas, heldForDays, holding_period),
                HoldingPeriod = holding_period
            };
            return stockToTradeGroup;
        }

        private List<TradeData> EnrichTrades(IGrouping<string, TradeData> stockGroup)
        {
            return stockGroup.OrderByDescending(x => DateTimeOffset.ParseExact(x.Date.Trim(), "dd-MMM-yyyy", CultureInfo.InvariantCulture))
                             .ThenByDescending(x => x.Action).ToList();
        }


    }
}
