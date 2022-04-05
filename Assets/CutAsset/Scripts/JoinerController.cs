using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinerController : MonoBehaviour
{
    Joiner joiner;

    void Update()
    {
        if (joiner != null)
            HandleJoiner();
    }

    public void Reset()
    {
        if (joiner == null) return;
        joiner.EndJoin();
        joiner.OnJoinSuccess -= JoinSuccess;
        joiner = null;
    }

    void HandleJoiner()
    {
       joiner.UpdateJoin(transform);
    }

    void OnTriggerEnter(Collider other)
    {
        print(other.gameObject.tag);
        print(joiner);
        if (other.gameObject.tag == "Joiner" || joiner != null)
            return;

        joiner = other.transform.root.GetComponent<Joiner>();

        if (joiner == null || joiner.transform.childCount <= 2)
            return;

        List<GameObject> children = new List<GameObject>();
        for (int i = 0; i < joiner.transform.childCount; i++)
            children.Add(joiner.transform.GetChild(i).gameObject);
        
        joiner.transform.DetachChildren();
        joiner.transform.position = transform.position;
        
        for (int i = 0; i < children.Count; i++)
            children[i].transform.parent = joiner.transform;

        joiner.OnJoinSuccess += JoinSuccess;
        joiner.StartJoin();
    }

    void OnTriggerExit(Collider other)
    {
        if (joiner == null || other.gameObject != joiner.gameObject)
            return;

        joiner.EndJoin();
        joiner.OnJoinSuccess -= JoinSuccess;
        joiner = null;
    }

    void JoinSuccess()
    {
        joiner.OnJoinSuccess -= JoinSuccess;
        joiner = null;
    }
}