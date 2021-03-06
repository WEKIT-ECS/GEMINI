using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Gemini.Plugin;

namespace Gemini.Managers
{
    /// <summary>
    /// Allows for non-editable public fields with the ReadOnly descriptor.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    public class ReadOnlyAttribute : PropertyAttribute { }

    /// <summary>
    /// Manages configuration of the Gemini Plugin
    /// </summary>
    [ExecuteInEditMode]
    public class Config : MonoBehaviour
    {
        /// <summary>
        /// Store the name of the digital twin
        /// </summary>
        [Header("Project Settings")]
        [Tooltip("Your settings will be loaded from this InputDatabaseObject, in the Resources folder.")]
        [SerializeField] public InputDatabaseObject YourDatabaseObject;

        /// <summary>
        /// User configurable mapping of incoming data on topic+path to visualisation in a GameObject (for example, ProgressBar).
        /// </summary>
        [System.Serializable]
        public class SensorMapping
        {
            public string sensorName = "Please choose a sensorname from the list.";
            public string topic = "";
            public string path = "";
            public GameObject[] mappedObjects;
            public int GetId()
            {
                for (int i = 0; i < Config.GetSensorCount(); i++)
                {
                    SensorMapping map = Config.Instance.sensorMapping[i];
                    if (this.Equals(map)) return i;
                }
                return -1;
            }
        }

        // These get loaded from InputDatabaseObject
        [Header("Data Settings")]
        public SensorMapping[] sensorMapping;

        private static Config config;
        /// <summary>
        /// Immutable property of the type Config to make the configuration class singleton
        /// </summary>
        public static Config Instance
        {
            get
            {
                if (!config)
                {
                    config = FindObjectOfType(typeof(Config)) as Config;
                }
                return config;
            }
        }

        private IEnumerator coroutine;

        /// <summary>
        /// add a sensor to the mapping dictionary
        /// </summary>
        public void AddSensorMapping(SensorMapping map)
        {
            int len = sensorMapping.Length;
            SensorMapping[] newMappings = new SensorMapping[len + 1];
            for (int i = 0; i < len; i++)
            {
                newMappings[i] = sensorMapping[i];
            }
            newMappings[len] = map;
            sensorMapping = newMappings;
        }

        public static SensorMapping GetSensorMapping(GameObject obj)
        {
            if (obj == null)
            {
                Debug.Log("Object looking for Sensor Mapping is null.");
            }
            if (!Instance || Instance.sensorMapping == null)
            {
                Debug.Log("Config SensorMappings is null.");
                return null;
            }
            foreach (SensorMapping mapping in Instance.sensorMapping)
            {
                if (mapping.mappedObjects.Contains(obj))
                {
                    return mapping;
                }
            }
            if (Application.isEditor && !Application.isPlaying)
            {
                Debug.Log("Please map inputs to gameobjects: " + obj.name + " mapped id not found.");
            }
            Debug.Log("Search for " + obj.name + " in Config SensorMappings could not be completed.");
            return null;
        }
        
        public static List<SensorMapping> GetSensorMappings(String topic)
        {
            List<SensorMapping> result = new List<SensorMapping>();
            if (!Instance || Instance.sensorMapping == null)
            {
                return result;
            }
            foreach (SensorMapping map in Instance.sensorMapping)
            {
                if (map.topic.Equals(topic))
                {
                    result.Add(map);
                }
            }
            if (Application.isEditor && !Application.isPlaying)
            {
                Debug.Log("Please map inputs to gameobjects: " + topic + " not mapped.");
            }
            return result;
        }

        public static int GetMappingId(GameObject obj)
        {
            for (int i = 0; i < GetSensorCount(); i++)
            {
                if (Instance.sensorMapping[i].mappedObjects.Contains(obj))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get the count of the mapped sensors
        /// </summary>
        /// <returns>int:number of mapped sensor</returns>
        public static int GetSensorCount()
        {
            if (Instance == null)
            {
                return -1;
            }
            return Instance.sensorMapping.Length;
        }

        /// <summary>
        /// After receiving string msg from MqttClient, tries to find a mapped gameobject to trigger update events. 
        /// </summary>
        /// <param name="topic">MQTT topic, under which the message being resolved was received.</param>
        /// <param name="msg">Received message, encoded in UTF8.</param>
        public static void Resolve(string topic, string msg)
        {
            // Look for possible sensor mappings:
            // Get list of sensormappings under "topic"
            List<SensorMapping> mappings = GetSensorMappings(topic);

            // For each of them try the path in msg as JSON object:
            foreach (SensorMapping map in mappings)
            {
                // http://james.newtonking.com/archive/2014/02/01/json-net-6-0-release-1-%E2%80%93-jsonpath-and-f-support
                // https://goessner.net/articles/JsonPath/
                JObject o;
                JToken mappedValue;
                try { o = JObject.Parse(msg); }
                catch
                { 
                    Debug.Log("Could not parse " + msg + ".");
                    continue;
                }
                try { mappedValue = o.SelectToken(map.path); }
                catch
                {
                    Debug.Log("Could not find path " + map.path + " in " + msg + ".");
                    continue;
                }
                EventManager.TriggerEvent(map.GetId().ToString(), DateTimeOffset.Now.ToString() + "#" + mappedValue);
                Debug.Log("Resolving on topic: \"" + topic + "\"\n" + msg);
            }
        }

        // for debugging and Gemini Tools
        public static IEnumerator ResolveDelayed(string topic, string msg, float delay)
        {
            yield return new WaitForSeconds(delay);
            Resolve(topic, msg);
        }

        public static void SendMessages(string message, string topic)
        {
            for (int i = 1; i < 12; i++)
            {
                Instance.coroutine = ResolveDelayed(topic, message, i * 5f);
                Instance.StartCoroutine(Instance.coroutine);
            }
        }
    }
}
