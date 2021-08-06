using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KettleJoined : MonoBehaviour
{
    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
    }

    public void ChangeMaterial(Material mat)
    {
        rend.sharedMaterial = mat;
    }
}
