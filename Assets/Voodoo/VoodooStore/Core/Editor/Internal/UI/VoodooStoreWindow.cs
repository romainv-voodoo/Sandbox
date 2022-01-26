using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class VoodooStoreWindow : EditorWindow
    {
        private const string EditorStateKey = "editorState";

        private static VoodooStoreWindow window;
        
        private Dictionary<State, IEditor> editors;

        public static bool ContainsVST => window.editors.ContainsKey(State.STORE);

        [MenuItem("Voodoo/Voodoo Store %#v", false, -50)]
        static void Init()
        {
            if (window != null)
            {
                window.Close();
            }

            window = GetWindow<VoodooStoreWindow>(false, "Voodoo Store");
            window.Show();
        }

        private void OnEnable()
        {
            if (window == null)
            {
                window = GetWindow<VoodooStoreWindow>("Voodoo Store", false);
            }
            
            AddEditors();

            foreach (KeyValuePair<State,IEditor> kvp in editors)
            {
                kvp.Value.OnEnable();
            }
            
            int lastState = PlayerPrefs.GetInt(EditorStateKey, -1);
            if (lastState >= 0)
            {
                VoodooStoreState.Current = (State)lastState;
            }

            VoodooStoreState.OnStateChanged                          += OnStateChanged;
            VoodooStoreState.AskForRepaint                           += ForceRepaint;
            GitHubBridge.fetchCompleted                              += OnFetchCompleted;
            GitHubDownload.downloadStatusChanged                     += OnDownloadStatusChanged;
            GitHubRepositoryManagement.createRepositoryStatusChanged += OnCreateRepositoryStatusChanged;
            GitHubExport.pushStatusChanged                           += OnPushStatusChanged;
        }

        private void ForceRepaint()
        {
            Repaint();
        }

        private void OnStateChanged(State state)
        {
            Repaint();
        }

        private void OnCreateRepositoryStatusChanged(CreateRepoState state)
        {
            EditorUtility.DisplayProgressBar("Creating github repository", state.ToFriendlyString(),(int)state/(float)Enum.GetValues(typeof(CreateRepoState)).Length);

            if (state == CreateRepoState.CREATE_STOPPED || state == CreateRepoState.CREATE_SUCCESSFULL)
            {
                EditorUtility.ClearProgressBar();
            }
            
            Repaint();
        }

        private void OnPushStatusChanged(PushState state)
        {
            EditorUtility.DisplayProgressBar("Pushing package", state.ToFriendlyString(),(int)state/(float)Enum.GetValues(typeof(PushState)).Length);
            
            if (state == PushState.PUSH_STOPPED || state == PushState.PUSH_SUCCESSFULL)
            {
                EditorUtility.ClearProgressBar();
            }
            
            Repaint();
        }
        
        private void OnDownloadStatusChanged(DownloadState state)
        {
            EditorUtility.DisplayProgressBar("Downloading package", state.ToFriendlyString(),(int)state/(float)Enum.GetValues(typeof(DownloadState)).Length);

            if (state == DownloadState.DOWNLOAD_STOPPED || state == DownloadState.DOWNLOAD_FINISHED)
            {
                EditorUtility.ClearProgressBar();
            }
            
            Repaint();
        }

        private void OnFetchCompleted(bool success)
        {
            Repaint();
        }

        private void AddEditors()
        {
            editors = new Dictionary<State, IEditor>
            {
                {State.AUTHENTICATION, new AuthenticationEditor()},
                {State.FETCHING, new FetchingEditor()}
#if VOODOO_STORE_EDITOR
                ,{State.STORE, new VoodooStoreEditor()},
                {State.EXPORTER, new ExporterEditor()},
                {State.OPTIONS, new OptionsEditor()}
                //{State.DOWNLOAD, new DownloadEditor()}
#endif            
            };
        }

        private void OnGUI()
        {
            if (editors.ContainsKey(VoodooStoreState.Current) && editors[VoodooStoreState.Current] != null)
            {
                editors[VoodooStoreState.Current].OnGUI();
            }
            else
            {
                VoodooStoreState.Current = State.FETCHING;
            }
		}

        private void OnDisable()
        {
            PlayerPrefs.SetInt(EditorStateKey, (int)VoodooStoreState.Current);

            foreach (KeyValuePair<State,IEditor> kvp in editors)
            {
                kvp.Value.OnDisable();
            }

            editors = null;
            
            VoodooStoreState.OnStateChanged                          -= OnStateChanged;
            VoodooStoreState.AskForRepaint                           -= ForceRepaint;
            GitHubBridge.fetchCompleted                              -= OnFetchCompleted;
            GitHubDownload.downloadStatusChanged                     -= OnDownloadStatusChanged;
            GitHubRepositoryManagement.createRepositoryStatusChanged -= OnCreateRepositoryStatusChanged;
            GitHubExport.pushStatusChanged                           -= OnPushStatusChanged;
        }

        private void OnDestroy()
        {
            VoodooStoreState.Current = State.AUTHENTICATION;
            PlayerPrefs.SetInt(EditorStateKey, -1);
        }
    }
}
