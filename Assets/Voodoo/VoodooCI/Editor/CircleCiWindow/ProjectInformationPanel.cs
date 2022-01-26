using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class ProjectInformationPanel : BuildPanelBase
    {
        public static void ShowPanel()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.Space(20);
            GUILayout.Label($"Git Project Name: {CircleProjectName}", EditorStyles.largeLabel);
            GUILayout.Label($"Git Branch: {Branch}", EditorStyles.largeLabel);
            GUILayout.Label($"Artifact Name: {BuildPipelineBase.ProductName}", EditorStyles.largeLabel);
            GUILayout.Label($"Your personal API Token: {CircleCiApiTokenPanel.CircleApiToken}",
                EditorStyles.largeLabel);

            EditorGUILayout.Space(10);
            GUILayout.EndVertical();
        }
    }
}