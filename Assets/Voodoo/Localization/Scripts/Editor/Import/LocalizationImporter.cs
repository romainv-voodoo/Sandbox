using System;
using UnityEngine;

namespace Voodoo.Distribution
{
    public static class LocalizationImporter
    {
        // Unity can't serialize to Json an array of arrays so we need to split this out into two classes
        // so we can serialize using the native Unity libs and not needing extra dependencies. 
        [Serializable]
        public class LocLine
        {
            public string[] w; // this has to match the json request, less letters less spelling mistakes :) 
        }

        [Serializable]
        public class LocImport
        {
            public LocLine[] result;

            public int LangCount
            {
                get { return result[0].w.Length; }
            }

            public int KeyCount
            {
                get { return result.Length; }
            }

            public string LangCode(int i)
            {
                return result[0].w[i].Trim().ToUpper();
            }
        }

        public static void JsonToSheet(string json, string name)
        {
            var locImport = JsonUtility.FromJson<LocImport>(json);

            int langCount = locImport.result[0].w.Length - 1;
            int keyCount = locImport.result.Length - 1;

            var sheet = new LocalizationSheet() 
            { 
                name = name,
                languageCodes = new string[langCount],
                translationRows = new LocalizationRow[keyCount]
            };

            // As the first row is for language codes and the first col is for keys 
            // When accessing LocImport we have to + 1 all index access

            for (int i = 0; i < langCount; i++)
            {
                sheet.languageCodes[i] = locImport.result[0].w[i + 1].ToUpper();
            }

            for (int keyIndex = 0; keyIndex < keyCount; keyIndex++)
            {
                sheet.translationRows[keyIndex] = new LocalizationRow
                {
                    key = locImport.result[keyIndex + 1].w[0],
                    translations = new string[langCount]
                };
                
                for (int langIndex = 0; langIndex < langCount; langIndex++)
                {
                    sheet.translationRows[keyIndex].translations[langIndex] = locImport.result[keyIndex + 1].w[langIndex + 1];
                }
            }

            LocalizationIO.Add(sheet);
        }
    }
}
