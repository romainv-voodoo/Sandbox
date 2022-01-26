using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
	public class PackageDownloadWindow : EditorWindow
	{
		public static PackageDownloadWindow window;
		
		public bool     download     = false;
		public string   message      = string.Empty;

		public List<OptionalElement> optionalElements;

		private Package package;
		private Vector2 scrollPos = Vector2.zero;

		public static bool ShowWindow(DownloadRequest request, List<OptionalElement> globalOptionalElements = null)
		{
			if (window == null)
			{
				window = CreateInstance<PackageDownloadWindow>();
				window.minSize = new Vector2(300,125);
				window.maxSize = new Vector2(300,4000f);
			}
			else
			{
				window = GetWindow<PackageDownloadWindow>();
			}

			window.optionalElements = globalOptionalElements ?? new List<OptionalElement>();
			window.InitParameters(request);
			
			window.titleContent = new GUIContent($"Downloading {request.Current.displayName}");
#if UNITY_2019_3_OR_NEWER
			window.ShowModal();
#else
			Type windowType = window.GetType(); 
 
			MethodInfo showModal = windowType.GetMethod("ShowModal", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
         
			if (showModal!= null)
			{
				showModal?.Invoke(window, null);
			}
#endif

			return window.download;
		}

		private void InitParameters(DownloadRequest request)
		{
			package = request.Current;
			download = false;

			if (package.additionalContents != null)
			{
				foreach (AdditionalContent additionalContent in package.additionalContents)
				{
					if (optionalElements.Exists(x => x.Name == additionalContent.Name) == false)
					{
						optionalElements.Add(new OptionalElement(additionalContent.folderPath, additionalContent.size));
					}
				}
			}

			UpdateMessage();
		}

		private void UpdateMessage()
		{
			int size = package.size;

			foreach (OptionalElement optionalElement in optionalElements)
			{
				if (optionalElement.isSelected)
				{
					size += optionalElement.size;
				}
			}

			message = string.Concat($"<b>Name</b>\t\t{package.displayName}", Environment.NewLine, $"<b>Version</b>\t\t{package.version}");
			
			
			if (string.IsNullOrEmpty(package.updatedAt) == false)
			{
				DateTime date = DateTime.Parse(package.updatedAt);
				string readableDate = date.ToString("dd MMMM yyyy HH:mm:ss");
				message = String.Concat(message, Environment.NewLine, $"<b>Update</b>\t\t{readableDate}");
			}
			
			message = string.Concat(message, Environment.NewLine,$"<b>Size</b>\t\t{size.ToOctetsSize()}", Environment.NewLine);
		}

		private void OnGUI()
		{
			DrawHeader();
			
			EditorGUILayout.Space();
			
			DrawContent();
			
			DrawFooter();
		}

		private void DrawHeader()
		{
			//TODO : display header if there is one 
		}

		private void DrawContent()
		{
			GUILayout.BeginVertical("box");
			{
				scrollPos = GUILayout.BeginScrollView(scrollPos);
				{
					GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
					{
						wordWrap = true,
						richText = true
					};

					EditorGUILayout.LabelField(message, labelStyle);

					DisplayAdditionalContent();

					GUILayout.FlexibleSpace();
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}

		private void DisplayAdditionalContent()
		{
			if (optionalElements == null || optionalElements.Count == 0)
			{
				return;
			}
			EditorGUILayout.Space();
					
			EditorGUILayout.LabelField("Additional Content", ContentHelper.StyleHeader);

			DisplayAdditionalContentLines();
		}

		private void DisplayAdditionalContentLines()
		{
			foreach (OptionalElement optionalElement in optionalElements)
			{
				EditorGUI.BeginChangeCheck();
				{
					EditorGUILayout.BeginHorizontal();
					optionalElement.isSelected = EditorGUILayout.Toggle(optionalElement.isSelected, GUILayout.Width(20f));
					// optionalElement.isSelected = EditorGUILayout.ToggleLeft(optionalElement.Name, optionalElement.isSelected, GUILayout.Width(150f));
					
					EditorGUILayout.LabelField(optionalElement.Name, GUILayout.Width(125f));

					// GUILayout.Space(10f);
					GUILayout.FlexibleSpace();

					EditorGUILayout.LabelField(optionalElement.size.ToOctetsSize(), GUILayout.Width(55f));
					EditorGUILayout.EndHorizontal();
				}
				
				if (EditorGUI.EndChangeCheck())
				{
					UpdateMessage();
				}
			}
		}

		private void DrawFooter()
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(5f);
				
				if (GUILayout.Button("Cancel"))
				{
					Close();
				}

				EditorGUILayout.Space();

				if (GUILayout.Button("Download"))
				{
					download = true;
					Close();
				}

				GUILayout.Space(5f);
			}
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(10f);
		}
	}
}