using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class CommentPanel : BuildPanelBase
    {
        private static Vector2 _commentScrollPos = Vector2.zero;

        public static void ShowPanel()
        {
            #region Parameters

            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(20);
            GUILayout.Label($"Enter Comment:", EditorStyles.largeLabel);
            EditorGUILayout.Space(10);
            _commentScrollPos = EditorGUILayout.BeginScrollView(_commentScrollPos, false, false, GUILayout.Height(100));
            Comment = EditorGUILayout.TextArea(Comment, GUILayout.Height(100));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            GUILayout.EndVertical();

            #endregion
        }
    }
}