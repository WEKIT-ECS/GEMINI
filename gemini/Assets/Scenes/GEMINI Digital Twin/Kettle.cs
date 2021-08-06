using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gemini.Managers;

public class Kettle : MonoBehaviour
{
    private KettleJoined kettleJoined;
    private Lid lid;
    private Manway manway;

    [SerializeField] public Material cold;
    [SerializeField] public Material hot;

    private void OnValidate()
    {
        kettleJoined = GetComponentInChildren<KettleJoined>();
        lid = GetComponentInChildren<Lid>();
        manway = GetComponentInChildren<Manway>();
    }

    void Start()
    {
        EventManager.StartListening(GetId().ToString(), UpdateValue);
    }

    void UpdateValue(string val)
    {
        string[] arr = val.Split('#');
        float value = float.Parse(arr[1], CultureInfo.InvariantCulture);
        if (value < 50)
        {
            kettleJoined.ChangeMaterial(cold);
            lid.ChangeMaterial(cold);
            manway.ChangeMaterial(cold);
        }
        else
        {
            kettleJoined.ChangeMaterial(hot);
            lid.ChangeMaterial(hot);
            manway.ChangeMaterial(hot);
        }
    }
    
    public int GetId()
    {
        return Config.GetMappingId(gameObject);
    }
}
