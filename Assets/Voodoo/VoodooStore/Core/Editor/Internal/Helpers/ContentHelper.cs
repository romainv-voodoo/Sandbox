using UnityEditor;
using UnityEngine;

namespace Voodoo.Store
{
    public static class ContentHelper
    {
        public static readonly Color VSTGreen      = new Color(0.36f, 0.76f, 0.45f);
        public static readonly Color VSTRed        = new Color(0.74f, 0.06f, 0.17f);
        public static readonly Color VSTBlue       = new Color(0.32f, 0.70f, 0.76f);
        public static readonly Color VSTDarkBlue   = new Color(0.12f, 0.5f, 1f);
        public static readonly Color VSTOrange     = new Color(0.86f, 0.45f, 0.16f);
        public static readonly Color VSTYellow     = new Color(1f, 1f, 0f);
        public static readonly Color VSTLightColor = new Color(194/255f, 194/255f, 194/255f);
        public static readonly Color VSTDarkColor  = new Color(55/255f, 55/255f, 55/255f);
        
        private static readonly string   TexturePath           = "Assets/Voodoo/VoodooStore/Core/Textures/";
        public static readonly Texture2D UIBanner              = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Banner.png");
        public static readonly Texture2D UIplus                = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Plus.png");
        public static readonly Texture2D UIreturn              = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Return.png");
        public static readonly Texture2D UIdragDrop            = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "DragAndDrop.png");
        public static readonly Texture2D UIfolder              = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Folder.png");
        public static readonly Texture2D UIdownload            = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Download.png");
        public static readonly Texture2D UIvalidate            = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Validate.png");
        public static readonly Texture2D UIcross               = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Cross.png");
        public static readonly Texture2D UIrefresh             = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Refresh.png");
        public static readonly Texture2D UIDependency          = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Dependency.png");
        public static readonly Texture2D UIAlphabetical        = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "AlphabeticalOrder.png");
        public static readonly Texture2D UIAlphabeticalReverse = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "AlphabeticalOrderReverse.png");
        public static readonly Texture2D UIFold                = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Fold.png");
        public static readonly Texture2D UIUnfold              = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Unfold.png");
        public static readonly Texture2D UIQuestionMark        = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "QuestionMark.png");
        public static readonly Texture2D UIBug                 = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "bug.png");
        public static readonly Texture2D UISlack               = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "slack.png");
        public static readonly Texture2D UIMultiSelection      = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "layer.png");
        public static readonly Texture2D UISave                = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "save.png");
        public static readonly Texture2D UIFavorite            = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Resources/star.png");
        public static readonly Texture2D UIFavoriteEmpty       = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "starOutline.png");
        public static readonly Texture2D UIAddToCart           = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "addToCart.png");
        public static readonly Texture2D UICart                = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Resources/cart.png");
        public static readonly Texture2D UIManual              = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "hammer.png");
        public static readonly Texture2D UITrash               = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "trash.png");
        public static readonly Texture2D UIStatusAscending     = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "switch01.png");
        public static readonly Texture2D UIStatusDescending    = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "switch02.png");
        public static readonly Texture2D UIBorder              = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "border.png");
        public static readonly Texture2D UIRepair              = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Repair.png");
        public static readonly Texture2D UIGear                = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Gear.png");
        public static readonly Texture2D UICloud               = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath + "Cloud.png");
        
        public static readonly GUIStyle StyleBanner = new GUIStyle
        {
            margin    = new RectOffset(0, 0, 10, 10),
            alignment = TextAnchor.MiddleCenter
        };

        public static readonly GUIStyle StyleTitle = new GUIStyle
        { 
            normal   = { textColor = Color.white },
            fontSize = 50,
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false
        };

        public static readonly GUIStyle WrapStyleTitle = new GUIStyle
        { 
            normal   = { textColor = Color.white },
            fontSize = 50,
            alignment = TextAnchor.UpperLeft,
            wordWrap = true
        };

        public static readonly GUIStyle StyleNormal = new GUIStyle
        {
            normal   = { textColor = Color.white },
            fontSize = 20,
            wordWrap = true
        };

        public static readonly GUIStyle StyleSubTitle = new GUIStyle
        { 
            normal    = { textColor = Color.white },
            fontStyle = FontStyle.Italic,
            fontSize  = 15,
            wordWrap  = true
        };
        
        public static readonly GUIStyle StyleHeader = new GUIStyle
        {
            normal    = {textColor = Color.white},
            fontSize  = 20,
            alignment = TextAnchor.MiddleCenter
        };

        public static readonly GUIStyle StyleLabelsToolBar = new GUIStyle
        { 
            normal    = { textColor = Color.white},
            fontStyle = FontStyle.Italic,
            fontSize  = 12,
            wordWrap  = false,
        };

        public static readonly GUIStyle StyleLabels = new GUIStyle
        {
            normal    = { textColor = Color.white, background = UIBorder},
            fontStyle = FontStyle.Italic,
            fontSize  = 14,
            wordWrap  = false,
            alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(7, 7, 2, 2)
        };
        
        public static readonly GUIStyle StyleLabelsToolBarBox = new GUIStyle
        { 
            normal    = { textColor = Color.white, background = UIBorder },
            fontStyle = FontStyle.Italic,
            fontSize  = 12,
            wordWrap  = false,
            alignment = TextAnchor.MiddleCenter,
            border = new RectOffset(7, 7, 2, 2)
        };
        
        public static string GetEllipsisString(string text, GUIStyle style, Rect rect)
        {
            string ellipsisStr = "...";
            float width = style.CalcSize(new GUIContent(text)).x;

            if (width <= rect.width || rect.width < 3)
            {
                return text;
            }
            
            for (int i = text.Length; i >= 0; i--)
            {
                if (style.CalcSize(new GUIContent(text + ellipsisStr)).x <= rect.width)
                {
                    return text + ellipsisStr;
                }

                text = text.Remove(i - 1, 1);
            }

            return text;
        }

        public static void DisplayHelpButtons(float buttonSize, float space = 0f)
        {
            if (space != 0)
            {
                GUILayout.Space(space);
            }
            
            GUI.contentColor = VSTBlue;
            if (GUILayout.Button(UIQuestionMark, GUILayout.Height(buttonSize), GUILayout.Width(buttonSize)))
            {
                Application.OpenURL(PathHelper.vstPortal);
            }
            GUI.contentColor = Color.white;

            if (space != 0)
            {
                GUILayout.Space(space);
            }
            
            if (GUILayout.Button(UIBug, GUILayout.Height(buttonSize), GUILayout.Width(buttonSize)))
            {
                Application.OpenURL(PathHelper.serviceDeskSupport);
            }

            if (space != 0)
            {
                GUILayout.Space(space);
            }
            
            if (GUILayout.Button(UISlack, GUILayout.Height(buttonSize), GUILayout.Width(buttonSize)))
            {
                Application.OpenURL(PathHelper.slackSupport);
                //System.Diagnostics.Process.Start("slack.exe");
            }
        }

        public static void DrawInfiniteLine(Color color, bool isHorizontal = true, int thickness = 1, int padding = 10)
        {
            Rect rect;
            int halfPadding = padding >> 1;
            if (isHorizontal)
            {
                rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(thickness + padding));
                rect.height = thickness;
                rect.y += halfPadding;
            }
            else
            {
                rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(thickness + padding));
                rect.width  = thickness;
                rect.height = Screen.height - rect.y;
                rect.x     += halfPadding;
            }
            EditorGUI.DrawRect(rect, color);
        }

        public static void DrawLine(Color color, bool isHorizontal = true, float size = 100f, int thickness = 1, int padding = 10)
        {
            Rect rect;
            int halfPadding = padding >> 1;
            if (isHorizontal)
            {
                rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(thickness + padding));
                rect.height = thickness;
                rect.width = size;
                rect.y += halfPadding;
            }
            else
            {
                rect = EditorGUILayout.GetControlRect(false, GUILayout.Width(thickness + padding));
                rect.width  = thickness;
                rect.height = size;
                rect.x     += halfPadding;
            }
            EditorGUI.DrawRect(rect, color);
        }
    }
}