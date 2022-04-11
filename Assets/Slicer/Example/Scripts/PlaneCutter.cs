using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCutter : MonoBehaviour
{
    List<SliceableData> toSlice = new List<SliceableData>();
    Cutter cutter;

    void Start()
    {
        cutter = FindObjectOfType<Cutter>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (SliceableData sliceable in toSlice)
                cutter.Cut(sliceable, new Vector3[] { transform.rotation.eulerAngles });

            toSlice.Clear();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        SliceableData data = other.gameObject.GetComponent<SliceableData>();
        if (data != null)
            toSlice.Add(data);
    }

    void OnTriggerExit(Collider other)
    {
        SliceableData data = other.gameObject.GetComponent<SliceableData>();
        if (data != null)
            toSlice.Remove(data);
    }
}
