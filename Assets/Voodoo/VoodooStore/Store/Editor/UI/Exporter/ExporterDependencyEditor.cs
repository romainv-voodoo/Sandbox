using System;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
	public class ExporterDependencyEditor : IEditor
	{
		private string dependencySearchString;
		private Vector2 dependencyScrollPosition;
		
		public void OnEnable()
		{
		}

		public void OnDisable()
		{
		}

		public void Controls()
		{
		}

		public void OnGUI()
		{
			ShowDependency();
		}
		
		private void ShowDependency()
		{
			ShowDependencySearchBar();
			
	        dependencyScrollPosition = EditorGUILayout.BeginScrollView(dependencyScrollPosition);
            for (int i = 0; i < Exporter.data.dependencyPackages.Count; i++)
            {
	            DependencyPackage dependencyPackage = Exporter.data.dependencyPackages[i];
	            
	            if (ShouldBeDisplayed(dependencyPackage) == false)
	            {
		            continue;
	            }

	            if (Exporter.data.unselectableDependencyPackages.Contains(dependencyPackage.name))
	            {
		            GUI.enabled = false;
	            }
	            
	            EditorGUILayout.BeginHorizontal();
	            {
		            EditorGUI.BeginChangeCheck();
		            bool value = EditorGUILayout.ToggleLeft(dependencyPackage.Name, dependencyPackage.isSelected);

		            if (EditorGUI.EndChangeCheck())
		            {
			            OnPackageSelected(dependencyPackage, value);
		            }
	            }
	            EditorGUILayout.EndHorizontal();
	            
		        GUI.enabled = true;
            }

            GUILayout.EndScrollView();
        }

		private bool ShouldBeDisplayed(DependencyPackage dependencyPackage)
		{
			string dependencyPackageName   = dependencyPackage.name;
			bool isVoodooStore             = dependencyPackageName == PathHelper.VST_NAME;
			bool isItself                  = dependencyPackageName == Exporter.data.package.name;
			bool isOutsideOfSearchString   = string.IsNullOrEmpty(dependencySearchString) == false
			                                 && dependencyPackage.Name.IndexOf(dependencySearchString, StringComparison.OrdinalIgnoreCase) < 0;
			
			return !isVoodooStore && !isItself && !isOutsideOfSearchString;
		}

		private void OnPackageSelected(DependencyPackage dependencyPackage, bool toggleValue)
		{
			string dependencyPackageName = dependencyPackage.name;
			Package package = VoodooStore.packages.Find(x => x.name == dependencyPackageName);
			Exporter.data.AddPackageToDependencyList(package, toggleValue);
		}

		private void ShowDependencySearchBar()
		{
			EditorGUILayout.BeginHorizontal();
	        
			dependencySearchString = GUILayout.TextField(dependencySearchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
            
			if (GUILayout.Button(GUIContent.none, GUI.skin.FindStyle("ToolbarSeachCancelButton")))
			{
				dependencySearchString = "";
				GUI.FocusControl(null);
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}