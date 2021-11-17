using CoinbasePro;
using CoinbasePro.Network.Authentication;
using System.Diagnostics;
using System.Windows;

namespace FawkesTrader
{
    public partial class App : Application
    {
        public static MainWindow mainWindow;

        public static CoinbaseProClient client;

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            client = _Login();

            if (client != null)
            {
                mainWindow = new MainWindow();
                mainWindow.ShowDialog();
                mainWindow.webSocket.Stop();
                Current.Shutdown();
            }
            else
            {
                Current.Shutdown();
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

        public static void message(string message, bool error = false)
        {
            if (!error)
            {
                mainWindow.confirmOut.Text = message;
                mainWindow.errorOut.Text = "";
            }
            else
            {
                mainWindow.confirmOut.Text = "";
                mainWindow.errorOut.Text = message;
            }
        }
    }
}
