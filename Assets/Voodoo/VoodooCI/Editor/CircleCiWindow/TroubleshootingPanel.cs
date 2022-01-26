using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class TroubleshootingPanel : BuildPanelBase
    {
        private static bool _showPanel;

        public static void ShowPanel()
        {
            _showPanel = EditorGUILayout.BeginFoldoutHeaderGroup(_showPanel, "Troubleshooting");
            if (_showPanel)
            {
                EditorGUILayout.Space(20);
                GUILayout.BeginVertical(EditorStyles.helpBox);

                if (GUILayout.Button("Set Target Android API Level to 30", GUILayout.Height(30)))
                {
                    PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;
                    Debug.Log($"Setting the Android API Level to 30. Consider commiting the change in git.");
                }

                if (GUILayout.Button("Set API Compatibility level .Net 4x (Android + iOS)", GUILayout.Height(30)))
                {
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_4_6);
                    PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_4_6);
                    Debug.Log(
                        $"Setting API Compatibility level to .Net4x for iOS and Android. Consider commiting the change in git.");
                }

                if (GUILayout.Button("Set Android Target architecture to ARMv7 and ARM64", GUILayout.Height(30)))
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
                    Debug.Log(
                        $"Setting Android Target Architecture to ARMv7 and ARM64. Consider commiting the change in git.");
                }

                GUILayout.EndVertical();
                EditorGUILayout.Space(20);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}