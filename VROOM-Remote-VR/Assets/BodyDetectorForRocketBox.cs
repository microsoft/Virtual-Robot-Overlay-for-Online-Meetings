using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

public class BodyDetectorForRocketBox : MonoBehaviour
{
    public static BodyDetectorForRocketBox instance;

    public NetworkEvents networkEvents;

    public Camera vrCamera;

    public float walkingThreshold = 0.5f;

    public Transform avatarRoot;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    public bool trackingLeftHand = false;
    public bool trackingRightHand = false;

    public InteractionSourceHandedness robotControlThumbstickHand = InteractionSourceHandedness.Left;

    private bool leftSelectButtonReleased = true;
    private bool rightSelectButtonReleased = true;

    private bool leftHandLastTrackingStatus = false;
    private bool rightHandLastTrackingStatus = false;

    private bool playWalkingBackwardAnimation = false;
    private bool playWalkingForwardAnimation = false;

    private Animator _anim;

    private Vector3 startPos;
    private Quaternion startRot;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
        InputTracking.disablePositionalTracking = true;

        _anim = GetComponent<Animator>();

        startPos = this.gameObject.transform.localPosition;
        startRot = this.gameObject.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        // reset avatar position and orientation (since the animation sometimes unintentionally shifts it)
        this.gameObject.transform.localPosition = startPos;
        this.gameObject.transform.localRotation = startRot;


        // send head orientation event
        Vector3 fullRot = vrCamera.transform.localRotation.eulerAngles;
        Vector2 rotation = new Vector2(fullRot.x, fullRot.y);

        NetworkEvent networkEvent = new NetworkEvent();
        networkEvent.EventName = "HeadOrientationUpdate";
        networkEvent.EventData = vector2ToEventString(rotation);
        networkEvents.PostEventMessage(networkEvent);


        // record and send hand move event
        // TODO: Clean up/refactor, as there is repeated code

        bool leftHandTrackingIsAccurate = false;
        bool rightHandTrackingIsAccurate = false;

        var interactionSourceStates = InteractionManager.GetCurrentReading();
        Debug.Log("interactionSourceStates: " + interactionSourceStates.Length);

        foreach (var interactionSourceState in interactionSourceStates)
        {
            if (interactionSourceState.source.handedness == InteractionSourceHandedness.Left)
            {
                if (interactionSourceState.selectPressed && leftSelectButtonReleased)
                {
                    leftSelectButtonReleased = false;
                    trackingLeftHand = !trackingLeftHand;
                }
                else if (!interactionSourceState.selectPressed)
                {
                    leftSelectButtonReleased = true;
                }

                if (interactionSourceState.touchpadPressed)
                {
                    robotControlThumbstickHand = InteractionSourceHandedness.Left;
                }

                // accuracy logic
                Debug.Log("sourcePose: " + interactionSourceState.sourcePose);
                var sourcePose = interactionSourceState.sourcePose;
                if (sourcePose.positionAccuracy == InteractionSourcePositionAccuracy.High ||
                    sourcePose.positionAccuracy == InteractionSourcePositionAccuracy.Approximate)
                {
                    leftHandTrackingIsAccurate = true;
                }

                if (robotControlThumbstickHand == InteractionSourceHandedness.Left)
                {
                    // send thumbstick position
                    networkEvent = new NetworkEvent();
                    networkEvent.EventName = "ThumbstickPositionUpdate";
                    networkEvent.EventData = vector3ToEventString(interactionSourceState.thumbstickPosition);
                    networkEvents.PostEventMessage(networkEvent);

                    // set walking animation
                    playWalkingBackwardAnimation = false;
                    playWalkingForwardAnimation = false;
                    if (interactionSourceState.thumbstickPosition.y < -walkingThreshold)
                    {
                        playWalkingBackwardAnimation = true;
                    }
                    else if (interactionSourceState.thumbstickPosition.y > walkingThreshold || Mathf.Abs(interactionSourceState.thumbstickPosition.x) > walkingThreshold)
                    {
                        playWalkingForwardAnimation = true;
                    }
                }
            }
            else if (interactionSourceState.source.handedness == InteractionSourceHandedness.Right)
            {
                if (interactionSourceState.selectPressed && rightSelectButtonReleased)
                {
                    rightSelectButtonReleased = false;
                    trackingRightHand = !trackingRightHand;
                }
                else if (!interactionSourceState.selectPressed)
                {
                    rightSelectButtonReleased = true;
                }

                if (interactionSourceState.touchpadPressed)
                {
                    robotControlThumbstickHand = InteractionSourceHandedness.Right;
                }

                // accuracy logic
                Debug.Log("sourcePose: " + interactionSourceState.sourcePose);
                var sourcePose = interactionSourceState.sourcePose;
                if (sourcePose.positionAccuracy == InteractionSourcePositionAccuracy.High ||
                    sourcePose.positionAccuracy == InteractionSourcePositionAccuracy.Approximate)
                {
                    rightHandTrackingIsAccurate = true;
                }

                if (robotControlThumbstickHand == InteractionSourceHandedness.Right)
                {
                    // send thumbstick position
                    networkEvent = new NetworkEvent();
                    networkEvent.EventName = "ThumbstickPositionUpdate";
                    networkEvent.EventData = vector3ToEventString(interactionSourceState.thumbstickPosition);
                    networkEvents.PostEventMessage(networkEvent);

                    // set walking animation
                    playWalkingBackwardAnimation = false;
                    playWalkingForwardAnimation = false;
                    if (interactionSourceState.thumbstickPosition.y < -walkingThreshold)
                    {
                        playWalkingBackwardAnimation = true;
                    }
                    else if (interactionSourceState.thumbstickPosition.y > walkingThreshold || Mathf.Abs(interactionSourceState.thumbstickPosition.x) > walkingThreshold)
                    {
                        playWalkingForwardAnimation = true;
                    }
                }
            }
        }

        bool didToggle = false;

        bool leftHandCurrentTrackingStatus = trackingLeftHand && leftHandTrackingIsAccurate;
        if (leftHandLastTrackingStatus != leftHandCurrentTrackingStatus)
        {
            didToggle = true;

            networkEvent = new NetworkEvent();
            networkEvent.EventName = "LeftHandTrackingStateToggle";
            networkEvent.EventData = leftHandCurrentTrackingStatus.ToString();
            networkEvents.PostEventMessage(networkEvent);

            leftHandLastTrackingStatus = leftHandCurrentTrackingStatus;
        }

        bool rightHandCurrentTrackingStatus = trackingRightHand && rightHandTrackingIsAccurate;
        if (rightHandLastTrackingStatus != rightHandCurrentTrackingStatus)
        {
            didToggle = true;

            networkEvent = new NetworkEvent();
            networkEvent.EventName = "RightHandTrackingStateToggle";
            networkEvent.EventData = rightHandCurrentTrackingStatus.ToString();
            networkEvents.PostEventMessage(networkEvent);

            rightHandLastTrackingStatus = rightHandCurrentTrackingStatus;
        }

        if (didToggle)
        {
            if (trackingLeftHand || trackingRightHand)
            {
                // change to normal idle animation
            }
            else
            {
                // change to more expressive idle animation
            }
        }

        // change animation based on walking state

        if (playWalkingBackwardAnimation)
        {
            _anim.SetBool("isWalkBackward", true);
        }
        else if (_anim.GetBool("isWalkBackward"))
        {
            _anim.SetBool("isWalkBackward", false);
        }

        if (playWalkingForwardAnimation)
        {
            _anim.SetBool("isWalk", true);
        }
        else if (_anim.GetBool("isWalk"))
        {
            _anim.SetBool("isWalk", false);
        }

        if (trackingLeftHand || trackingRightHand)
        {
            Vector3 leftHandPosition = InputTracking.GetLocalPosition(XRNode.LeftHand) - InputTracking.GetLocalPosition(XRNode.CenterEye);
            Quaternion leftHandRotation = InputTracking.GetLocalRotation(XRNode.LeftHand);
            Vector3 rightHandPosition = InputTracking.GetLocalPosition(XRNode.RightHand) - InputTracking.GetLocalPosition(XRNode.CenterEye);
            Quaternion rightHandRotation = InputTracking.GetLocalRotation(XRNode.RightHand);

            networkEvent = new NetworkEvent();
            networkEvent.EventName = "HandMoveUpdate";
            networkEvent.EventData = vector3ToEventString(leftHandPosition) + ";" + quaternionToEventString(leftHandRotation) + ";" +
                vector3ToEventString(rightHandPosition) + ";" + quaternionToEventString(rightHandRotation);
            networkEvents.PostEventMessage(networkEvent);

            leftHandTarget.localPosition = leftHandPosition;
            leftHandTarget.localRotation = leftHandRotation;
            rightHandTarget.localPosition = rightHandPosition;
            rightHandTarget.localRotation = rightHandRotation;
        }
    }

    private string vector2ToEventString(Vector2 input)
    {
        return input.x + "," + input.y;
    }

    private string vector3ToEventString(Vector3 input)
    {
        return input.x + "," + input.y + "," + input.z;
    }

    private string quaternionToEventString(Quaternion input)
    {
        return input.w + "," + input.x + "," + input.y + "," + input.z;
    }
}
