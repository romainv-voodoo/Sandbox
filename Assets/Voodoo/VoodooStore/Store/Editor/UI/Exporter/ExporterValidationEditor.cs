using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
	public static class ExporterValidationEditor
	{
		private static Vector2 validationScrollView;
		private static ExporterHelpBoxArea exporterHelpBoxArea;
		private static string parentFolderPath;
		public static bool isValid;

		public static void Validate()
		{
			if (Exporter.data == null)
				return;
			
			exporterHelpBoxArea = new ExporterHelpBoxArea();
			exporterHelpBoxArea.onFixAsked += FixAsked;
			
			ValidateFields();
			
			if (Exporter.data.elementsToExport == null || Exporter.data.elementsToExport.Count == 0)
				return;

			foreach (string elementToExport in Exporter.data.elementsToExport)
			{
				parentFolderPath = elementToExport;
			
				if (Directory.Exists(parentFolderPath))
				{
					ValidateFolderOrganization();
					ValidateNamespaces();
				}
				else if (File.Exists(parentFolderPath) == false)
				{
					Exporter.data = new ExporterData();
				}
			}
		}
		
		public static void ShowValidation()
		{
			if (exporterHelpBoxArea.IsEmpty)
			{
				isValid = true;
				return;
			}
			
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Validations", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Fix All", GUILayout.Height(40.0f),GUILayout.Width(80.0f)))
			{
				exporterHelpBoxArea.FixAll();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			// Do not display the scroll view if there are no data to display
			validationScrollView = EditorGUILayout.BeginScrollView(validationScrollView);
			{
				exporterHelpBoxArea?.Display();
			}
			EditorGUILayout.EndScrollView();
		}

		public static void ClearValidation()
		{
			exporterHelpBoxArea = new ExporterHelpBoxArea();
		}

		private static void ValidateFields()
		{
			bool res = true;

			ExporterHelpBox exporterHelpBox = new ExporterHelpBox
			{
				helpBoxType   = HelpBoxType.Data,
				messageType   = MessageType.Error,
				projectObject = null,
			};

			if (Exporter.data.elementsToExport == null || Exporter.data.elementsToExport.Count == 0)
			{
				ExporterHelpBox tempExporterHelpBox = new ExporterHelpBox(exporterHelpBox)
				{
					objectType = "Missing Package",
					message    = "You need a content to create a package, drag and drop element(s) on the + zone"
				};
				exporterHelpBoxArea.Add(tempExporterHelpBox);
				
				isValid = false;
				return;
			}
			
			if (string.IsNullOrEmpty(Exporter.data.package.author) && string.IsNullOrEmpty(Exporter.data.newAuthor))
			{
				ExporterHelpBox tempExporterHelpBox = new ExporterHelpBox(exporterHelpBox)
				{
					objectType = "Missing Author",
					message    = "You need to fill the New Author field or select a proper Author"
				};
				exporterHelpBoxArea.Add(tempExporterHelpBox);
				
				res = false;
			}

			if (string.IsNullOrEmpty(Exporter.data.package.description))
			{
				ExporterHelpBox tempExporterHelpBox = new ExporterHelpBox(exporterHelpBox)
				{
					objectType = "Missing Description",
					message    = "You need to fill the Description field"
				};
				exporterHelpBoxArea.Add(tempExporterHelpBox);
				
				res = false;
			}

			// if (string.IsNullOrEmpty(Exporter.data.commitMessage))
			// {
			// 	ExporterHelpBox tempExporterHelpBox = new ExporterHelpBox(exporterHelpBox)
			// 	{
			// 		objectType = "Missing Patch Note",
			// 		message    = "You need to write a Patch note"
			// 	};
			// 	exporterHelpBoxArea.Add(tempExporterHelpBox);
			// 	
			// 	res = false;
			// }

			if (string.IsNullOrEmpty(Exporter.data.package.displayName))
			{
				ExporterHelpBox tempExporterHelpBox = new ExporterHelpBox(exporterHelpBox)
				{
					objectType = "Missing Display Name",
					message    = "You need to fill the Display Name field"
				};
				exporterHelpBoxArea.Add(tempExporterHelpBox);
				
				res = false;
			}

			if (string.IsNullOrEmpty(Exporter.data.package.version))
			{
				ExporterHelpBox tempExporterHelpBox = new ExporterHelpBox(exporterHelpBox)
				{
					objectType = "Missing Version",
					message    = "You need to fill the Version field"
				};
				exporterHelpBoxArea.Add(tempExporterHelpBox);
				
				res = false;
			}

			if (Exporter.data.onlyUpdateInfo == false && Exporter.data.package.tags != null && Exporter.data.package.tags.Contains(Exporter.data.package.version))
			{
				ExporterHelpBox tempExporterHelpBox = new ExporterHelpBox(exporterHelpBox)
				{
					objectType = "Identical Version",
					message    = "The version you are trying to push already exists"
				};
				exporterHelpBoxArea.Add(tempExporterHelpBox);
				
				res = false;
			}
			
			isValid = res;
		}

		private static void ValidateFolderOrganization()
		{
			Match isUnderVoodooFolder = Regex.Match(parentFolderPath, @"(Voodoo)", RegexOptions.IgnoreCase); ;
			
			if (isUnderVoodooFolder.Success == false)
			{
				ExporterHelpBox exporterHelpBox = new ExporterHelpBox
				{
					helpBoxType   = HelpBoxType.IncorrectPath,
					objectType    = "Voodoo",
					message       = "Your parent folder must be under a \"Voodoo\" folder.",
					messageType   = MessageType.Warning,
					projectObject = AssetDatabase.LoadAssetAtPath<Object>(parentFolderPath),
					isClickable   = true,
					isFixable     = true,
				};
				
				exporterHelpBoxArea.Add(exporterHelpBox);
			}
			
			List<string> folderNames = new List<string>{"Scripts","Editor"};
			ValidateFilePath("cs", folderNames);

			folderNames = new List<string> {"Scenes"};
			ValidateFilePath("unity", folderNames);
			
			folderNames = new List<string> {"Shaders"};
			ValidateFilePath("shader", folderNames);
			
			folderNames = new List<string> {"Materials"};
			ValidateFilePath(".mat", folderNames);
			
			folderNames = new List<string> {"Textures", "Sprites", "Resources"};
			ValidateFilePath(".png", folderNames);
			
			folderNames = new List<string> {"Textures", "Sprites", "Resources"};
			ValidateFilePath(".jpg", folderNames);
		}
		
		private static void ValidateFilePath(string extension, List<string> folderNames)
		{
			bool res = false;
			string extensionName = extension.Replace(".", "").Replace("*", "");
			List<string> files = Directory.GetFiles(parentFolderPath, "*." + extensionName, SearchOption.AllDirectories).ToList();
			
			if (files.Count == 0)
			{
				res = true;
			}
			
			foreach (var file in files)
			{
				if (folderNames != null && folderNames.Count > 0)
				{
					foreach (string folderName in folderNames)
					{
						if (file.Contains(folderName))
						{
							res = true;
							break;
						}
					}
				}
				else
				{
					res = true;
				}

				if (res)
				{
					continue;
				}
				
				string message = file + " has an incorrect path. Please put it under";
				if (folderNames.Count > 1)
				{
					message += " one of the following folders : ";
				}
				else
				{
					message += " the following folder : ";
				}
				
				foreach (string folderName in folderNames)
				{
					message += folderName + ", ";
				}

				message = message.Remove(message.Length - 2);

				ExporterHelpBox exporterHelpBox = new ExporterHelpBox
				{
					helpBoxType   = HelpBoxType.IncorrectPath,
					objectType    = folderNames[0],
					message       = message,
					messageType   = MessageType.Warning,
					projectObject = AssetDatabase.LoadAssetAtPath<Object>(file),
					isClickable   = true,
					isFixable     = true
				};

				exporterHelpBoxArea.Add(exporterHelpBox);
			}
		}

		private static void ValidateNamespaces()
		{
			List<string> files = Directory.GetFiles(parentFolderPath, "*.cs", SearchOption.AllDirectories).ToList();
			
			foreach (string file in files)
			{
				List<string> path = new List<string>();
				if (Application.platform == RuntimePlatform.WindowsEditor)
				{
					path = file.Split('\\').ToList();
				}
				
				if (Application.platform == RuntimePlatform.OSXEditor)
				{
					path = file.Split('/').ToList();
				}
				
				int baseIndex = path.IndexOf("Voodoo");
				string packageName = path.ElementAt(baseIndex + 1);
				string fileContent = File.ReadAllText(file);
				string awaitedNamespace = "Voodoo.Brand.Product..." + packageName;
				MatchCollection hasNamespace = Regex.Matches(fileContent, @"((namespace)\s+\w+([.]\w*)*)", RegexOptions.IgnoreCase);

				if (hasNamespace.Count == 0)
				{
					ExporterHelpBox exporterHelpBox = new ExporterHelpBox
					{
						helpBoxType   = HelpBoxType.MissingNamespace,
						objectType    = "Scripts",
						message       = file + " doesn't have a namespace. Maybe you should add " + awaitedNamespace,
						messageType   = MessageType.Warning,
						projectObject = AssetDatabase.LoadAssetAtPath<Object>(file),
						isClickable   = true
					};

					exporterHelpBoxArea.Add(exporterHelpBox);
				}
				
				foreach (Match match in hasNamespace)
				{
					Match isRightNamespace = Regex.Match(match.Value,@"((namespace)\s+(Voodoo)[.]\w+([.]\w*)*)", RegexOptions.IgnoreCase);

					if (isRightNamespace.Success)
						continue;
					
					ExporterHelpBox exporterHelpBox = new ExporterHelpBox
					{
						helpBoxType   = HelpBoxType.IncorrectNamespace,
						objectType    = "Scripts",
						message       = file + " has an incorrect namespace. You should prefer " + awaitedNamespace,
						messageType   = MessageType.Info,
						projectObject = AssetDatabase.LoadAssetAtPath<Object>(file),
						isClickable   = true
					};
						
					exporterHelpBoxArea.Add(exporterHelpBox);
				}
			}
		}

		private static void FixAsked(ExporterHelpBox exporterHelpBox, bool silently)
		{
			if (exporterHelpBox.helpBoxType != HelpBoxType.IncorrectPath)
				return;
			
			string filePath = AssetDatabase.GetAssetPath(exporterHelpBox.projectObject);
			if (filePath == null)
				return;
				
			List<string> folders = new List<string>();
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				folders = filePath.Split('\\').ToList();
			}
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				folders = filePath.Split('/').ToList();
			}

			if (exporterHelpBox.objectType == "Voodoo")
			{
				MoveToVoodooFolder(filePath, folders, !silently);
			}
			else
			{
				MoveToObjectTypeFolder(exporterHelpBox, filePath, folders, !silently);
			}
		}

		private static void MoveToObjectTypeFolder(ExporterHelpBox exporterHelpBox, string filePath, List<string> folders, bool displayDialogBox)
		{
			int indexOfMainFolder = folders.IndexOf("Voodoo");
			if (indexOfMainFolder < 0)
			{
				indexOfMainFolder = folders.IndexOf("Assets");
			}
			
			string newFilePath = string.Empty;
			
			//Browse all the folders until the "Voodoo" one or "Assets" if there is none
			//in order to find the folder with the same name as exporterHelpBox.objectType
			for (int i = folders.Count - 1; i > indexOfMainFolder + 1; i--)
			{
				newFilePath = "";
				for (int j = 0; j < i; j++)
				{
					newFilePath = Path.Combine(newFilePath, folders[j]);
				}
				
				List<string> matchFolders = ExistInFolder(exporterHelpBox.objectType, newFilePath);
				
				if (matchFolders.Count <= 0)
					continue;
				
				//If your find a folder with the same name as exporterHelpBox.objectType
				//Move the file to this folder
				newFilePath = Path.Combine(matchFolders[0], folders.Last());
				MoveAsset(filePath, newFilePath);
				return;
			}
			
			//If you didn't find any folder with the same name as exporterHelpBox.objectType
			//Create the folder under the main folder of your package
			//Move the file to this folder
			newFilePath = "";
			for (int i = 0; i <= indexOfMainFolder+1; i++)
			{
				newFilePath = Path.Combine(newFilePath, folders[i]);
			}
			
			if (displayDialogBox)
			{
				string dialogMessage = "Your are going to move \"" + filePath + "\" to \"" + Path.Combine(newFilePath, exporterHelpBox.objectType) + "\"";
				bool shouldMoveAsset = EditorUtility.DisplayDialog("Fix path problem", dialogMessage, "Do it", "Cancel");

				if (!shouldMoveAsset)
					return;
			}

			AssetDatabase.CreateFolder(newFilePath, exporterHelpBox.objectType);
			newFilePath = Path.Combine(newFilePath, exporterHelpBox.objectType, folders.Last());
			
			MoveAsset(filePath, newFilePath, false);
		}

		private static List<string> ExistInFolder(string folderName, string parentFolderPath)
		{
			string[] matchFolder = Directory.GetDirectories(parentFolderPath, folderName, SearchOption.AllDirectories);
			return matchFolder.ToList();
		}

		private static void MoveToVoodooFolder(string filePath, List<string> folders, bool displayDialogBox)
		{
			string VoodooDirectory = GetNearestVoodooDirectory(filePath);
			string newPath = Path.Combine(VoodooDirectory, folders.Last());

			MoveAsset(filePath, newPath, displayDialogBox);
		}

		private static string GetNearestVoodooDirectory(string filePath)
		{
			string res = "";
			
			DirectoryInfo parentDirectory = Directory.GetParent(filePath);
			string[] folderPaths = Directory.GetDirectories(parentDirectory.FullName, "Voodoo", SearchOption.AllDirectories);
				
			if (folderPaths.Length > 0)
			{
				if (folderPaths[0].StartsWith(Application.dataPath))
				{
					folderPaths[0] =  "Assets" + folderPaths[0].Substring(Application.dataPath.Length);
				}
				
				res = folderPaths[0];
			}
			else
			{
				if (parentDirectory.Name != "Assets")
				{
					res = GetNearestVoodooDirectory(parentDirectory.FullName);
				}
			}

			return res;
		}

		private static void MoveAsset(string filePath, string newFilePath, bool displayDialogBox = true)
		{
			if (displayDialogBox)
			{
				bool shouldMoveAsset = EditorUtility.DisplayDialog("Fix path problem", "Your are going to move \"" + filePath + "\" to \"" + newFilePath + "\"", "Do it", "Cancel");

				if (!shouldMoveAsset)
					return;
			}
			
			string errorMessage = AssetDatabase.MoveAsset(filePath, newFilePath);
			if (string.IsNullOrEmpty(errorMessage))
			{
				Debug.Log("File located at \"" + filePath + "\" was successfully moved to \"" + newFilePath + "\"");
			}
			else
			{
				Debug.Log(errorMessage);
			}

			for (var i = 0; i < Exporter.data.elementsToExport.Count; i++)
			{
				string elementToPackage = Exporter.data.elementsToExport[i];

				if (elementToPackage == filePath)
				{
					Exporter.data.elementsToExport[i] = newFilePath;
					break;
				}
			}

			AssetDatabase.Refresh();
		}
	}
}