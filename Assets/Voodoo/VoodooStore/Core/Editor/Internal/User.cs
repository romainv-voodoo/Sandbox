namespace Voodoo.Store
{ 
    public static class User
    {
        public static string signInToken;
        public static bool isGTD;

        static User() 
        {
            UserSerializer.Read();
        }
    }
}