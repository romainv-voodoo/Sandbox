using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class FuzzySearchWidgetVST
    {
        public int    hoveredIndex;
        public string searchInput;
       
        struct Candidate 
        {
            public string name;
            public double distance;
        }

        private Candidate[]     candidates;
        private List<Candidate> bestMatch;
        private double          threshold = 0.7d;
        private readonly double weightThreshold = 0.7;
        private readonly int    numChars = 4;
        
        
        private const float elementSize = 15f;

        private const int   elementToDisplay = 10;
        private Vector2     scrollPos;

        private GUIStyle    buttonStyle;
        
        public event Action applyFilters;
        public event Action cancel;
        public event Action<string> selectLabel;

        public FuzzySearchWidgetVST(string[] items, Action applyFilters, Action<string> selectLabel, Action cancel)
        {
            hoveredIndex = -1;
            
            this.applyFilters = applyFilters;
            this.cancel = cancel;
            this.selectLabel = selectLabel;
            
            Setup(items);
            
            SetButtonStyle();
        }

        private void SetButtonStyle()
        {
            Texture2D texture = new Texture2D(1, 1);
            
            buttonStyle = new GUIStyle
            {
                active = {background = texture, textColor = Color.black},
                normal = {background = texture, textColor = Color.white}
            };
        }

        public void Setup(string[] items)
        {
            candidates = new Candidate[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                candidates[i] = new Candidate {name = items[i], distance = 0d };
            }
        }
        
        public void ShowAsSearchBar(Rect rect)
        {
            if (GUI.Button(rect, new GUIContent(searchInput), GUI.skin.FindStyle("ToolbarSeachTextField")))
            {
                ShowAsDropDown(rect);
            }

            if (GUILayout.Button(string.Empty, GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                searchInput = String.Empty;

                GUI.FocusControl(null);
            }
        }

        public void ShowAsDropDown(Rect rect)
        {
            Rect newRect = new Rect(rect.x - 1, rect.y + 1, rect.width + 5, rect.height);
			
            FuzzySearchWindow window = ScriptableObject.CreateInstance<FuzzySearchWindow>();
            window.Widget = this;

            newRect.x += EditorWindow.focusedWindow.position.x;
            newRect.y += EditorWindow.focusedWindow.position.y;

            window.ShowAsDropDown(newRect, new Vector2(newRect.width, 200f));
			
            hoveredIndex = -1;
            UpdateBestMatch();
        }
        
        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                EditorGUI.BeginChangeCheck();
                {
                    GUI.SetNextControlName("SearchInput");

                    searchInput = EditorGUILayout.TextField(searchInput, GUI.skin.FindStyle("ToolbarSeachTextField"));
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateBestMatch();
                    applyFilters?.Invoke();
                }

                if (buttonStyle.active.background == null)
                {
                    SetButtonStyle();
                }
                
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);
                for (int i = 0; i < bestMatch.Count; i++)
                {
                    GUI.backgroundColor = hoveredIndex == i ? GUI.skin.settings.selectionColor : Color.clear;
                   
                    if (GUILayout.Button(bestMatch[i].name, buttonStyle))
                    {
                        SelectLabel(bestMatch[i].name);
                    }
                        
                    if (hoveredIndex == i)
                    {
                        GUI.backgroundColor = Color.clear;
                    }
                }
                EditorGUILayout.EndScrollView();

            }
            EditorGUILayout.EndVertical();
            
            ShortCut();
            
            GUI.FocusControl("SearchInput");
        }

        void ShortCut()
        {
            Event e = Event.current;

            if (e.type != EventType.KeyUp)
            {
                return;
            }
            
            switch (e.keyCode)
            {
                case KeyCode.Escape:
                    cancel?.Invoke();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (bestMatch.Count == 1)
                    {
                        SelectLabel(bestMatch[0].name);
                    }
                    else 
                    {
                        if (bestMatch.Count == 0 || hoveredIndex == -1)
                        {
                            searchInput = string.Empty;
                            UpdateBestMatch();
                            applyFilters?.Invoke();
                            cancel?.Invoke();
                        }
                        else
                        {
                            SelectLabel(bestMatch[hoveredIndex].name);
                        }
                    }
                    break;
                case KeyCode.DownArrow when hoveredIndex + 1 < bestMatch.Count:
                    hoveredIndex++;
                    break;
                case KeyCode.UpArrow when hoveredIndex > 0:
                    hoveredIndex--;
                    break;
            }
            EditorWindow.focusedWindow.Repaint();
        }
        
        void SelectLabel(string label)
        {
            searchInput = String.Empty;
            UpdateBestMatch();
            
            selectLabel?.Invoke(label);
            applyFilters?.Invoke();
        }

        public void UpdateBestMatch()
        {
            if (bestMatch == null)
            {
                bestMatch = new List<Candidate>();
            }
            else
            {
                bestMatch.Clear();
            }
            
            if (string.IsNullOrEmpty(searchInput))
            {
                bestMatch = new List<Candidate>(candidates.ToArray());

                RemoveNotCompatibleLabels();

                bestMatch = bestMatch.OrderBy(o => o.name).ToList();
            }
            else
            {
                for (int i = 0; i < candidates.Length; i++)
                {
                    candidates[i].distance = JaroWinklerProximity(searchInput.ToLower(), candidates[i].name.ToLower());

                    if (candidates[i].distance > threshold)
                    {
                        bestMatch.Add(candidates[i]);
                    }
                }

                RemoveNotCompatibleLabels();

                bestMatch.Sort((a, b) => -a.distance.CompareTo(b.distance));
            }
            
            UpdateWindowSize();
        }
        
        private void RemoveNotCompatibleLabels()
        {
            if (FiltersEditor.filteredLabels.Count < 1)
            {
                return;
            }

            List<string> compatiblesLabels = new List<string>();
            for (int i = 0; i < VoodooStore.packages.Count; i++)
            {
                List<string> labels = VoodooStore.packages[i].labels;

                if (FiltersEditor.filteredLabels.TrueForAll(label => labels.Contains(label)))
                {
                    compatiblesLabels.AddRange(labels);
                }
            }

            compatiblesLabels = compatiblesLabels.Distinct().ToList();
            compatiblesLabels = compatiblesLabels.Except(FiltersEditor.filteredLabels).ToList();

            bestMatch.RemoveAll(candidate => compatiblesLabels.Exists(label => candidate.name == label) == false);
        }

        private void UpdateWindowSize()
        {
            Rect rect = EditorWindow.focusedWindow.position;
            rect.height = 25f + elementSize * Math.Min(bestMatch.Count,elementToDisplay);
            
            EditorWindow.focusedWindow.maxSize = rect.size;
            EditorWindow.focusedWindow.minSize = EditorWindow.focusedWindow.maxSize;
        }

        private double JaroWinklerProximity(string aString1, string aString2)
        {
            int lLen1 = aString1.Length;
            int lLen2 = aString2.Length;
            if (lLen1 == 0)
                return lLen2 == 0 ? 1.0 : 0.0;
            int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);
            // default initialized to false
            bool[] lMatched1 = new bool[lLen1];
            bool[] lMatched2 = new bool[lLen2];
            int lNumCommon = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                int lStart = Math.Max(0, i - lSearchRange);
                int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
                for (int j = lStart; j < lEnd; ++j)
                {
                    if (lMatched2[j]) continue;
                    if (aString1[i] != aString2[j])
                        continue;
                    lMatched1[i] = true;
                    lMatched2[j] = true;
                    ++lNumCommon;
                    break;
                }
            }
            if (lNumCommon == 0) return 0.0;
            int lNumHalfTransposed = 0;
            int k = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                if (!lMatched1[i]) continue;
                while (!lMatched2[k]) ++k;
                if (aString1[i] != aString2[k])
                    ++lNumHalfTransposed;
                ++k;
            }
            // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
            int lNumTransposed = lNumHalfTransposed / 2;
            // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
            double lNumCommonD = lNumCommon;
            double lWeight = (lNumCommonD / lLen1
                             + lNumCommonD / lLen2
                             + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;
            if (lWeight <= weightThreshold) return lWeight;
            int lMax = Math.Min(numChars, Math.Min(aString1.Length, aString2.Length));
            int lPos = 0;
            while (lPos < lMax && aString1[lPos] == aString2[lPos])
                ++lPos;
            if (lPos == 0) return lWeight;
            return lWeight + 0.1 * lPos * (1.0 - lWeight);
        }
        
    }
}