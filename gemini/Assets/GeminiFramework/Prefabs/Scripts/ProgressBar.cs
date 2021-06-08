using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Gemini.Managers;
using Gemini.Plugin;

namespace Gemini
{
    [ExecuteInEditMode]
    public class ProgressBar : EventObject
    {
        private Text title;
        private float barValue;
        private Image progressBar;

        public Color BarColor;
        public List<InputDatabaseObject.State> states;
        public List<Color> stateColors;

        void Awake()
        {
            progressBar = transform.Find("BarImage").GetComponent<Image>();
            title = transform.Find("Text").GetComponent<Text>();
        }

        // Start is called before the first frame update
        void Start()
        {
            id = GetId();
            EventManager.StartListening(id.ToString(), UpdateValue);

            title.text = Title;
            title.color = TitleColor;
            title.font = TitleFont;
            title.fontSize = TitleFontSize;

            progressBar.color = BarColor;
            barValue = DefaultValue;

            DrawValue(barValue);
        }

        // Update is called once per frame
        void Update()
        {
            if (!Application.isPlaying)
            {
                DrawValue(DefaultValue);
                title.color = TitleColor;
                title.font = TitleFont;
                title.fontSize = TitleFontSize;

                progressBar.color = BarColor;
            }
        }

        /// <summary>
        /// Updated the bar depending on the value.
        /// <param name="val">"TIMESTAMP#float"</param>
        /// </summary>
        protected void UpdateValue(string val)
        {
            string[] arr = val.Split('#');
            timestamp = arr[0];
            barValue = float.Parse(arr[1], CultureInfo.InvariantCulture);
            DrawValue(barValue);
        }

        /// <summary>
        /// Used to manage the filling of the progressBar. Depending on the value of "val", the circle is filled. 
        /// </summary>
        private void DrawValue(float val)
        {
            barValue = Mathf.Clamp(val, RangeLow, RangeHigh);
            progressBar.fillAmount = (val + (-1) * RangeLow) / (RangeHigh - RangeLow);
            title.text = (Title != "" ? Title : Config.GetSensorMapping(gameObject).sensorName) + "\n" + timestamp + "\n" + val + " " + Unit;

            progressBar.color = BarColor;
            for (int i = 0; i < states.Count; i++)
            {
                // first element: skip if value bigger than threshold
                if (i == 0 && val <= float.Parse(states[i].identifier))
                {
                    progressBar.color = stateColors[i];
                    break;
                }
                // last element: threshold for value higher than it, below use default color
                else if (i == states.Count - 1 && float.Parse(states[i].identifier) < val)
                {
                    progressBar.color = stateColors[i];
                    break;
                }
                // state identified
                else if (i > 0 && float.Parse(states[i - 1].identifier) < val && val <= float.Parse(states[i].identifier))
                {
                    progressBar.color = stateColors[i];
                }
            }
        }

        public void OnDisable()
        {
            EventManager.StopListening(id.ToString(), UpdateValue);
        }
    }
}

