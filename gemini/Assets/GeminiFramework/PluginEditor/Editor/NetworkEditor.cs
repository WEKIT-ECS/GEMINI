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
        /// Window that lets the user configure his preferred network protocol.
        /// </summary>
        /// <value></value>
        public class NetworkProtocolEditor : PluginEditor
        {
            private float _space = 20f;
            [SerializeField] private string _mqttIpAddress = "";
            [SerializeField] private string _mqttPort = "";

            private ReorderableList reorderableTopicList = null;

            public static void GetEditorWindow()
            {
                SelectAsset();
                NetworkProtocolEditor window = GetWindow<NetworkProtocolEditor>("Gemini PluginEditor");
                CustomEditorWindow(window);
                window.SelectDatabaseObject(Selection.activeObject as InputDatabaseObject);
            }

            ///<summary>
            /// Custom selction method that sets up a reorderable list containing the MQTT options saved in the asset.
            /// </summary>
            /// <value></value>
            void SelectDatabaseObject(InputDatabaseObject inputDatabaseObject)
            {
                if (inputDatabaseObject == null)
                {
                    reorderableTopicList = null;
                    so = null;
                }
                else
                {
                    reorderableTopicList = new ReorderableList(inputDatabaseObject.mqtt.topics, typeof(string), true, true, true, true);
                    reorderableTopicList.onAddCallback += AddItem;
                    reorderableTopicList.onRemoveCallback += RemoveItem;
                    reorderableTopicList.onSelectCallback += SelectInput;
                    reorderableTopicList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Topics"); };
                    so = new SerializedObject(inputDatabaseObject);
                    if (reorderableTopicList.count > 0)
                    {
                        reorderableTopicList.index = 0;
                        SelectInput(reorderableTopicList);
                    }
                }
                Repaint();
            }

            private void OnGUI()
            {
                GUILayout.Label("Step 1/"+TotalNumberOfWindows, EditorStyles.largeLabel);
                GUILayout.Label("Configure your MQTT connection details here.", EditorStyles.largeLabel);
                GUILayout.Space(_space);

                var inputDatabaseObject = Selection.activeObject as InputDatabaseObject;
                if (inputDatabaseObject != null) so=null;
                so = new SerializedObject(inputDatabaseObject);
                if (so != null && reorderableTopicList != null)
                {
                    // update needs to be called to pull changes
                    so.Update();
                    var mqttProperty = so.FindProperty("mqtt");
                    GUILayout.Label("MQTT Details", EditorStyles.boldLabel);
                    mqttProperty.FindPropertyRelative("ipAddress").stringValue = EditorGUILayout.TextField("IP Address*", mqttProperty.FindPropertyRelative("ipAddress").stringValue);
                    _mqttIpAddress = mqttProperty.FindPropertyRelative("ipAddress").stringValue;
                    mqttProperty.FindPropertyRelative("port").stringValue = EditorGUILayout.TextField("Port*", mqttProperty.FindPropertyRelative("port").stringValue);
                    _mqttPort = mqttProperty.FindPropertyRelative("port").stringValue;
                    reorderableTopicList.DoLayoutList();
                    so.ApplyModifiedProperties();
                }
                Repaint();


                if (GUILayout.Button("Back", GUILayout.Width(150)))
                {
                    this.Close();
                    inStartingWindow = true;
                    StartWindow.GetEditorWindow();
                }

                if (GUILayout.Button("Next", GUILayout.Width(150)))
                {
                    if (!_mqttIpAddress.Trim().Equals(string.Empty) && !_mqttPort.Trim().Equals(string.Empty))
                    {
                        this.Close();
                        MessageFormatEditor.GetEditorWindow();
                    }
                }

                GUILayout.Space(_space);
                GUILayout.Label("* are mandatory", EditorStyles.boldLabel);

            }

            void AddItem(ReorderableList inputList)
            {
                // When we add an item, select that item:
                inputList.list.Add("");
                inputList.index = inputList.count - 1;
                SelectInput(inputList);
            }

            void RemoveItem(ReorderableList inputList)
            {
                // When we remove an item, clear the properties list:
                ReorderableList.defaultBehaviours.DoRemoveButton(inputList);
                Repaint();
            }

            void SelectInput(ReorderableList inputList)
            {
                // We when select an item, init the properties list for that item:
                // var item = inputList.list[inputList.index].stringValue;
                reorderableTopicList.drawElementCallback = DrawProperty;
                Repaint();
            }

            ///<summary>
            /// Draws the reorderable list using properties from the serialized object asset.
            /// </summary>
            /// <value></value>
            void DrawProperty(Rect rect, int index, bool isActive, bool isFocused)
            {
                var mqttProperty = so.FindProperty("mqtt");
                var topicListProperty = mqttProperty.FindPropertyRelative("topics");
                if (topicListProperty == null) { Debug.Log("topicListProperty is null!"); return; }
                if (!topicListProperty.isArray) { Debug.Log("topicListProperty is not an array!"); return; }
                if (!(0 <= reorderableTopicList.index && reorderableTopicList.index < topicListProperty.arraySize)) { Debug.Log("inputList[" + reorderableTopicList.index + "] is outside array bounds!"); return; }

                if (0 <= index && index < topicListProperty.arraySize){
                    var topicProperty = topicListProperty.GetArrayElementAtIndex(index);
                    if (topicProperty==null){
                        Debug.Log("topicProperty[" + index + "] is null!");
                        topicListProperty.InsertArrayElementAtIndex(index);
                        topicProperty = topicListProperty.GetArrayElementAtIndex(index);
                    }
                    if (topicProperty==null)
                        Debug.Log("topicProperty[" + index + "] is null!");
                    topicProperty.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width, rect.height), topicProperty.stringValue);
                }
            }

            protected void OnEnable() //load preferences
            {
                // Here we retrieve the data if it exists or we save the default field initialisers we set above
                var data = EditorPrefs.GetString("NetworkProtocolEditor", JsonUtility.ToJson(this, false));
                // Then we apply them to this window
                JsonUtility.FromJsonOverwrite(data, this);
            }

            protected void OnDisable() //write preferences
            {
                // We get the Json data
                var data = JsonUtility.ToJson(this, false);
                // And we save it
                EditorPrefs.SetString("NetworkProtocolEditor", data);
            }
        }

    }
#endif
