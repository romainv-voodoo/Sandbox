using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Voodoo.Store
{
    public class FiltersEditor : IEditor
    {
        // SearchBar
        public static List<Package> filteredPackages = new List<Package>();
        private FuzzySearchWidgetVST fuzzySearchWidget;

        public static List<string> filteredLabels = new List<string>();

        public static event Action<List<Package>> onApplyFilter;

        public void OnEnable()
        {
            GitHubBridge.fetchCompleted += OnFetchCompleted;
            OnFetchCompleted(true);
            
            onApplyFilter = null;
            fuzzySearchWidget = new FuzzySearchWidgetVST(VoodooStore.labels.Except(filteredLabels).ToArray(), ApplyFilters, SelectLabel, Cancel);
        }

        public void OnDisable()
        {
            GitHubBridge.fetchCompleted -= OnFetchCompleted;
            onApplyFilter = null;
            fuzzySearchWidget = null;
        }

        public void Controls()
        {
        }

        private void OnFetchCompleted(bool success)
        {
            if (success == false)
            {
                return;
            }
            
            VoodooStore.UpdateLabels();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            filteredPackages.Clear();

            for (int i = 0; i < VoodooStore.packages.Count; i++)
            {
                if (IsPackageMatchingFilters(VoodooStore.packages[i]) == false)
                {
                    continue;
                }

                filteredPackages.Add(VoodooStore.packages[i]);
            }

            if (VoodooStore.selection?.Count > 0 && VoodooStore.selection[0] is Package)
            {
                if (filteredPackages.Count == 0)
                {
                    VoodooStore.selection.Clear();
                }
                else
                {
                    List<Package> currentFilteredPackages = new List<Package>();

                    foreach (IDownloadable downloadable in VoodooStore.selection)
                    {
                        if (downloadable is Package pkg && filteredPackages.Contains(pkg))
                        {
                            currentFilteredPackages.Add(pkg);
                        }
                    }

                    VoodooStore.selection.Clear();
                    foreach (Package currentFilteredPackage in currentFilteredPackages)
                    {
                        VoodooStore.selection.Add(currentFilteredPackage);
                    }
                }
            }

            onApplyFilter?.Invoke(filteredPackages);
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            {
                ShowFilters();
                ShowSearchBar();
                ShowLabelsBar();
            }
            GUILayout.EndHorizontal();
        }

        private void ShowFilters()
        {
            EditorGUI.BeginChangeCheck();

            if (VoodooStore.IsFilterActive(Filters.NotInstalled))
            {
                if (GUILayout.Button(ContentHelper.UIvalidate, GUI.skin.FindStyle("toolbarButton"), GUILayout.Width(30)))
                {
                    VoodooStore.filters &= ~(int)Filters.NotInstalled;
                    VoodooStore.filters |= (int)Filters.Installed;
                }
            }
            if (VoodooStore.IsFilterActive(Filters.Installed))
            {
                GUI.contentColor = ContentHelper.VSTGreen;
                if (GUILayout.Button(ContentHelper.UIvalidate, GUI.skin.FindStyle("toolbarButton"), GUILayout.Width(30)))
                {
                    VoodooStore.filters &= ~(int)Filters.Installed;
                    VoodooStore.filters |= (int)Filters.Updatable;
                }
            }
            else if (VoodooStore.IsFilterActive(Filters.Updatable))
            {
                GUI.contentColor = ContentHelper.VSTOrange;
                if (GUILayout.Button(ContentHelper.UIrefresh, GUI.skin.FindStyle("toolbarButton"), GUILayout.Width(30)))
                {
                    VoodooStore.filters &= ~(int)Filters.Updatable;
                }
            }
            else
            {
                GUI.contentColor = Color.white;
                if (GUILayout.Button(ContentHelper.UIdownload, GUI.skin.FindStyle("toolbarButton"), GUILayout.Width(30)))
                {
                    VoodooStore.filters |= (int)Filters.Installed;
                }
            }

            GUI.contentColor = Color.white;

            // Change
            if (EditorGUI.EndChangeCheck())
            {
                ApplyFilters();
            }
        }

        private void ShowSearchBar()
        {
            GUILayout.Space(5);

            Rect rect = GUILayoutUtility.GetRect(new GUIContent(), GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.Width(250f));

            EditorGUI.BeginChangeCheck();
            {
                fuzzySearchWidget.ShowAsSearchBar(rect);
            }
            if (EditorGUI.EndChangeCheck())
            {
                ApplyFilters();
            }
        }

        private void SelectLabel(string selectionName)
        {
            filteredLabels.Add(selectionName);

            fuzzySearchWidget.Setup(VoodooStore.labels.Except(filteredLabels).ToArray());

            Cancel();

            ApplyFilters();
        }
        
        private void Cancel()
        {
            EditorWindow.focusedWindow.Close();
        }
        
        private void ShowLabelsBar()
        {
            for (int i = 0; i < filteredLabels.Count; i++)
            {
                Vector2 size = ContentHelper.StyleLabelsToolBar.CalcSize(new GUIContent(filteredLabels[i]));

                GUI.color = SpecialLabels.GetLabelColor(filteredLabels[i]);

                GUILayout.BeginHorizontal(ContentHelper.StyleLabelsToolBarBox, GUILayout.Width(size.x+30));
                GUILayout.Space(10);

                EditorGUILayout.LabelField(filteredLabels[i], ContentHelper.StyleLabelsToolBar, GUILayout.Width(size.x));
                if (GUILayout.Button(string.Empty, GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    filteredLabels.RemoveAt(i);
                    fuzzySearchWidget.Setup(VoodooStore.labels.Except(filteredLabels).ToArray()); 
                    ApplyFilters();
                }
                GUILayout.EndHorizontal();

                GUI.color = Color.white;
            }
            GUILayout.FlexibleSpace();

            if (filteredLabels.Count>1)
            {
                if (GUILayout.Button(string.Empty, GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    filteredLabels.Clear();
                    fuzzySearchWidget.Setup(VoodooStore.labels.Except(filteredLabels).ToArray()); 
                    ApplyFilters();
                }
            }
        }

        private bool IsPackageMatchingFilters(Package pkg)
        {
            if (fuzzySearchWidget == null)
            {
                return true;
            }

            if (filteredLabels.Count > 0 && filteredLabels.Except(pkg.labels).Any())
            {
                return false;
            }

            if (string.IsNullOrEmpty(fuzzySearchWidget.searchInput) == false && pkg.Name.IndexOf(fuzzySearchWidget.searchInput, StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            if (VoodooStore.IsFilterActive(Filters.Installed) && pkg.VersionStatus != VersionState.UpToDate)
            {
                return false;
            }

            if (VoodooStore.IsFilterActive(Filters.Updatable) && pkg.VersionStatus != VersionState.OutDated)
            {
                return false;
            }
            
            if (VoodooStore.IsFilterActive(Filters.NotInstalled) && pkg.VersionStatus != VersionState.NotPresent)
            {
                return false;
            }

            return true;
        }
    }
}