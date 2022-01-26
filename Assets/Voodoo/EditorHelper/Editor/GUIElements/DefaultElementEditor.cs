using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Voodoo.Utils
{
    //TODO : make it DataGUIExecutor or IWidget so that it can be called similarly to the other inspectors
    public static class DefaultElementEditor
    {
        public static T OnGUI<T>(T value, string label = null)
        {
            //TODO later on offer the possibilities to check for known predefine specific editors

            return (T)OnGUI(typeof(T), value, label);
        }

        public static object OnGUI(Type type, object value, string label = null)
        {
            if (type.IsValueType) // type valeur primitive + custom structs
            {
                if (type.IsPrimitive)
                {
                    return DrawPrimitives(type, value, label);
                }
                else if (type.IsEnum)
                {
                    return EditorGUILayout.Popup((int)value, Enum.GetNames(type));
                }

                var unityValueType = DrawUnityValueTypes(type, value, label);
                if (unityValueType != null)
                {
                    return unityValueType;
                }

                // if it's a custom struct do nothing we want to run through all fields just like a class
            }

            if (type.Equals(typeof(string)))
            {
                return EditorGUILayout.TextField(label, value as string);
            }

            object valueCopy = value?.DeepCopy();

            //Past this point value can be null therefore check for nullity and instanciate them if need be
            if (valueCopy == null)
            {
                valueCopy = Instantiate(type);            
            }

            if (typeof(ICollection).IsAssignableFrom(type))
            {
                // All kind of Collection
                return DrawCollection(type, valueCopy as IList, label);
            }

            // reaching this point it is most certainly a class 
            /*
             * we will parcour each and individual fields to give tha adapted editor for each one 
             */
            return DrawReference(type, valueCopy, label);
        }

        static object DrawPrimitives(Type type, object value, string label = "")
        {
            if (type.Equals(typeof(int)))
            {
                return EditorGUILayout.IntField(label, (int)value);
            }

            if (type.Equals(typeof(long)))
            {
                return EditorGUILayout.LongField(label, (long)value);
            }

            if (type.Equals(typeof(float)))
            {
                return EditorGUILayout.FloatField(label, (float)value);
            }

            if (type.Equals(typeof(double)))
            {
                return EditorGUILayout.DoubleField(label, (double)value);
            }

            if (type.Equals(typeof(bool)))
            {
                return EditorGUILayout.Toggle(label, (bool)value);
            }

            EditorGUILayout.LabelField(label + " of type " + (value?.GetType().FullName ?? "unknown") + " is not know to default editor");

            return null;
        }

        static object DrawUnityValueTypes(Type type, object value, string label = "")
        {
            if (type.Equals(typeof(Color)))
            {
                return EditorGUILayout.ColorField(label, (Color)value);
            }

            if (type.Equals(typeof(Color32)))
            {
                return EditorGUILayout.ColorField(label, (Color32)value);
            }

            if (type.Equals(typeof(Vector2)))
            {
                return EditorGUILayout.Vector2Field(label, (Vector2)value);
            }

            if (type.Equals(typeof(Vector3)))
            {
                return EditorGUILayout.Vector3Field(label, (Vector3)value);
            }

            return null;
        }

        static object Instantiate(Type type)
        {
            if (type.IsArray)
            {
                Type itemsType = type.GetIEnumerableElementType();
                return Array.CreateInstance(itemsType, 0);
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }

        static List<FieldInfo> GetFieldsRecursively(Type type)
        {
            Type baseType = type;
            List<FieldInfo> fields = new List<FieldInfo>();

            fields.AddRange(baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            while (baseType.BaseType != null && baseType.BaseType != typeof(Object))
            {
                baseType = baseType.BaseType;
                fields.InsertRange(0, baseType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            }

            return fields;
        }

        static object DrawCollection(Type type, IList list, string label = null)
        {
            Type itemsType = type.GetIEnumerableElementType();

            DrawCollectionHeader(itemsType, ref list, label);

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            {
                DrawItems(itemsType, ref list);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            return list;
        }

        static void DrawCollectionHeader(Type itemsType, ref IList list, string label = null)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label);

                if (GUILayout.Button(EditorHelper.GetIcon("add"), GUIStyle.none, GUILayout.Width(16f), GUILayout.Height(16f)))
                {
                    if (list.IsFixedSize)
                    {
                        var array = Array.CreateInstance(itemsType, list.Count + 1);
                        list.CopyTo(array, 0);
                        list = array;
                    }
                    else
                    {
                        list.Add(Activator.CreateInstance(itemsType));
                    }
                }

            }
            EditorGUILayout.EndHorizontal();
        }

        static void DrawItems(Type itemsType, ref IList list)
        {
            int indexToRemove = -1;
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("Box");
                {
                    if (GUILayout.Button(EditorHelper.GetIcon("delete"), GUIStyle.none, GUILayout.Width(16f), GUILayout.Height(16f)))
                    {
                        indexToRemove = i;
                    }

                    EditorGUIUtility.labelWidth -= 23f;
                    EditorGUILayout.BeginVertical();
                    list[i] = OnGUI(itemsType, list[i], "Item " + i.ToString());
                    EditorGUILayout.EndVertical();
                    EditorGUIUtility.labelWidth += 23f;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (indexToRemove >= 0)
            {
                if (list.IsFixedSize)
                {
                    var array = Array.CreateInstance(itemsType, list.Count - 1);
                    int offset = 0;
                    for (int i = 0; i + offset < array.Length; i++)
                    {
                        if (i == indexToRemove)
                        {
                            offset++;
                        }

                        array.SetValue(list[i + offset], i);
                    }

                    list = array;
                }
                else
                {
                    list.RemoveAt(indexToRemove);
                }
            }
        }

        static object DrawReference(Type type, object value, string label = null)
        {
            List<FieldInfo> fields = GetFieldsRecursively(value.GetType());
            for (int i = 0; i < fields.Count; i++)
            {
                object fieldValue = OnGUI(fields[i].FieldType, fields[i].GetValue(value), fields[i].Name);
                fields[i].SetValue(value, fieldValue);
            }

            return value;
        }
    }
}