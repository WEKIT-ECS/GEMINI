using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Collections.Generic;
using Gemini.Managers;

#if UNITY_EDITOR
    namespace Gemini.Plugin
    {
        public class StateRepresentationEditor : PluginEditor
        {
            private ReorderableList m_inputList = null;
            private ReorderableList m_statesList = null;
            
            public static void GetEditorWindow()
            {
                SelectAsset();
                StateRepresentationEditor window = GetWindow<StateRepresentationEditor>("Gemini Editor");
                CustomEditorWindow(window);
                window.SelectDatabaseObject(Selection.activeObject as InputDatabaseObject);
            }

            void OnSelectionChange()
            {
                var inputDatabaseObject = Selection.activeObject as InputDatabaseObject;
                if (inputDatabaseObject != null) SelectDatabaseObject(inputDatabaseObject);
            }

            ///<summary>
            /// This method selects an InputDatabaseObject and initializes a ReorderableList on the window according to its inputs.
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
                    m_inputList = new ReorderableList(inputDatabaseObject.inputList, typeof(InputDatabaseObject.Input), true, true, false, false);
                    m_inputList.onAddCallback += AddInput;
                    m_inputList.onRemoveCallback += RemoveInput;
                    m_inputList.onSelectCallback += SelectInput;
                    m_inputList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Inputs"); };
                    so = new SerializedObject(inputDatabaseObject);
                    m_inputList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                    {
                        var inputListSerializedState = so.FindProperty("inputList");
                        if (inputListSerializedState == null) { Debug.Log("inputList is null!"); return; }
                        if (!inputListSerializedState.isArray) { Debug.Log("inputList is not an array!"); return; }
                        if (!(0 <= m_inputList.index && m_inputList.index < inputListSerializedState.arraySize)) { Debug.Log("inputList[" + m_inputList.index + "] is outside array bounds!"); return; }
                        var inputSerializedState = inputListSerializedState.GetArrayElementAtIndex(index);
                        rect.y += 2;
                        EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width / 2, rect.height), inputSerializedState.FindPropertyRelative("name").stringValue);
                        EditorGUI.LabelField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height), inputSerializedState.FindPropertyRelative("stateRanges").stringValue);
                        UpdateNames();
                    };
                    if (m_inputList.count > 0)
                    {
                        m_inputList.index = 0;
                        SelectInput(m_inputList);
                    }
                }
                m_statesList = null;
                Repaint();
            }

            void OnGUI()
            {
                GUILayout.Label("Step 4/"+TotalNumberOfWindows, EditorStyles.largeLabel);
                GUILayout.Label("You can partition the possible values of sensor key/value pairs (specified in Step 3/4) into states.", EditorStyles.largeLabel);
                GUILayout.Label("- States are defined by a name and threshold value. (e.g. low_pressure, 20)", EditorStyles.largeLabel);
                GUILayout.Label("- The threshold value is the lowest upper bound to the state's value set.", EditorStyles.largeLabel);
                GUILayout.Space(20f);
                GUILayout.Label("To add new states, first, click on the respective input's name, and then on the plus.", EditorStyles.largeLabel);
                GUILayout.Space(20f);
                GUILayout.Label("Example:", EditorStyles.largeLabel);
                GUILayout.Label("state name: low_pressure | t_value: 20", EditorStyles.largeLabel);
                GUILayout.Space(20f);

                if (so != null && m_inputList != null)
                {
                    so.Update();
                    m_inputList.DoLayoutList();
                    if (m_statesList != null)
                    {
                        m_statesList.DoLayoutList();
                    }
                    so.ApplyModifiedProperties();
                }

                if (GUILayout.Button("Back", GUILayout.Width(100)))
                {
                    this.Close();
                    JsonEditor.GetEditorWindow();
                }

                if (GUILayout.Button("Next", GUILayout.Width(100)))
                {
                    CreatePrefabVariant();
                    this.Close();
                    EndWindow.GetEditorWindow();
                }

            }

            ///<summary>
            /// Add an input to the list.
            /// </summary>
            /// <value></value>
            void AddInput(ReorderableList inputList)
            {
                // When we add an input, select that input:
                inputList.list.Add(new InputDatabaseObject.Input());
                inputList.index = inputList.count - 1;
                SelectInput(inputList);
            }

            ///<summary>
            /// Delete an input from the list.
            /// </summary>
            /// <value></value>
            void RemoveInput(ReorderableList inputList)
            {
                // When we remove an input, clear the states list:
                ReorderableList.defaultBehaviours.DoRemoveButton(inputList);
                m_statesList = null;
                Repaint();
            }

            ///<summary>
            /// If an input is selected on the window, show a second ReordableList with its states.
            /// </summary>
            /// <value></value>
            void SelectInput(ReorderableList inputList)
            {
                // We when select an input, init the states list for that input:
                if (0 <= inputList.index && inputList.index < inputList.count)
                {
                    var input = inputList.list[inputList.index] as InputDatabaseObject.Input;
                    if (input != null)
                    {
                        m_statesList = new ReorderableList(input.states, typeof(string), true, false, true, true);
                        m_statesList.drawElementCallback = DrawProperty;
                        m_statesList.drawHeaderCallback = (Rect rect) =>
                        {
                            EditorGUI.LabelField(rect, "States");
                        };
                    }
                    Repaint();
                }
            }

            ///<summary>
            /// This method synchronizes all serialized entries on the InputDatabaseObject with the ones on the window.
            /// </summary>
            /// <value></value>
            void UpdateNames()
            {
                var inputListSerializedProperty = so.FindProperty("inputList");
                if (inputListSerializedProperty == null) { Debug.Log("inputList is null!"); return; }
                if (!inputListSerializedProperty.isArray) { Debug.Log("inputList is not an array!"); return; }
                
                for (int i = 0; i < inputListSerializedProperty.arraySize; i++)
                {
                    var itemSerializedProperty = inputListSerializedProperty.GetArrayElementAtIndex(i);
                    if (itemSerializedProperty == null) { Debug.Log("itemSerializedProperty[" + i + "] is null!"); return; }
                    
                    var statesListSerializedState = itemSerializedProperty.FindPropertyRelative("states");
                    if (statesListSerializedState == null) { Debug.Log("statesListSerializedState is null!"); return; }
                    if (!statesListSerializedState.isArray) { Debug.Log("statesListSerializedState is not an array!"); return; }
                    
                    List<(string,double)> t_values = new List<(string,double)>();

                    for (int j = 0; j < statesListSerializedState.arraySize; j++)
                    {
                        var stateSerializedState = statesListSerializedState.GetArrayElementAtIndex(j);
                        if (stateSerializedState == null) { Debug.Log("stateSerializedState[" + j + "] is null!"); return; }
                        try
                        {
                            t_values.Add(
                                (stateSerializedState.FindPropertyRelative("name").stringValue,
                                Double.Parse(stateSerializedState.FindPropertyRelative("identifier").stringValue))
                            );
                        }
                        catch
                        {
                            Debug.Log("t_value should be numerical!");
                        }
                    }
                    
                    t_values.Sort((x, y) => x.Item2.CompareTo(y.Item2));

                    // Updating sensor inputs name:
                    // string[] inputName = itemSerializedProperty.FindPropertyRelative("name").stringValue.Split(' ');
                    // string inputBaseName = inputName[0];
                    itemSerializedProperty.FindPropertyRelative("stateRanges").stringValue = TValuesToString(t_values);
                }
            }

            ///<summary>
            /// Maps states onto the name of an input.
            /// </summary>
            /// <value></value>
            private string TValuesToString (List<(string,double)> t_values)
            {
                string stateNames = " [";
                foreach ((string,double) t in t_values)
                {
                    stateNames += t.Item1 + " ; ";
                }
                return stateNames += "+INF]";
            }

            ///<summary>
            /// Draws the reorderable list using properties from the serialized object asset.
            /// </summary>
            /// <value></value>
            void DrawProperty(Rect rect, int index, bool isActive, bool isFocused)
            {
                // Added tons of debugging to help if you have issues:
                var inputListSerializedState = so.FindProperty("inputList");
                if (inputListSerializedState == null) { Debug.Log("inputList is null!"); return; }
                if (!inputListSerializedState.isArray) { Debug.Log("inputList is not an array!"); return; }
                if (!(0 <= m_inputList.index && m_inputList.index < inputListSerializedState.arraySize)) { Debug.Log("inputList[" + m_inputList.index + "] is outside array bounds!"); return; }
                
                // Configuring sensor input states at position index:
                if (0 <= m_inputList.index && m_inputList.index < inputListSerializedState.arraySize)
                {
                    var inputSerializedState = inputListSerializedState.GetArrayElementAtIndex(m_inputList.index);
                    if (inputSerializedState == null) { Debug.Log("inputSerializedState[" + m_inputList.index + "] is null!"); return; }

                    var statesListSerializedState = inputSerializedState.FindPropertyRelative("states");
                    if (statesListSerializedState == null) { Debug.Log("statesListSerializedState is null!"); return; }
                    if (!statesListSerializedState.isArray) { Debug.Log("statesListSerializedState is not an array!"); return; }

                    if (0 <= index && index < statesListSerializedState.arraySize)
                    {
                        var stateSerializedState = statesListSerializedState.GetArrayElementAtIndex(index);
                        if (stateSerializedState == null) { Debug.Log("stateSerializedState[" + index + "] is null!"); return; }

                        // If you have a custom state drawer, you can use StateField:
                        //---EditorGUI.StateField(rect, stateSerializedState);

                        // I didn't bother with one, so I just use TextField:
                        stateSerializedState.FindPropertyRelative("name").stringValue =
                            EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width / 2, rect.height),
                            stateSerializedState.FindPropertyRelative("name").stringValue);
                        stateSerializedState.FindPropertyRelative("identifier").stringValue =
                            EditorGUI.TextField(new Rect(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height),
                            stateSerializedState.FindPropertyRelative("identifier").stringValue);
                    }
                }
            }

            ///<summary>
            /// For each of the gathered inputs create a prefab variant of the ProgressBar that contains all its states.
            /// </summary>
            /// <value></value>
            public void CreatePrefabVariant()
            {
                // sensor mapping entry needs to be changed
                Config instance = Config.Instance;
                instance.sensorMapping = new Config.SensorMapping[0];

                var inputDatabaseObject = Selection.activeObject as InputDatabaseObject;
                GameObject barPrefabRef = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets/GeminiFramework/Prefabs/ProgressBar/ProgressBar.prefab");
                foreach (InputDatabaseObject.Input input in inputDatabaseObject.inputList)
                {                    
                    GameObject instanceRoot = (GameObject)PrefabUtility.InstantiatePrefab(barPrefabRef);
                    instanceRoot.GetComponent<ProgressBar>().states = input.states;
                    List < Color > sColors = new List<Color>();
                    foreach (InputDatabaseObject.State state in input.states)
                    {
                        sColors.Add(instanceRoot.GetComponent<ProgressBar>().BarColor);
                    }
                    instanceRoot.GetComponent<ProgressBar>().stateColors = sColors;
                    GameObject pVariant = PrefabUtility.SaveAsPrefabAsset(instanceRoot, "Assets/GeminiFramework/Prefabs/Generated/"+input.name+"PBar.prefab");

                    Config.SensorMapping mapping = new Config.SensorMapping();
                    mapping.sensorName = input.name;
                    mapping.topic = input.topic;
                    mapping.path = input.path;
                    mapping.mappedObject = instanceRoot;
                    instance.AddSensorMapping(mapping);
                }
            }
        }
    }
#endif
