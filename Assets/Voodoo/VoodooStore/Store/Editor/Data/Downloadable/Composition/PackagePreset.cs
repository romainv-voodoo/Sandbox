using UnityEngine;

namespace Voodoo.Store
{
    [System.Serializable]
    public class PackagePreset : PackageComposition
    {
        public static readonly PackagePreset cart = new PackagePreset { Name = "Cart", iconPath = "cart"};
        public static readonly PackagePreset favorites = new PackagePreset { Name = "Favorites", iconPath = "star", colorText = "FFFF00" };
        public string   description;
        public string   iconPath;
        public string   colorText;

        [System.NonSerialized]
        private bool isRefreshed = false;
        [System.NonSerialized]
        private Texture  icon;
        [System.NonSerialized]
        private Color    color;

        public Texture Icon
        {
            get 
            {
                if (isRefreshed == false)
                {
                    Refresh();
                }
                return icon;
            }

            set
            {
                icon = value;
                
                if (icon != null)
                    iconPath = icon.name;
            }
        }

        public Color Color
        {
            get
            {
                if (isRefreshed == false)
                {
                    Refresh();
                }

                return color;
            }

            set
            {
                color = value;
                
                if (color != default)
                    colorText = ColorUtility.ToHtmlStringRGB(color);
            }
        }

        public void Refresh()
        {
            icon = Resources.Load<Texture>(iconPath);
            ColorUtility.TryParseHtmlString("#" + colorText, out color);
            isRefreshed = true;
        }
    }
}