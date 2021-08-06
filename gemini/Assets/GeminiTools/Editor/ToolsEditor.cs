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
    namespace Gemini.Tools
    {
        ///<summary>
        /// Superclass of all tools editor windows, derived from the EditorWindow class.
        /// </summary>
        /// <value></value>
        public class ToolsEditor : EditorWindow
        {
            protected static SerializedObject so = null;
            
            public static int TotalNumberOfWindows = 4;

            ///<summary>
            /// Resizes a given window and reposition it centered in the middle of the editor.
            /// </summary>
            /// <value></value>
            public static void CustomEditorWindow(ToolsEditor window)
            {
                Rect main = EditorGUIUtility.GetMainWindowPosition();
                Rect pos = window.position;
                float centerWidth = (main.width - pos.width) * 0.5f;
                float centerHeight = (main.height - pos.height) * 0.2f;
                pos.x = main.x + centerWidth;
                pos.y = main.y + centerHeight;
                window.maxSize = new Vector2(600f, 300f);
                window.minSize = window.maxSize;
                window.position = pos;
            }
        }

        ///<summary>
        /// Welcome window, where the user selects the task he wishes the tool to accomplish.
        /// </summary>
        /// <value></value>
        public class StartWindow : ToolsEditor
        {
            private float _space = 20f;
            [SerializeField] string message = "Your MQTT message here";
            [SerializeField] string topic = "Your MQTT topic here";

            [MenuItem("Gemini Editor/Tools")]
            public static void GetEditorWindow()
            {
                StartWindow window = GetWindow<StartWindow>("Gemini Editor");
                CustomEditorWindow(window);
            }

            private void OnGUI()
            {
                GUILayout.Label("Welcome to Gemini Tools!", EditorStyles.largeLabel);
                GUILayout.Label("Test your project by sending a message (scene has to be playing).", EditorStyles.largeLabel);
                GUILayout.Space(_space);

                message = EditorGUILayout.TextField("Message", message);
                topic = EditorGUILayout.TextField("Topic", topic);

                GUILayout.Space(_space);

                if (GUILayout.Button("Next", GUILayout.Width(150)))
                {
                    this.Close();
                    EndWindow.GetEditorWindow();
                    Config.SendMessages(message, topic);
                }
            }
        }

        ///<summary>
        /// Final window.
        /// </summary>
        /// <value></value>
        public class EndWindow : ToolsEditor
        {
            public static void GetEditorWindow()
            {
                EndWindow window = GetWindow<EndWindow>("Gemini Tools");
                CustomEditorWindow(window);
            }

            private void OnGUI()
            {
                GUILayout.Label("Great job!", EditorStyles.largeLabel);
                GUILayout.Label("Now you can view your scene, and see data arriving at your mapped objects.", EditorStyles.largeLabel);

                GUILayout.Space(20f);

                if (GUILayout.Button("Back", GUILayout.Width(150)))
                {
                    this.Close();
                    StartWindow.GetEditorWindow();
                }

                if (GUILayout.Button("Close", GUILayout.Width(150)))
                {
                    this.Close();
                }
            }
        }
    }
#endif
