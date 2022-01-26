using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class CircleCIWindow : EditorWindow
    {
        [MenuItem("Voodoo/VoodooCI/Remote Builds with CircleCI")]
        public static void ShowWindow()
        {
            GetWindow(typeof(CircleCIWindow), false, "Circle CI Remote");
        }

        private static CiSettingsData _voodooCiSettings;

        private Vector2 _scrollPos = Vector2.zero;


        private void Awake()
        {
            BuildPanelBase.RefreshSettings();
        }

        private void OnEnable()
        {
            BuildPanelBase.RefreshSettings();
            AssemblyReloadEvents.afterAssemblyReload += AssemblyReload;
        }

        private void AssemblyReload()
        {
            BuildPanelBase.RefreshSettings();
        }

        private void OnDisable()
        {
            AssemblyReloadEvents.afterAssemblyReload -= AssemblyReload;
        }

        void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
            EditorGUILayout.Space(5);
            GUILayout.Label($"Voodoo CI Settings", EditorStyles.largeLabel);
            EditorGUILayout.Space(5);

            if (CircleCiApiTokenPanel.IsCircleCiApiTokenAvailable() == false)
            {
                CircleCiApiTokenPanel.ShowApiTokenPanel();
            }
            else
            {
                ProjectInformationPanel.ShowPanel();

                SettingsButtonPanel.ShowPanel();

                CommentPanel.ShowPanel();

                AndroidBuildTriggerPanel.ShowPanel();

                IosBuildTriggerPanel.ShowPanel();
            }

            #region Navigation

            EditorGUILayout.Space(20);
            GUILayout.BeginVertical(EditorStyles.helpBox);

            if (GUILayout.Button("Go to Circle CI page", GUILayout.Height(30)))
            {
                BuildPanelBase.GoToCircleCiPage();
            }

            EditorGUILayout.Space(20);
            GUILayout.EndVertical();

            #endregion

            #region Advanded Options Panel

            EditorGUILayout.Space(20);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"Advanced Options (!)", EditorStyles.largeLabel);
            EditorGUILayout.Space(5);
            BuildVersionsPanel.ShowPanel();
            FastlaneConfigPanel.ShowPanel();
            TroubleshootingPanel.ShowPanel();
            PackageManagementPanel.ShowPanel();
            GUILayout.EndVertical();
            EditorGUILayout.Space(20);

            #endregion

            GUILayout.EndScrollView();
        }
    }
}