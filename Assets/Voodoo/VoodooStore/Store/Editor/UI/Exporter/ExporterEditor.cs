using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Voodoo.Store
{
	public class ExporterEditor : IEditor
	{
		private Texture2D              dragDropZoneTexture;
		private int                    packageToExportIndex;
		private bool                   displayPackageInfo;
		private bool                   displayPackageValidation;
		private ExporterHelpBoxArea    exporterHelpBoxArea;
		private Vector2                validationScrollView;
		private int                    toolbarInt;
		private string[]               toolbarStrings = {"Information", "Dependencies", "Labels", "Additional Content"};
		private string                 multiPackageText;

		//Allow catching the uppercase of a PascalCase writing
		private const string           regexDisplayName = @"((?<=[^A-Z])\B[A-Z]+(?=[A-Z][^A-Z]+)|\B[A-Z](?=[^A-Z]))";
		
		private ExporterState state = ExporterState.EXPORTER_READY;
		
		private List<IEditor> tabEditors;

		private IEditor CurrentEditor => tabEditors[toolbarInt];

		public void OnEnable()
		{
			VoodooStoreState.OnStateChanged += OnStateChanged;

			tabEditors = new List<IEditor>
			{
				new ExporterInfoEditor(),
				new ExporterDependencyEditor(),
				new ExporterLabelEditor(),
				new ExporterAdditionalContentEditor()
			};

			foreach (IEditor editor in tabEditors)
			{
				editor.OnEnable();
			}
		}

		public void OnDisable()
		{
			ExporterSerializer.Write();
			VoodooStoreState.OnStateChanged -= OnStateChanged;
			
			foreach (IEditor editor in tabEditors)
			{
				editor.OnDisable();
			}
			
			tabEditors.Clear();
		}

		public void Controls()
		{
			foreach (IEditor editor in tabEditors)
			{
				editor.Controls();
			}
		}
		
		public void OnGUI()
		{
			Controls();
			ShowButtonsBar();

			if (VoodooStoreState.Current != State.EXPORTER)
			{
				return;
			}

			GUI.enabled = state == ExporterState.EXPORTER_READY;
            Show();
            GUI.enabled = true;
        }

		private void OnStateChanged(State state)
		{
			if (state != State.EXPORTER)
			{
				return;
			}

			if (VoodooStoreState.previous == State.STORE)
			{
				Exporter.data = new ExporterData();
			}
			else
			{
				ExporterSerializer.Read();
			}
				
			ExporterValidationEditor.Validate();
		}
		
		private void ShowButtonsBar()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(ContentHelper.UIreturn, GUILayout.Height(25), GUILayout.Width(25)))
            {
	            Reset();
				VoodooStoreState.Current = State.STORE;
            }

            GUILayout.FlexibleSpace();
            
            ContentHelper.DisplayHelpButtons(25f);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void Show()
        {
	        GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
	        EditorGUILayout.LabelField("PACKAGE EXPORTER");
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            DropAreaGUI();
            
	        multiPackageText = Exporter.data.CalculateMultiPackageNameChoice();
            
            if (!string.IsNullOrEmpty(multiPackageText))
            {
	            EditorGUILayout.LabelField(multiPackageText);
            }
            else
            {
	            GUILayout.Space(3);
            }

            ShowPackageName();
            
	        EditorGUILayout.Space();
		    toolbarInt = GUILayout.Toolbar(toolbarInt, toolbarStrings);
		    EditorGUILayout.Space();
		    
		    EditorGUI.BeginChangeCheck();
		    {
			    EditorGUILayout.BeginVertical("box");
			    {
				    CurrentEditor?.OnGUI();
			    }
			    EditorGUILayout.EndVertical();
		    }
		    
		    bool needValidation = EditorGUI.EndChangeCheck();

		    if (CurrentEditor != null && CurrentEditor.GetType() == typeof(ExporterInfoEditor))
		    {
			    ExporterValidationEditor.ShowValidation();
			    EditorGUILayout.Space();

			    EditorGUILayout.LabelField("Patch note", EditorStyles.boldLabel);
			    Exporter.data.commitMessage = EditorGUILayout.TextArea(Exporter.data.commitMessage, GUILayout.MinHeight(54));
		    }

		    if (needValidation)
		    {
			    ExporterValidationEditor.Validate();
		    }
		    
		    ShowExportButton();
        }

        private void DropAreaGUI()
        {
	        Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 100.0f, GUILayout.ExpandWidth(true));
            var TextStyle = new GUIStyle();
            TextStyle.normal.textColor = Color.white;

            if (Exporter.data.elementsToExport == null || Exporter.data.elementsToExport.Count == 0)
            {
	            dragDropZoneTexture = ContentHelper.UIdragDrop;
            }
            else if (dragDropZoneTexture == null)
            {
	            dragDropZoneTexture = ContentHelper.UIfolder;
            }

            GUI.Box(drop_area, dragDropZoneTexture);

            //Stop here if not (drag updated or drag perform) or mouse isn't in drop area
            if (!(evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)|| !drop_area.Contains(evt.mousePosition))
            {
	            return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

            if (evt.type != EventType.DragPerform) 
	            return;
            
            DragAndDrop.AcceptDrag();

            OnDragAccepted();
        }

        private void OnDragAccepted()
        {
	        dragDropZoneTexture = ContentHelper.UIfolder;
	        
	        SetElementsToExport(DragAndDrop.objectReferences);
            
            SetExportPackage();
            ExporterValidationEditor.Validate();
        }

        private void SetElementsToExport(Object[] objectReferences)
        {
	        ExporterData data = Exporter.data;
	        data.elementsToExport = new List<string>();

	        for (int i = 0; i < objectReferences.Length; i++)
	        {
		        data.elementsToExport.Add(AssetDatabase.GetAssetPath(objectReferences[i]));
	        }

	        data.onlinePackage = VoodooStore.packages.Find(x => x.Name == objectReferences[0].name);
	        data.package.name = data.onlinePackage != null ? data.onlinePackage.name : objectReferences[0].name;
        }

        private void SetExportPackage()
        {
	        Exporter.data.package.displayName = Regex.Replace(Exporter.data.package.Name, regexDisplayName, " $1");

	        if (Exporter.data.onlinePackage == null)
	        {
		        Exporter.data.ResetExportPackage();
		        Event.current.Use();
		        return;
	        }

	        Exporter.data.SetExportPackage();
            
            packageToExportIndex = 0;
            Event.current.Use();
            
            multiPackageText = Exporter.data.CalculateMultiPackageNameChoice();
        }

        private void ShowPackageName()
        {
	        if (string.IsNullOrEmpty(Exporter.data.package.name))
		        return;
	        
	        EditorGUILayout.BeginHorizontal();
	        {
		        EditorGUILayout.LabelField(Exporter.data.package.Name, ContentHelper.StyleHeader);
		        if (Exporter.data.elementsToExport != null && Exporter.data.elementsToExport.Count > 1)
		        {
			        if (GUILayout.Button(">", GUILayout.Width(20)))
			        {
				        packageToExportIndex++;
				        if (packageToExportIndex > Exporter.data.elementsToExport.Count - 1)
				        {
					        packageToExportIndex = 0;
				        }
				        
				        string newPackageName = Path.GetFileName(Exporter.data.elementsToExport[packageToExportIndex]);
				        Exporter.data.onlinePackage = VoodooStore.packages.Find(x => x.Name == newPackageName);
				        Exporter.data.package.name = Exporter.data.onlinePackage != null ? Exporter.data.onlinePackage.name : newPackageName;
				        Exporter.data.package.displayName = Regex.Replace(Exporter.data.package.Name, regexDisplayName, " $1");

				        if (Exporter.data.onlinePackage == null)
				        {
					        Exporter.data.ResetExportPackage();
				        }
				        else
				        {
					        Exporter.data.SetExportPackage();
				        }
				        
				        ExporterValidationEditor.Validate();
			        }
		        }
	        }
	        EditorGUILayout.EndHorizontal();
        }
        
        private void ShowExportButton()
        {
	        EditorGUILayout.Space();
	        if (ExporterValidationEditor.isValid == false || string.IsNullOrEmpty(Exporter.data.commitMessage))
	        {
		        GUI.enabled = false;
	        }
	        
	        if (GUILayout.Button("Export !", GUILayout.Height(40.0f)))
	        {
		        bool exportValidatedByUser = EditorUtility.DisplayDialog(Exporter.data.package.Name,
			        "You are going to push the package \"" + Exporter.data.package.Name + "\" to the VoodooStore. Are you sure ?", "Yes", "Cancel");

		        if (exportValidatedByUser)
		        {
			        ExportPackage();
		        }
	        }

	        EditorGUILayout.Space();

	        GUI.enabled = true;
        }

        private async void ExportPackage()
        {
	        state = ExporterState.EXPORTING;
	        await Exporter.ExportPackage();

	        Reset();
	        
	        state = ExporterState.EXPORTER_READY;
        }

        private void Reset()
        {
	        Exporter.data = null;
	        GUI.FocusControl(null);
	        ExporterValidationEditor.ClearValidation();
	        ExporterLabelEditor.Reset();
	        toolbarInt = 0;
        }
        
        private enum ExporterState
        {
	        EXPORTER_READY,
	        EXPORTING
        }
	}
}