using System;
using UnityEngine;

namespace Voodoo.Distribution
{
    public static class Localization
    {
        static string           _languageCode = "EN";
        static LocalizationDebugger _debugger;
        
        static LocalizationSet  _set;
        static string[] _languageCodes;
        static string[] _translationKeys;

        public static string   forcedLanguage;

        public static event Action languageChanged;

        public static LocalizationSet Set 
        {
            get 
            {
                if (_set != null)
                {
                    return _set;
                }

                Mount();
                return _set;
            }
        }

        public static string[] LanguageCodes
        {
            get
            {
                if (_languageCodes != null)
                {
                    return _languageCodes;
                }

                Mount();
                return _languageCodes;
            }
        }

        public static string[] TranslationKeys
        {
            get
            {
                if (_translationKeys != null)
                {
                    return _translationKeys;
                }

                Mount();
                return _translationKeys;
            }
        }

        public static bool IsValid => Set != null;
        
        public static string LanguageCode => forcedLanguage ?? _languageCode;

        public static bool IsRightToLeftLanguage => LanguageCode == "HE";

        static Localization()
        {
            LocalizationIO.sheetChanged += Mount;
            Initialize();
        }

        static void Initialize() 
        {
            Mount();

            if (_set == null)
            {
                Debug.LogError("No valid Localization dataset could be load.");
                return;
            }

            _languageCode = GetLanguageCode();

            if (Application.isPlaying == false)
            {
                return;
            }

            TranslateAll();

            if (Debug.isDebugBuild)
            {
                _debugger = new GameObject().AddComponent<LocalizationDebugger>();
            }

            Application.quitting += Dispose;
        }

        public static void SetLanguage(string langCode)
        {
            _languageCode = langCode;
            TranslateAll();
        }

        public static void Dispose()
        {
            _set?.Dispose();
            _languageCodes = null;
            _translationKeys = null;

            Application.quitting -= Dispose;
        }

        private static void Mount() => LocalizationIO.SetupSheet(ref _languageCodes, ref _translationKeys, out _set);

        // TODO default language should be optionable rather than automatic english
        static string GetLanguageCode()
        {
            string languageCode = SystemLanguageToLanguageCode();

            if (Set.languages.ContainsKey(languageCode))
            {
                return languageCode;
            }

            Debug.LogError($"{languageCode} is not defined in the localization registry. Switching to English as default.");

            return "EN";
        }

        static string SystemLanguageToLanguageCode()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Arabic:
                    return "AR";
                    
                case SystemLanguage.Chinese:
                    return "ZH-CN";
                    
                case SystemLanguage.French:
                    return "FR";
                    
                case SystemLanguage.German:
                    return "DE";
                    
                case SystemLanguage.Indonesian:
                    return "ID";
                    
                case SystemLanguage.Italian:
                    return "IT";
                    
                case SystemLanguage.Japanese:
                    return "JA";

                case SystemLanguage.Korean:
                    return "KO";

                case SystemLanguage.Hebrew:
                    return "HE";

                case SystemLanguage.Portuguese:
                    return "PT";

                case SystemLanguage.Russian:
                    return "RU";

                case SystemLanguage.Spanish:
                    return "ES";

                case SystemLanguage.Thai:
                    return "TH";

                case SystemLanguage.ChineseSimplified:
                    return "ZH-CN";

                case SystemLanguage.ChineseTraditional:
                    return "ZH-TW";

                default:
                    return "EN";
            }
        }

        /// <summary>
        /// Return the value for the _key in the _language (EN if not found)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="_language"></param>
        /// <returns></returns>
        public static string GetTranslation(string key, string languageCode = null, bool logError = true)
        {
            if (KeyExist(key) == false)
            {
                Debug.LogError("Key: " + "<color=cyan>" + key + "</color>" + " does exist.");
                return string.Empty;
            }
            
            if (_debugger != null && _debugger.longestStringModeEnabled)
            {
                return GetLongestString(key);
            }

            string code = languageCode ?? LanguageCode;
            
            return Set.languages[code].translations[key];
        }

        public static bool KeyExist(string key) 
        {
            for (int i = 0; i < TranslationKeys.Length; i++)
            {
                if (TranslationKeys[i] == key)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetLongestString(string key)
        {
            int longestCount = 0;
            string retPhrase = "";
            foreach (var language in Set.languages.Values)
            {
                var phrase = language.translations[key];
                int len = GetRoughLength(phrase);
                if (len > longestCount)
                {
                    longestCount = len;
                    retPhrase = phrase;
                }
            }

            return retPhrase;
        }

        // TODO Make something more accurate
        // Is very approximative, plus it only concern occidental language
        private static int GetRoughLength(string word)
        {
            int len = 0;
            foreach (var c in word)
            {
                if (c == 'l' || c == 'i' || c == 'I')
                {
                    len += 1;
                }
                else if (c == 'M' || c == 'm' || c == 'W' || c == 'w')
                {
                    len += 4;
                }
                else
                {
                    len += 2;
                }
            }

            return len;
        }

        public static void TranslateAll()
        {
            var translators = GameObject.FindObjectsOfType<Translator>();
            for (int i = 0; i < translators.Length; i++)
            {
                translators[i].Translate();
            }

            languageChanged?.Invoke();
        }
    }
}