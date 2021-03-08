using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CameraStabilizer : MonoBehaviour
{
    public double positionMoveFactor = 0.35;

    void Start()
    {
        XRDevice.SetTrackingSpaceType(TrackingSpaceType.Stationary);
        InputTracking.disablePositionalTracking = true;
        this.gameObject.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        //UnityEngine.XR.InputTracking.Recenter();
    }

    void Update()
    {
        double newX, newZ;

        Vector3 trackingPos = InputTracking.GetLocalPosition(XRNode.CenterEye);
        newX = trackingPos.x * positionMoveFactor;
        newZ = trackingPos.z * positionMoveFactor;

        this.gameObject.transform.position = new Vector3((float)newX, 0.0f, (float)newZ);
    }
}
