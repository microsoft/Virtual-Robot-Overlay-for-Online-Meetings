using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarLogicForHumanoid : MonoBehaviour
{
    public enum Gender { Female, Male };
 
    public NetworkEvents networkEvents;

    public Gender gender;

    public GameObject AMPMaleHead;
    public GameObject AMPFemaleHead;
    private GameObject _AMPHead;

    public GameObject AMPMaleBody;
    public GameObject AMPFemaleBody;
    private GameObject _AMPBody;

    public Transform maleLeftHandTarget;
    public Transform femaleLeftHandTarget;
    public Transform maleRightHandTarget;
    public Transform femaleRightHandTarget;
    private Transform _leftHandTarget;
    private Transform _rightHandTarget;

    private HumanoidArmIK _ikControl;

    public float walkingThreshold = 0.5f;

    private bool playWalkingBackwardAnimation = false;
    private bool playWalkingForwardAnimation = false;

    private Animator _headAnim;
    private Animator _bodyAnim;

    private Vector3 startPos;
    private Quaternion startRot;

    // Use this for initialization
    void Start()
    {
        AMPFemaleBody.SetActive(false);
        AMPMaleBody.SetActive(false);
        _AMPBody = gender == Gender.Female? AMPFemaleBody : AMPMaleBody;
        _AMPBody.SetActive(true);

        AMPFemaleHead.SetActive(false);
        AMPMaleHead.SetActive(false);
        _AMPHead = gender == Gender.Female ? AMPFemaleHead : AMPMaleHead;
        _AMPHead.SetActive(true);

        _leftHandTarget = gender == Gender.Female ? femaleLeftHandTarget : maleLeftHandTarget;
        _rightHandTarget = gender == Gender.Female ? femaleRightHandTarget : maleRightHandTarget;

        AMPFemaleHead.SetActive(false);
        AMPMaleHead.SetActive(false);
        _AMPHead = gender == Gender.Female ? AMPFemaleHead : AMPMaleHead;
        _AMPHead.SetActive(true);

        _ikControl = _AMPBody.GetComponent<HumanoidArmIK>();

        ///_leftHandTarget = ///

        networkEvents.AddHandler("AvatarStartTalking", avatarStartTalking);
        networkEvents.AddHandler("AvatarStopTalking", avatarStopTalking);
        networkEvents.AddHandler("HeadOrientationUpdate", headOrientationUpdate);
        networkEvents.AddHandler("HandMoveUpdate", handMoveUpdate);
        networkEvents.AddHandler("LeftHandTrackingStateToggle", leftHandTrackingStateToggle);
        networkEvents.AddHandler("RightHandTrackingStateToggle", rightHandTrackingStateToggle);
        networkEvents.AddHandler("ThumbstickPositionUpdate", thumbstickPositionUpdate);

        _headAnim = _AMPHead.GetComponent<Animator>();
        _bodyAnim = _AMPBody.GetComponent<Animator>();

        startPos = _AMPBody.transform.localPosition;
        startRot = _AMPBody.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        // reset avatar position and orientation (since the animation sometimes unintentionally shifts it)
        _AMPBody.transform.localPosition = startPos;
        _AMPBody.transform.localRotation = startRot;
    }

    private void avatarStartTalking(string data)
    {
        Debug.Log("AvatarStartTalking");
        _headAnim.SetBool("isTalk", true);
    }

    private void avatarStopTalking(string data)
    {
        Debug.Log("AvatarStopTalking");
        _headAnim.SetBool("isTalk", false);
    }

    private void headOrientationUpdate(string data)
    {
        Debug.Log("HeadOrientationUpdate: " + data);

        string[] axis = data.Split(',');
        Vector2 rotation = new Vector2(float.Parse(axis[0]), float.Parse(axis[1]));

        // Y = 0 ==> head turn forward
        // Y = 90 ==> head turn right
        // Y = 180 ==> head turn back
        // Y = 270 ==> head turn left

        float headTurn = rotation.y;
        if (headTurn > 180)
            headTurn -= 360;

        _headAnim.SetFloat("headTurn", headTurn);
        _bodyAnim.SetFloat("neckTurn", headTurn);

        // X = 0 ==> head tilt forward
        // X = 90 ==> head tilt down
        // X = 270 ==> head tilt up
        float headNod = rotation.x;
        if (headNod > 180)
            headNod -= 360;

        _headAnim.SetFloat("headNod", headNod);
        _bodyAnim.SetFloat("headNod", headNod);
    }

    private void handMoveUpdate(string data)
    {
        Debug.Log("HandMoveUpdate: " + data);

        string[] values = data.Split(';');

        Vector3 leftHandPosition = eventStringToVector3(values[0]);
        Quaternion leftHandRotation = eventStringToQuaternion(values[1]);
        Vector3 rightHandPosition = eventStringToVector3(values[2]);
        Quaternion rightHandRotation = eventStringToQuaternion(values[3]);

        _leftHandTarget.localPosition = leftHandPosition;
        _leftHandTarget.localRotation = leftHandRotation;
        _rightHandTarget.localPosition = rightHandPosition;
        _rightHandTarget.localRotation = rightHandRotation;
    }

    private void leftHandTrackingStateToggle(string data)
    {
        Debug.Log("LeftHandTrackingStateToggle: " + data);

        _ikControl.trackingLeftHand = bool.Parse(data);
    }

    private void rightHandTrackingStateToggle(string data)
    {
        Debug.Log("RightHandTrackingStateToggle: " + data);

        _ikControl.trackingRightHand = bool.Parse(data);
    }

    private void thumbstickPositionUpdate(string data)
    {
        Vector3 thumbstickPosition = eventStringToVector3(data);

        // set walking animation
        playWalkingBackwardAnimation = false;
        playWalkingForwardAnimation = false;
        if (thumbstickPosition.y < -walkingThreshold)
        {
            playWalkingBackwardAnimation = true;
        }
        else if (thumbstickPosition.y > walkingThreshold || Mathf.Abs(thumbstickPosition.x) > walkingThreshold)
        {
            playWalkingForwardAnimation = true;
        }

        // change animation based on walking state

        if (playWalkingBackwardAnimation)
        {
            _bodyAnim.SetBool("isWalkBackward", true);
        }
        else if (_bodyAnim.GetBool("isWalkBackward"))
        {
            _bodyAnim.SetBool("isWalkBackward", false);
        }

        if (playWalkingForwardAnimation)
        {
            _bodyAnim.SetBool("isWalk", true);
        }
        else if (_bodyAnim.GetBool("isWalk"))
        {
            _bodyAnim.SetBool("isWalk", false);
        }
    }

    Vector3 eventStringToVector3 (string s)
    {
        string[] axis = s.Split(',');
        return new Vector3(float.Parse(axis[0]), float.Parse(axis[1]), float.Parse(axis[2]));
    }

    Quaternion eventStringToQuaternion(string s)
    {
        string[] axis = s.Split(',');
        return new Quaternion(float.Parse(axis[0]), float.Parse(axis[1]), float.Parse(axis[2]), float.Parse(axis[3]));
    }
}