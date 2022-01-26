using UnityEditor;
using UnityEngine;

namespace Voodoo.CI
{
    public class CircleCiApiTokenPanel : BuildPanelBase
    {
        private static string _circleCiApiToken = "";
        private const string CircleCiApiTokenKey = "CircleCiApiToken";

        private const string CircleCiApiDocLink =
            "https://www.notion.so/voodoo/How-to-setup-and-use-CircleCI-5065fe5bc9c3431d9dee2b1a55b5f1f0#c19d8914e5574550bba8864e13120403";

        public static string CircleApiToken => _circleCiApiToken;

        public static void Initialize()
        {
            _circleCiApiToken = EditorPrefs.GetString(CircleCiApiTokenKey);
            Debug.Log($"CircleCI API Token: {_circleCiApiToken}");
        }

        public static void ShowApiTokenPanel()
        {
            EditorGUILayout.Space(20);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label($"Circle CI API Token is not created!. " +
                            $"\nFollow the link below and create a \"Personal API Token\" and paste in the Text box." +
                            $"\nPaste the created token.", EditorStyles.largeLabel);
            EditorGUILayout.Space(10);


            EditorGUILayout.Space(10);
            if (GUILayout.Button("Step1: Go to Project's Circle CI Page. Use your GitHub credentials to login.",
                GUILayout.Height(20)))
            {
                GoToCircleCiPage();
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button(
                "Step 2: Create a \"Personal API Token\" from CircleCI. Follow the instructions and copy the token.",
                GUILayout.Height(20)))
            {
                Application.OpenURL(CircleCiApiDocLink);
            }

            EditorGUILayout.Space(10);

            GUILayout.Label($"Step 3: Paste the Personal API Token in the following field..");
            EditorGUILayout.Space(10);
            _circleCiApiToken = EditorGUILayout.TextField("API Token : ", _circleCiApiToken);

            EditorGUILayout.Space(10);

            if (string.IsNullOrWhiteSpace(_circleCiApiToken))
            {
                EditorGUILayout.HelpBox("Circle CI API Token can't be empty!", MessageType.Warning);
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Save", GUILayout.Height(20)))
            {
                if (string.IsNullOrWhiteSpace(_circleCiApiToken) == false)
                {
                    EditorPrefs.SetString(CircleCiApiTokenKey, _circleCiApiToken);
                }
            }

            GUILayout.EndVertical();
        }

        public static bool IsCircleCiApiTokenAvailable()
        {
            if (EditorPrefs.HasKey(CircleCiApiTokenKey) == false)
                return false;

            var apiToken = EditorPrefs.GetString(CircleCiApiTokenKey);
            return !string.IsNullOrWhiteSpace(apiToken);
        }

        public static void ResetKey()
        {
            if (EditorUtility.DisplayDialog(
                "Are you sure?",
                $"Do you want to remove your current Circle CI Personal API Token?"
                , "Yes",
                "Cancel"))
            {
                EditorPrefs.SetString(CircleCiApiTokenKey, string.Empty);
                _circleCiApiToken = string.Empty;
            }
        }
    }
}