/*
The MIT License (MIT)

Copyright (c) 2018 Giovanni Paolo Vigano'

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

using Gemini.Plugin;
using Gemini.Managers;

/// <summary>
/// Adaptation for Unity of the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// modified to run on UWP (also tested on Microsoft HoloLens).
/// </summary>
namespace Gemini.Network
{
    /// <summary>
    /// Generic MonoBehavior wrapping an M2MQTT client, using a double buffer to postpone message processing in the main thread. 
    /// </summary>
    public class MqttClient : MonoBehaviour
    {
        [Header("MQTT broker configuration")]
        [Tooltip("Use encrypted connection")]
        public bool IsEncrypted = false;
        [Header("Connection parameters")]
        [Tooltip("Connection to the broker is delayed by the the given milliseconds")]
        public int ConnectionDelay = 500;
        [Tooltip("Connection timeout in milliseconds")]
        public int TimeoutOnConnection = MqttSettings.MQTT_CONNECT_TIMEOUT;
        [Tooltip("Connect on startup")]
        public bool AutoConnect = false;
        
        /// <summary>
        /// Broker connection details.
        /// </summary>
        private string address;
        private int brokerPort;
        private List<string> topics;

        /// <summary>
        /// Wrapped M2MQTT client
        /// </summary>
        protected M2MqttClient client;

        private List<MqttMsgPublishEventArgs> messageQueue1 = new List<MqttMsgPublishEventArgs>();
        private List<MqttMsgPublishEventArgs> messageQueue2 = new List<MqttMsgPublishEventArgs>();
        private List<MqttMsgPublishEventArgs> frontMessageQueue = null;
        private List<MqttMsgPublishEventArgs> backMessageQueue = null;
        private bool mqttClientConnectionClosed = false;
        private bool mqttClientConnected = false;

        /// <summary>
        /// Event fired when a connection is successfully established
        /// </summary>
        public event Action ConnectionSucceeded;
        /// <summary>
        /// Event fired when failing to connect
        /// </summary>
        public event Action ConnectionFailed;

        /// <summary>
        /// Tries to load the settings from "Assets\Resources\YourDT.xml".
        /// </summary>
        /// <returns>InputDatabaseObject Settings</returns>
        private static string GetMqttIpAddress()
        {
            try
            {
                return Config.Instance.YourDatabaseObject.mqtt.ipAddress;
            }
            catch
            {
                Debug.Log("Network settings not found. Did you run the Gemini Editor?");
            }
            return "localhost";
        }

        /// <summary>
        /// Tries to load the settings from "Assets\Resources\YourDT.xml".
        /// </summary>
        /// <returns>InputDatabaseObject Settings</returns>
        private static int GetMqttPort()
        {
            try
            {
                return Int32.Parse(Config.Instance.YourDatabaseObject.mqtt.port);
            }
            catch
            {
                Debug.Log("Network settings not found. Did you run the Gemini Editor?");
            }
            return 1883;
        }

        /// <summary>
        /// Tries to load the settings from "Assets\Resources\YourDT.xml".
        /// </summary>
        /// <returns>InputDatabaseObject Settings</returns>
        private static List<string> GetMqttTopics()
        {
            try
            {
                return Config.Instance.YourDatabaseObject.GetTopics();
            }
            catch
            {
                Debug.Log("Network settings not found. Did you run the Gemini Editor?");
            }
            return new List<string>();
        }

        /// <summary>
        /// Connect to the broker using current settings.
        /// </summary>
        public void Connect()
        {
            if (client == null || !client.IsConnected)
            {
                StartCoroutine(DoConnect());
            }
        }

        /// <summary>
        /// Disconnect from the broker, if connected.
        /// </summary>
        public void Disconnect()
        {
            if (client != null)
            {
                StartCoroutine(DoDisconnect());
            }
        }

        /// <summary>
        /// Override this method to take some actions before connection (e.g. display a message)
        /// </summary>
        protected void OnConnecting()
        {
            Debug.LogFormat("Connecting to broker on {0}:{1}...\n", address, brokerPort.ToString());
        }

        /// <summary>
        /// Override this method to take some actions if the connection succeeded.
        /// </summary>
        protected void OnConnected()
        {
            Debug.LogFormat("Connected to {0}:{1}...\n", address, brokerPort.ToString());

            if (ConnectionSucceeded != null)
            {
                ConnectionSucceeded();
            }
            
            SubscribeTopics();
        }

        /// <summary>
        /// Override this method to take some actions if the connection failed.
        /// </summary>
        protected void OnConnectionFailed(string errorMessage)
        {
            Debug.LogWarning("Connection failed.");
            if (ConnectionFailed != null)
            {
                ConnectionFailed();
            }
        }

        /// <summary>
        /// Override this method to subscribe to MQTT topics.
        /// </summary>
        protected void SubscribeTopics()
        {
            if (topics != null)
            {
                foreach (string topic in topics)
                {
                    if (!String.IsNullOrEmpty(topic))
                    {
                        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                        Debug.Log("Subscribed to "+ topic);   
                    }
                }
            }
            else
            {
                Debug.Log("Please configure MQTT topic.");
            }
        }

        /// <summary>
        /// Override this method to unsubscribe to MQTT topics (they should be the same you subscribed to with SubscribeTopics() ).
        /// </summary>
        protected void UnsubscribeTopics()
        {
        }

        /// <summary>
        /// Disconnect before the application quits.
        /// </summary>
        protected void OnApplicationQuit()
        {
            CloseConnection();
        }

        /// <summary>
        /// Initialize MQTT message queue and start a new QueryResult object.
        /// Remember to call base.Awake() if you override this method.
        /// </summary>
        protected void Awake()
        {
            frontMessageQueue = messageQueue1;
            backMessageQueue = messageQueue2;
        }

        /// <summary>
        /// Connect on startup.
        /// </summary>
        protected void Start()
        {
            // before Connect(), address, port, and topics have to be set
            address = GetMqttIpAddress();
            brokerPort = GetMqttPort();
            topics = GetMqttTopics();
            
            Connect();
        }

        /// <summary>
        /// Override this method for each received message you need to process.
        /// </summary>
        protected void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            
            Debug.Log("Incoming message on topic: \"" + topic + "\"\n" + msg);
            Config.Resolve(topic,msg);
        }

        /// <summary>
        /// Override this method to take some actions when disconnected.
        /// </summary>
        protected void OnDisconnected()
        {
            Debug.Log("Disconnected.");
        }

        /// <summary>
        /// Override this method to take some actions when the connection is closed.
        /// </summary>
        protected void OnConnectionLost()
        {
            Debug.LogWarning("CONNECTION LOST!");
        }

        /// <summary>
        /// Processing of income messages and events is postponed here in the main thread.
        /// Remember to call ProcessMqttEvents() in Update() method if you override it.
        /// </summary>
        protected void Update()
        {
            ProcessMqttEvents();
        }

        protected void ProcessMqttEvents()
        {
            // process messages in the main queue
            SwapMqttMessageQueues();
            ProcessMqttMessageBackgroundQueue();
            // process messages income in the meanwhile
            SwapMqttMessageQueues();
            ProcessMqttMessageBackgroundQueue();

            if (mqttClientConnectionClosed)
            {
                mqttClientConnectionClosed = false;
                OnConnectionLost();
            }
        }

        private void ProcessMqttMessageBackgroundQueue()
        {
            foreach (MqttMsgPublishEventArgs msg in backMessageQueue)
            {
                DecodeMessage(msg.Topic, msg.Message);
            }
            backMessageQueue.Clear();
        }

        /// <summary>
        /// Swap the message queues to continue receiving message when processing a queue.
        /// </summary>
        private void SwapMqttMessageQueues()
        {
            frontMessageQueue = frontMessageQueue == messageQueue1 ? messageQueue2 : messageQueue1;
            backMessageQueue = backMessageQueue == messageQueue1 ? messageQueue2 : messageQueue1;
        }

        private void OnMqttMessageReceived(object sender, MqttMsgPublishEventArgs msg)
        {
            frontMessageQueue.Add(msg);
        }

        private void OnMqttConnectionClosed(object sender, EventArgs e)
        {
            // Set unexpected connection closed only if connected (avoid event handling in case of controlled disconnection)
            mqttClientConnectionClosed = mqttClientConnected;
            mqttClientConnected = false;
        }

        /// <summary>
        /// Connects to the broker using the current settings.
        /// </summary>
        /// <returns>The execution is done in a coroutine.</returns>
        private IEnumerator DoConnect()
        {
            // wait for the given delay
            yield return new WaitForSecondsRealtime(ConnectionDelay / 1000f);
            // leave some time to Unity to refresh the UI
            yield return new WaitForEndOfFrame();

            // create client instance 
            if (client == null)
            {
                try
                {
#if (!UNITY_EDITOR && UNITY_WSA_10_0 && !ENABLE_IL2CPP)
                    client = new M2MqttClient(address, brokerPort, IsEncrypted, IsEncrypted ? MqttSslProtocols.SSLv3 : MqttSslProtocols.None);
#else
                    client = new M2MqttClient(address, brokerPort, IsEncrypted, null, null, IsEncrypted ? MqttSslProtocols.SSLv3 : MqttSslProtocols.None);
                    //System.Security.Cryptography.X509Certificates.X509Certificate cert = new System.Security.Cryptography.X509Certificates.X509Certificate();
                    //client = new MqttClient(Address, brokerPort, isEncrypted, cert, null, MqttSslProtocols.TLSv1_0, MyRemoteCertificateValidationCallback);
#endif
                }
                catch (Exception e)
                {
                    client = null;
                    Debug.LogErrorFormat("CONNECTION FAILED! {0}", e.ToString());
                    OnConnectionFailed(e.Message);
                    yield break;
                }
            }
            else if (client.IsConnected)
            {
                yield break;
            }
            OnConnecting();

            // leave some time to Unity to refresh the UI
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            client.Settings.TimeoutOnConnection = TimeoutOnConnection;
            string clientId = Guid.NewGuid().ToString();
            try
            {
                client.Connect(clientId, null, null); // MQTT username and password would go here
            }
            catch (Exception e)
            {
                client = null;
                Debug.LogErrorFormat("Failed to connect to {0}:{1}:\n{2}", address, brokerPort, e.ToString());
                OnConnectionFailed(e.Message);
                yield break;
            }
            if (client.IsConnected)
            {
                client.ConnectionClosed += OnMqttConnectionClosed;
                // register to message received 
                client.MqttMsgPublishReceived += OnMqttMessageReceived;
                mqttClientConnected = true;
                OnConnected();
            }
            else
            {
                OnConnectionFailed("CONNECTION FAILED!");
            }
        }

        private IEnumerator DoDisconnect()
        {
            yield return new WaitForEndOfFrame();
            CloseConnection();
            OnDisconnected();
        }

        private void CloseConnection()
        {
            mqttClientConnected = false;
            if (client != null)
            {
                if (client.IsConnected)
                {
                    UnsubscribeTopics();
                    client.Disconnect();
                }
                client.MqttMsgPublishReceived -= OnMqttMessageReceived;
                client.ConnectionClosed -= OnMqttConnectionClosed;
                client = null;
            }
        }

#if ((!UNITY_EDITOR && UNITY_WSA_10_0))
        private void OnApplicationFocus(bool focus)
        {
            // On UWP 10 (HoloLens) we cannot tell whether the application actually got closed or just minimized.
            // (https://forum.unity.com/threads/onapplicationquit-and-ondestroy-are-not-called-on-uwp-10.462597/)
            if (focus)
            {
                Connect();
            }
            else
            {
                CloseConnection();
            }
        }
#endif
    }
}
