using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Voodoo.Store
{
    public class FetchingEditor : IEditor
    {
        private FetchingState state = FetchingState.FETCH_READY;

        private bool firstRepaint = true;

        public void OnEnable() {}
        public void OnDisable() {}
        public void Controls() {}

        public void OnGUI()
        {
            Controls();
            ShowBanner();
            
            if (state == FetchingState.FETCH_READY)
            {
                EditorGUILayout.HelpBox("Signed successfully, start fetching", MessageType.Info);
                //This is done to prevent launching fetch in repaint mode.
                //Else the following error will be thrown : ArgumentException: GUILayout: Mismatched LayoutGroup.repaint
                if (firstRepaint)
                {
                    firstRepaint = false;
                    return;
                }
                _ = StartFetching();
            }
            else if (state == FetchingState.FETCHING)
            {
                EditorGUILayout.HelpBox("Fetching package, please wait...", MessageType.Info);
            }
            else if (state == FetchingState.FETCH_ERROR)
            {
                EditorGUILayout.HelpBox("Fetch failed. Please take a look at the documentation, report a bug or contact us on slack", MessageType.Error);
                
                //TODO : find a way to avoid duplicates from AuthenticationEditor
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                
                    ContentHelper.DisplayHelpButtons(40f, 6f);

                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private async Task StartFetching()
        {
            state = FetchingState.FETCHING;
            bool success = await GitHubBridge.FetchMaster();

            if (success)
            {
                if (VoodooStoreWindow.ContainsVST)
                {
                    VoodooStoreState.Current = State.STORE;
                    ResetState();
                }
                else
                {
                    CompilationPipeline.assemblyCompilationFinished += OnCompileEnded;
                }
            }
            else
            {
                state = FetchingState.FETCH_ERROR;
            }
        }

        private void OnCompileEnded(string assembly, CompilerMessage[] messages)
        {
            CompilationPipeline.assemblyCompilationFinished -= OnCompileEnded;
            ResetState();
        }

        private void ResetState()
        {
            firstRepaint = true;
            state = FetchingState.FETCH_READY;
        }

        private void ShowBanner()
        {
            GUILayout.Space(10);
            GUILayout.Box(ContentHelper.UIBanner, ContentHelper.StyleBanner);
        }
    
        private enum FetchingState
        {
            FETCH_READY,
            FETCHING,
            FETCH_ERROR
        }
    }
}