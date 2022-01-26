using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public class OptionsEditor : IEditor
    {
        private List<IOptionWidget> optionsEditors;

        private int selectedOption;
        
        private Vector2 scrollPosTab;
        private Vector2 scrollPosContent;
        public void OnEnable()
        {
            optionsEditors = new List<IOptionWidget>
            {
                new GitignoreOptionWidget()
            };
            
            foreach (IOptionWidget editor in optionsEditors)
            {
                editor.OnEnable();
            }

            if (optionsEditors.Count > 0)
            {
                selectedOption = 0;
            }
        }

        public void OnDisable()
        {
            foreach (IEditor editor in optionsEditors)
            {
                editor.OnDisable();
            }
			
            optionsEditors.Clear();
        }

        public void Controls()
        {
            foreach (IEditor editor in optionsEditors)
            {
                editor.Controls();
            }
        }

        public void OnGUI()
        {
            if (VoodooStoreState.Current != State.OPTIONS)
            {
                return;
            }
            
            Controls();

            ShowButtonsBar();
            
            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            EditorGUILayout.LabelField("OPTIONS");
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            if (optionsEditors.Count > 1)
            {
                ShowTabs();
                ContentHelper.DrawInfiniteLine(Color.black, false);
            }
            ShowOption();
            GUILayout.EndHorizontal();
            
        }

        private static void ShowButtonsBar()
        {
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(ContentHelper.UIreturn, GUILayout.Height(25), GUILayout.Width(25)))
            {
                VoodooStoreState.Current = State.STORE;
            }

            GUILayout.FlexibleSpace();
            
            ContentHelper.DisplayHelpButtons(25f);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        private void ShowTabs()
        {
            GUILayout.BeginHorizontal();

            scrollPosTab = EditorGUILayout.BeginScrollView(scrollPosTab, false, false);
            GUILayout.BeginVertical();
            for (int i = 0; i < optionsEditors.Count; i++)
            {
                if (i == selectedOption)
                {
                    Texture2D background = new Texture2D(1, 1);
                    background.SetPixel(1,1,new Color(0.5f,0.5f,0.5f,0.5f));
                    background.Apply();
                    
                    OptionStyleHelper.buttonStyle.normal.background = background;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                if (GUILayout.Button(optionsEditors[i].Name, OptionStyleHelper.buttonStyle))
                {
                    selectedOption = i;
                }
                GUILayout.EndHorizontal();
                
              
                OptionStyleHelper.buttonStyle.normal.background = null;
            }
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();

        }

        private void ShowOption()
        {
            scrollPosContent = EditorGUILayout.BeginScrollView(scrollPosTab, false, false);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(optionsEditors[selectedOption].Name, OptionStyleHelper.titleStyle, GUILayout.Height(OptionStyleHelper.titleHeight));
            optionsEditors[selectedOption]?.OnGUI();
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}