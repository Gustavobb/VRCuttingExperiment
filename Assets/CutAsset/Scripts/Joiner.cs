using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joiner : MonoBehaviour
{
    GameObject originalChild;
    List<GameObject> children = new List<GameObject>();
    bool needsToBeJoined = false;

    public delegate void JoinEvent();
    public JoinEvent OnJoin;

    public delegate void JoinStartEvent();
    public JoinStartEvent OnJoinStart;

    public delegate void JoinEndEvent();
    public JoinEndEvent OnJoinEnd;

    public delegate void JoinSuccessEvent();
    public JoinSuccessEvent OnJoinSuccess;

    public int JoinCount = 0;
    float smoothTime = 0.3f;
    Vector3 velocity = Vector3.zero;

    void Start()
    {
        DuplicateChild();
    }

    void DuplicateChild()
    {
        originalChild = transform.GetChild(0).gameObject;
        originalChild.transform.localPosition = Vector3.zero;
        originalChild.transform.localScale = Vector3.one;
        originalChild.transform.localRotation = Quaternion.identity;

        GameObject duplicate = Instantiate(originalChild);
        duplicate.transform.parent = transform;
        originalChild = duplicate;
        originalChild.SetActive(false);
    }

    public void UpdateJoin(Transform follower)
    {
        if (needsToBeJoined)
        {
            if (OnJoin != null)
                OnJoin();

            if (JoinCount >= children.Count)
            {
                EndJoin();

                if (OnJoinSuccess != null)
                    OnJoinSuccess();
                
                originalChild.SetActive(true);
                DuplicateChild();
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, follower.transform.position, ref velocity, smoothTime);
        }
    }

    public void StartJoin()
    {
        if (OnJoinStart != null)
            OnJoinStart();

        JoinCount = 0;
        needsToBeJoined = true;

        for (int i = 0; i < transform.childCount; i++)
            if (transform.GetChild(i).gameObject != originalChild)
                children.Add(transform.GetChild(i).gameObject);
    }

    public void EndJoin()
    {
        if (OnJoinEnd != null)
            OnJoinEnd();

        needsToBeJoined = false;
        children.Clear();
        JoinCount = 0;
    }
}
