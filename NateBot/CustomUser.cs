namespace NateBot
{
    class CustomUser // This is user !!!
    {
        public string Name;
        public string[] Authentication;

        public CustomUser(string name, string[] authentication)
        {
            CustomUser tmp = CustomUserData.GetUser(name);
            if (tmp != null)
            {
                Name = tmp.Name;
                Authentication = tmp.Authentication;
                return;
            }
            else
            {
                Name = name;
                Authentication = authentication;
            }
            CustomUserData.SetUser(this);
        }
    }
}
