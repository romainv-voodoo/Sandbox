using UnityEngine;

namespace Voodoo.Store
{
    public static class OptionStyleHelper
    {
        public static float titleHeight = 30;
        private static readonly Texture2D buttonTexture;

        static OptionStyleHelper()
        {
            buttonTexture = new Texture2D(1, 1);
            buttonTexture.SetPixel(1, 1, Color.white);
            buttonTexture.Apply();
        }

        public static readonly GUIStyle titleStyle = new GUIStyle
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = Color.white
            }
        };

        public static readonly GUIStyle buttonStyle = new GUIStyle
        {
            normal = {background = buttonTexture, textColor = Color.white}
        };
    }
}