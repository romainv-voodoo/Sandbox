using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Voodoo.Utils
{
    public static class EditorHelper
    {
        private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static readonly string TexturePath = "Assets/Voodoo/EditorHelper/Textures/";

        private static void InitTextures()
        {
            textures = new Dictionary<string, Texture2D>
            {
                ["add"]          = LoadTexture2DAtPath("add.png"),
                ["bugReport"]    = LoadTexture2DAtPath("bug.png"),
                ["bullsEye"]     = LoadTexture2DAtPath("bullsEye.png"),
                ["cube"]         = LoadTexture2DAtPath("cube.png"),
                ["csv"]          = LoadTexture2DAtPath("csv.png"),
                ["delete"]       = LoadTexture2DAtPath("delete.png"),
                ["duplicate"]    = LoadTexture2DAtPath("duplicate.png"),
                ["error"]        = LoadTexture2DAtPath("error.png"),
                ["file"]         = LoadTexture2DAtPath("file.png"),
                ["fold"]         = LoadTexture2DAtPath("fold.png"),
                ["folder"]       = LoadTexture2DAtPath("folder.png"),
                ["json"]         = LoadTexture2DAtPath("json.png"),
                ["openFolder"]   = LoadTexture2DAtPath("openFolder.png"),
                ["questionMark"] = LoadTexture2DAtPath("questionMark.png"),
                ["save"]         = LoadTexture2DAtPath("save.png"),
                ["settings"]     = LoadTexture2DAtPath("settings.png"),
                ["squaredCross"] = LoadTexture2DAtPath("squaredCross.png"),
                ["squaredPlus"]  = LoadTexture2DAtPath("squaredPlus.png"),
                ["unfold"]       = LoadTexture2DAtPath("unfold.png"),
                ["warning"]      = LoadTexture2DAtPath("warning.png"),
            };
        }

        private static Texture2D LoadTexture2DAtPath(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexturePath}{fileName}");
        }

        public static readonly Color[] backgrounds = new Color[]
        {
            new Color(0.8f, 0.8f, 0.8f),
            new Color(0.5f, 0.5f, 0.5f)
        };
        
        public const float unitSize = 30f;

        public const float Tiny = unitSize;
        public const float Small = unitSize * 2;
        public const float Medium = unitSize * 4;
        public const float big = unitSize * 8;
        
        public static Texture2D GetTexture(string textureName)
        {
            Texture2D res;
            
            if (textures == null || textures.Count == 0)
            {
                InitTextures();
            }
            
            if (textures.ContainsKey(textureName))
            {
                res = textures[textureName];
            }
            else
            {
                res = LoadTexture2DAtPath(textureName);
                
                if (res == null)
                {
                    res = LoadTexture2DAtPath($"{textureName}.png");
                }
                
                if (res == null)
                {
                    res = LoadTexture2DAtPath($"{textureName}.jpg");
                }
                
                if (res != null)
                {
                    textures.Add(textureName, res);
                }
            }
            
            return res;
        }

        public static GUIContent GetIcon(string textureName)
        {
            GUIContent res = new GUIContent(textureName);
            Texture2D texture = GetTexture(textureName);
            
            if (texture != null)
            {
                res = new GUIContent(texture);
            }

            return res;
        }

        public static void MakePreviousElementClickable(Rect rect, Action OnClick = null, Action OnDoubleClick = null)
        {
            Rect previousRect = EditorGUI.IndentedRect(rect);
            GUIStyle buttonStyle = new GUIStyle();
            buttonStyle.active.background = new Texture2D(1, 1);
			
            Color[] pixels = buttonStyle.active.background.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0.804f, 0.804f, 0.804f, 0.05f);
            }
            buttonStyle.active.background.SetPixels(pixels);
			
            GUI.Box(previousRect, GUIContent.none, buttonStyle);
            Event current = Event.current;
            
            if (current.type == EventType.MouseDown && current.button == 0 && previousRect.Contains(current.mousePosition))
            {
                if (current.clickCount == 1)
                {
                    OnClick?.Invoke();
                }

                if (current.clickCount == 2)
                {
                    OnDoubleClick?.Invoke();
                }
                current.Use();
            }
        }

        /// <summary>
        /// Draws a line in the editor.
        /// </summary>
        /// <param name="height">Height.</param>
        public static void DrawLine(int height = 1)
        {
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 1));
        }

        public static void DrawLine(Color color, bool isHorizontal = true, int thickness = 2, int padding = 10)
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

        public static string GetEllipsisString(string text, GUIStyle style, Rect rect)
        {
            string ellipsisStr = "...";
            float width = style.CalcSize(new GUIContent(text)).x;

            if (width <= rect.width)
            {
                return text;
            }
            
            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (style.CalcSize(new GUIContent(text + ellipsisStr)).x <= rect.width)
                {
                    return text.Substring(0, i) + ellipsisStr;
                }

                text = text.Remove(i - 1, 1);
            }

            return text;
        }
    }
}