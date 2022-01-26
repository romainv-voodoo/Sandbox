using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Voodoo.Store
{
	[Serializable]
	public class ExporterData
	{
		public bool                             isNewPackage;
		public bool                             onlyUpdateInfo;
		public Package                          onlinePackage;
	    public Package                          package;
	    public List<string>                     elementsToExport;
	   
	    //TODO : change to state system (Selected, Unselected, UnSelectable)
	    public List<DependencyPackage>          dependencyPackages;
	    public List<string>                     unselectableDependencyPackages;
	    
	    public string                           commitMessage;
	    public List<ExporterLabel>              labels;
	    
	    public List<ExporterAdditionalContent>  additionalContents;
		
		//Author
		public string[]                         authors;
		public int                              selectedAuthor;
		public string                           newAuthor;

		public ExporterData()
		{
			InitExporterPackage();
		}
		
		private void InitExporterPackage()
		{
			// Suppose that is a new package
			isNewPackage = true;
			
			onlyUpdateInfo = false;

			package = new Package();
			package.version = "1.0.0";
			package.unityVersion = Application.unityVersion;

			newAuthor = "";

			List<string> _authors = new List<string>();

			foreach (Package _package in VoodooStore.packages)
			{
				if (!_authors.Contains(_package.author))
				{
					_authors.Add(_package.author);
				}
			}

			_authors = _authors.OrderBy(author => author).ToList();

			_authors.Add("New Author");
			authors = _authors.ToArray();

			selectedAuthor = 0;
			package.author = authors[selectedAuthor];

			// Reset Object
			elementsToExport = new List<string>();

			// Dependency
			unselectableDependencyPackages = new List<string>();
			package.dependencies = new List<string>();
			dependencyPackages = new List<DependencyPackage>();

			for (int i = 0; i < VoodooStore.packages.Count; i++)
			{
				DependencyPackage dependencyPackage = new DependencyPackage
				{
					name = VoodooStore.packages[i].name,
					isSelected = false,
				};
				dependencyPackages.Add(dependencyPackage);
			}
			
			ReOrderDependencyPackages();

			additionalContents = new List<ExporterAdditionalContent>();
			
			//Labels
			package.labels = new List<string>();
			labels = new List<ExporterLabel>();

			for (int i = 0; i < VoodooStore.labels.Count; i++)
			{
				ExporterLabel exporterLabel = new ExporterLabel
				{
					labelName = VoodooStore.labels[i],
					isSelected = false
				};
				labels.Add(exporterLabel);
			}

			ReOrderLabels();
		}

		public string CalculateMultiPackageNameChoice()
		{
			string exportPackageName = string.Empty;

			if (elementsToExport == null || elementsToExport?.Count <= 1)
				return exportPackageName;
	        
			exportPackageName = elementsToExport[0];
		        
			for (int i = 1; i < elementsToExport.Count; i++)
			{
				exportPackageName += " + " + elementsToExport[i];
			}

			return exportPackageName;
		}

		public void ResetExportPackage()
		{
			isNewPackage = true;
			onlyUpdateInfo = false;
			package.version = "1.0.0";
			package.description = "";
			package.unityVersion = Application.unityVersion;
			package.documentationLink = "";
			
			package.dependencies = new List<string>();
			dependencyPackages.ForEach(x => x.isSelected = false);

			unselectableDependencyPackages = new List<string>();
			
			InitializeAdditionalContent();

			package.labels = new List<string>();
			labels.ForEach(x => x.isSelected = false);
			InitializeLabels(package);
			
			newAuthor = "";
			selectedAuthor = 0;
			package.author = authors[selectedAuthor];
		}

		public void SetExportPackage()
		{
			isNewPackage = false;
			onlyUpdateInfo = false;
			package.displayName = onlinePackage.displayName;
			package.author = onlinePackage.author;
			selectedAuthor = Array.IndexOf(authors, package.author);
            
			if (selectedAuthor == -1)
			{
				selectedAuthor = 0;
			}

			package.size = onlinePackage.size;
			package.documentationLink = onlinePackage.documentationLink;
			
			package.dependencies = onlinePackage.dependencies;
			InitializeDependencies(onlinePackage);
			
			InitializeAdditionalContent();
			
			package.labels = onlinePackage.labels;
			InitializeLabels(onlinePackage);

			package.tags = onlinePackage.tags;
			package.description = onlinePackage.description;
			package.version = onlinePackage.version;

			Version _version = Version.Parse(package.version);
			_version = new Version(_version.Major, _version.Minor, _version.Build + 1);
			package.version = _version.ToString();
		}
		
		public void InitializeDependencies(Package package)
		{
			foreach (DependencyPackage dependencyPackage in dependencyPackages)
			{
				dependencyPackage.isSelected = false;
			}
			
			unselectableDependencyPackages = new List<string>();
			
			if (package.dependencies == null)
				return;

			for (int i = 0; i < package.dependencies.Count; i++)
			{
				Package childPackage = VoodooStore.GetOrCreatePackage(package.dependencies[i]);

				if (childPackage == null)
					continue;

				AddPackageToDependencyList(childPackage);
			}
		}

		public void AddPackageToDependencyList(Package package, bool add = true)
		{
			DependencyPackage dependencyPackage = dependencyPackages.Find(x => x.name == package.name);
			dependencyPackage.isSelected = add;

			SetDependencyRecursive(package, add);

			ReOrderDependencyPackages();
		}

		public void ReOrderDependencyPackages()
		{
			dependencyPackages = dependencyPackages.OrderByDescending(x => x.isSelected).ThenBy(x => x.name).ToList();
		}

		private void SetDependencyRecursive(Package package, bool add)
		{
			if (package.dependencies == null)
				return;
			
			for (int i = 0; i < package.dependencies.Count; i++)
			{
				Package childPackage = VoodooStore.packages.Find(x => x.name == package.dependencies[i]);
				
				if (childPackage == null)
					continue;
				
				if (add)
				{
					unselectableDependencyPackages.Add(childPackage.name);
					DependencyPackage dependencyPackage = dependencyPackages.Find(x => x.name == childPackage.name);
					if (dependencyPackage.isSelected)
					{
						dependencyPackage.isSelected = false;
					}
				}
				else
				{
					unselectableDependencyPackages.Remove(childPackage.name);
				}
				
				SetDependencyRecursive(childPackage, add);
			}
		}
		
		private void InitializeAdditionalContent()
		{
			additionalContents = new List<ExporterAdditionalContent>();
			
			//Retrieve additional content that is online
			if (onlinePackage != null)
			{
				foreach (AdditionalContent additionalContent in onlinePackage.additionalContents)
				{
					ExporterAdditionalContent exporterAdditionalContent = new ExporterAdditionalContent(additionalContent);
					additionalContents.Add(exporterAdditionalContent);
					
					UpdateAdditionalContentLocalData(exporterAdditionalContent);
				}
			}

			string additionalContentDirectory = string.Concat(elementsToExport[0], Path.AltDirectorySeparatorChar, PathHelper.AdditionalContentFolderName);
			if (Directory.Exists(additionalContentDirectory) == false)
			{
				return;
			}
			
			//Retrieve additional content that is local
			string[] directories = Directory.GetDirectories(additionalContentDirectory);
			foreach (string directory in directories)
			{
				AddLocalFolderToAdditionalContent(directory);
			}
			
			additionalContents = additionalContents.OrderBy(x => x.additionalContent.Name).ToList();
		}

		private void UpdateAdditionalContentLocalData(ExporterAdditionalContent exporterAdditionalContent)
		{
			DirectoryInfo dir = new DirectoryInfo(exporterAdditionalContent.additionalContent.folderPath);
			if (dir.Exists == false)
			{
				return;
			}

			int size = (int)dir.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
					
			exporterAdditionalContent.existLocal = true;
			exporterAdditionalContent.sizeDiff += size;
		
			if (exporterAdditionalContent.sizeDiff != 0)
			{
				exporterAdditionalContent.status = AdditionalContentState.UPDATE;
				exporterAdditionalContent.additionalContent.size = size;
			}
		}

		public void AddLocalFolderToAdditionalContent(string directoryPath)
		{
			DirectoryInfo dir = new DirectoryInfo(directoryPath);
			int size = (int)dir.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);

			ExporterAdditionalContent existingAdditionalContent = additionalContents.Find(x => x.additionalContent.folderPath == PathHelper.GetMacPath(directoryPath));
				
			if (existingAdditionalContent != null) //The online additional content is present and already updated
			{
				return;
			}
			
			//The online additional content is not present and will be added
			ExporterAdditionalContent additionalContent = new ExporterAdditionalContent(directoryPath, size);
			additionalContents.Add(additionalContent);
		}
		
		public void InitializeLabels(Package package)
		{
			foreach (ExporterLabel label in labels)
			{
				label.isSelected = package.labels.Contains(label.labelName) || (User.isGTD == false && label.labelName == "unverified");
			}

			ReOrderLabels();
		}

		public void AddLabelToList(ExporterLabel exporterLabel)
		{
			if (labels.Contains(exporterLabel))
			{
				return;
			}
			
			labels.Add(exporterLabel);
			ReOrderLabels();
		}

		public void RemoveLabelFromList(ExporterLabel exporterLabel)
		{
			if (labels.Contains(exporterLabel) == false)
			{
				return;
			}
			
			labels.Remove(exporterLabel);
			ReOrderLabels();
		}

		public void ReOrderLabels()
		{
			labels = labels.OrderByDescending(x => x.isSelected).ThenBy(x => x.labelName).ToList();
		}

	}
}
