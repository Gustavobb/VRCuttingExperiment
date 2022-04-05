using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.Events;

public class HandPowerController : MonoBehaviour
{
    [HideInInspector] public bool usingHandPower = false;
    public SteamVR_Action_Boolean button = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("GrabGrip");
    public SteamVR_ActionSet actionSet;
    public SteamVR_Input_Sources hand;
    [SerializeField] GameObject handPower;
    [SerializeField] HandPowerController otherHand;
    public UnityEvent onActivate;
    public UnityEvent onDeactivate;

    Hand selfHand;
    SteamVR_Behaviour_Pose trackedObj;
    bool buttonIsPressed = false;

    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_Behaviour_Pose>();
        handPower.SetActive(false);
        selfHand = GetComponent<Hand>();
    }

    private void Start() 
    {
        actionSet.Activate(hand);
    }

    private void Update()
    {
        buttonIsPressed = button.GetState(trackedObj.inputSource);

        if (buttonIsPressed)
        {
            // if not hovering and highlighed
            if (!otherHand.usingHandPower && !usingHandPower)
            {
                onActivate.Invoke();
                usingHandPower = true;
                selfHand.renderModelInstance.SetActive(false);
                handPower.SetActive(true);
            }
        }

        else if (usingHandPower)
        {
            onDeactivate.Invoke();
            usingHandPower = false;
            handPower.SetActive(false);
            selfHand.renderModelInstance.SetActive(true);
        }
    }
}
