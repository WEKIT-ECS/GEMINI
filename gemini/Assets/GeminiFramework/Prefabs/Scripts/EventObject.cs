using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gemini.Managers;

namespace Gemini
{
    /// <summary>
    /// Data visualization container.
    /// </summary>
    public abstract class EventObject : MonoBehaviour
    {
        protected int id;

        [Header("Data Setting")]
        public int RangeLow = 0;
        public int RangeHigh = 100;
        public int DefaultValue = 0;
        public string Unit;

        [Header("Title Setting")]
        public string Title;
        public Color TitleColor;
        public Font TitleFont;
        public int TitleFontSize = 10;

        protected string timestamp = DateTimeOffset.Now.ToString();
        
        public int GetId()
        {
            return Config.GetMappingId(gameObject);
        }
    }
}
