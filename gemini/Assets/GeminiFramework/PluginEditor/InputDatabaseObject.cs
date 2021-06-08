using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Gemini.Plugin
{

    ///<summary>
    /// Scriptable object that saves all user defined settings in an asset.
    /// </summary>
    /// <value></value>
    public class InputDatabaseObject : ScriptableObject
    {

        ///<summary>
        /// Network protocol class that contains all relevant information for a MQTT connection.
        /// </summary>
        /// <value></value>
        [Serializable]
        public class MQTT
        {
            public string type = "MQTT";
            public string ipAddress = string.Empty;
            public string port = string.Empty;
            public List<string> topics = new List<string>();

            public MQTT() { }

            public MQTT(string add, string ipPort, List<string> topicsList)
            {
                ipAddress = add;
                port = ipPort;
                topics = new List<string>(topicsList);
            }
        }

        [Serializable]
        public class State
        {
            public string name = string.Empty;
            public string identifier = string.Empty;
        }

        [Serializable]
        public class Input
        {
            public List<State> states = new List<State>();
            public string name = string.Empty;
            public string stateRanges = string.Empty;
            public string topic = string.Empty;
            public string path = string.Empty;
            public string type = string.Empty;
        }

        public MQTT mqtt = new MQTT();
        [SerializeField] public string format = "JSON";
        public List<Input> inputList = new List<Input>();

        public static InputDatabaseObject LoadAsset(string path)
        {
            return (InputDatabaseObject)Resources.Load(path);
        }
        
        public List<string> GetTopics()
        {
            HashSet<string> inputTopics = new HashSet<string>();
            foreach (Input i in inputList)
            {
                inputTopics.Add(i.topic);
            }
            this.mqtt.topics = inputTopics.ToList();
            return inputTopics.ToList();
        }
    }
}
