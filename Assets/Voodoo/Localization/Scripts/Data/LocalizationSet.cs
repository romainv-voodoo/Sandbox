using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voodoo.Distribution
{
    [Serializable]
	public class LocalizationSet : IDisposable
	{
		bool _disposed = false;

		[SerializeField] 
		public bool allNone= true;
		
		[SerializeField]
		public Dictionary<string, LanguageRegistry> languages = new Dictionary<string, LanguageRegistry>();

		public static LocalizationSet FromSheet(LocalizationSheet sheet) 
		{
			var set = new LocalizationSet();

			int langCount = sheet.languageCodes?.Length ?? 0;
			int keyCount = sheet.translationRows?.Length ?? 0;
			for (int langIndex = 0; langIndex < langCount; langIndex++)
            {
				var languageRegistry = new LanguageRegistry();

                for (int tradIndex = 0; tradIndex < keyCount; tradIndex++)
                {
					var row = sheet.translationRows[tradIndex];
					languageRegistry.translations.Add(row.key, row.translations[langIndex]);
                }

				set.languages.Add(sheet.languageCodes[langIndex].ToUpper(), languageRegistry);
            }

			return set;
		}

        public void Dispose()
        {
            if (_disposed)
            {
				return;
            }

			_disposed = true;

            foreach (var language in languages.Values)
            {
				language.Dispose();
            }

			languages.Clear();
			languages = null;

			GC.SuppressFinalize(this);
		}
    }

	[Serializable]
	public class LanguageRegistry : IDisposable
	{
        bool _disposed = false;
		
		[SerializeField]
		public string code;
		
		[SerializeField]
		public bool isComplete;
		
		[SerializeField]
		public bool isIncluded = true;

		[SerializeField]
		public Dictionary<string, string> translations = new Dictionary<string, string>();

        public void Dispose()
        {
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			translations.Clear();
			translations = null;

			GC.SuppressFinalize(this);
		}
	}
}