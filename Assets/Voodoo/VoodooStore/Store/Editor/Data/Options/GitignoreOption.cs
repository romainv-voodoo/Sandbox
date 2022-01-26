using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public static class GitignoreOption
    {
        private const string optionName = ".gitignore";
        public static string name => Path.GetFileName(optionName);

        public static bool autoGitignore;
        public static bool gameopsVerification;
        
        static GitignoreOption()
        {
            GitignoreOptionData gitignoreOptionData = OptionSerializer.Read<GitignoreOptionData>(optionName);

            autoGitignore = gitignoreOptionData.autoGitignore;
            gameopsVerification = gitignoreOptionData.gameopsVerification;
        }

        public static void Write()
        {
            GitignoreOptionData gitIgnoreOptionData = new GitignoreOptionData
            {
                autoGitignore = autoGitignore,
                gameopsVerification = gameopsVerification
            };

            OptionSerializer.Write(gitIgnoreOptionData, optionName);
        }
    }

    public struct GitignoreOptionData
    {
        public bool autoGitignore;
        public bool gameopsVerification;
    }
    
    public class GitignoreOptionWidget : IOptionWidget
    {
        public string Name => GitignoreOption.name;

        public void OnEnable()
        {
            VoodooStoreState.OnStateChanged += OnStateChanged;
        }
        
        public void OnDisable()
        {
            VoodooStoreState.OnStateChanged -= OnStateChanged;
            GitignoreOption.Write();
        }

        private void OnStateChanged(State state)
        {
            if (VoodooStoreState.previous != State.AUTHENTICATION)
            {
                return;
            }
            
            CheckTeam();
        }

        private async void CheckTeam()
        {
            bool isInTeam = await GitHubBridge.IsUserInTeams(GitHubConstants.GameOpsTeamID);
            
            if (isInTeam && GitignoreOption.gameopsVerification == false)
            {
                GitignoreOption.autoGitignore = true;
            }

            GitignoreOption.gameopsVerification = isInTeam;
            
            if (GitignoreOption.autoGitignore)
            {
                GitignoreHelper.AddGitignore();
            }
        }

        public void Controls()
        {

        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (GitignoreHelper.HasVSTGitignore())
            {
                if (GUILayout.Button(new GUIContent("Remove VoodooStore from .gitignore", "To include the VoodooStore and its meta into the git sharing process"), GUILayout.Width(225)))
                {
                    GitignoreHelper.RemoveGitignore();
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent("Add VoodooStore to .gitignore", "To ignore the VoodooStore and its meta from being shared by git"), GUILayout.Width(200)))
                {
                    GitignoreHelper.AddGitignore();
                }
            }
            GUILayout.EndHorizontal();

            if (GitignoreOption.gameopsVerification)
            {
                GUILayout.Space(15);
            
                EditorGUILayout.LabelField("For GameOps members:");
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Auto gitignore", "If toggled, VoodooStore will be added to .gitignore on every projects"), GUILayout.Width(150));
                GitignoreOption.autoGitignore = EditorGUILayout.Toggle(GitignoreOption.autoGitignore);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            
        }
    }
}