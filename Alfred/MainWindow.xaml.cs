using CoinbasePro.Services.Accounts.Models;
using CoinbasePro.Services.Orders.Models.Responses;
using CoinbasePro.Services.Orders.Types;
using CoinbasePro.WebSocket.Models.Response;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Alfred
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<OrderResponse> openTrades;
        private List<OrderResponse> filledTrades;

        private OrderSide tradeSide;
        private decimal tradeAmount;
        private decimal tradePrice;
        private decimal tradeFee;
        private decimal tradeTotal;

        private Product product;
        private string currentPair;
        private decimal currentPrice;

        private string baseCurrency;
        private decimal availableBase;
        private string quoteCurrency;
        private decimal availableQuote;

        private Account baseAccount;
        private Account quoteAccount;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Product CurrentProduct
        {
            get => product;
            set
            {
                if (product != value)
                {
                    product = value;
                    OnPropertyChanged();
                }
            }
        }


        public string CurrentPair
        {
            get => currentPair;
            set
            {
                if (currentPair != value)
                {
                    currentPair = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal CurrentPrice
        {
            get => currentPrice;
            set
            {
                if (currentPrice != value)
                {
                    currentPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BaseCurrency
        {
            get => baseCurrency;
            set
            {
                if (baseCurrency != value)
                {
                    baseCurrency = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal AvailableBase
        {
            get => availableBase;
            set
            {
                if (availableBase != value)
                {
                    availableBase = value;
                    OnPropertyChanged();
                }
            }
        }
        public string QuoteCurrency
        {
            get => quoteCurrency;
            set
            {
                if (quoteCurrency != value)
                {
                    quoteCurrency = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal AvailableQuote
        {
            get => availableQuote;
            set
            {
                if (availableQuote != value)
                {
                    availableQuote = value;
                    OnPropertyChanged();
                }
            }
        }
        public Account BaseAccount
        {
            get => baseAccount;
            set
            {
                if (baseAccount != value)
                {
                    baseAccount = value;
                    OnPropertyChanged();
                }
            }
        }
        public Account QuoteAccount
        {
            get => quoteAccount;
            set
            {
                if (quoteAccount != value)
                {
                    quoteAccount = value;
                    OnPropertyChanged();
                }
            }
        }

        public OrderSide TradeSide
        {
            get => tradeSide;
            set
            {
                if (tradeSide != value)
                {
                    tradeSide = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal TradeAmount
        {
            get => tradeAmount;
            set
            {
                if (tradeAmount != value)
                {
                    tradeAmount = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal TradePrice
        {
            get => tradePrice;
            set
            {
                if (tradePrice != value)
                {
                    tradePrice = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TradeFee
        {
            get => tradeFee;
            set
            {
                if (tradeFee != value)
                {
                    tradeFee = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal TradeTotal
        {
            get => tradeTotal;
            set
            {
                if (tradeTotal != value)
                {
                    tradeTotal = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            while (App.products == null)
            {
                /// wait
                Trace.WriteLine("waiting");
            }
            while (App.accounts == null)
            {
                Trace.WriteLine("waiting");
            }
            foreach (Product pro in App.products.Values)
            {
                if (App.accounts.ContainsKey(pro.BaseCurrency) && App.accounts.ContainsKey(pro.QuoteCurrency))
                {
                    listBoxPro.Items.Add(pro.Id);
                }
            }

            openTrades = new List<OrderResponse>();
            filledTrades = new List<OrderResponse>();
        }

        private void listBoxPro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeSelectedProduct(e.AddedItems[0].ToString());
        }

        private void ChangeSelectedProduct(string productID)
        {
            if (CurrentProduct != null && CurrentProduct.Id == productID)
                return;

            Trace.WriteLine("Product Changed!: " + productID);
            CurrentProduct = App.products[productID];
            CurrentPair = CurrentProduct.Id;
            BaseCurrency = CurrentProduct.BaseCurrency;
            QuoteCurrency = CurrentProduct.QuoteCurrency;
            BaseAccount = App.accounts[BaseCurrency];
            AvailableBase = BaseAccount.Available;
            QuoteAccount = App.accounts[QuoteCurrency];
            AvailableQuote = QuoteAccount.Available;
            TradeAmount = CurrentProduct.BaseMinSize;
            TradePrice = CurrentPrice;
            App.AddProductSub(productID);
        }

        private void ChangeSide(object sender, RoutedEventArgs e)
        {
            if (sender == btnBuy)
                TradeSide = OrderSide.Buy;
            else if (sender == btnSell)
                TradeSide = OrderSide.Sell;
            else
                TradeSide = OrderSide.Buy;
        }

        private void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Sending " + TradeSide.ToString() + " Trade Of " + TradeAmount + " @ " + TradePrice);
            PlaceLimitOrder(TradeSide, TradeAmount, TradePrice);
        }

        private async void PlaceLimitOrder(OrderSide side, decimal size, decimal price)
        {
            if (size < CurrentProduct.BaseMinSize)
            {
                return;
            }

            if (side == OrderSide.Buy && size * price > AvailableQuote)
            {
                return;
            }

            if (side == OrderSide.Sell && size > AvailableBase)
            {
                return;
            }
            // Final Stop Before Trade!!
            var response = await App.client.OrdersService.PlaceLimitOrderAsync(side, CurrentPair, size, price, postOnly: false);
            LogTrade(response);
        }

        private void LogTrade(OrderResponse orderResponse)
        {
            Trace.WriteLine("Trade Created At: " + orderResponse.CreatedAt);
            openTrades.Add(orderResponse);
            tradeBox.Items.Add((orderResponse.Side.ToString() + "\t" + orderResponse.Size + " " + orderResponse.Price + "\t " + orderResponse.FillFees + "\t" + orderResponse.Status));
            //// WORK ON THIS
        }


        private void tradeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedOrder = e.AddedItems[0];
            if (selectedOrder != null)
            {
                Trace.WriteLine(selectedOrder);
            }
        }
    }
}
