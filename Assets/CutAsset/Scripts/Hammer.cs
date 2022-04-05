using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts;

public class Hammer : MonoBehaviour
{
    float _forceAppliedToCut = 4f;
    [SerializeField] List<Vector3> planeRotations = new List<Vector3>();
    [SerializeField] float cooldown = .5f;
    [SerializeField] float randomAngle = 10f;
    [SerializeField] float force = .2f;
    [SerializeField] LayerMask layer;
    Vector3 lastPosition;
    float time = 0;

    void OnEnable()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if ((transform.position - lastPosition).magnitude > force && time <= 0f)
            CastSphereHammer();

        lastPosition = transform.position;
        time -= Time.deltaTime;
    }

    void CastSphereHammer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, .4f, layer);
        foreach (Collider hit in hits)
        {
            print(hit.gameObject.tag);
            if (hit.gameObject.tag != "Sliceable") return;
            time = cooldown;
            Plane plane = new Plane();
            Vector3 transformedNormal, sliceableCenter;
            Quaternion rotation;
            List<GameObject> toSlice = new List<GameObject>();
            toSlice.Add(hit.gameObject);

            List<GameObject> toSliceCopy;
            
            for (int i = 0; i < planeRotations.Count; i++)
            {
                rotation = Quaternion.Euler(planeRotations[i] + new Vector3(Random.Range(-randomAngle, randomAngle), Random.Range(-randomAngle, randomAngle), Random.Range(-randomAngle, randomAngle)));
                transformedNormal = rotation * new Vector3(0, 1, 0);

                var direction = Vector3.Dot(Vector3.up, transformedNormal);
                if (direction < 0)
                    plane = plane.flipped;

                toSliceCopy = new List<GameObject>(toSlice);
                foreach (GameObject sliceable in toSliceCopy)
                {
                    sliceableCenter = sliceable.GetComponent<Renderer>().bounds.center;

                    plane.SetNormalAndPosition(transformedNormal, sliceable.transform.InverseTransformPoint(sliceableCenter));
                    GameObject[] slices = Slicer.Slice(plane, sliceable);

                    foreach (GameObject slice in slices)
                    {
                        if (slice.GetComponent<MeshFilter>().sharedMesh.vertices.Length == 0)
                        {
                            foreach (GameObject obj in slices)
                                Destroy(obj);
                        }

                        else
                        {
                            Rigidbody rigidbody = slices[1].GetComponent<Rigidbody>();
                            Vector3 newNormal = transformedNormal + (transform.position - lastPosition).normalized * _forceAppliedToCut;
                            rigidbody.AddForce(newNormal, ForceMode.Impulse);
                            toSlice.Add(slice);
                            toSlice.Remove(sliceable);
                            Destroy(sliceable);
                        }
                    }
                }
            }
        }
    }
}
