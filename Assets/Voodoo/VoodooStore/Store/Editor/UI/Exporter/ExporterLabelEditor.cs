using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
   public class ExporterLabelEditor : IEditor
   {
      private static string searchField;
      private static string searchString;
      private static Vector2 scrollPosition;

      private static int searchResults;
      private static readonly List<string> tempLabels = new List<string>();

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
         ShowLabels();
      }

      public static void Reset()
      {
         CancelSearch();
      }

      private static void ShowLabels()
      {
         ShowLabelSearchBar();
         ShowLabelList();
      }

      private static void ShowLabelSearchBar()
      {
         EditorGUILayout.BeginHorizontal();

         EditorGUI.BeginChangeCheck();
         searchField = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
         if (EditorGUI.EndChangeCheck())
         {
            searchString = FormatText(searchField);
         }

         if (GUILayout.Button(GUIContent.none, GUI.skin.FindStyle("ToolbarSeachCancelButton")))
         {
            CancelSearch();
         }

         EditorGUILayout.EndHorizontal();
      }

      private static void ShowLabelList()
      {
         searchResults = 0;

         scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
         for (int i = 0; i < Exporter.data.labels.Count; i++)
         {
            ExporterLabel exporterLabel = Exporter.data.labels[i];

            if (ShouldBeDisplayed(exporterLabel) == false)
            {
               continue;
            }

            ++searchResults;
            ShowLabel(exporterLabel);
         }

         if (searchResults == 0 && !String.IsNullOrEmpty(searchString) && !String.IsNullOrWhiteSpace(searchString))
         {
            ShowNewLabel();
         }

         GUILayout.EndScrollView();
      }

      private static void ShowLabel(ExporterLabel exporterLabel)
      {
         EditorGUILayout.BeginHorizontal();
         {
            EditorGUI.BeginChangeCheck();

            ExporterLabel exporterLabelFromList = Exporter.data.labels.Find(i => i.labelName == exporterLabel.labelName);

            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
               normal =
               {
                  textColor = SpecialLabels.GetLabelColor(exporterLabel.labelName)
               },
               hover =
               {
                  textColor = SpecialLabels.GetLabelColor(exporterLabel.labelName)
               }
            };

            bool isSelected = exporterLabelFromList.isSelected;
            
            if (User.isGTD || SpecialLabels.IsSpecial(exporterLabel.labelName)==false)
            {
               isSelected = EditorGUILayout.ToggleLeft(exporterLabel.labelName, exporterLabelFromList.isSelected, labelStyle);
            }
            else if(isSelected)
            {
               EditorGUI.BeginDisabledGroup(true);
               isSelected = EditorGUILayout.ToggleLeft(exporterLabel.labelName, exporterLabelFromList.isSelected, labelStyle);
               EditorGUI.EndDisabledGroup();
            }

            if (EditorGUI.EndChangeCheck())
            {
               if (isSelected)
               {
                  exporterLabelFromList.isSelected = true;
               }
               else
               {
                  if (tempLabels.Contains(exporterLabelFromList.labelName))
                  {
                     tempLabels.Remove(exporterLabelFromList.labelName);
                     Exporter.data.RemoveLabelFromList(exporterLabelFromList);
                  }
                  else
                  {
                     exporterLabelFromList.isSelected = false;
                  }
               }

               Exporter.data.ReOrderLabels();
            }
         }
         EditorGUILayout.EndHorizontal();

         GUI.enabled = true;
      }

      private static void ShowNewLabel()
      {
         EditorGUILayout.BeginHorizontal();
         {
            if (GUILayout.Button("Add \"" + searchString + "\" as a new Label", EditorStyles.toolbarButton))
            {
               ExporterLabel tempExporterLabel = new ExporterLabel
               {
                  labelName = searchString,
                  isSelected = true
               };

               tempLabels.Add(tempExporterLabel.labelName);
               Exporter.data.AddLabelToList(tempExporterLabel);

               CancelSearch();
            }
         }
         EditorGUILayout.EndHorizontal();
      }

      private static void CancelSearch()
      {
         searchString = string.Empty;
         GUI.FocusControl(null);
      }

      private static bool ShouldBeDisplayed(ExporterLabel exporterLabel)
      {
         bool isOutsideOfSearchString = string.IsNullOrEmpty(searchString) == false &&
                                        exporterLabel.labelName.IndexOf(searchString,
                                           StringComparison.OrdinalIgnoreCase) < 0;
         return !isOutsideOfSearchString;
      }

      private static string FormatText(string baseString)
      {
         baseString = baseString.Replace(" ", String.Empty);
         baseString = baseString.Trim();
         baseString = baseString.ToLower();
         return baseString;
      }

   }
}