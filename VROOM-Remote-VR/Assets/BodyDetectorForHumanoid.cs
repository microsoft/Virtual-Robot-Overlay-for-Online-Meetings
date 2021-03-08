using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA.Input;

public class BodyDetector : MonoBehaviour
{
    public NetworkEvents networkEvents;

    public Camera vrCamera;

    public float walkingThreshold = 0.5f;

    public Transform leftHandTarget;
    public Transform rightHandTarget;

    public bool trackingLeftHand = false;
    public bool trackingRightHand = false;

    public InteractionSourceHandedness robotControlThumbstickHand = InteractionSourceHandedness.Left;

    private float leftHandIKWeight = 0;
    private float rightHandIKWeight = 0;

    private bool leftSelectButtonReleased = true;
    private bool rightSelectButtonReleased = true;

    private bool leftHandLastTrackingStatus = false;
    private bool rightHandLastTrackingStatus = false;

    private bool playWalkingBackwardAnimation = false;
    private bool playWalkingForwardAnimation = false;

    private Animator _anim;

    private Vector3 startPos;
    private Quaternion startRot;

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

        leftHandIKWeight = 0;
        rightHandIKWeight = 0;

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
        leftHandIKWeight = leftHandCurrentTrackingStatus ? 1 : 0;
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
        rightHandIKWeight = rightHandCurrentTrackingStatus ? 1 : 0;
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
            // calculate handOffset to make the hand transform in relation to body transform (not absolute in world)
            Vector3 headsetPos = InputTracking.GetLocalPosition(XRNode.CenterEye);
            Vector3 avatarPos = this.gameObject.transform.localPosition;
            Vector3 handsOffset = -1 * headsetPos - avatarPos;

            Vector3 leftHandPosition = InputTracking.GetLocalPosition(XRNode.LeftHand) + handsOffset;
            Quaternion leftHandRotation = InputTracking.GetLocalRotation(XRNode.LeftHand);
            Vector3 rightHandPosition = InputTracking.GetLocalPosition(XRNode.RightHand) + handsOffset;
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

    void OnAnimatorIK(int layerIndex)
    {
        // set the hand to the target position

        _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
        _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);

        _anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
        _anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        _anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
        _anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
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
