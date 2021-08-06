﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Xml.Serialization;

using Gemini.Managers;

#if UNITY_EDITOR
    namespace Gemini.Plugin
    {
        ///<summary>
        /// Superclass of all custom editor windows, derived from the EditorWindow class.
        /// </summary>
        /// <value></value>
        public class PluginEditor : EditorWindow
        {
            protected static string path = null;
            protected static SerializedObject so = null;
            protected static InputDatabaseObject io = null;
            protected static bool loadFromSettings = false;
            protected static bool inStartingWindow = true;
            
            public static int TotalNumberOfWindows = 4;

            ///<summary>
            /// Resizes a given window and reposition it centered in the middle of the editor.
            /// </summary>
            /// <value></value>
            public static void CustomEditorWindow(PluginEditor window)
            {
                Rect main = EditorGUIUtility.GetMainWindowPosition();
                Rect pos = window.position;
                float centerWidth = (main.width - pos.width) * 0.5f;
                float centerHeight = (main.height - pos.height) * 0.2f;
                pos.x = main.x + centerWidth;
                pos.y = main.y + centerHeight;
                window.maxSize = new Vector2(750f, 750f);
                window.minSize = window.maxSize;
                window.position = pos;
            }

            /// <summary>
            /// Path to Resources folder.
            /// </summary>
            public static string GetResourcesPath()
            {
                return "Assets/Resources";
            }

            ///<summary>
            /// Creates a new InputDatabaseObject asset used for storing the plugin settings.
            /// </summary>
            /// <value></value>
            public static InputDatabaseObject CreateAsset()
            {
                string time = DateTime.Now.ToString().Replace(' ','_').Replace('.','-').Replace(':','-');
                path = GetResourcesPath() + "/YourDT_" + time + ".asset";
                InputDatabaseObject asset = ScriptableObject.CreateInstance<InputDatabaseObject>();
                if (System.IO.File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
                asset.path = path;
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.FocusProjectWindow();
                return asset;
            }

            ///<summary>
            /// Selects an InputDatabaseObject to be able to use in the windows (required for Reorderable lists).
            /// </summary>
            /// <value></value>
            public static void SelectAsset()
            {
                if (loadFromSettings && io != null)
                {
                    path = io.path;
                }
                var asset = AssetDatabase.LoadAssetAtPath(path, typeof(InputDatabaseObject));
                Selection.activeObject = asset;
            }

            ///<summary>
            /// Refocuses the asset when user clicks outside the editor window. Otherwise Reorderable list would not work.
            /// </summary>
            /// <value></value>
            public void OnFocus()
            {
                if (!inStartingWindow)
                    SelectAsset();
            }
        }

    }
#endif
