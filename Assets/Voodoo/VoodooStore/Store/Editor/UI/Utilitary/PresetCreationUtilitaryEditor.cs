using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Voodoo.Store
{
    public class PresetCreationUtilitaryEditor : EditorWindow
    {
        public static PresetCreationUtilitaryEditor window;

        private PackagePreset preset = new PackagePreset{Name = "Name", description = "Description"};

        private Texture[] resourcesTextures;

        public static void Open(List<Package> packages)
        {
            if (window == null)
            {
                window = CreateInstance<PresetCreationUtilitaryEditor>();
                window.minSize = new Vector2(300,100);
            }
            
            window.resourcesTextures = Resources.LoadAll<Texture>("");
            window.preset.AddRange(packages);
            
            window.ShowUtility();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                DrawIcon();
                
                EditorGUILayout.BeginVertical();
                {
                    preset.Name        = EditorGUILayout.TextField(preset.Name);
                    preset.description = EditorGUILayout.TextField(preset.description);
                    preset.Color       = EditorGUILayout.ColorField(preset.Color);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                if (preset.Name == "" || preset.Name == "Name")
                {
                    GUI.enabled = false;
                }
                
                if (GUILayout.Button("Add"))
                {
                    if (preset.description == "Description")
                    {
                        preset.description = "";
                    }
                    
                    VoodooStore.SaveAsPreset(preset);
                    window.Close();
                }
                GUI.enabled = true;

                if (GUILayout.Button("Cancel"))
                {
                    window.Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawIcon()
        {
            Texture currentIcon = resourcesTextures.FirstOrDefault(x => x.name == preset.iconPath);

            //TODO : Find a way to remove showed sprites that aren't under a Resources folder.
            currentIcon = (Texture) EditorGUILayout.ObjectField(currentIcon, typeof(Texture), false, GUILayout.Height(EditorGUIUtility.singleLineHeight * 4), GUILayout.Width(EditorGUIUtility.singleLineHeight * 4));

            GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", "The image must be under a Resources folder"));

            preset.iconPath = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(currentIcon));
            preset.Icon = Resources.Load<Texture>(preset.iconPath);
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}