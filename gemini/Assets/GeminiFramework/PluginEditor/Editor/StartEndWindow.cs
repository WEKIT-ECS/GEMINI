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
        /// Welcome window, where the user can load an existing DT.
        /// </summary>
        /// <value></value>
        public class StartWindow : PluginEditor
        {
            private float _space = 20f;
            private static int _selected = 1; // 0=New, 1=Load
            [SerializeField] string dtName="YourDT";

            [MenuItem("Gemini Editor/Run")]
            public static void GetEditorWindow()
            {
                StartWindow window = GetWindow<StartWindow>("Gemini Editor");
                CustomEditorWindow(window);
            }

            private void OnGUI()
            {
                GUILayout.Label("Welcome to the Gemini Plugin!", EditorStyles.largeLabel);
                GUILayout.Label("The guide will assist you in configuring your Digital Twin successfully.", EditorStyles.largeLabel);
                GUILayout.Label("You now have the possibility to configure a new Digital Twin or to load an already created configuration from a file.", EditorStyles.largeLabel);
                GUILayout.Space(_space);

                string[] options = new string[] { "      New Digital Twin", "      Load from the configuration file" };
                _selected = GUILayout.SelectionGrid(_selected, options, 1, EditorStyles.radioButton);
                GUILayout.Space(_space);

                dtName = EditorGUILayout.TextField("Name of Digital Twin", dtName);

                if (GUILayout.Button("Next", GUILayout.Width(150)))
                {
                    loadFromSettings = _selected!=0;
                    inStartingWindow = false;
                    if (dtName.Equals(""))
                        dtName = "YourDT";
                    if (!loadFromSettings)
                        EditorPrefs.DeleteAll();
                    Config.Instance.Name_ofYourDigitalTwin = dtName;
                    CreateAsset();
                    SelectAsset();
                    this.Close();
                    NetworkProtocolEditor.GetEditorWindow();
                }

            }

            protected void OnEnable() //load preferences
            {
                // Here we retrieve the data if it exists or we save the default field initialisers we set above
                var data = EditorPrefs.GetString("StartWindow", JsonUtility.ToJson(this, false));
                // Then we apply them to this window
                JsonUtility.FromJsonOverwrite(data, this);
            }

            protected void OnDisable() //write preferences
            {
                // We get the Json data
                var data = JsonUtility.ToJson(this, false);
                // And we save it
                EditorPrefs.SetString("StartWindow", data);
            }
        }


        ///<summary>
        /// Final window.
        /// </summary>
        /// <value></value>
        public class EndWindow : PluginEditor
        {

            public static void GetEditorWindow()
            {
                SelectAsset();
                EndWindow window = GetWindow<EndWindow>("Gemini Editor");
                CustomEditorWindow(window);
            }

            private void OnGUI()
            {
                GUILayout.Label("Great job!", EditorStyles.largeLabel);
                GUILayout.Label("Now you can switch back to the user guide and start editing your scene.", EditorStyles.largeLabel);

                if (GUILayout.Button("Back", GUILayout.Width(150)))
                {
                    this.Close();
                    StateRepresentationEditor.GetEditorWindow();
                }

                if (GUILayout.Button("Close", GUILayout.Width(150)))
                {
                    this.Close();
                }
            }

            protected void OnEnable() //load preferences
            {
                // Here we retrieve the data if it exists or we save the default field initialisers we set above
                var data = EditorPrefs.GetString("Window6", JsonUtility.ToJson(this, false)); //shoud be the name of the window in ''
                // Then we apply them to this window
                JsonUtility.FromJsonOverwrite(data, this);
            }

            protected void OnDisable() //write preferences
            {
                // We get the Json data
                var data = JsonUtility.ToJson(this, false);
                // And we save it
                EditorPrefs.SetString("Window6", data);
            }
        }
    }
#endif
