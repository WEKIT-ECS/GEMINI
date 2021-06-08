﻿
using System;
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
            protected static SerializedObject so = null;
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

            ///<summary>
            /// Creates a new InputDatabaseObject asset used for storing the plugin settings.
            /// </summary>
            /// <value></value>
            public static void CreateAsset()
            {
                string currentAssetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                string registeredPath = Config.GetAssetPath();
                /* create new asset only if:
                    loadFromSettings=False (only set after first window passed and user selected a new asset to create)
                    or the registered asset file does not exist
                    else load existing asset
                */
                InputDatabaseObject asset = ScriptableObject.CreateInstance<InputDatabaseObject>();
                // if not load asset or file does not exist: create new asset
                if (!loadFromSettings || !System.IO.File.Exists(registeredPath)){
                    // if file exists, but create new selected: delete old asset
                    if (!loadFromSettings && System.IO.File.Exists(registeredPath))
                        AssetDatabase.DeleteAsset(registeredPath);
                    AssetDatabase.CreateAsset(asset, registeredPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                // load from settings=true and file exists
                else{
                    asset = (InputDatabaseObject) AssetDatabase.LoadAssetAtPath(registeredPath, typeof(InputDatabaseObject));
                }
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = asset;
            }

            ///<summary>
            /// Selects an InputDatabaseObject to be able to use in the windows (required for Reordable lists).
            /// </summary>
            /// <value></value>
            public static void SelectAsset()
            {
                var asset = AssetDatabase.LoadAssetAtPath(Config.GetAssetPath(), typeof(InputDatabaseObject));
                if (asset == null)
                {
                    CreateAsset();
                }
                else
                {
                    Selection.activeObject = asset;
                }
                so = new SerializedObject(Selection.activeObject as InputDatabaseObject);
            }

            ///<summary>
            /// Refocuses the asset when user clicks outside the editor window. Otherwise reordable list would not work.
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
