using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class VoodooStoreEditor : IEditor
    {
        private static readonly int ContentId  = "Content".GetHashCode();

        private Vector2         listScrollPosition;
        private FiltersEditor   filtersEditor;

        private DownloadableTreeView<PackageItem> downloadablePackageTreeView;
        private DownloadableTreeView<PackagePresetItem> downloadableCompositionTreeView;

        public void OnEnable()
        {
            AddFactories();
            
            VoodooStoreSerializer.Read();
            
            filtersEditor = new FiltersEditor();
            filtersEditor.OnEnable();
            FiltersEditor.onApplyFilter += OnApplyFilter;

            AddTreeView();
        }

        private void OnApplyFilter(List<Package> obj)
        {
            VoodooStoreState.RepaintWindow();
        }

        private void AddFactories() 
        {
            BaseFactory contentFactory = new BaseFactory
            {
                context = ContentId,
                factors = new List<IEditorTarget> 
                {
                    new StoreContentEditor(),
                    new PackageEditor(),
                    new PresetEditor()
                }
            };

            EditorRetailer.AddFactory(contentFactory);
        }

        private void AddTreeView()
        {
            downloadablePackageTreeView = new DownloadableTreeView<PackageItem>(new PackageTreeView(VoodooStore.packages));
            
            downloadableCompositionTreeView = new DownloadableTreeView<PackagePresetItem>(new PackagePresetTreeView(new List<PackagePreset>()));
            downloadableCompositionTreeView.viewContent.Add(VoodooStore.favorites);
            downloadableCompositionTreeView.viewContent.Add(VoodooStore.cart);
            foreach (PackagePreset packagePreset in VoodooStore.presets)
            {
                downloadableCompositionTreeView.viewContent.Add(packagePreset);
            }
        }

        public void Controls()
        {
            filtersEditor.Controls();
        }
        
        public void OnGUI()
        {
            Controls();
				
            ShowButtonsBar();

            ShowStore();
		}

        public void OnDisable()
        {
            VoodooStoreSerializer.Write();

            FiltersEditor.onApplyFilter -= OnApplyFilter;
            filtersEditor.OnDisable();
            filtersEditor = null;
            
            EditorRetailer.Clear();

            VoodooStore.Dispose();
        }

        private void ShowButtonsBar()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(ContentHelper.UIplus, GUILayout.Height(25), GUILayout.Width(25)))
                {
                    VoodooStoreState.Current = State.EXPORTER;
                }

                if (GUILayout.Button(ContentHelper.UIrefresh, GUILayout.Height(25), GUILayout.Width(25)))
                {
                    VoodooStoreState.Current = State.FETCHING;
                }
                
                if (GUILayout.Button(ContentHelper.UIGear, GUILayout.Height(25), GUILayout.Width(25)))
                {
                    VoodooStoreState.Current = State.OPTIONS;
                }

                GUILayout.FlexibleSpace();

                GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    wordWrap = true,
                    alignment = TextAnchor.LowerRight,
                    fontSize = 20,
                };

                EditorGUILayout.LabelField($"V{VoodooStore.Info.localVersion}", labelStyle);

                if (VoodooStore.Info.VersionStatus != VersionState.UpToDate) 
                {
                    GUI.contentColor = ContentHelper.VSTOrange;
                    if (GUILayout.Button(ContentHelper.UIrefresh, GUILayout.Height(25), GUILayout.Width(25)))
                    {
                        AssetDatabase.DeleteAsset(PathHelper.RelativeStorePath);
                    }
                    GUI.contentColor = Color.white;
                }

                GUILayout.Space(10);

                ContentHelper.DisplayHelpButtons(25f);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        #region ShowStore
       
        private void ShowStore()
        {
            filtersEditor.OnGUI();

            EditorGUILayout.BeginHorizontal();
            { 
                ShowScrollableSelection();
                GUI.backgroundColor = Color.white;
                ShowContent();
            }
            
            if (Event.current?.type != EventType.Used)
            {
                EditorGUILayout.EndHorizontal();
            }
		}

        private void ShowScrollableSelection() 
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            
            listScrollPosition = GUILayout.BeginScrollView(listScrollPosition, false, false, GUILayout.ExpandHeight(true));
            {
                ShowPresetsList();
        
                EditorGUILayout.Space();

                ShowPackageList();
            }
            GUILayout.EndScrollView();
            
            if (Event.current?.type != EventType.Used)
            {
                EditorGUILayout.EndVertical();
            }
        }

        private void ShowPresetsList()
        {
            DrawCompositionTreeView();
        }

        private void ShowPackageList()
        {
            DrawPackageTreeView();
        }

        private void DrawCompositionTreeView()
        {
            int compositionNumber = 2 + VoodooStore.presets.Count;
            if (compositionNumber != downloadableCompositionTreeView.viewContent.GetData().Count)
            {
                List<IDownloadable> compositions = new List<IDownloadable> {VoodooStore.favorites, VoodooStore.cart};
                compositions.AddRange(VoodooStore.presets);
                downloadableCompositionTreeView.viewContent.UpdateContent(compositions);
            }
            
            downloadableCompositionTreeView?.Refresh();
			
            Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.MaxHeight(141));
            downloadableCompositionTreeView?.OnGUI(controlRect);
        }

        private void DrawPackageTreeView()
        {
            downloadablePackageTreeView?.Refresh();
			
            Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            downloadablePackageTreeView?.OnGUI(controlRect);
        }

        private void ShowContent()
        {
            EditorGUILayout.BeginVertical();
            IList<IDownloadable> target = VoodooStore.selection;
            if (target == null || target.Count == 0)
            {
                EditorRetailer.OnGUI(ContentId, this);
            }
            else
            {
                if (target[0] is PackagePreset preset)
                {
                    EditorRetailer.OnGUI(ContentId, preset);
                }
                else if (target[0] is Package)
                {
                    List<Package> packages = target.Cast<Package>().ToList();
                    EditorRetailer.OnGUI(ContentId, packages);
                }
            }
            
            if (Event.current?.type != EventType.Used)
            {
                EditorGUILayout.EndVertical();
            }
        }

        #endregion
    }
}
