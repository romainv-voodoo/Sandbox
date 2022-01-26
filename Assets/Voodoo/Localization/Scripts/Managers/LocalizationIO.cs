using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Voodoo.Distribution
{
    public static class LocalizationIO
    {
        static readonly string GameData = "GameData";
        static readonly string Localization = "Localization";
        static readonly string ResourcesDir = "Resources";
        static readonly string File = "LocData";
        static readonly string Extension = ".asset";

        static LocalizationData _data;

        public static bool HasData => Data != null && Data.sheetVersions.Count > 0;

        public static LocalizationData Data
        {
            get
            {
                if (_data != null)
                {
                    return _data;
                }

                Initialize();
                return _data;
            }
        }

        public static LocalizationSheet Sheet => Data.SelectedSheet;
        
        public static event Action sheetChanged;

        static LocalizationIO()
        {
            Initialize();
        }

        static void Initialize()
        {
            string dataFolder = Path.Combine(PathHelper.ToolAbsolutePath(GameData), Localization, ResourcesDir);
            string dataLocalPath = Path.Combine(dataFolder.ToLocalPath(), File + Extension);

            _data = Resources.Load<LocalizationData>(File);

            if (_data == null)
            {
                _data = ScriptableObject.CreateInstance<LocalizationData>();

                if (Directory.Exists(dataFolder) == false)
                {
                    Directory.CreateDirectory(dataFolder);
                }
#if UNITY_EDITOR
                AssetDatabase.CreateAsset(_data, dataLocalPath);
#endif
            }

            Application.quitting += Dispose;
        }

        public static void Dispose()
        {
            _data = null;

            Application.quitting -= Dispose;
        }

        public static void Add(LocalizationSheet sheet)
        {


            int index = SheetIndexOf(sheet.name);

            if (index >= 0)
            {
                Data.sheetVersions[index] = sheet;
            }
            else
            {
                index = Data.sheetVersions.Count;
                Data.sheetVersions.Add(sheet);
            }

            SelectSheet(index);
#if UNITY_EDITOR
            EditorUtility.SetDirty(Data);
            AssetDatabase.Refresh();
#endif
        }

        public static int SheetIndexOf(string name)
        {
            if (HasData == false)
            {
                return -1;
            }

            int count = Data.sheetVersions.Count;
            for (int i = 0; i < count; i++)
            {
                if (Data.sheetVersions[i].name == name)
                {
                    return i;
                }
            }

            return -1;
        }

        public static void SelectSheet(int index) 
        {
            if (index < 0 || index >= Data.sheetVersions.Count)
            {
                return;
            }

            Data.selectedSheetIndex = index;
            sheetChanged?.Invoke();
        }

        public static void SetupSheet(ref string[] langCodes, ref string[] translationKeys, out LocalizationSet set) 
        {
            if (HasData == false)
            {
                langCodes = null;
                translationKeys = null;
                set = null;
                return;
            }

            langCodes = Sheet.languageCodes;
            
            translationKeys = new string[Sheet.translationRows.Length];
            for (int i = 0; i < translationKeys.Length; i++)
            {
                translationKeys[i] = Sheet.translationRows[i].key;
            }

            set = LocalizationSet.FromSheet(Sheet);
        }

        public static string[] sheetNames() 
        {
            if (Data.sheetVersions.Count <= 0)
            {
                return null;
            }

            string[] sheetNames = new string[Data.sheetVersions.Count];
            for (int i = 0; i < sheetNames.Length; i++)
            {
                sheetNames[i] = Data.sheetVersions[i].name;
            }

            return sheetNames;
        }
    }
}
