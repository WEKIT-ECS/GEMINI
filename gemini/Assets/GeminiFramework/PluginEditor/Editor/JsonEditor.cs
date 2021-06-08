﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.IO;
using Gemini.Managers;

#if UNITY_EDITOR
    namespace Gemini.Plugin
    {
        ///<summary>
        /// Window that lets the user specify inputs from the message format.
        /// </summary>
        /// <value></value>
        public class JsonEditor : PluginEditor
        {
            private ReorderableList m_inputList = null;

            public static void GetEditorWindow()
            {
                SelectAsset();
                JsonEditor window = GetWindow<JsonEditor>("Gemini Editor");
                CustomEditorWindow(window);
                window.SelectDatabaseObject(Selection.activeObject as InputDatabaseObject);
            }

            public void OpenWindow()
            {
                var asset = AssetDatabase.LoadAssetAtPath(Config.GetSettingsPath(), typeof(InputDatabaseObject));
                if (asset == null)
                {
                    CreateAsset();
                }
                else
                {
                    Selection.activeObject = asset;
                }
            }

            void OnSelectionChange()
            {
                var inputDatabaseObject = Selection.activeObject as InputDatabaseObject;
                if (inputDatabaseObject != null) SelectDatabaseObject(inputDatabaseObject);
            }

            ///<summary>
            /// Custom selction method that sets up a reorderable list containing the input options saved in the asset.
            /// </summary>
            /// <value></value>
            void SelectDatabaseObject(InputDatabaseObject inputDatabaseObject)
            {
                if (inputDatabaseObject == null)
                {
                    m_inputList = null;
                    so = null;
                }
                else
                {
                    m_inputList = new ReorderableList(inputDatabaseObject.inputList, typeof(InputDatabaseObject.Input), true, true, true, true);
                    m_inputList.onAddCallback += AddItem;
                    m_inputList.onRemoveCallback += RemoveItem;
                    m_inputList.onSelectCallback += SelectInput;
                    m_inputList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Input name | topic | path | type"); };
                    so = new SerializedObject(inputDatabaseObject);
                    if (m_inputList.count > 0)
                    {
                        m_inputList.index = 0;
                        SelectInput(m_inputList);
                    }
                }
                Repaint();
            }

            void OnGUI()
            {
                GUILayout.Label("Step 3/"+TotalNumberOfWindows, EditorStyles.largeLabel);
                GUILayout.Label("Now you can define the key/value pairs of your sensor recordings.", EditorStyles.largeLabel);
                GUILayout.Label("- A sensor recording is understood as a single message (encoding JSON or XML) \n received via your chosen network protocol.", EditorStyles.largeLabel);
                GUILayout.Label("- A sensor recording can contain multiple nested key/value pairs, identified by their topic and JSONPath.", EditorStyles.largeLabel);
                GUILayout.Label("- Should sensor recordings of different types share identical key/value pairs, their JSONPath \n and topics need to clearly identify them.", EditorStyles.largeLabel);
                GUILayout.Space(20f);
                GUILayout.Label("Specify the sensor key/value pairs in the fields:", EditorStyles.largeLabel);
                GUILayout.Label("input - A brief description of the associated value.", EditorStyles.largeLabel);
                GUILayout.Label("topic - The topic that messages are published with.", EditorStyles.largeLabel);
                GUILayout.Label("path - The key/value-pair's path in the JSON object, in JSONPath format. ($ : root object, arr[42] : element #43 in arr)", EditorStyles.largeLabel);
                GUILayout.Label("type - The value's type. (string, int, float)", EditorStyles.largeLabel);
                GUILayout.Space(20f);
                GUILayout.Label("Example:", EditorStyles.largeLabel);
                GUILayout.Label("{\n  \"boiling\" : {\n    \"kettle\" : {\n      \"pressure\" : 99\n    }\n  }\n}", EditorStyles.largeLabel);
                GUILayout.Label("name: pressure | topic: brewery/sensors | path: $.boiling.kettle.pressure | type: float", EditorStyles.largeLabel);
                GUILayout.Space(20f);

                if (so != null && m_inputList != null)
                {
                    so.Update();
                    m_inputList.DoLayoutList();
                    so.ApplyModifiedProperties();
                }

                if (GUILayout.Button("Back", GUILayout.Width(150)))
                {
                    this.Close();
                    MessageFormatEditor.GetEditorWindow();
                }

                if (GUILayout.Button("Next", GUILayout.Width(150)))
                {
                    this.Close();
                    StateRepresentationEditor.GetEditorWindow();
                }

            }

            void AddItem(ReorderableList inputList)
            {
                // When we add an item, select that item:
                inputList.list.Add(new InputDatabaseObject.Input());
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
                var item = inputList.list[inputList.index] as InputDatabaseObject.Input;
                m_inputList.drawElementCallback = DrawProperty;
                Repaint();
            }

            ///<summary>
            /// Draws the reorderable list using properties from the serialized object asset.
            /// </summary>
            /// <value></value>
            void DrawProperty(Rect rect, int index, bool isActive, bool isFocused)
            {
                // Added tons of debugging to help if you have issues:
                var inputListSerializedProperty = so.FindProperty("inputList");
                if (inputListSerializedProperty == null) { Debug.Log("inputList is null!"); return; }
                if (!inputListSerializedProperty.isArray) { Debug.Log("inputList is not an array!"); return; }
                if (!(0 <= m_inputList.index && m_inputList.index < inputListSerializedProperty.arraySize)) { Debug.Log("inputList[" + m_inputList.index + "] is outside array bounds!"); return; }

                if (0 <= index && index < inputListSerializedProperty.arraySize)
                {
                    var itemSerializedProperty = inputListSerializedProperty.GetArrayElementAtIndex(index);
                    if (itemSerializedProperty == null) { Debug.Log("itemSerializedProperty[" + m_inputList.index + "] is null!"); return; }

                    itemSerializedProperty.FindPropertyRelative("name").stringValue =
                            EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width / 4, rect.height),
                            itemSerializedProperty.FindPropertyRelative("name").stringValue);
                    itemSerializedProperty.FindPropertyRelative("topic").stringValue =
                        EditorGUI.TextField(new Rect(rect.x + rect.width / 4, rect.y, rect.width / 4, rect.height),
                        itemSerializedProperty.FindPropertyRelative("topic").stringValue);
                    itemSerializedProperty.FindPropertyRelative("path").stringValue =
                        EditorGUI.TextField(new Rect(rect.x + 2 * rect.width / 4, rect.y, rect.width / 4, rect.height),
                        itemSerializedProperty.FindPropertyRelative("path").stringValue);
                    itemSerializedProperty.FindPropertyRelative("type").stringValue =
                        EditorGUI.TextField(new Rect(rect.x + 3 * rect.width / 4, rect.y, rect.width / 4, rect.height),
                        itemSerializedProperty.FindPropertyRelative("type").stringValue);
                }
            }
        }
    }
#endif