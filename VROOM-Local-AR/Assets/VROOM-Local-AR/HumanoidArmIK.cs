using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidArmIK : MonoBehaviour {
    public Transform leftHandTarget;
    public Transform rightHandTarget;

    public bool trackingLeftHand = false;
    public bool trackingRightHand = false;

    private float leftHandIKWeight = 1;
    private float rightHandIKWeight = 1;

    private Animator _anim;

    // Use this for initialization
    void Start () {
        _anim = GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {
        leftHandIKWeight = trackingLeftHand ? 1 : 0;
        rightHandIKWeight = trackingRightHand ? 1 : 0;

        if (trackingLeftHand || trackingRightHand)
        {
            _anim.SetBool("isExpressive", false);
        }
        else
        {
            _anim.SetBool("isExpressive", true);
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        _anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandIKWeight);
        _anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);

        _anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
        _anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        _anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
        _anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
    }
}
