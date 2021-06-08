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
        /// Window that lets the user select their preferred message format.
        /// </summary>
        /// <value></value>
        public class MessageFormatEditor : PluginEditor
        {
            private float _space = 20f;
            private static int selected = 0; // 0=JSON, 1=XML

            public static void GetEditorWindow()
            {
                SelectAsset();
                MessageFormatEditor window = GetWindow<MessageFormatEditor>("Gemini PluginEditor");
                CustomEditorWindow(window);
            }

            private void OnGUI()
            {
                GUILayout.Label("Step 2/"+TotalNumberOfWindows, EditorStyles.largeLabel);
                GUILayout.Label("Now, select the message format you would like to use.", EditorStyles.largeLabel);
                GUILayout.Label("Currently, only JSON is supported.", EditorStyles.largeLabel);

                GUILayout.Space(_space);
                string[] options = new string[] { "      JSON"};
                selected = GUILayout.SelectionGrid(selected, options, 1, EditorStyles.radioButton);
                GUILayout.Space(_space);

                if (GUILayout.Button("Back", GUILayout.Width(150)))
                {
                    this.Close();
                    NetworkProtocolEditor.GetEditorWindow();
                }

                if (GUILayout.Button("Next", GUILayout.Width(150)))
                {
                    this.Close();
                    JsonEditor.GetEditorWindow();
                }

            }

            protected void OnEnable() //load preferences
            {
                // Here we retrieve the data if it exists or we save the default field initialisers we set above
                var data = EditorPrefs.GetString("MessageFormatEditor", JsonUtility.ToJson(this, false));
                // Then we apply them to this window
                JsonUtility.FromJsonOverwrite(data, this);
            }

            protected void OnDisable() //write preferences
            {
                // We get the Json data
                var data = JsonUtility.ToJson(this, false);
                // And we save it
                EditorPrefs.SetString("MessageFormatEditor", data);
            }
        }

    }
#endif
