using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverController : MonoBehaviour
{
    float smoothTime = 0.1f;
    Vector3 velocity = Vector3.zero;
    GameObject sliceable;
    Rigidbody sliceableRigidbody;

    public void Update()
    {
        if (sliceable != null)
        {   
            sliceable.transform.position = Vector3.SmoothDamp(sliceable.transform.position, transform.position, ref velocity, smoothTime);
            sliceable.transform.rotation *= Quaternion.Euler(0, 15f * Time.deltaTime, 0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Sliceable" || sliceable != null)
            return;

        sliceable = other.gameObject;
        sliceableRigidbody = sliceable.GetComponent<Rigidbody>();
        sliceableRigidbody.isKinematic = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject != sliceable)
            return;
        
        sliceable = null;
    }

    void OnDisable()
    {
        sliceable = null;
    }
}
