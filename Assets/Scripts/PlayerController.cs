using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerController : MonoBehaviour
{
    public SteamVR_Action_Boolean teleportAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("Teleport");
    public SteamVR_ActionSet actionSet;
    public SteamVR_Action_Vector2 moveAction;
    public SteamVR_Input_Sources hand;
    
    [SerializeField] CharacterController characterController;
    [SerializeField] GameObject vrCamera;
    [SerializeField] float moveSpeed = 50f;

    void Start()
    {
        actionSet.Activate(hand);
    }
    
    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (teleportAction.lastState) return;

        Vector2 m = moveAction[hand].axis;
        Vector3 move = vrCamera.transform.rotation * new Vector3(m.x, 0, m.y);
        move.y = -10f;

        characterController.Move(move * Time.deltaTime * moveSpeed);
    }
}