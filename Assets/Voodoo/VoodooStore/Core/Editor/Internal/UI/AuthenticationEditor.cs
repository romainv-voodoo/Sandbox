using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class AuthenticationEditor : IEditor
    {
        private AuthenticationState state = AuthenticationState.UNSIGNED;
        private string errorMessage = string.Empty;

        public void OnEnable()
        {
            GitHubBridge.OnEnable();
        }

        public void OnDisable()
        {
            UserSerializer.Write();
            GitHubBridge.Dispose();
        }

        public void Controls()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                Event e = Event.current;
                if (state == AuthenticationState.UNSIGNED && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
                {
                    Login();
                }
            }
        }

        public void OnGUI()
        {
            Controls();
            
            ShowBanner();

            if (state == AuthenticationState.UNSIGNED)
            {
                ShowLogin();
            }

            if (string.IsNullOrEmpty(errorMessage) == false)
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
            else if (state == AuthenticationState.SIGNING_IN)
            {
                EditorGUILayout.HelpBox("Signing in, please wait...", MessageType.Info);
            }
        }

        private void ShowBanner()
        {
            GUILayout.Space(10);
            GUILayout.Box(ContentHelper.UIBanner, ContentHelper.StyleBanner);
        }

        private void ShowLogin()
        {
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Token", GUILayout.Width(70));
                User.signInToken = EditorGUILayout.PasswordField(User.signInToken, GUILayout.Width(300));
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Login", GUILayout.Height(40.0f), GUILayout.Width(370.0f)))
                {
                    Login();
                }

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(6f);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                ContentHelper.DisplayHelpButtons(40f, 6f);

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();
            //VoodooStore.autoSignIn = EditorGUILayout.Toggle("Remember me", VoodooStore.autoSignIn);
            //GUILayout.FlexibleSpace();
            //EditorGUILayout.EndHorizontal();
        }

        private void Login()
        {
            // Security
            if (state == AuthenticationState.SIGNING_IN || User.signInToken == "")
            {
                return;
            }

            state = AuthenticationState.SIGNING_IN;

            TryLogin();
        }

        private async void TryLogin()
        {
            errorMessage = "";
            bool authenticationSucceeded = await GitHubUtility.IsUserAuthorized();

            if (authenticationSucceeded)
            {
                User.isGTD = await GitHubBridge.IsUserInTeam(GitHubConstants.GamingEngineeringTeamId);
                VoodooStoreState.Current = State.FETCHING;
            }
            else
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    errorMessage = "You don't seem to have access to internet. Please verify your internet connection before retrying.";
                }
                else
                {
                    errorMessage = "Invalid credentials. Please check your rights toward the store with GTD team.";
                }
            }
            
            state = AuthenticationState.UNSIGNED;
        }
    
        private enum AuthenticationState
        {
            UNSIGNED,
            SIGNING_IN
        }
    }
}