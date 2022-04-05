using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class Sliceable : MonoBehaviour
{
    [SerializeField]
    private bool _isSolid = true;

    [SerializeField]
    private bool _reverseWindTriangles = false;

    [SerializeField]
    private bool _useGravity = false;

    [SerializeField]
    private bool _shareVertices = false;

    [SerializeField]
    private bool _smoothVertices = false;

    [SerializeField] Transform _parent;
    public Joiner joiner;
    float smoothTime = 0.6f;
    float timeCount = 0.0f;
    Vector3 velocity = Vector3.zero;
    bool joined = false;
    Rigidbody rb;

    void Start()
    {
        transform.parent = _parent;
        joiner.OnJoin += Join;
        joiner.OnJoinStart += JoinStart;
        joiner.OnJoinEnd += JoinEnd;
        joiner.OnJoinSuccess += JoinSuccess;
        rb = GetComponent<Rigidbody>();
    }

    void OnDestroy()
    {
        joiner.OnJoin -= Join;
        joiner.OnJoinStart -= JoinStart;
        joiner.OnJoinEnd -= JoinEnd;
        joiner.OnJoinSuccess -= JoinSuccess;
    }

    void FindPCDCenterOfMass()
    {
        MeshRenderer[] mr = transform.GetComponentsInChildren<MeshRenderer>();
        Vector3 minBounds = mr[0].bounds.min;
        Vector3 maxBounds = mr[0].bounds.max;

        foreach (MeshRenderer meshRenderer in mr)
        {
            if (meshRenderer.bounds.min.x < minBounds.x)
                minBounds.x = meshRenderer.bounds.min.x;
            else if (meshRenderer.bounds.max.x > maxBounds.x)
                maxBounds.x = meshRenderer.bounds.max.x;

            if (meshRenderer.bounds.min.y < minBounds.y)
                minBounds.y = meshRenderer.bounds.min.y;
            else if (meshRenderer.bounds.max.y > maxBounds.y)
                maxBounds.y = meshRenderer.bounds.max.y;

            if (meshRenderer.bounds.min.z < minBounds.z)
                minBounds.z = meshRenderer.bounds.min.z;
            else if (meshRenderer.bounds.max.z > maxBounds.z)
                maxBounds.z = meshRenderer.bounds.max.z;
        }

        float centerX = (maxBounds.x + minBounds.x) / 2;
        float centerY = (maxBounds.y + minBounds.y) / 2;
        float centerZ = (maxBounds.z + minBounds.z) / 2;

        GameObject p = new GameObject();
        p.transform.position = new Vector3(centerX, centerY, centerZ);
        transform.parent = p.transform;
    }

    void Update()
    {
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, 3f);
    }

    void JoinStart()
    {
        timeCount = 0.0f;
        joined = false;
        rb.isKinematic = true;
    }

    void JoinSuccess()
    {
        Destroy(gameObject);
    }

    void JoinEnd()
    {
        rb.velocity = Vector3.zero;
        timeCount = 0.0f;
        rb.isKinematic = false;
    }

    void JoinParent()
    {
        joiner.JoinCount++;
        joined = true;
    }

    void Join()
    {
        if (joined)
            return;

        transform.position = Vector3.SmoothDamp(transform.position, _parent.transform.position, ref velocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, _parent.transform.rotation, timeCount * (1f - smoothTime));
        timeCount += Time.deltaTime;

        if (timeCount > 1.0f && (transform.position - _parent.transform.position).magnitude < 0.001f)
            JoinParent();
    }

    public bool IsSolid
    {
        get
        {
            return _isSolid;
        }
        set
        {
            _isSolid = value;
        }
    }

    public bool ReverseWireTriangles
    {
        get
        {
            return _reverseWindTriangles;
        }
        set
        {
            _reverseWindTriangles = value;
        }
    }

    public Transform Parent
    {
        get
        {
            return _parent;
        }
        set
        {
            _parent = value;
        }
    }

    public bool UseGravity 
    {
        get
        {
            return _useGravity;
        }
        set
        {
            _useGravity = value;
        }
    }

    public bool ShareVertices 
    {
        get
        {
            return _shareVertices;
        }
        set
        {
            _shareVertices = value;
        }
    }

    public bool SmoothVertices 
    {
        get
        {
            return _smoothVertices;
        }
        set
        {
            _smoothVertices = value;
        }
    }

}
