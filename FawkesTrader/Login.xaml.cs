using System.Diagnostics;
using System.Windows;

namespace FawkesTrader
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            txtName.Text = "";
            txtKey.Text = "";
            txtSec.Text = "";
            txtPas.Text = "";
        }

        private void LoginButton_Clicked(object sender, RoutedEventArgs e = null)
        {
            if (txtName.Text != "")
                SaveUserApi();
            Hide();
        }

        private void SaveUserApi()
        {
            Trace.WriteLine("Saving User");
            string[] auths = { txtKey.Text, txtSec.Text, txtPas.Text };
            CustomUser newUser = new CustomUser(txtName.Text, auths);
            CustomUserData.SetUser(newUser);
        }
    }
}
