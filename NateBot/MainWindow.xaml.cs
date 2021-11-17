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
using System.Timers;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System;

namespace NateBot
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        #region Private Variables

        private Timer MainTimer;
        private Timer AppTimer;
        private int appTime;
        private int appTimeMin;

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
        private decimal currentChange;

        private string baseCurrency;
        private decimal availableBase;
        private string quoteCurrency;
        private decimal availableQuote;

        private Account baseAccount;
        private Account quoteAccount;

        private decimal totalOfBuy;
        private decimal totalOfSell;

        private bool botActive;

        private bool botSellActive;

        private bool botUpperLimitActive;

        private bool botBuyActive;

        private bool botLowerLimitActive;

        private bool trailingUp;
        private bool trailingDown;

        private bool trailedUp;
        private decimal trailedUpPrice;
        private bool trailedDown;
        private decimal trailedDownPrice;

        private bool repeatBuy;
        private decimal repeatBuyAmount;
        
        private bool repeatSell;
        private decimal repeatSellAmount;

        private decimal buyLimitPrice;
        private decimal buyLimitAmount;

        private decimal lowerLimitPrice;
        private decimal lowerLimitAmount;

        private decimal sellLimitPrice;
        private decimal sellLimitAmount;

        private decimal upperLimitPrice;
        private decimal upperLimitAmount;

        private decimal buyTrailAmount;
        private decimal sellTrailAmount;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            // if we hit enter anywhere on the grid we will set focas to returner :P
            // this removes focas from selected input so we can change values faster
            if (e.Key == Key.Return)
            {
                returner.Focus();
            }
        }

        private void SetMainTimer()
        {
            if (MainTimer != null)
                MainTimer.Close();
            MainTimer = new Timer(10000);
            MainTimer.Elapsed += OnTimedMainEvent;
            MainTimer.AutoReset = true;
            MainTimer.Enabled = true;
        }

        private void SetAppTimer()
        {
            if (AppTimer != null)
                AppTimer.Close();
            AppTimer = new Timer(1000);
            AppTimer.Elapsed += OnTimedAppEvent;
            AppTimer.AutoReset = true;
            AppTimer.Enabled = true;
        }

        private void OnTimedAppEvent(object source, ElapsedEventArgs e)
        {
            AppTime += 1;
            if (BotActive)
            {
                CheckMarket();
            }

            if (AppTime % 60 == 0)
            {
                AppTimeMin += 1;
                Trace.WriteLine(AppTimeMin + " min tick");
            }

        }

        private void OnTimedMainEvent(object source, ElapsedEventArgs e)
        {
            if (App.tickers.ContainsKey(CurrentPair))
            {
                decimal openPrice = App.tickers[CurrentPair].Open24H;
                CurrentChange = decimal.Round((CurrentPrice - openPrice) / openPrice * 100, 2);
            }
            UpdateBotStuff();
            Profit();
        }

        #endregion

        #region Public Variables

        public event PropertyChangedEventHandler PropertyChanged;

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
        public decimal CurrentChange
        {
            get => currentChange;
            set
            {
                if (currentChange != value)
                {
                    currentChange = value;
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

        public bool BotActive 
        { 
            get => botActive; 
            set
            {
                if (botActive != value)
                {
                    botActive = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool BotSellActive 
        { 
            get => botSellActive; 
            set
            {
                if (botSellActive != value)
                {
                    if (value == false)
                    {
                        RepeatSell = false;
                        TrailingUp = false;
                    }
                    
                    botSellActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool BotUpperLimitActive 
        { 
            get => botUpperLimitActive; 
            set
            {
                if (botUpperLimitActive != value)
                {
                    botUpperLimitActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool BotBuyActive 
        { 
            get => botBuyActive; 
            set
            {
                if (botBuyActive != value)
                {
                    if (value == false)
                    {
                        RepeatBuy = false;
                        TrailingDown = false;
                    }
                    botBuyActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool BotLowerLimitActive 
        { 
            get => botLowerLimitActive; 
            set
            {
                if (botLowerLimitActive != value)
                {
                    botLowerLimitActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool TrailingUp 
        { 
            get => trailingUp; 
            set
            {
                if (trailingUp != value)
                {
                    trailingUp = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal SellTrailAmount
        {
            get => sellTrailAmount;
            set
            {
                if (sellTrailAmount != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Sell Trail Amount To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = sellTrailAmount;
                        }
                    }
                    sellTrailAmount = value;
                    OnPropertyChanged();
                }
            }
        }


        public bool TrailingDown 
        { 
            get => trailingDown; 
            set
            {
                if (trailingDown != value)
                {
                    trailingDown = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal BuyTrailAmount
        {
            get => buyTrailAmount;
            set
            {
                if (buyTrailAmount != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Buy Trail Amount To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = buyTrailAmount;
                        }
                    }
                    buyTrailAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool RepeatBuy 
        { 
            get => repeatBuy; 
            set
            {
                if (repeatBuy!= value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Repeat Buy To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = repeatBuy;
                        }
                    }
                    repeatBuy = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal RepeatBuyAmount 
        { 
            get => repeatBuyAmount;
            set
            {
                if (repeatBuyAmount != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Repeat Buy Amount To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = repeatBuyAmount;
                        }
                    }
                    repeatBuyAmount = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool RepeatSell 
        { 
            get => repeatSell; 
            set
            {
                if (repeatSell != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Repeat Sell To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = repeatSell;
                        }
                    }
                    repeatSell = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal RepeatSellAmount 
        { 
            get => repeatSellAmount; 
            set
            {
                if (repeatSellAmount != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Repeat Sell Amount To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = repeatSellAmount;
                        }
                    }
                    repeatSellAmount = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal BuyLimitPrice 
        { 
            get => buyLimitPrice; 
            set
            {
                if (buyLimitPrice != value)
                {
                    if (BotActive && !TrailingDown && BotBuyActive)
                    {
                        if (MessageBox.Show("Change Buy Limit Price To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = buyLimitPrice;
                        }
                    }
                    TotalOfBuy = decimal.Round(BuyLimitAmount * value, GetDecimalPlaces(CurrentProduct.QuoteIncrement));
                    buyLimitPrice = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal BuyLimitAmount 
        { 
            get => buyLimitAmount; 
            set
            {
                if (buyLimitAmount != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Buy Limit Amount To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = buyLimitAmount;
                        }
                    }
                    TotalOfBuy = decimal.Round(value * BuyLimitPrice, GetDecimalPlaces(CurrentProduct.QuoteIncrement));
                    buyLimitAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal LowerLimitPrice 
        { 
            get => lowerLimitPrice; 
            set
            {
                if (lowerLimitPrice != value)
                {
                    if (BotActive && !TrailingDown)
                    {
                        if (MessageBox.Show("Change Lower Limit Price To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = lowerLimitPrice;
                        }
                    }
                    lowerLimitPrice = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal LowerLimitAmount
        { 
            get => lowerLimitAmount; 
            set
            {
                if (lowerLimitAmount != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Lower Limit Amount To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = lowerLimitAmount;
                        }
                    }
                    lowerLimitAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal SellLimitPrice 
        { 
            get => sellLimitPrice; 
            set
            {
                if (sellLimitPrice != value)
                {
                    if (BotActive && !TrailingUp && BotSellActive)
                    {
                        if (MessageBox.Show("Change Sell Limit Price To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = sellLimitPrice;
                        }
                    }
                    TotalOfSell = decimal.Round(SellLimitAmount * value, GetDecimalPlaces(CurrentProduct.QuoteIncrement));
                    sellLimitPrice = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal SellLimitAmount 
        { 
            get => sellLimitAmount; 
            set
            {
                if (sellLimitAmount != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Sell Limit Amount To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = sellLimitAmount;
                        }
                        TotalOfSell = decimal.Round(value * SellLimitPrice, GetDecimalPlaces(CurrentProduct.QuoteIncrement));
                    }
                    sellLimitAmount = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal UpperLimitPrice
        {
            get => upperLimitPrice;
            set
            {
                if (upperLimitPrice != value)
                {
                    if (BotActive && !TrailingUp)
                    {
                        if (MessageBox.Show("Change Stop Loss Price To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = upperLimitPrice;
                        }
                    }

                    upperLimitPrice = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal UpperLimitAmount
        {
            get => upperLimitAmount;
            set
            {
                if (upperLimitAmount != value)
                {
                    if (BotActive)
                    {
                        if (MessageBox.Show("Change Stop Loss Amount To :" + value.ToString(), "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        {
                            value = upperLimitAmount;
                        }
                    }
                    upperLimitAmount = value;
                    OnPropertyChanged();
                }
            }
        }
        public int AppTime
        {
            get => appTime;
            set
            {
                if (appTime != value)
                {
                    appTime = value;
                    OnPropertyChanged();
                }
            }
        }
        public int AppTimeMin 
        { 
            get => appTimeMin; 
            set
            {
                if (appTimeMin != value)
                {
                    appTimeMin = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal TotalOfBuy 
        { 
            get => totalOfBuy;
            set
            {
                if (totalOfBuy != value)
                {
                    totalOfBuy = value;
                    OnPropertyChanged();
                }
            }
        }
        public decimal TotalOfSell 
        { 
            get => totalOfSell;
            set
            {
                if (totalOfSell != value)
                {
                    totalOfSell = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        // Initialize!

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            InitializeWindow();
            SetAppTimer();
        }
        private void InitializeWindow()
        {
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
                    comBoxPro.Items.Add(pro.Id);
                }
            }
        }
        private void InitializeTradeBot()
        {
            openTrades = new List<OrderResponse>();
            filledTrades = new List<OrderResponse>();
            ToggleAllOff();
            SetMainTimer();
        }


        #region Buttons

        private void ToggleAllOff()
        {
            BotActive = false;
            btnBotOff.IsChecked = true;

            BotBuyActive = false;
            btnBotBuyOff.IsChecked = true;
            BotLowerLimitActive = false;
            btnLowerLimitOff.IsChecked = true;
            TrailingDown = false;
            btnBotBuyTrailOff.IsChecked = true;
            RepeatBuy = false;
            btnBotBuyRepeatOff.IsChecked = true;

            BotSellActive = false;
            btnBotSellOff.IsChecked = true;
            BotUpperLimitActive = false;
            btnUpperLimitOff.IsChecked = true;
            TrailingUp = false;
            btnBotSellTrailOff.IsChecked = true;
            RepeatSell = false;
            btnBotSellRepeatOff.IsChecked = true;

            trailedDownPrice = 0;
            trailedUp = false;
            trailedUpPrice = 0;
            trailedDown = false;
        }

        public void SetMaxAmount(object sender, RoutedEventArgs e)
        {
            if (CurrentProduct != null)
            {
                if (sender == btnMAXBuyAmount && BuyLimitPrice != 0)
                {
                    BuyLimitAmount = QuoteConvert(BuyLimitPrice);
                }

                if (sender == btnMAXSellAmount && SellLimitPrice != 0)
                {
                    SellLimitAmount = decimal.Round(AvailableBase, GetDecimalPlaces(CurrentProduct.BaseIncrement));
                }
            }
        }

        // PANIC

        private void PanicButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Sell All Coins & Stop?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            else
            {
                Trace.WriteLine("Panic Button Not Implemented Yet");
                ToggleAllOff();
            }
        }

        // MAIN Power Switch!

        private void TurnBotOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("BOT ENABLED!");
            BotActive = true;
        }

        private void TurnBotOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("BOT DISABLED!");
            BotActive = false;
            trailedDownPrice = 0;
            trailedUp = false;
            trailedUpPrice = 0;
            trailedDown = false;
        }

        // Buy Switch

        private void TurnBuyOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Buying ENABLED!");
        }

        private void TurnBuyOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Buying DISABLED!");
        }

        // Buy Repeat

        private void TurnBuyRepeatOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Buy Repeat ENABLED!");
        }

        private void TurnBuyRepeatOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Buy Repeat DISABLED!");
        }

        // Buy Stop

        private void TurnLowerLimitOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Buy Stop ENABLED!");
        }

        private void TurnLowerLimitOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Buy Stop DISABLED!");
        }

        // Buy Trail

        private void TurnBuyTrailOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Buy Trail ENABLED!");
        }

        private void TurnBuyTrailOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Buy Trail DISABLED!");
        }

        // Sell Switch

        private void TurnSellOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Selling ENABLED!");
        }

        private void TurnSellOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Selling DISABLED!");
        }

        // Sell Repeat

        private void TurnSellRepeatOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Sell Repeat ENABLED!");
        }

        private void TurnSellRepeatOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Sell Repeat DISABLED!");
        }

        // Upper Limit

        private void TurnUpperLimitOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Sell Stop ENABLED!");
        }

        private void TurnUpperLimitOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Sell Stop DISABLED!");
        }

        // Sell Trail

        private void TurnSellTrailOn(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Sell Trail ENABLED!");
        }

        private void TurnSellTrailOff(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("Sell Trail DISABLED!");
        }

        // On Product Change!

        private void comBoxPro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            for (int i = 0; i < e.AddedItems.Count; i++)
            {
                Trace.WriteLine(e.AddedItems[i].ToString());
                ChangeSelectedProduct(e.AddedItems[i].ToString());
            }
        }


        #endregion

        #region Functions

        private void ChangeSelectedProduct(string productID)
        {
            if (CurrentProduct != null && CurrentProduct.Id == productID)
                return;
            Trace.WriteLine("Product Changed To " + productID);
            SetProduct(productID);
            SetAccounts();
            InitializeTradeBot();
        }

        public static int GetDecimalPlaces(decimal n)
        {
            n = Math.Abs(n); //make sure it is positive.
            n -= (int)n;     //remove the integer part of the number.
            var decimalPlaces = 0;
            while (n > 0)
            {
                decimalPlaces++;
                n *= 10;
                n -= (int)n;
            }
            return decimalPlaces;
        }

        private void SetDefaults(object sender, RoutedEventArgs e)
        { 
            if (CurrentProduct != null)
            {
                BuyLimitAmount = CurrentProduct.BaseMinSize;
                BuyLimitPrice = CurrentPrice - (0.01m * CurrentPrice);
                LowerLimitPrice = CurrentPrice - (0.03m * CurrentPrice);
                LowerLimitAmount = -1;
                BuyTrailAmount = 0.01m;

                trailedDownPrice = 0;
                trailedUp = false;
                trailedUpPrice = 0;
                trailedDown = false;

                SellLimitAmount = CurrentProduct.BaseMinSize;
                SellLimitPrice = CurrentPrice + (0.01m * CurrentPrice);
                UpperLimitPrice = CurrentPrice + (0.03m * CurrentPrice);
                UpperLimitAmount = -1;
                SellTrailAmount = 0.01m;
            }
        }

        private void SetProduct(string productID)
        {
            // Set Our Product
            CurrentProduct = App.products[productID];
            CurrentPair = CurrentProduct.Id;

            // Set Our Extentions
            BaseCurrency = CurrentProduct.BaseCurrency;
            QuoteCurrency = CurrentProduct.QuoteCurrency;
            
            App.AddProductSub(productID);
        }

        private void SetAccounts()
        {
            BaseAccount = App.accounts[BaseCurrency];
            AvailableBase = BaseAccount.Available;

            QuoteAccount = App.accounts[QuoteCurrency];
            AvailableQuote = QuoteAccount.Available;
        }

        private void SetTickers()
        {
            var tmp = App.tickers;
            if (tmp != null)
            {
                foreach (Ticker ticker in tmp.Values)
                {
                    if (ticker.ProductId == CurrentPair)
                    {
                        bool uiAccess = scrollingText.Dispatcher.CheckAccess();
                        if (uiAccess)
                            scrollingText.Text = " 24h Open: " + ticker.Open24H + " 24h Low: " + ticker.Low24H + " 24h High: " + ticker.High24H; 
                        else
                            scrollingText.Dispatcher.Invoke(() => { scrollingText.Text = " Open: " + ticker.Open24H + " Low: " + ticker.Low24H + " High: " + ticker.High24H;  });
                        
                    }
                }
            }
        }

        public void UpdateBotStuff()
        {
            SetAccounts();
            SetTickers();
        }

        private decimal QuoteConvert(decimal price)
        {
            if (CurrentProduct.QuoteCurrency == "BTC" || CurrentProduct.QuoteCurrency == "ETH" || CurrentProduct.QuoteCurrency == "USDC" || CurrentProduct.QuoteCurrency == "USD" || CurrentProduct.QuoteCurrency == "GPD")
            {
                return decimal.Round(AvailableQuote * (1 / price), GetDecimalPlaces(CurrentProduct.BaseIncrement));
            }
            else if (CurrentProduct.QuoteCurrency == "BTC" && CurrentProduct.BaseCurrency == "ETH")
            {
                return decimal.Round(1 * AvailableQuote * (1 / price), GetDecimalPlaces(CurrentProduct.BaseIncrement));
            }
            else
            {
                return decimal.Round(AvailableQuote * (CurrentProduct.MinMarketFunds / price), GetDecimalPlaces(CurrentProduct.BaseIncrement));
            }
        }

        private void Profit()
        {
            decimal profit = QuoteConvert(CurrentPrice) + AvailableBase;

            //Trace.WriteLine("Current Total In Base:" + profit.ToString());
            //Trace.WriteLine("Current Total In Quote:" + (profit * CurrentPrice).ToString());
        }

        #endregion

        #region Market Checks

        public void CheckMarket()
        {
            if (BotActive)
            {
                var startT = DateTime.Now;
                Trace.WriteLine("\n\t\t** BOT LOOP START!\t\t\n");
                decimal currPrice = CurrentPrice;

                // If Weve made any trades lets check out last trade difference
                // NOT IMP

                if (BotLowerLimitActive)
                {
                    if (CheckLowerLimit(currPrice))
                    {
                        Trace.WriteLine("Lower Limit Triggered On " + CurrentPair + " At " + currPrice + " " + QuoteCurrency + " With Target Of " + LowerLimitAmount + " " + BaseCurrency);
                    }
                }

                if (BotBuyActive)
                {
                    if (CheckForBuy(currPrice))
                    {
                        Trace.WriteLine("Sending Buy Trade of " + CurrentPair + " With " + BuyLimitAmount + " " + BaseCurrency + " at " + currPrice + " " + QuoteCurrency);
                        PlaceLimitOrder(OrderSide.Buy, BuyLimitAmount, currPrice);
                        if (!RepeatBuy)
                        {
                            BotBuyActive = false;
                            BuyLimitPrice = BuyLimitPrice - (0.001m * BuyLimitPrice);
                        }
                    }
                }

                if (BotSellActive)
                {
                    if (CheckForSell(currPrice))
                    {
                        Trace.WriteLine("Sending Sell Trade of " + CurrentPair + " With " + SellLimitAmount + " " + BaseCurrency + " at " + currPrice + " " + QuoteCurrency); ;
                        PlaceLimitOrder(OrderSide.Sell, SellLimitAmount, currPrice);
                        if (!RepeatSell)
                        {
                            BotSellActive = false;
                            SellLimitPrice = SellLimitPrice + (0.001m * SellLimitPrice);
                        }
                            
                    }
                }

                if (BotUpperLimitActive)
                {
                    if (CheckUpperLimit(currPrice))
                    {
                        Trace.WriteLine("Upper Limit Triggered On " + CurrentPair + " At " + currPrice + " " + QuoteCurrency + " With Target Of " + UpperLimitAmount + " " + BaseCurrency);
                    }
                }


                var endT = DateTime.Now;
                var ftime = endT - startT; 
                Trace.WriteLine("\n*\t\t*Elapsed Time: "+ ftime.ToString());
                Trace.WriteLine("\t\t** BOT LOOP END!\t\t\n");
            }
        }

        private bool CheckForBuy(decimal currPrice)
        {
            if (TrailingDown)
            {
                Trace.WriteLine("Trail Price = " + trailedDownPrice);
                if (currPrice < BuyLimitPrice)
                {
                    Trace.WriteLine("Lower Trail Reached! : Trailing Down!");
                    trailedDownPrice = decimal.Round(currPrice + (0.005m * currPrice), GetDecimalPlaces(CurrentProduct.QuoteIncrement));
                    BuyLimitPrice = decimal.Round(currPrice - (BuyTrailAmount * currPrice), GetDecimalPlaces(CurrentProduct.QuoteIncrement));
                    trailedDown = true;
                }
                else if (trailedDown && trailedDownPrice <= currentPrice)
                {
                    Trace.WriteLine("Stop Loss Trail Triggered! : Buying Now!");
                    trailedDown = false;
                }
            }

            if (currPrice <= BuyLimitPrice)
            {
                Trace.WriteLine("The Price Is Below Or At Our Buy Limit");
                if (AvailableQuote >= CurrentProduct.BaseMinSize * currPrice)
                {
                    Trace.WriteLine("And We Have Enough To Buy At Min");
                    if (BuyLimitAmount * AvailableQuote >= CurrentProduct.BaseMinSize)
                    {
                        Trace.WriteLine("So We Return 'True' To Let The Bot Do It's Thing");
                        return true;
                    }
                    else
                    {
                        Trace.WriteLine("But We Dont Have Enough To Make Your 'Requested' Trade!");
                        return false;
                    }
                }
                else
                {
                    Trace.WriteLine("But We Don't Have Enough Quote Currency To Make A Trade!");
                    return false;
                }
            }
            return false;
        }

        private bool CheckForSell(decimal currPrice)
        {
            if (TrailingUp)
            {
                Trace.WriteLine("Trail Price = " + trailedUpPrice);
                Trace.WriteLine("Trailing = " + TrailingUp);
                Trace.WriteLine("Trailed  = " + trailedUp);
                if (currPrice > SellLimitPrice)
                {
                    Trace.WriteLine("Up Trail Reached! : Trailing Up!");
                    trailedUpPrice = decimal.Round(currPrice - (0.005m * currPrice), GetDecimalPlaces(CurrentProduct.QuoteIncrement));
                    SellLimitPrice = decimal.Round(currPrice + (SellTrailAmount * currPrice), GetDecimalPlaces(CurrentProduct.QuoteIncrement));
                    trailedUp = true;
                    return false;
                }
                else if (trailedUp && trailedUpPrice >= currPrice)
                {
                    Trace.WriteLine("Up Loss Trail Triggered! : Selling Now!");
                    PlaceLimitOrder(OrderSide.Sell, currPrice, SellLimitAmount);
                    trailedUp = false;
                    if (!RepeatSell)
                    {
                        TrailingUp = false;
                    }
                    return false;
                }
            }

            if (currPrice >= SellLimitPrice)
            {
                Trace.WriteLine("The Price Is Above Or At Our Sell Limit");
                if (AvailableBase >= CurrentProduct.BaseMinSize)
                {
                    Trace.WriteLine("And We Have Enough To Sell At Min");
                    return true; // If We Can Make A Sell Trade At Our Limit
                }
                else
                {
                    Trace.WriteLine("But We Don't Have Enough Base Currency To Make A Trade");
                    return false;
                }
            }
            return false;
        }

        private bool CheckUpperLimit(decimal currPrice)
        {
            if (currPrice > UpperLimitPrice && UpperLimitAmount >= 0)
            {
                Trace.WriteLine("Upper Limit Hit");
                decimal newamount = UpperLimitAmount - AvailableBase;
                Trace.WriteLine(newamount);
                if (newamount > 0)
                {
                    if (newamount >= CurrentProduct.BaseMinSize)
                    {
                        if (QuoteConvert(UpperLimitPrice) >= newamount)
                        {
                            Trace.WriteLine("Top Buy Stop Loss Reached!");
                            Trace.WriteLine("** SEND BUY TRADE OF " + newamount);
                            return true;
                        }   
                    }
                }
                else if (newamount < 0)
                {
                    decimal d = newamount > 0 ? newamount : -newamount;
                    if (d >= CurrentProduct.BaseMinSize)
                    {
                        if (AvailableBase >= d)
                        {
                            Trace.WriteLine("Top Sell Stop Loss Reached!");
                            Trace.WriteLine("** SEND Sell TRADE OF " + d);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool CheckLowerLimit(decimal currPrice)
        {
            if (currPrice < LowerLimitPrice && LowerLimitAmount >= 0)
            {
                Trace.WriteLine("Lower Limit Hit");
                decimal newamount = LowerLimitAmount - AvailableBase;
                Trace.WriteLine(newamount);
                if (newamount > 0)
                {
                    if (newamount >= CurrentProduct.BaseMinSize)
                    {
                        if (QuoteConvert(LowerLimitPrice) >= newamount)
                        {
                            Trace.WriteLine("Bottom Buy Stop Loss Reached!");
                            Trace.WriteLine("** SEND BUY TRADE OF " + newamount);
                            return true;
                        }
                    }
                }
                else if (newamount < 0)
                {
                    decimal d = newamount > 0 ? newamount : -newamount;
                    if (d >= CurrentProduct.BaseMinSize)
                    {
                        if (AvailableBase >= d)
                        {
                            Trace.WriteLine("Bottom Sell Stop Loss Reached!");
                            Trace.WriteLine("** SEND Sell TRADE OF " + d);
                            return true;
                        }   
                    }
                }
            }
            return false;
        }
        #endregion

        #region Trade Stuff
        private async void PlaceLimitOrder(OrderSide side, decimal size, decimal price)
        {
            if (size < CurrentProduct.BaseMinSize && size > CurrentProduct.BaseMaxSize)
            {
                Trace.WriteLine("Size Is To Small!");
                return;
            }

            if (side == OrderSide.Buy && size * price > AvailableQuote)
            {
                Trace.WriteLine("Insufficent Base Currency Available!");
                return;
            }

            if (side == OrderSide.Sell && size > AvailableBase)
            {
                Trace.WriteLine("Insufficent Quote Currency Available!");
                return;
            }
            // Final Stop Before Trade!!
            var response = await App.client.OrdersService.PlaceLimitOrderAsync(side, CurrentPair, size, price, postOnly: false);
            LogTradeOrder(response);
            Trace.WriteLine("Trade Sent!");
            Trace.WriteLine("ID: "+response.Id);
            Trace.WriteLine("RESPONSE: "+response.ToString());
        }

        private async void LogTradeOrder(OrderResponse order)
        {
            openTrades.Add(order);
            string path = (string)Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/trades-" + order.ProductId;
            string logText = order.Id.ToString() + "," + order.Side.ToString() + "," + order.Size.ToString() + "," + order.Price.ToString() + "," + order.Status.ToString() + "\n";
            await File.AppendAllTextAsync(path, logText);
        }

        private async void ReadTradeOrder(OrderResponse order)
        {
            string path = (string)Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/trades-" + order.ProductId;
            
            var logText = await File.ReadAllLinesAsync(path);
            for (int i = 0; i < logText.Length; i++)
            {
                var trade = logText[i].Split(",");
                if (trade[0] == order.Id.ToString())
                {
                    Trace.WriteLine(trade);
                    if (trade[4] != OrderStatus.Settled.ToString())
                    {
                        var fillcheck = await App.client.OrdersService.GetOrderByIdAsync(order.Id.ToString());
                        if (fillcheck.Settled)
                        {
                            openTrades.Remove(order);
                            filledTrades.Add(order);
                            ;
                            string newpath = (string)Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/filled-trades-" + order.ProductId;
                            string newlogText = order.Side.ToString() + "," + order.Size.ToString() + "," + order.Price.ToString() + "\n";
                            await File.AppendAllTextAsync(path, newlogText);
                        }
                    }
                }
            }
        }
        #endregion
    }
}