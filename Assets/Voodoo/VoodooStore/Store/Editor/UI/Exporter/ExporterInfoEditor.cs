using System;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
	public class ExporterInfoEditor : IEditor
	{
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
			ShowInfo();
		}
		
		public void ShowInfo()
		{
			Exporter.data.package.displayName = EditorGUILayout.TextField("Display Name", Exporter.data.package.displayName);
            
			ShowAuthor();
            
			Exporter.data.package.description = EditorGUILayout.TextField("Description", Exporter.data.package.description);
            
			ShowVersion();

			ShowDocumentationLink();

			if (Exporter.data.isNewPackage == false)
			{
				EditorGUI.BeginChangeCheck();
				Exporter.data.onlyUpdateInfo = EditorGUILayout.Toggle("Only update information", Exporter.data.onlyUpdateInfo);
				if (EditorGUI.EndChangeCheck())
				{
					Exporter.data.package.version = Exporter.data.onlinePackage.version;
				}
			}
		}

		private void ShowAuthor()
        {
	        EditorGUI.BeginChangeCheck();
	        Exporter.data.selectedAuthor = EditorGUILayout.Popup("Author", Exporter.data.selectedAuthor, Exporter.data.authors);
	        if (EditorGUI.EndChangeCheck())
	        {
		        if (Exporter.data.authors[Exporter.data.selectedAuthor] == "New Author")
		        {
			        string tempNewAuthor = Exporter.data.newAuthor.Split('@')[0];
			        Exporter.data.package.author = string.IsNullOrEmpty(tempNewAuthor) ? "" : Exporter.data.newAuthor;
		        }
	        }

	        if (Exporter.data.authors[Exporter.data.selectedAuthor] == "New Author")
	        {
		        EditorGUILayout.BeginHorizontal();
	            
		        EditorGUI.BeginChangeCheck();
		        EditorGUI.indentLevel++;
		        Exporter.data.newAuthor = EditorGUILayout.TextField("New Author", Exporter.data.newAuthor);
		        EditorGUI.indentLevel--;
		        if (EditorGUI.EndChangeCheck())
		        {
			        string domain = "@voodoo.io";

			        for (int i = 0; i < Exporter.data.authors.Length; i++)
			        {
				        if (Exporter.data.authors[i] == Exporter.data.newAuthor + domain)
				        {
					        Debug.Log("The author " + Exporter.data.newAuthor + " already exist");
					        Exporter.data.newAuthor = "";
					        Exporter.data.selectedAuthor = i;
					        break;
				        }
			        }

			        Exporter.data.newAuthor = Exporter.data.newAuthor.Split('@')[0];
			        Exporter.data.package.author = Exporter.data.newAuthor + domain;
		        }

		        EditorGUILayout.LabelField("@voodoo.io", GUILayout.Width(85));
		        EditorGUILayout.EndHorizontal();
	        }
	        else
	        {
		        Exporter.data.package.author = Exporter.data.authors[Exporter.data.selectedAuthor];
	        }
        }

        private void ShowVersion()
        {
	        if (Exporter.data.isNewPackage)
            {
	            Exporter.data.package.version = EditorGUILayout.TextField("Version", Exporter.data.package.version);
            }
            else
			{
				Package package = Exporter.data.onlinePackage;

				if (package == null)
				{
					Exporter.data.package.version = EditorGUILayout.TextField("Version", Exporter.data.package.version);
					return;
				}
				
				if (Exporter.data.onlyUpdateInfo)
				{
					EditorGUILayout.LabelField("Version", Exporter.data.package.version);
					return;
				}
                
                EditorGUILayout.LabelField("Old Version", package.version);

                EditorGUILayout.BeginHorizontal();
                
                Exporter.data.package.version = EditorGUILayout.TextField("New Version", Exporter.data.package.version);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Major", GUILayout.Width(70)))
                {
	                Version _version = Version.Parse(package.version);
	                _version = new Version(_version.Major + 1, 0, 0);
	                Exporter.data.package.version = _version.ToString();
                }

                if (GUILayout.Button("Minor", GUILayout.Width(70)))
                {
                    Version _version = Version.Parse(package.version);
                    _version = new Version(_version.Major, _version.Minor + 1, 0);
                    Exporter.data.package.version = _version.ToString();
                }

                if (GUILayout.Button("Build", GUILayout.Width(70)))
                {
	                Version _version = Version.Parse(package.version);
	                _version = new Version(_version.Major, _version.Minor, _version.Build + 1);
	                Exporter.data.package.version = _version.ToString();
                }

                if (GUILayout.Button("Revision", GUILayout.Width(70)))
                {
                    Version _version = Version.Parse(package.version);
					_version = new Version(_version.Major, _version.Minor, Mathf.Max(0,_version.Build), Mathf.Max(1,_version.Revision + 1));
					Exporter.data.package.version = _version.ToString();
                }

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }
        }
        
        private void ShowDocumentationLink()
        {
	        EditorGUILayout.BeginHorizontal();

	        if (!string.IsNullOrEmpty(Exporter.data.package.documentationLink))
	        {
		        Exporter.data.package.documentationLink = EditorGUILayout.TextField("Documentation Link ", Exporter.data.package.documentationLink);

		        if (GUILayout.Button("Preview ", GUILayout.Width(60), GUILayout.Height(16)))
		        {
			        Application.OpenURL(Exporter.data.package.documentationLink);
		        }
	        }
	        else
	        {
		        Exporter.data.package.documentationLink = EditorGUILayout.TextField("Documentation Link ", Exporter.data.package.documentationLink);
	        }
	        
	        EditorGUILayout.EndHorizontal();
        }
	}
}