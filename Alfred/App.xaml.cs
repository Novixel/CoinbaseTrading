using CoinbasePro;
using CoinbasePro.Network.Authentication;
using CoinbasePro.Services.Accounts.Models;
using CoinbasePro.WebSocket;
using CoinbasePro.WebSocket.Models.Response;
using CoinbasePro.WebSocket.Types;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace Alfred
{
    public partial class App : Application
    {
        public static MainWindow mainWindow;

        public static CoinbaseProClient client;

        public static IWebSocket webSocket;

        public static Dictionary<string, Product> products;

        public static Dictionary<string, Ticker> tickers;

        public static Dictionary<string, Account> accounts;

        public static List<ChannelType> channels;
        public static List<string> productTypes;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            client = _Login();
            if (client != null)
            {
                _init();
                while (accounts == null)
                {
                    //
                }
                mainWindow = new MainWindow();
                mainWindow.ShowDialog();
                if (webSocket != null)
                    webSocket.Stop();
                Current.Shutdown();
            }
        }

        public static void AddProductSub(string productID)
        {
            if (webSocket.State == WebSocket4Net.WebSocketState.Open)
            {
                webSocket.Stop();
                productTypes.Add(productID);
                webSocket.Start(productTypes, channels);
            }
        }

        private void _init()
        {
            productTypes = new List<string>() { "BTC-USD", };
            channels = new List<ChannelType>() { ChannelType.User, ChannelType.Ticker, ChannelType.Status, ChannelType.Level2 };
            webSocket = client.WebSocket;
            webSocket.Start(productTypes, channels);
            webSocket.OnTickerReceived += WebSocket_OnTickerReceived;
            webSocket.OnStatusReceived += WebSocket_OnStatusReceived;
            webSocket.OnSnapShotReceived += WebSocket_OnSnapShotReceived;
        }

        private async void UpdateAccounts()
        {
            var allAccounts = await client.AccountsService.GetAllAccountsAsync();
            if (allAccounts != null)
            {
                if (accounts == null)
                {
                    accounts = new Dictionary<string, Account>();
                    foreach (Account account in allAccounts)
                    {
                        accounts.Add(account.Currency, account);
                    }
                }
                else
                {
                    foreach (Account account in allAccounts)
                    {
                        accounts[account.Currency] = account;
                    }
                }
            }
        }

        private void WebSocket_OnSnapShotReceived(object sender, WebfeedEventArgs<Snapshot> e)
        {
            Trace.WriteLine(e.LastOrder.Bids);
            Trace.WriteLine(e.LastOrder.Asks);
        }

        private void WebSocket_OnStatusReceived(object sender, WebfeedEventArgs<Status> e)
        {
            if (products == null)
            {
                UpdateAccounts();
                products = new Dictionary<string, Product>();
                foreach (Product product in e.LastOrder.Products)
                {
                    products.Add(product.Id, product);
                }
            }
            else
            {
                foreach (Product product in e.LastOrder.Products)
                {
                    products[product.Id] = product;
                }
            }
        }

        private void WebSocket_OnTickerReceived(object sender, WebfeedEventArgs<Ticker> e)
        {
            Ticker ticker = e.LastOrder;
            if (mainWindow != null && mainWindow.CurrentPair != null && mainWindow.CurrentPair == e.LastOrder.ProductId)
            {
                mainWindow.CurrentPrice = e.LastOrder.Price;
            }
            if (tickers == null)
            {
                tickers = new Dictionary<string, Ticker>();
                tickers.Add(e.LastOrder.ProductId, e.LastOrder);
            }
            else
            {
                if (tickers.TryAdd(e.LastOrder.ProductId, e.LastOrder))
                {

                }
                else
                {
                    tickers[e.LastOrder.ProductId] = e.LastOrder;
                }

            }
        }

        CoinbaseProClient _Login()
        {
            // variable init
            Login login = new Login();
            Authenticator auth = null;

            // Open The login menu
            login.ShowDialog();

            // After we close thay window 
            Trace.WriteLine("Log In Attempt");
            CustomUser user = CustomUserData.GetUser(login.txtKey.Text);

            if (user != null)
            {
                auth = new Authenticator(user.Authentication[0], user.Authentication[1], user.Authentication[2]);
                return new CoinbaseProClient(auth);
            }
            else if (user == null && login.txtKey.Text != "" && login.txtSec.Text == "")
            {
                Trace.WriteLine("Loading " + login.txtKey.Text);
                user = CustomUserData.LoadUser(login.txtKey.Text);
                auth = new Authenticator(user.Authentication[0], user.Authentication[1], user.Authentication[2]);
                return new CoinbaseProClient(auth);
            }
            else if (user == null && login.txtKey.Text != "" && login.txtSec.Text == "sand")
            {
                Trace.WriteLine("Sandbox " + login.txtKey.Text);
                user = CustomUserData.LoadUser(login.txtKey.Text);
                auth = new Authenticator(user.Authentication[0], user.Authentication[1], user.Authentication[2]);
                return new CoinbaseProClient(auth, true);
            }
            else if (login.txtKey.Text != "" && login.txtSec.Text != "")
            {
                Trace.WriteLine("Default");
                auth = new Authenticator(login.txtKey.Text, login.txtSec.Text, login.txtPas.Text);
                return new CoinbaseProClient(auth);
            }
            else
            {
                return null;
            }
        }
    }
}
