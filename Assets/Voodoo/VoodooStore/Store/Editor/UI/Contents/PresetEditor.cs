using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class PresetEditor : AbstractGenericEditor<PackagePreset>
    {
        private DownloadableTreeView<PackageItem> downloadableTreeView;
        private CompositionTreeView compositionTreeView;

        public override void OnGUI(PackagePreset preset)
        {
            UpdatePackageCompositionContent(preset);

            EditorGUILayout.BeginHorizontal();
            {
                Rect labelRect = EditorGUILayout.GetControlRect(false, ContentHelper.StyleTitle.lineHeight, ContentHelper.StyleTitle);
                string newName = ContentHelper.GetEllipsisString(preset.Name, ContentHelper.StyleTitle, labelRect);
                EditorGUI.LabelField(labelRect, newName, ContentHelper.StyleTitle);

                if (preset.Name != "Cart" && GUILayout.Button(ContentHelper.UIAddToCart, GUI.skin.box, GUILayout.Height(50), GUILayout.Width(50)))
                {
                    TryAddingToCart();
                }
                
                //Modify the offset because the default image is too small (32x32)
                GUIStyle iconStyle = new GUIStyle(GUI.skin.box);
                iconStyle.contentOffset = new Vector2(1,7);
                
                GUIContent guiContent = new GUIContent(ContentHelper.UISave, "Create a preset with the current selected packages");
                if ((preset.Name == "Cart" || preset.Name == "Selection") && GUILayout.Button(guiContent, iconStyle, GUILayout.Height(50), GUILayout.Width(50)))
                {
                    PresetCreationUtilitaryEditor.Open(new List<Package>(preset.Content));
                }
                
                if (GUILayout.Button(ContentHelper.UITrash, GUI.skin.box, GUILayout.Height(50), GUILayout.Width(50)))
                {
                    TryToRemoveAllPackages(preset);
                }
            }
            EditorGUILayout.EndHorizontal();

            DrawDescriptionLabel(preset);
            
            DrawDownloadableTreeView();
        }

        private void TryAddingToCart()
        {
            List<Package> packagesToAdd = new List<Package>();
            string message = "You are going to add the following packages to the cart:\n";
            foreach (IDownloadable downloadable in VoodooStore.selection)
            {
                if (downloadable is Package package && VoodooStore.cart.Contains(package) == false)
                {
                    message += "\n" + downloadable.Name;
                    packagesToAdd.Add(package);
                    continue;
                }

                if (downloadable is PackagePreset packagePreset)
                {
                    foreach (Package newPackage in packagePreset)
                    {
                        if (VoodooStore.cart.Contains(newPackage) == false)
                        {
                            message += "\n" + newPackage.Name;
                            packagesToAdd.Add(newPackage);
                        }
                    }
                }
            }

            message += "\n\nAre you sure ?";

            if (packagesToAdd.Count == 0)
            {
                message = "All the packages that you want to add are already in the cart.";
                EditorUtility.DisplayDialog("Information", message, "OK");
                return;
            }
            
            bool confirmation = EditorUtility.DisplayDialog("Warning", message, "Yes", "Cancel");
            
            if (confirmation)
            {
                foreach (Package package in packagesToAdd)
                {
                    VoodooStore.cart.Add(package);
                }

                VoodooStore.selection.Clear();
                VoodooStore.selection.Add(VoodooStore.cart);
            }
        }

        private void TryToRemoveAllPackages(PackagePreset preset)
        {
            bool confirmation;
            string message;
            if (preset.CanBeDeleted)
            {
                message = "You are going to delete the preset \"" + preset.Name + "\"";
            }
            else
            {
                message = "You are going to clear the " + preset.Name;
            }

            message += ". Are you sure ?";
            confirmation = EditorUtility.DisplayDialog("Warning", message, "Yes", "Cancel");

            if (confirmation)
            {
                if (preset.CanBeDeleted)
                {
                    VoodooStore.presets.Remove(preset);
                    VoodooStore.selection.Clear();
                }
                        
                preset.Clear();
            }
        }

        private void DrawDescriptionLabel(PackagePreset preset)
        {
            float totalSpace = 92;
            if (string.IsNullOrEmpty(preset.description) == false)
            {
                Rect labelRect = EditorGUILayout.GetControlRect(false, ContentHelper.StyleNormal.lineHeight*3, ContentHelper.StyleNormal);
                labelRect.position = new Vector2(labelRect.x+4, labelRect.y);
                labelRect.width -= 8;
                // string newName = ContentHelper.GetEllipsisString(preset.description, ContentHelper.StyleNormal, labelRect);
                EditorGUI.LabelField(labelRect, preset.description, ContentHelper.StyleNormal);
                totalSpace -= ContentHelper.StyleNormal.lineHeight * 3;
            }

            GUILayout.Space(totalSpace);
        }

        private void UpdatePackageCompositionContent(PackagePreset preset)
        {
            if (downloadableTreeView == null)
            {
                compositionTreeView = new CompositionTreeView(preset);
                downloadableTreeView    = new DownloadableTreeView<PackageItem>(compositionTreeView, false);
            }
            
            if (compositionTreeView.Name != preset.Name)
            {
                ResetTreeView(preset);
            }
            
            List<UnityEditor.IMGUI.Controls.TreeViewItem> data = compositionTreeView.GetData();
            if (data.Count != preset.Count)
            {
                ResetTreeView(preset);
            }
        }

        private void ResetTreeView(PackagePreset preset)
        {
            compositionTreeView.SetRawContent(preset);
        }
        
        private void DrawDownloadableTreeView()
        {
            downloadableTreeView?.Refresh();
			
            Rect controlRect = EditorGUILayout.GetControlRect(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            downloadableTreeView?.OnGUI(controlRect);
        }
    }
}