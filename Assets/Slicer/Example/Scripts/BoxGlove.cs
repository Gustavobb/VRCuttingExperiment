using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxGlove : MonoBehaviour
{
    float _forceAppliedToCut = 2f;
    [SerializeField] List<Vector3> planeRotations = new List<Vector3>();
    [SerializeField] float cooldown = .5f;
    [SerializeField] float randomAngle = 10f;
    [SerializeField] float force = .2f;
    [SerializeField] LayerMask layer;
    Vector3 lastPosition;
    float time = 0;
    Cutter cutter;

    void Start()
    {
        cutter = FindObjectOfType<Cutter>();
    }

    void OnEnable()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if ((transform.position - lastPosition).magnitude > force && time <= 0f)
            TryBreak();

        lastPosition = transform.position;
        time -= Time.deltaTime;
    }

    void TryBreak()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, .4f, layer);
        for (int i = 0; i < planeRotations.Count; i++)
        {
            planeRotations[i] = new Vector3(planeRotations[i].x + Random.Range(-randomAngle, randomAngle), 
            planeRotations[i].y + Random.Range(-randomAngle, randomAngle), 
            planeRotations[i].z + Random.Range(-randomAngle, randomAngle));
        }
        
        foreach (Collider hit in hits)
        {
            SliceableData data = hit.gameObject.GetComponent<SliceableData>();
            if (data == null) continue;
            time = cooldown;
            GameObject[] meshes = cutter.Cut(data, planeRotations.ToArray());

            if (meshes == null) continue;
            for (int i = 0; i < meshes.Length; i++)
            {
                Rigidbody rigidbody = meshes[i].GetComponent<Rigidbody>();
                rigidbody.isKinematic = false;
                rigidbody.useGravity = true;
                Vector3 newNormal = (transform.position - lastPosition).normalized * Random.Range(_forceAppliedToCut * .5f, _forceAppliedToCut);
                newNormal = Quaternion.Euler(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f)) * newNormal;
                rigidbody.AddForce(newNormal, ForceMode.Impulse);
            }
        }
    }
}
