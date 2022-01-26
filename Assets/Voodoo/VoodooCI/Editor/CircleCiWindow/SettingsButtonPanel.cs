using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class SettingsButtonPanel : BuildPanelBase
    {
        public static void ShowPanel()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(10);
            GUILayout.Label($"Refresh Settings after switching branch");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Settings", GUILayout.Height(50)))
            {
                RefreshSettings();
            }

            if (GUILayout.Button("Open CI Settings", GUILayout.Height(50)))
            {
                CiSettingsWindow.Initialize();
            }

            if (GUILayout.Button("Provide New CircleCI API Key", GUILayout.Height(50)))
            {
                CircleCiApiTokenPanel.ResetKey();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            EditorGUILayout.Space(10);
        }
    }
}