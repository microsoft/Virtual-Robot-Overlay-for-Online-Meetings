using System;
using UnityEngine;

/// <summary>
/// Class for the Arm IK for RocketBox avatar
/// Attach this script to RocketBox avatar gameobject.
/// </summary>
public class RocketBoxArmIK : MonoBehaviour
{
    public bool enableLeftArm = true;
    public bool enableRightArm = true;
    public Transform effectorL;
    public Transform effectorR;

    private RocketBoxArm armL;
    private RocketBoxArm armR;

    void Start()
    {
        Transform handBoneL = GameObject.Find("Bip01 L Hand").transform;
        Transform handBoneR = GameObject.Find("Bip01 R Hand").transform;
        armL = new RocketBoxArm(handBoneL, effectorL, new Vector3(0.0f, 180.0f, 90.0f));
        armR = new RocketBoxArm(handBoneR, effectorR, new Vector3(-90.0f, 90.0f, 0.0f));
    }

    private void Update()
    {
        enableLeftArm = AvatarLogicForRocketBox.instance.enableLeftHand;
        enableRightArm = AvatarLogicForRocketBox.instance.enableRightHand;
    }

    void LateUpdate()
    {
        if (enableLeftArm) armL.ApplyEffector();
        if (enableRightArm) armR.ApplyEffector();
    }

    /// <summary>
    /// Class for RocketBox avatar's arm, including 3 bones.
    /// </summary>
    class RocketBoxArm
    {
        // only deal with IK when arm is complete (no bone missing).
        public bool isComplete = false;
        public Transform effector;

        public Transform hand;
        public Transform forearm;
        public Transform upperarm;
        public Vector3 offset;

        public RocketBoxArm(Transform hand, Transform effectorX, Vector3 offsetX)
        {
            isComplete = false;

            if (hand == null)
                return;

            upperarm = hand;

            forearm = upperarm.parent;
            if (forearm == null)
                return;

            this.hand = forearm.parent;
            if (upperarm == null)
                return;

            isComplete = true;

            offset = offsetX;

            effector = effectorX;
        }

        /// <summary>
        /// If the arm is detected complete, let the arm move along with hand (wrist) joint according to the effector's position and rotation. 
        /// Call this method in your LateUpdate() 
        /// </summary>
        public void ApplyEffector()
        {
            if (isComplete)
            {
                Solve();
            }
        }

        /// <summary>
        /// Returns the angle needed between v1 and v2 so that their extremities are
        /// spaced with a specific length.
        /// </summary>
        /// <returns>The angle between v1 and v2.</returns>
        /// <param name="aLen">The desired length between the extremities of v1 and v2.</param>
        /// <param name="v1">First triangle edge.</param>
        /// <param name="v2">Second triangle edge.</param>
        private static float TriangleAngle(float aLen, Vector3 v1, Vector3 v2)
        {
            float aLen1 = v1.magnitude;
            float aLen2 = v2.magnitude;
            float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
            return Mathf.Acos(c);
        }

        private void Solve()
        {
            Quaternion aRotation = hand.rotation;
            Quaternion bRotation = forearm.rotation;
            Quaternion eRotation = effector.rotation * Quaternion.Euler(offset);

            Vector3 aPosition = hand.position;
            Vector3 bPosition = forearm.position;
            Vector3 cPosition = upperarm.position;
            Vector3 ePosition = effector.position;

            Vector3 ab = bPosition - aPosition;
            Vector3 bc = cPosition - bPosition;
            Vector3 ac = cPosition - aPosition;
            Vector3 ae = ePosition - aPosition;

            float abcAngle = TriangleAngle(ac.magnitude, ab, bc);
            float abeAngle = TriangleAngle(ae.magnitude, ab, bc);
            float angle = (abcAngle - abeAngle) * Mathf.Rad2Deg;
            Vector3 axis = Vector3.Cross(ab, bc).normalized;

            Quaternion fromToRotation = Quaternion.AngleAxis(angle, axis);

            Quaternion worldQ = fromToRotation * bRotation;
            forearm.rotation = worldQ;

            cPosition = upperarm.position;
            ac = cPosition - aPosition;
            Quaternion fromTo = Quaternion.FromToRotation(ac, ae);
            hand.rotation = fromTo * aRotation;
            upperarm.rotation = eRotation;
        }
    }
}