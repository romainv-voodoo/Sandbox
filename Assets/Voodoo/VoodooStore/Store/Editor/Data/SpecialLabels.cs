using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voodoo.Store
{
    public static class SpecialLabels
    {
        private static readonly Color normalColor = Color.white;

        private static readonly Dictionary<SpecialLabelCategory, Color> ColorByCategory = new Dictionary<SpecialLabelCategory, Color>
        {
            {SpecialLabelCategory.obsolete, new Color(1f, 0.2f, 0)},
            {SpecialLabelCategory.system, new Color(0.98f, 0.5f, 0f)},
            {SpecialLabelCategory.job, new Color(0.06f, 0.65f, 0.94f)}
        };
        
        private static readonly List<Color> orderPriority = new List<Color>()
        {
            ColorByCategory[SpecialLabelCategory.obsolete],
            ColorByCategory[SpecialLabelCategory.system],
            ColorByCategory[SpecialLabelCategory.job],
            normalColor
        };
        
        private static readonly List<string> jobNames = new List<string>()
        {
            "game_tool_dev",
            "game_ops",
            "marketing_dev",
            "casual",
            "hypercasual",
            "gaming"
        };

        private static readonly List<string> systemNames = new List<string>()
        {
            "test",
            "unverified",
            "external",
            "mac_only",
            "win_only"
        };

        private static readonly List<string> obsoleteNames = new List<string>()
        {
            "obsolete",
        };

        private static Dictionary<string, SpecialLabelCategory> specialLabels;

        private static Dictionary<string, SpecialLabelCategory> SpecialLabelDictionary
        {
            get
            {
                if (specialLabels == null)
                {
                    InitDictionary();
                }

                return specialLabels;
            }
        }

        public static bool IsSpecial(string labelName)
        {
            return SpecialLabelDictionary.ContainsKey(labelName);
        }
        
        public static Color GetLabelColor(string labelName)
        {
            return IsSpecial(labelName) ? GetLabelColor(SpecialLabelDictionary[labelName]) : normalColor;
        }

        private static Color GetLabelColor(SpecialLabelCategory specialLabelCategory)
        {
            return ColorByCategory[specialLabelCategory];
        }

        public static List<string> GetAllSpecialLabels()
        {
            return SpecialLabelDictionary.Keys.ToList();
        }

        public static List<(string, Color)> GetSpecialLabelFromList(List<string> originalList)
        {
            List<(string, Color)> result = new List<(string, Color)>();

            for (int i = 0; i < originalList.Count; i++)
            {
                string name = originalList[i];

                result.Add(SpecialLabelDictionary.ContainsKey(name)
                    ? (name, GetLabelColor(SpecialLabelDictionary[name]))
                    : (name, normalColor));
            }
            
            result = result.OrderBy(tuple => orderPriority.IndexOf(tuple.Item2)).ToList();
            
            return result;
        }

        private static void InitDictionary()
        {
            specialLabels = new Dictionary<string, SpecialLabelCategory>();

            for (int i = 0; i < obsoleteNames.Count; i++)
            {
                specialLabels.Add(obsoleteNames[i], SpecialLabelCategory.obsolete);
            }
            
            for (int i = 0; i < systemNames.Count; i++)
            {
                specialLabels.Add(systemNames[i], SpecialLabelCategory.system);
            }
            
            for (int i = 0; i < jobNames.Count; i++)
            {
                specialLabels.Add(jobNames[i], SpecialLabelCategory.job);
            }
        }
    }
}