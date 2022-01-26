using UnityEngine;
using System;

namespace Voodoo.Distribution
{
    [Serializable]
    public class LocalizationOptions : ScriptableObject
    {
        public string sheetUrl; // for reference only
        public string getRequestUrl;
        public bool bakefallbackFonts    = true;
        public bool closeAfterConversion = false;
    }
}
