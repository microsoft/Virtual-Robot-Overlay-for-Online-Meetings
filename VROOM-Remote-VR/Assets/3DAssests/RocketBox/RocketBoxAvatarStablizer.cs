using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBoxAvatarStablizer : MonoBehaviour
{
    public Camera vrCamera;
    public Transform avatarRoot;

    void LateUpdate()
    {
        Transform head = GameObject.Find("Bip01 Head").transform;
        head.localScale = new Vector3(1, 1, 1);

        Transform eyeLeft = GameObject.Find("Bip01 LEye").transform;
        Transform eyeRight = GameObject.Find("Bip01 REye").transform;
        Vector3 eyeCenter = (eyeLeft.position + eyeRight.position) * 0.5f;
        Vector3 offset = vrCamera.transform.position - eyeCenter;
        avatarRoot.position += offset; 

        head.localScale = new Vector3(0, 0, 0);
    }
}
