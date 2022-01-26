using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Distribution
{
    public static class FontBaker
    {
        static readonly string FontKey = "MainFontPathKey";

        static readonly string Fonts     = "Fonts";
        static readonly string Main      = "Main";
        static readonly string Fallbacks = "Fallbacks";
        static readonly string Sources   = "Sources~";

        static readonly string FallbacksPath;
        static readonly string BakingSourcesPath;
        static readonly string HiddenSourcesPath;

        public static List<TMP_FontAsset>          fallbackFonts  = new List<TMP_FontAsset>();
        public static List<(TMP_FontAsset, bool)>  candidateFonts = new List<(TMP_FontAsset, bool)>();

        static string _bakingFolder;
        public static string bakingFolder
        {
            get => _bakingFolder;
            set 
            {
                _bakingFolder = value;
                EditorPrefs.SetString(FontKey, _bakingFolder);
                LoadBakingFonts();
            }
        }

        public static bool AreInputsValid => candidateFonts.Exists(font => font.Item2) && LocalizationIO.HasData;
        
        static FontBaker()
        {
            FallbacksPath     = Path.Combine(PathHelper.ToolAbsolutePath("Localization"), Fonts, Fallbacks);
            
            HiddenSourcesPath = Path.Combine(FallbacksPath, Sources);
            BakingSourcesPath = HiddenSourcesPath.Remove(HiddenSourcesPath.Length -1);

            bakingFolder = EditorPrefs.GetString(FontKey, string.Empty);
        }

        public static void LoadBakingFonts()
        {
            candidateFonts.Clear();
            if (Directory.Exists(_bakingFolder) == false)
            {
                return;
            }

            string[] paths = Directory.GetFiles(_bakingFolder);
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].IndexOf(".meta") >= 0)
                {
                    continue;
                }

                string localPath = paths[i].ToLocalPath();

                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(localPath);
                if (font == null)
                {
                    continue;
                }

                candidateFonts.Add((font, true));
            }
        }

        public static void LocalizationToFont()
        {
            if (Directory.Exists(HiddenSourcesPath) == false)
            {
                Debug.LogError($"Folder containing sources could not be found. \nIt should be located here: {HiddenSourcesPath}");
                return;
            }

            if (Directory.Exists(_bakingFolder) == false)
            {
                Debug.LogError("Main font is null.");
                return;
            }

            if (LocalizationIO.HasData == false)
            {
                Debug.LogError("Sheet is null.");
                return;
            }

            if (candidateFonts.Count <= 0)
            {
                Debug.LogError("No baking fonts available or selected.");
                return;
            }

            Directory.Move(HiddenSourcesPath, BakingSourcesPath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar("Baking Font", "Setup fallback fonts.", 0.25f);
            FindFallbackFonts();
            EditorUtility.DisplayProgressBar("Baking Font", "Fill fonts with localization characters.", 0.5f);

            int count = candidateFonts.Count;
            for (int i = 0; i < count; i++)
            {
                if (candidateFonts[i].Item2 == false)
                {
                    continue;
                }

                var font = candidateFonts[i].Item1;

                font.fallbackFontAssetTable.Clear();
                font.fallbackFontAssetTable.AddRange(fallbackFonts);
            }

            FillCharacters();
            CleanUp();
        }

        static void FindFallbackFonts()
        {
            string[] paths = Directory.GetFiles(FallbacksPath);
            for (int i = 0; i < paths.Length; i++)
            {
                if (paths[i].IndexOf(".meta") >= 0)
                {
                    continue;
                }

                string localPath = paths[i].ToLocalPath();

                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(localPath);
                if (font == null)
                {
                    continue;
                }

                for (int j = 1; j < font.atlasTextures.Length; j++)
                {
                    if (font.atlasTextures[j] == null)
                    {
                        continue;
                    }

                    AssetDatabase.RemoveObjectFromAsset(font.atlasTextures[j]);
                    AssetDatabase.ImportAsset(localPath);
                }

                font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                font.isMultiAtlasTexturesEnabled = false;

                font.ClearFontAssetData();

                fallbackFonts.Add(font);
            }
        }

        static void FillCharacters()
        {
            var mainFont = candidateFonts.Find(pair => pair.Item1.name == Main).Item1;
            if (mainFont is null)
            {
                mainFont = candidateFonts[0].Item1;
            }

            var sheet = LocalizationIO.Sheet;

            float counter = 0f, unit = 1f / sheet.translationRows.Length;
            for (int i = 0; i < sheet.languageCodes.Length; i++)
            {
                counter += unit;
                EditorUtility.DisplayProgressBar("Baking Font", $"Adding characters for {sheet.languageCodes[i]}", counter);

                for (int j = 0; j < sheet.translationRows.Length; j++)
                {
                    string value = sheet.translationRows[j].translations[i];
                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    if (mainFont.HasCharacters(value, out _, true, true) == false) 
                    {
                        MultiAtlasSwitch(mainFont, value, sheet.languageCodes[i]);
                    }
                }
            }
        }

        /// <summary>
        /// If more than 80 % of an atlas is filled with characters, allow multiatlas to avoid character loss
        /// </summary>
        static void MultiAtlasSwitch(TMP_FontAsset main, string localization, string language)
        {
            float upsizeThreshold = 0.8f;
            int count = fallbackFonts.Count;
            for (int i = 0; i < count; i++)
            {
                var   font          = fallbackFonts[i];                
                float charSize      = font.atlasPadding + font.faceInfo.pointSize;
                float charMaxCount  = (font.atlasWidth / charSize) * (font.atlasHeight / charSize);

                if (font.characterTable.Count > (charMaxCount * upsizeThreshold))
                {
                    font.isMultiAtlasTexturesEnabled = true;
                }
            }

            if (main.HasCharacters(localization, out _, true, true) == false)
            {
                Debug.LogError($"A character could not be added from dictionary {language}");
            }
        }

        static void CleanUp()
        {
            EditorUtility.ClearProgressBar();

            int count = fallbackFonts.Count;
            for (int i = 0; i < count; i++)
            {
                fallbackFonts[i].atlasPopulationMode = AtlasPopulationMode.Static;
            }

            fallbackFonts.Clear();

            Directory.Move(BakingSourcesPath, HiddenSourcesPath);
            AssetDatabase.Refresh();
        }
    }
}