using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColorCube : MonoBehaviour
{
    void Awake()
    {
        MeshRenderer[] mr = transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in mr)
            meshRenderer.material.color = Random.ColorHSV();
        
        // Quaternion rotation = Random.rotation;
        // transform.localRotation = rotation;

        // Vector3 scale = new Vector3(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
        // transform.localScale = scale;
    }
}
