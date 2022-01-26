using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
	public class MultiDownloadWindow : EditorWindow
	{
		public static MultiDownloadWindow window;

		// public bool   downloadDemo = true;
		public string message      = string.Empty;

		private DownloadChoice downloadChoice = DownloadChoice.CANCEL;
		private List<Package>  packages;
		private Vector2        scrollPos = Vector2.zero;

		public static int ShowWindow(List<Package> downloadPackages)
		{
			if (window == null)
			{
				window = CreateInstance<MultiDownloadWindow>();
				window.minSize = new Vector2(280,100);
			}
			else
			{
				window = GetWindow<MultiDownloadWindow>();
			}

			window.InitParameters(downloadPackages);
			
			window.titleContent = new GUIContent("Warning");
#if UNITY_2019_3_OR_NEWER
			window.ShowModal();
#else
			Type ourType = window.GetType(); 
 
			MethodInfo showModal = ourType.GetMethod("ShowModal", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
         
			if (showModal!= null)
			{
				showModal?.Invoke(window, null);
			}
#endif

			return (int)window.downloadChoice;
		}

		private void InitParameters(List<Package> downloadPackages)
		{
			packages = new List<Package>();
			StringBuilder content = new StringBuilder("There are multiple packages to download due to dependencies")
				.Append(Environment.NewLine).Append(Environment.NewLine);

			int count = downloadPackages.Count;
			for (int i = 0; i < count; i++)
			{
				if (downloadPackages[i].VersionStatus != VersionState.UpToDate)
				{
					packages.Add(downloadPackages[i]);
					content.Append($"<b>{downloadPackages[i].displayName}</b>").Append(Environment.NewLine);
				}
			}

			content.Append(Environment.NewLine).Append("Do you want to download <b>all of them</b> at the same time or <b>one by one</b> ?");

			message = content.ToString();
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

					// EditorGUIUtility.labelWidth = 95;
					// downloadDemo = EditorGUILayout.Toggle("Demo", downloadDemo);
					// EditorGUIUtility.labelWidth = 150;

					GUILayout.FlexibleSpace();
				}
				GUILayout.EndScrollView();
			}
			GUILayout.EndVertical();
		}
		
		private void DrawFooter()
		{
			EditorGUILayout.BeginHorizontal();
			{
				GUILayout.Space(5f);
				
				if (GUILayout.Button("Cancel"))
				{
					downloadChoice = DownloadChoice.CANCEL;
					Close();
				}

				EditorGUILayout.Space();
				EditorGUILayout.Space();

				if (GUILayout.Button("One by One"))
				{
					downloadChoice = DownloadChoice.ONE_BY_ONE;
					Close();
				}

				if (GUILayout.Button("All of them"))
				{
					downloadChoice = DownloadChoice.ALL_OF_THEM;
					Close();
				}

				GUILayout.Space(5f);
			}
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(10f);
		}

		private enum DownloadChoice
		{
			ALL_OF_THEM = 0,
			CANCEL = 1,
			ONE_BY_ONE = 2,
		}
	}
}