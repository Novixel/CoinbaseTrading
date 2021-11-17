using CoinbasePro.Services.Orders.Types;
using CoinbasePro.WebSocket;
using CoinbasePro.WebSocket.Models.Response;
using CoinbasePro.WebSocket.Types;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FawkesTrader
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        List<string> productTypes;
        List<ChannelType> channels;
        public IWebSocket webSocket;
        MyBotControl[] control = new MyBotControl[2];
        int selectedBot = 0;

        string selected;

        Product selectedProduct;
        public static OrderSide PrefferedSideOverride;
        private Product[] _products;
        private string[] _productIDs;
        private Currency[] _currencies;

        private string _currentPair;
        public static decimal _currentPrice;
        private decimal _currentHigh;
        private decimal _currentLow;

        public static Dictionary<string, decimal> botPrices = new Dictionary<string, decimal>();

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            init();
        }

        private void init()
        {
            productTypes = new List<string>() { "BTC-USD", };
            channels = new List<ChannelType>() { ChannelType.Full, ChannelType.User, ChannelType.Ticker, ChannelType.Status };
            webSocket = App.client.WebSocket;
            webSocket.Start(ProductTypes, channels);
            sortProducts();
            webSocket.OnTickerReceived += WebSocket_OnTickerReceived;
            webSocket.OnStatusReceived += WebSocket_OnStatusReceived;
            productBox.SelectedItem = "BTC-USD";
            availablePairs.SelectedItem = "BTC-USD";

            productBox.SelectedItem = productBox.SelectedItem;
            availablePairs.SelectedItem = availablePairs.SelectedItem;

            for (int i = 0; i < control.Length; i++)
            {
                activeBots.Items.Add(i);
            }
        }

        private void sortProducts()
        {
            productBox.Items.Clear();
            for (int i = 0; i < productTypes.Count; i++)
            {
                productBox.Items.Add(productTypes[i]);
            }
        }

        private void restart(string NewPro)
        {
            if (!ProductTypes.Contains(NewPro))
            {
                ProductTypes.Add(NewPro);
            }

            sortProducts();

            if (webSocket != null)
            {
                webSocket.Stop();
                webSocket.Start(ProductTypes, channels);
                webSocket.OnTickerReceived += WebSocket_OnTickerReceived;
                webSocket.OnStatusReceived += WebSocket_OnStatusReceived;
            }
        }

        public decimal CurrentPrice
        {
            get { return _currentPrice; }
            set
            {
                if (_currentPrice != value)
                {
                    _currentPrice = value;
                    OnPropertyChanged();
                }
            }
        }
        public string CurrentPair
        {
            get => _currentPair;
            set
            {
                if (_currentPair != value)
                {
                    _currentPair = value;
                    OnPropertyChanged();
                }
            }
        }

        public Product[] Products
        {
            get => _products;
            set
            {
                if (_products != value)
                {
                    _products = value;
                    OnPropertyChanged();
                }
            }
        }

        public Currency[] Currencies
        {
            get => _currencies;
            set
            {
                if (_currencies != value)
                {
                    _currencies = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal CurrentHigh
        {
            get => _currentHigh;
            set
            {
                if (_currentHigh != value)
                {
                    _currentHigh = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal CurrentLow
        {
            get => _currentLow;
            set
            {
                if (_currentLow != value)
                {
                    _currentLow = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> ProductTypes
        {
            get => productTypes;
            set
            {
                if (productTypes != value)
                {
                    productTypes = value;
                    OnPropertyChanged();
                }
            }
        }

        public string[] ProductIDs
        {
            get => _productIDs;
            set
            {
                if (_productIDs != value)
                {
                    _productIDs = value;
                    OnPropertyChanged();
                }
            }

        }
        public Product SelectedProduct
        {
            get => selectedProduct;
            set
            {
                if (selectedProduct != value)
                {
                    selectedProduct = value;
                    OnPropertyChanged();
                }
            }
        }

        private void WebSocket_OnTickerReceived(object sender, WebfeedEventArgs<Ticker> e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (productBox.Items != null)
                {
                    if (productBox.SelectedItem != null)
                    {
                        SelectedProduct = GetProduct(productBox.SelectedItem.ToString());
                        selected = productBox.SelectedItem.ToString();
                        if (SelectedProduct != null)
                            App.message("Product Set : " + SelectedProduct.Id);
                    }
                    CurrentPair = e.LastOrder.ProductId;
                    CurrentPrice = e.LastOrder.Price;
                    CurrentHigh = e.LastOrder.High24H;
                    CurrentLow = e.LastOrder.Low24H;
                    botPrices[e.LastOrder.ProductId] = e.LastOrder.Price;
                }
            });
        }

        private void WebSocket_OnStatusReceived(object sender, WebfeedEventArgs<Status> e)
        {
            Products = e.LastOrder.Products;
            this.Dispatcher.Invoke(() =>
            {
                if (Products != null)
                {
                    for (int i = 0; i < Products.Length; i++)
                    {
                        if (!availablePairs.Items.Contains(Products[i].Id) && !Products[i].Id.Contains("XRP"))
                        {
                            availablePairs.Items.Add(Products[i].Id);
                        }
                    }
                }
            });
            Currencies = e.LastOrder.Currencies;
        }

        private Product GetProduct(string Product_id)
        {
            if (Products != null)
            {
                for (int i = 0; i < Products.Length; i++)
                {
                    if (Products[i].Id == Product_id)
                    {
                        App.message("Product Set : ");
                        return Products[i];
                    }
                }
            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (availablePairs != null)
            {
                restart(availablePairs.SelectedItem.ToString());
            }
        }



        private void botButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (selected != null)
            {

                if (botButton.Content != null && control[0] == null && selectedBot == 0)
                {
                    TradeBotStartUp(selectedBot);
                    App.message("Bot Started");
                }
                else if (botButton.Content != null && control[1] == null && selectedBot == 1)
                {
                    TradeBotStartUp(selectedBot);
                    App.message("Bot Started");
                }
                else
                {
                    TradeBotShutDown(selectedBot);
                    App.message("Bot Stopped");
                }
            }
            else
            {
                App.message("No Product Selected", true);
            }
        }

        private void TradeBotStartUp(int bot)
        {
            if (SelectedProduct != null && selectedBot == bot)
            {
                control[bot] = new MyBotControl(SelectedProduct);
                control[bot].activeStatus.Background = Brushes.Green;
                if (botContentArea.Content == null && control[0] != null)
                {
                    botContentArea.Content = control[0];
                }
                else if (botContentArea2.Content == null && control[1] != null)
                {
                    botContentArea2.Content = control[1];
                }
                control[bot].StartWatch();
            }
            else
            {
                App.message("Seleceted Product is null", true);
            }


        }

        private void TradeBotShutDown(int bot)
        {

            if (control[bot] != null && selectedBot == bot)
            {
                control[bot].StopWatch();
                control[bot].activeStatus.Background = Brushes.Red;
                control[bot] = null;
                if (selectedBot == 0)
                    botContentArea.Content = null;
                if (selectedBot == 1)
                    botContentArea2.Content = null;
            }
            else
            {
                TradeBotStartUp(bot);
            }
        }

        private void BotBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (activeBots.SelectedItem != null)
            {
                selectedBot = (int)activeBots.SelectedItem;
                if (control[selectedBot] != null)
                {
                    botButton.Content = "Bot " + selectedBot;
                }
                else
                {
                    botButton.Content = "Bot " + selectedBot;
                }
            }
            App.message("Selected Bot #" + selectedBot.ToString());
        }
    }
}
