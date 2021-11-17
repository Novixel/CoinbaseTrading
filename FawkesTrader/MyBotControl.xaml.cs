using CoinbasePro.Services.Accounts.Models;
using CoinbasePro.Services.Orders.Models.Responses;
using CoinbasePro.Services.Orders.Types;
using CoinbasePro.WebSocket.Models.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace FawkesTrader
{
    public partial class MyBotControl : UserControl, INotifyPropertyChanged
    {
        public Product myProduct;

        public Dictionary<Product, List<Tuple<OrderSide, decimal, decimal>>> myTrades;

        bool Watching = true;

        decimal productPrice;
        decimal startPrice;
        decimal startPriceC;
        bool startPriceSwitch;

        decimal targetSellProfit;
        decimal targetBuyProfit;
        decimal targetProfit;

        bool autoSwitch;

        decimal availableBase;
        decimal availableQuote;

        decimal baseMin;
        decimal quoteMin;

        bool ableToBuy;
        bool ableToSell;

        decimal buyAmount;
        decimal sellAmount;

        decimal sellLimitPrice;
        decimal buyLimitPrice;

        bool buyLimitSwitch;
        bool sellLimitSwitch;

        int cyclesLimit;

        string productText = "Start";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public decimal SellLimitPrice
        {
            get
            {
                return sellLimitPrice;
            }
            set
            {
                if (sellLimitPrice != value)
                {
                    sellLimitPrice = value;
                    if (!TargetSellProfitBox.IsFocused)
                    {
                        decimal targetincrease = sellLimitPrice - StartPrice;
                        decimal targetinpercentage = targetincrease / StartPrice * 100;
                        TargetSellProfitBox.Text = (decimal.Round(targetinpercentage, 3)).ToString();
                    }
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
                    buyLimitPrice = value;
                    if (!TargetBuyProfitBox.IsFocused)
                    {
                        decimal targetdecrease = StartPrice - buyLimitPrice;
                        decimal targetdepercentage = targetdecrease / StartPrice * 100;
                        TargetBuyProfitBox.Text = (decimal.Round(targetdepercentage, 3)).ToString();
                    }
                    OnPropertyChanged();
                }
            }
        }
        public bool BuyLimitSwitch
        {
            get => buyLimitSwitch;
            set
            {
                if (buyLimitSwitch != value)
                {
                    buyLimitSwitch = value;

                    OnPropertyChanged();
                }
            }
        }
        public bool SellLimitSwitch
        {
            get => sellLimitSwitch;
            set
            {
                sellLimitSwitch = value;
                OnPropertyChanged();
            }
        }
        public int CyclesLimit
        {
            get => cyclesLimit; set
            {
                cyclesLimit = value;
                OnPropertyChanged();
            }
        }
        public string ProductText
        {
            get => productText;
            set
            {
                productText = value;
                OnPropertyChanged();
            }
        }
        public bool AbleToBuy
        {
            get => ableToBuy; set
            {
                ableToBuy = value;
                OnPropertyChanged();
            }
        }
        public bool AbleToSell
        {
            get => ableToSell; set
            {
                ableToSell = value;
                OnPropertyChanged();
            }
        }
        public decimal BuyAmount
        {
            get => buyAmount; set
            {
                buyAmount = value;
                OnPropertyChanged();
            }
        }
        public decimal SellAmount
        {
            get => sellAmount; set
            {
                sellAmount = value;
                OnPropertyChanged();
            }
        }

        public decimal ProductPrice
        {
            get => productPrice;
            set
            {
                productPrice = value;
                OnPropertyChanged();
            }
        }

        public decimal StartPrice
        {
            get => startPrice;
            set
            {
                startPrice = value;
                OnPropertyChanged();
            }
        }

        public decimal StartPriceC
        {
            get => startPriceC;
            set
            {
                startPriceC = value;
                OnPropertyChanged();
            }
        }

        public decimal TargetProfit
        {
            get => targetProfit;
            set
            {
                targetProfit = value;
                OnPropertyChanged();
            }
        }
        public decimal TargetSellProfit
        {
            get => targetSellProfit;
            set
            {
                targetSellProfit = value;
                if (TargetSellProfitBox.Text == targetSellProfit.ToString() && !TargetSellProfitBox.IsFocused)
                    SellLimitPrice = decimal.Round(StartPrice + ((targetSellProfit / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                OnPropertyChanged();
            }
        }

        public decimal TargetBuyProfit
        {
            get => targetBuyProfit;
            set
            {
                targetBuyProfit = value;
                if (TargetBuyProfitBox.Text == targetBuyProfit.ToString() && !TargetBuyProfitBox.IsFocused)
                    BuyLimitPrice = decimal.Round(StartPrice - ((targetBuyProfit / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                OnPropertyChanged();
            }
        }
        public bool AutoSwitch
        {
            get => autoSwitch; set
            {
                autoSwitch = value;
                OnPropertyChanged();
            }
        }

        public bool StartPriceSwitch
        {
            get => startPriceSwitch; set
            {
                startPriceSwitch = value;
                OnPropertyChanged();
            }
        }

        public MyBotControl(Product _product)
        {
            InitializeComponent();
            DataContext = this;
            myTrades = new Dictionary<Product, List<Tuple<OrderSide, decimal, decimal>>>();
            myProduct = _product;
            ProductText = myProduct.DisplayName;

            init();
        }
        public void init()
        {

            BuyAmount = myProduct.BaseMinSize;
            SellAmount = myProduct.BaseMinSize;
            TargetProfit = 1.2m;
            TargetBuyProfit = TargetProfit / 2;
            TargetSellProfit = TargetProfit / 2;
        }

        public async void StartWatch()
        {
            // run a method in another thread
            Trace.WriteLine("Watch Start: " + myProduct.DisplayName);
            await AStartWatch();
            Trace.WriteLine("Watch End");
        }

        public void StopWatch()
        {
            // run a method in another thread
            Watching = false;
            Trace.WriteLine("Watch End");
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

        public async Task AStartWatch()
        {
            // Get Account Data Before We Start The loop
            GatherAccountData();

            decimal SellTotal = 0;
            decimal BuyTotal = 0;

            // Last Trade Price to Keep Track Of Profits
            decimal lastTradePrice;
            decimal lastTradeAmount;
            decimal lastTradeTotal;
            OrderSide lastTradeSide;

            // Grab The Price For Reference
            StartPrice = GetPrice();

            decimal currentPrice = StartPrice;
            decimal changeInPrice;

            while (Watching)
            {
                BuyTotal = decimal.Round(BuyAmount * BuyLimitPrice, GetDecimalPlaces(myProduct.QuoteIncrement));
                SellTotal = decimal.Round(SellAmount * SellLimitPrice, GetDecimalPlaces(myProduct.QuoteIncrement));

                BuyLimitLabel.Text = BuyTotal.ToString() + " " + myProduct.QuoteCurrency;
                SellLimitLabel.Text = SellTotal.ToString() + " " + myProduct.QuoteCurrency;

                if (StartPriceSwitch)
                    StarPriceLabel.Visibility = System.Windows.Visibility.Collapsed;
                else if (!StartPriceSwitch)
                    StarPriceLabel.Visibility = System.Windows.Visibility.Visible;

                if (AutoSwitch)
                {
                    BuyLimitPrice = decimal.Round(StartPrice - (((TargetProfit / 2) / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                    SellLimitPrice = decimal.Round(StartPrice + (((TargetProfit / 2) / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                }

                if (AbleToBuy && !AutoSwitch)
                {
                    BuyLimitPrice = decimal.Round(StartPrice - (((TargetBuyProfit) / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                }

                if (AbleToSell && !AutoSwitch)
                {
                    SellLimitPrice = decimal.Round(StartPrice + (((TargetSellProfit) / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                }


                // Get The Price
                currentPrice = GetPrice();

                if (currentPrice > StartPrice)
                {
                    // price goes up
                    decimal increase = currentPrice - StartPrice;
                    decimal percentage = increase / StartPrice * 100;
                    message("+" + (increase / StartPrice).ToString("P2"));
                    changeInPrice = percentage;

                    if (changeInPrice >= 2 * TargetSellProfit && availableBase > myProduct.BaseMinSize)
                    {
                        decimal tmp = decimal.Round(availableBase, GetDecimalPlaces(myProduct.BaseIncrement));
                        if (tmp >= SellAmount)
                        {
                            SellLimitSwitch = true;
                        }

                    }

                    if (AbleToSell)
                    {
                        if (currentPrice >= SellLimitPrice && SellLimitSwitch)
                        {
                            decimal tradeAmount = decimal.Round(SellAmount);
                            if (GetDecimalPlaces(myProduct.BaseIncrement) >= GetDecimalPlaces(SellAmount))
                            {
                                if (SellAmount >= myProduct.BaseMinSize)
                                {
                                    if (availableBase >= myProduct.BaseMinSize)
                                    {
                                        SendTrade(currentPrice, SellAmount, OrderSide.Sell);
                                        SellLimitSwitch = false;
                                        if (AutoSwitch)
                                        {
                                            StartPrice = currentPrice;
                                            BuyAmount = SellAmount;
                                            BuyLimitPrice = decimal.Round(StartPrice - (((TargetBuyProfit) / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                                            AbleToBuy = true;
                                            BuyLimitSwitch = true;
                                            lastTradePrice = currentPrice;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        message("insufficient funds... " + availableBase + myProduct.BaseCurrency, true);
                                        Trace.WriteLine("insufficient funds... " + availableBase + myProduct.BaseCurrency);
                                    }
                                }
                                else
                                {
                                    message("size is too small. Minimum size is " + myProduct.BaseMinSize, true);
                                    Trace.WriteLine("size is too small. Minimum size is " + myProduct.BaseMinSize);
                                    SellLimitSwitch = false;
                                }
                            }
                            else
                            {
                                message("Sell Amount Is To Accurate!, Minimun: " + myProduct.BaseIncrement.ToString(), true);
                                Trace.WriteLine("Sell Amount Is To Accurate!, Minimun: " + myProduct.BaseIncrement.ToString());
                                SellLimitSwitch = false;
                            }
                        }
                    }

                }
                else if (currentPrice < StartPrice)
                {
                    // price goes down
                    decimal decrease = StartPrice - currentPrice;
                    decimal percentage = decrease / StartPrice * 100;
                    message("-" + (decrease / StartPrice).ToString("P2"), true);
                    changeInPrice = percentage;

                    if (changeInPrice >= 2 * TargetBuyProfit && availableQuote > myProduct.MinMarketFunds)
                    {
                        decimal tmp = decimal.Round(availableQuote * currentPrice, GetDecimalPlaces(myProduct.QuoteIncrement));
                        if (tmp >= BuyAmount)
                        {
                            BuyLimitSwitch = true;
                        }

                    }

                    if (AbleToBuy)
                    {
                        if (currentPrice < BuyLimitPrice && BuyLimitSwitch)
                        {
                            if (GetDecimalPlaces(myProduct.BaseIncrement) >= GetDecimalPlaces(SellAmount))
                            {
                                if (BuyAmount >= myProduct.BaseMinSize)
                                {
                                    if (availableQuote * (1 / currentPrice) >= myProduct.BaseMinSize)
                                    {
                                        SendTrade(currentPrice, BuyAmount, OrderSide.Buy);
                                        BuyLimitSwitch = false;
                                        if (AutoSwitch)
                                        {
                                            SellAmount = BuyAmount;
                                            SellLimitPrice = decimal.Round(StartPrice + (((TargetSellProfit) / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                                            lastTradePrice = currentPrice;
                                            AbleToSell = true;
                                            SellLimitSwitch = true;
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        message("insufficient funds... " + availableQuote + myProduct.QuoteCurrency, true);
                                        BuyLimitSwitch = false;
                                    }
                                }
                                else
                                {
                                    message("size is too small. Minimum size is " + myProduct.BaseMinSize, true);
                                    BuyLimitSwitch = false;
                                }
                            }
                            else
                            {
                                message("Buy Amount Is To Accurate!, Minimun: " + myProduct.BaseIncrement.ToString(), true);
                                BuyLimitSwitch = false;
                            }

                        }
                    }
                }
                else
                {
                    // Price Hasn't Moved
                    changeInPrice = 0;
                    message("0.00%");
                }
                message("Potential Profit: " + decimal.Round((SellTotal - BuyTotal - (0.0050m * (SellTotal - BuyTotal))), 8).ToString() + " " + myProduct.QuoteCurrency.ToString(), true, true);
                CyclesLimit++;
                //if (CyclesLimit % 300 == 0)
                //startPrice = currentPrice;

                await Task.Delay(500);
            }
        }

        public async void GatherAccountData()
        {
            var response = await App.client.AccountsService.GetAllAccountsAsync();
            foreach (Account item in response)
            {
                if (item.Currency == myProduct.BaseCurrency)
                {
                    availableBase = item.Available;

                    baseMin = myProduct.BaseIncrement;
                }
                else if (item.Currency == myProduct.QuoteCurrency)
                {
                    availableQuote = item.Available;

                    quoteMin = myProduct.QuoteIncrement;
                }
            }
        }

        public decimal GetPrice()
        {
            ProductPrice = MainWindow.botPrices[myProduct.Id];
            return ProductPrice;
        }

        public bool CheckDown(decimal price, decimal currentPrice)
        {
            if (currentPrice <= price)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CheckUp(decimal price, decimal currentPrice)
        {
            if (currentPrice >= price)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async void SendTrade(decimal price, decimal size, OrderSide side)
        {
            Trace.WriteLine("\n\nTrade Attempt: " + size + " " + price + " " + side + " " + myProduct.Id + " Trade Sent!\n\n");

            OrderResponse response = await App.client.OrdersService.PlaceLimitOrderAsync(side, myProduct.Id, size, price, GoodTillTime.Day, false);

            if (response != null)
            {
                Tuple<OrderSide, decimal, decimal> t1 = new Tuple<OrderSide, decimal, decimal>(response.Side, response.Size, response.Price);
                if (File.Exists("Trades.txt"))
                {
                    using StreamWriter file = new("Trades.txt", append: true);
                    await file.WriteLineAsync(myProduct.Id + "," + t1.ToString());
                }
                else
                {
                    await File.WriteAllTextAsync("Trades.txt", myProduct.Id + "," + t1.ToString());
                }

                if (myTrades.ContainsKey(myProduct))
                {
                    myTrades[myProduct].Add(t1);
                    for (int i = 0; i < myTrades[myProduct].Count; i++)
                    {
                        Trace.WriteLine(myTrades[myProduct][i]);
                    }
                }
                else
                {
                    myTrades.Add(myProduct, new List<Tuple<OrderSide, decimal, decimal>>());
                    myTrades[myProduct].Add(t1);
                }
                if (response.Side == OrderSide.Buy && response.Settled)
                {
                    StartPrice = response.Price;
                    SellLimitPrice = decimal.Round(StartPrice + (((TargetSellProfit) / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                    BuyLimitPrice = decimal.Round(StartPrice - (((TargetBuyProfit) / 100) * StartPrice), GetDecimalPlaces(myProduct.QuoteIncrement));
                }
                GatherAccountData();
            }

        }
        public void message(string message, bool error = false, bool other = false)
        {
            if (!error)
            {
                confirmOut.Text = message;
                errorOut.Text = "";
            }
            else if (other)
            {
                otherOut.Text = message;
            }
            else
            {
                confirmOut.Text = "";
                errorOut.Text = message;
            }
        }

        private void StarPriceBut_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!StartPriceSwitch)
                SetStartPrice();
            else
                StartPrice = StartPriceC;
        }

        private void SetStartPrice()
        {
            StartPrice = GetPrice();
            StarPriceLabel.Text = StartPrice.ToString();
        }


        private void Box_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender != null && e.Key == Key.Enter)
            {

                TextBox sender1 = (TextBox)sender;
                AutoToggle.Focus();
                sender1.Focus();
            }

        }
    }
}
