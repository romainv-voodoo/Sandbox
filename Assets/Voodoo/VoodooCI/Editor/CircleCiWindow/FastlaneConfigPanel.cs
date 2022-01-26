using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class FastlaneConfigPanel : BuildPanelBase
    {
        private static bool _showPanel;

        public static void Initialize()
        {
            IosFastlaneModificationPanel.UpdateFiles();
            AndroidFastlaneModificationPanel.UpdateFiles();
        }

        public static void ShowPanel()
        {
            _showPanel = EditorGUILayout.BeginFoldoutHeaderGroup(_showPanel, "Fastlane Configuration");

            if (_showPanel)
            {
                EditorGUILayout.Space(20);
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(5);
                GUILayout.Label($"Fastlane Config", EditorStyles.largeLabel);
                GUILayout.Label($"Instruction\n\n" +
                                $"> Use this panel to Modify Fastlane AppFile for Firebase App Distribution for Android." +
                                $"\n" +
                                $"> And AppFile and Match File in iOS Test Flight.", EditorStyles.whiteLargeLabel);
                EditorGUILayout.Space(10);

                IosFastlaneModificationPanel.ShowPanel();
                AndroidFastlaneModificationPanel.ShowPanel();

                GUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}