using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketBoxAvatarStablizerAR : MonoBehaviour
{
    public Transform robotMarkerRoot;
    public Transform avatarRoot;

    void LateUpdate()
    {
        Transform head = GameObject.Find("Bip01 Head").transform;
        head.localScale = new Vector3(1, 1, 1);

        Transform clavicleLeft = GameObject.Find("Bip01 L Clavicle").transform;
        Transform clavicleRight = GameObject.Find("Bip01 R Clavicle").transform;
        Vector3 clavicleCenter = (clavicleLeft.position + clavicleRight.position) * 0.5f;
        Vector3 offset = robotMarkerRoot.transform.position - clavicleCenter;
        avatarRoot.position += offset; 

        head.localScale = new Vector3(0, 0, 0);
    }
}
