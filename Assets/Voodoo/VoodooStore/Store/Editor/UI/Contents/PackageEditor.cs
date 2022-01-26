using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class PackageEditor : AbstractGenericEditor<List<Package>>
    {
        private static readonly int ContentId = "Content".GetHashCode();

        private Package package;
        
        public override void OnGUI(List<Package> packages)
        {
            if (VoodooStore.selection.Count > 1)
            {
                PackagePreset selection = new PackagePreset {Name = "Selection"};
                foreach (IDownloadable downloadable in VoodooStore.selection)
                {
                    selection.Add(downloadable as Package);
                }

                EditorRetailer.OnGUI(ContentId, selection);
            }
            else
            {
                package = packages[0];

                // Prevent KeyUp error when Drawing the editor
                if (Event.current.isKey)
                {
                    return;
                }

                ShowHeader();
                
                ShowDependencies();
                
                ShowLabels();

                ShowNameAndHelp();

                ShowVersions();
                
                ShowAuthorAndSize();
                
                ShowDescription();
            }
        }

        private void ShowHeader()
        {
            EditorGUILayout.BeginHorizontal();
            {
                Rect labelRect = EditorGUILayout.GetControlRect(false, ContentHelper.StyleTitle.lineHeight, ContentHelper.StyleTitle);
                string newName = ContentHelper.GetEllipsisString(package.displayName, ContentHelper.StyleTitle, labelRect);
                EditorGUI.LabelField(labelRect, newName, ContentHelper.StyleTitle);

                EditorGUIHelper.ShowPackagesButtons(package);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
        }

        private void ShowDependencies()
        {
            if (package.dependencies?.Count > 0)
            {
                if (Event.current.isKey)
                {
                    return;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Box(ContentHelper.UIDependency, GUIStyle.none, GUILayout.Width(30), GUILayout.Height(30));
                    EditorGUILayout.LabelField(package.dependencies[0], ContentHelper.StyleSubTitle);
                }
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginVertical();
                for (int i = 1; i < package.dependencies.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Box(GUIContent.none, GUIStyle.none, GUILayout.Width(30), GUILayout.Height(30));
                        EditorGUILayout.LabelField(package.dependencies[i], ContentHelper.StyleSubTitle);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            GUILayout.Space(10);
        }

        private void ShowNameAndHelp()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (Event.current.isKey)
                {
                    EditorGUILayout.EndHorizontal();
                    return;
                }

                EditorGUILayout.LabelField(package.Name, ContentHelper.StyleSubTitle);
                GUILayout.FlexibleSpace();

                GUI.contentColor = string.IsNullOrEmpty(package.documentationLink) ? Color.gray : ContentHelper.VSTBlue;
                
                if (GUILayout.Button(ContentHelper.UIQuestionMark, GUILayout.Height(25), GUILayout.Width(25)))
                {
                    Application.OpenURL(package.documentationLink);
                }

                GUI.contentColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        private void ShowVersions()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("v" + package.version, ContentHelper.StyleNormal);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(package.unityVersion, ContentHelper.StyleSubTitle);
                GUILayout.Space(10);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
        }

        private void ShowAuthorAndSize()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Author: " + package.author, ContentHelper.StyleNormal);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Size: " + package.size.ToOctetsSize(), ContentHelper.StyleSubTitle);
                GUILayout.Space(10);
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);
        }
        
        private void ShowDescription()
        {
            EditorGUILayout.LabelField("Description", ContentHelper.StyleNormal);
            GUILayout.Space(5);

            EditorGUILayout.LabelField(package.description, ContentHelper.StyleNormal);

            GUILayout.Space(ContentHelper.StyleNormal.fontSize);
        }

        private void ShowLabels()
        {
            if (package.labels?.Count > 0 == false)
            {
                return;
            }
            
            List<(string,Color)> allLabels = SpecialLabels.GetSpecialLabelFromList(package.labels);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < allLabels.Count; i++)
            {
                ShowLabel(allLabels[i].Item1, allLabels[i].Item2);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        private void ShowLabel(string label, Color color)
        {
            GUIContent content = new GUIContent(label);
                    
            var size = ContentHelper.StyleLabels.CalcSize(new GUIContent(content));
                    
            GUI.color = color;
            GUILayout.Box(content, ContentHelper.StyleLabels, GUILayout.Width(size.x+ 14), GUILayout.Height(size.y+2));
            GUI.color = Color.white;
        }
    }
}