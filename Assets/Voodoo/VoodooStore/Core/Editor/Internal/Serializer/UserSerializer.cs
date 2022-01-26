using System.IO;
using UnityEngine;

namespace Voodoo.Store
{
    public static class UserSerializer
    {
        private const string UserFileName = "User-Info.json";

        public static void Read()
        {
            //TODO offer the possibility to redefine the path were info are save
            string infoPath = Path.Combine(PathHelper.DirectoryPath, UserFileName);

            if (File.Exists(infoPath) == false)
            {
                return;
            }

            string text;
            using (StreamReader reader = File.OpenText(infoPath))
            {
                text = reader.ReadToEnd();
                reader.Close();
            }

            UserData data = JsonUtility.FromJson<UserData>(text);
            if (data != null)
            {
                UserDataToUser(data);
            }
        }

        private static void UserDataToUser(UserData data)
        {
            User.signInToken = data.signInToken;
            User.isGTD = data.isGTD;
        }

        public static void Write()
        {
            UserData data = UserToUserData();

            string text     = JsonUtility.ToJson(data, true);
            string infoPath = Path.Combine(PathHelper.DirectoryPath, UserFileName);

            FileInfo fileInfo = new FileInfo(infoPath);
            fileInfo.Directory?.Create();
            File.WriteAllText(infoPath, text);
        }

        private static UserData UserToUserData() 
        {
            return new UserData
            {
                signInToken = User.signInToken,
                isGTD = User.isGTD,
            };
        }
    }
}