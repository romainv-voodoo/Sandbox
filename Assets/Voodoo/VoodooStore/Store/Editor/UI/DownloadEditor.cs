using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class DownloadEditor : IEditor
    {
	    private DownloadState state = DownloadState.DOWNLOADING;

        public void OnEnable() {}

        public void Controls() {}
        
        public void OnGUI()
        {
            Controls();
				
            ShowButtonsBar();
            
			if (state == DownloadState.DOWNLOADING)
			{
				EditorGUILayout.HelpBox("Downloading package, please wait...", MessageType.Info);
			}
			else if (state == DownloadState.DOWNLOAD_ERROR)
            {
	            Debug.LogError("There was an error when downloading your package, please retry...");
                VoodooStoreState.Current = State.FETCHING;
            }
		}
        
        private void ShowButtonsBar()
        {
            if (state == DownloadState.DOWNLOAD_ERROR)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button(ContentHelper.UIreturn, GUILayout.Height(25), GUILayout.Width(25)))
                {
	                VoodooStoreState.Current = State.STORE;
					GUI.FocusControl(null);
				}

                GUILayout.Space(10);
            }
        }

        public void OnDisable()
        {
        }
        
        private enum DownloadState
        {
	        DOWNLOADING,
	        DOWNLOAD_ERROR
        }
    }
}
