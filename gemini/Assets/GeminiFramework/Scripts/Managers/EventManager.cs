using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gemini.Managers
{
    /// <summary>
    /// UpdateEvent class to manage the events for the sensors data in the unity
    /// </summary>
    public class UpdateEvent : UnityEvent<string> {}
    /// <summary>
    /// EventManager class to manage the events for the sensors data in the unity
    /// </summary>
    public class EventManager : MonoBehaviour
    {  
        /// <summary>
        /// dictionary to store the events to update and manage it
        /// </summary>
        private Dictionary <string, UpdateEvent> eventDictionary;
        /// <summary>
        /// property to store the Singleton object of the event manager
        /// </summary>
        private static EventManager eventManager;
        /// <summary>
        /// Immutable EventManager instance of the singleton class
        /// </summary>
        public static EventManager Instance
        {
            get
            {
                if (!eventManager)
                {
                    eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;

                    if (!eventManager)
                    {
                        Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                    }
                    else
                    {
                        eventManager.Init(); 
                    }
                }

                return eventManager;
            }
        }
        /// <summary>
        /// Initialize the eventManager instance
        /// Start is called before the first frame update
        /// </summary>
        protected void Start()
        {
            Init();
        }
        /// <summary>
        /// Initialize the eventManager instance
        /// </summary>
        protected void Init ()
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<string, UpdateEvent>();
            }
        }

        /// <summary>
        /// Simple messaging system which will allow items in our projects to subscribe
        /// to events, and have events trigger actions in our games.
        /// </summary>
        /// <param name="eventName">string: name of the event</param>
        /// <param name="listener">UnityAction<string>:event listener</param>
        public static void StartListening (string eventName, UnityAction<string> listener)
        {
            UpdateEvent thisEvent = null;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(listener);
            } 
            else
            {
                thisEvent = new UpdateEvent ();
                thisEvent.AddListener(listener);
                Instance.eventDictionary.Add(eventName, thisEvent);
            }
        }
        
        /// <summary>
        /// Will stop the behavior enabled by StartListing function. 
        /// </summary>
        /// <param name="eventName">string: name of the event</param>
        /// <param name="listener">UnityAction<string>:event listener</param>
        public static void StopListening (string eventName, UnityAction<string> listener)
        {
            if (eventManager == null) return;
            UpdateEvent thisEvent = null;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener (listener);
            }
        }
        
        /// <summary>
        /// Use to trigger an event of type eventName.
        /// </summary>
        /// <param name="eventName">string: name of the event</param>
        /// <param name="val">string:trigger event by the value provided</param>
        public static void TriggerEvent (string eventName, string val)
        {
            UpdateEvent thisEvent = null;
            if (Instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(val);
            }
            else
            {
                foreach (string key in Instance.eventDictionary.Keys)
                {
                    Debug.Log(key + " was found, but not " + eventName+".");
                }
            }
        }
    }
}