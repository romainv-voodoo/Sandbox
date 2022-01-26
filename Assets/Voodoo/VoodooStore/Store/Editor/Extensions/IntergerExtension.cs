namespace Voodoo.Store
{ 
    public static class IntergerExtension 
    {
        public static string ToOctetsSize(this int value)
        {
            if (value > 1000000)
            {
                value = value / 1000000;

                return value.ToString() + "Mo";

            }
            else if (value > 100000)
            {
                value = value / 100000;

                return  "0." + value.ToString() + "Mo";

            }
            else if (value > 1000)
            {
                value = value / 1000;

                return value.ToString() + "ko";

            }
            else
            {
                return value.ToString() + "octets";
            }
        }
    }
}