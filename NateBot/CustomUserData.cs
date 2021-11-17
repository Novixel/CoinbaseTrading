using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;

namespace NateBot
{
    static class CustomUserData
    {
        public static List<CustomUser> users = new List<CustomUser>();

        public static CustomUser GetUser(string _name)
        {
            foreach (CustomUser user in users)
            {
                if (user.Name == _name)
                {
                    return user;
                }
            }
            return null;
        }

        public static void SetUser(CustomUser user)
        {
            IsolatedStorageFileStream userDataFile = new IsolatedStorageFileStream(user.Name.Trim() + "-UserData.dat", FileMode.Create);
            StreamWriter writeStream = new StreamWriter(userDataFile);
            writeStream.WriteLine(user.Authentication[0]);
            writeStream.WriteLine(user.Authentication[1]);
            writeStream.WriteLine(user.Authentication[2]);
            writeStream.Flush();
            writeStream.Close();
            userDataFile.Close();
        }

        public static CustomUser LoadUser(string _name)
        {
            string[] auth = new string[3];
            IsolatedStorageFileStream userDataFile = new IsolatedStorageFileStream(_name + "-UserData.dat", FileMode.Open);
            StreamReader readStream = new StreamReader(userDataFile);
            auth[0] = readStream.ReadLine().Trim();
            auth[1] = readStream.ReadLine().Trim();
            auth[2] = readStream.ReadLine().Trim();
            readStream.Close();
            userDataFile.Close();
            return new CustomUser(_name, auth);
        }
    }
}
