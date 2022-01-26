using UnityEngine;

namespace Voodoo.Distribution
{
    public sealed class LocalizationDebugger : MonoBehaviour
    {
        static KeyCode debugKeyForNextLanguage = KeyCode.L;
        static KeyCode debugKeyForLongestStringMode = KeyCode.O;

        string[]    _languageCodes;
        int         _forcedIndex = -1;
        public bool longestStringModeEnabled = false;

        void Awake()
        {
            _languageCodes = Localization.LanguageCodes;

            DontDestroyOnLoad(this);
        }
        
        void Update()
        {
            if (Input.GetKeyDown(debugKeyForNextLanguage))
            {
                longestStringModeEnabled = false;
                _forcedIndex = (_forcedIndex + 1) % _languageCodes.Length;

                Localization.forcedLanguage = _languageCodes[_forcedIndex];
                Localization.TranslateAll();
            }

            if (Input.GetKeyDown(debugKeyForLongestStringMode))
            {
                longestStringModeEnabled = true;
                Localization.TranslateAll();
            }
        }
    }
}
