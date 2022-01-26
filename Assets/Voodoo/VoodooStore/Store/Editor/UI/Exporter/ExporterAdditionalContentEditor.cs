using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Voodoo.Store
{
	public class ExporterAdditionalContentEditor : IEditor
	{
		public void OnEnable() { }

		public void OnDisable() { }

		public void Controls() { }

		public void OnGUI()
		{
			Controls();
			
			DropAreaGUI();
			
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			ShowAdditionalContentArea();
		}
		
		private void DropAreaGUI()
        {
	        Event evt = Event.current;
	        EditorGUILayout.LabelField("Add other folders", ContentHelper.StyleHeader);
	        EditorGUILayout.Space();
		        
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, ContentHelper.UIdragDrop);

            //Stop here if not (drag updated or drag perform) or mouse isn't in drop area
            if (!(evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)|| !drop_area.Contains(evt.mousePosition))
            {
	            return;
            }

            bool draggingAtLeastOneFolder = false;
            foreach (Object obj in DragAndDrop.objectReferences)
            {
	            if (obj is DefaultAsset)
	            {
		            draggingAtLeastOneFolder = true;
		            break;
	            }
            }

            if (draggingAtLeastOneFolder == false)
            {
	            return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type != EventType.DragPerform)
            {
	            return;
            }
            
            DragAndDrop.AcceptDrag();

            OnDragAccepted();
        }

        private void OnDragAccepted()
        {
	        for (var i = 0; i < DragAndDrop.objectReferences.Length; i++)
	        {
		        Object obj = DragAndDrop.objectReferences[i];
		        if (obj is DefaultAsset)
		        {
			        Exporter.data.AddLocalFolderToAdditionalContent(DragAndDrop.paths[i]);
		        }
	        }
	        
	        Event.current.Use();
        }

        private void ShowAdditionalContentArea()
        {
	        DisplayAdditionalContentEntries();
        }

        private void DisplayAdditionalContentEntries()
        {
	        foreach (ExporterAdditionalContent exporterAdditionalContent in Exporter.data.additionalContents)
	        {
		        DisplayAdditionalContentLine(exporterAdditionalContent);
	        }
        }

        private void DisplayAdditionalContentLine(ExporterAdditionalContent exporterAdditionalContent)
        {
	        EditorGUILayout.BeginHorizontal("box");
	        {
		        float buttonSize = 34f;
		        float leftLabelWidth = EditorGUIUtility.currentViewWidth / 2f - buttonSize*1.5f;
		        
		        GUIStyle leftLabelStyle = new GUIStyle(EditorStyles.label)
		        {
			        alignment = TextAnchor.MiddleLeft,
			        normal = {textColor = Color.white}
		        };
		        
		        EditorGUILayout.LabelField(exporterAdditionalContent.additionalContent.Name, leftLabelStyle, GUILayout.Height(buttonSize), GUILayout.Width(leftLabelWidth));

		        if (exporterAdditionalContent.existOnline)
		        {
			        if (exporterAdditionalContent.status == AdditionalContentState.KEEP)
			        {
				        GUI.backgroundColor = Color.blue;
			        }

			        if (GUILayout.Button(ContentHelper.UICloud, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
			        {
				        if (exporterAdditionalContent.status == AdditionalContentState.KEEP)
				        {
					        exporterAdditionalContent.status = AdditionalContentState.REMOVE;
				        }
				        else
				        {
					        exporterAdditionalContent.status = AdditionalContentState.KEEP;
				        }
			        }

			        GUI.backgroundColor = Color.white;
		        }
		        else
		        {
			        GUILayout.Space(buttonSize + 3f);
		        }

		        if (exporterAdditionalContent.existLocal)
		        {
			        if (exporterAdditionalContent.status == AdditionalContentState.UPDATE)
			        {
				        GUI.backgroundColor = Color.green;
			        }

			        if (GUILayout.Button(ContentHelper.UIfolder, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
			        {
				        if (exporterAdditionalContent.status == AdditionalContentState.UPDATE)
				        {
					        exporterAdditionalContent.status = AdditionalContentState.REMOVE;
				        }
				        else
				        {
					        exporterAdditionalContent.status = AdditionalContentState.UPDATE;
				        }
			        }

			        GUI.backgroundColor = Color.white;
		        }
		        else
		        {
			        GUILayout.Space(buttonSize + 3f);
		        }
		        
		        GUIStyle rightLabelStyle = new GUIStyle(EditorStyles.label)
		        {
			        alignment = TextAnchor.MiddleRight,
			        normal = {textColor = Color.white},
			        fontStyle = FontStyle.BoldAndItalic
		        };
		        
		        float rightLabelWidth = EditorGUIUtility.currentViewWidth - leftLabelWidth - buttonSize * 2f - 30f;
		        string message = string.Empty;

		        switch (exporterAdditionalContent.status)
		        {
			        case AdditionalContentState.KEEP:
				        message = "Online data will be used";
				        GUI.contentColor = ContentHelper.VSTDarkBlue;
				        break;
			        case AdditionalContentState.UPDATE:
				        message = "Local data will be used";
				        GUI.contentColor = ContentHelper.VSTGreen;
				        break;
			        case AdditionalContentState.REMOVE:
				        message = exporterAdditionalContent.existOnline ? "Online data will be deleted" : "The data will not be exported";
				        GUI.contentColor = ContentHelper.VSTRed;
				        break;
		        }
		        
		        EditorGUILayout.LabelField(message, rightLabelStyle, GUILayout.Height(buttonSize), GUILayout.Width(rightLabelWidth));

		        GUI.contentColor = Color.white;
	        }
	        EditorGUILayout.EndHorizontal();
        }
	}
}