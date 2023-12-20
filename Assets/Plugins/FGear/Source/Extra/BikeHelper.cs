// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using FGear;

public class BikeHelper : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    float LeanSpring = 1000f;
    [SerializeField]
    float AntiRollSpring = 5000f;
    [SerializeField]
    float AntiPitchSpring = 1000f;
    [SerializeField]
    float MaxLeanAngle = 60f;
    [SerializeField]
    bool LeanInAir = true;

    Rigidbody mBody;
    Transform mTrans;

    void Start()
    {
        mBody = Vehicle.getBody();
        mTrans = mBody.transform;
    }

    void FixedUpdate()
    {
        float leanTorque = 0f;
        float antiRollTorque = 0f;
        float rollDamperTorque = 0f;
        float antiPitchTorque = 0f;
        float pitchDamperTorque = 0f;

        //all wheel contact?
        bool wheelContact = Vehicle.getAxle(0).getLeftWheel().hasContact() && Vehicle.getAxle(1).getLeftWheel().hasContact();

        //anti roll
        float roll = mTrans.localEulerAngles.z;
        if (roll > 180) roll = -360 + roll;
        antiRollTorque = roll / 90f * AntiRollSpring;

        //roll damper
        float zVelocity = mTrans.InverseTransformVector(mBody.angularVelocity).z;
        float ra = zVelocity / Time.fixedDeltaTime;
        float rollCoeff = Mathf.Clamp01(Mathf.Abs(roll / 3f)); //avoid jitters for small angles
        rollDamperTorque = mBody.inertiaTensor.z * ra * rollCoeff;

        if (wheelContact || LeanInAir)
        {
            //steering roll
            float speedCoeff = Mathf.Max(0, Vehicle.getKMHSpeed()) / 5f;
            float steer = speedCoeff * Vehicle.getNormalizedSteering();
            leanTorque = steer * LeanSpring;

            //less steering torque close to roll limit
            if (Mathf.Sign(roll) != Mathf.Sign(steer))
            {
                leanTorque *= 1f - Mathf.Min(1f, Mathf.Abs(roll) / MaxLeanAngle);
            }
        }

        if (!wheelContact)
        {
            //anti pitch
            float pitch = mTrans.localEulerAngles.x;
            if (pitch > 180) pitch = -360 + pitch;
            antiPitchTorque = pitch / 90f * AntiPitchSpring;

            //pitch damper
            float xVelocity = mTrans.InverseTransformVector(mBody.angularVelocity).x;
            float pa = xVelocity / Time.fixedDeltaTime;
            pitchDamperTorque = mBody.inertiaTensor.x * pa;
        }

        //apply torques
        float xTorque = -antiPitchTorque - pitchDamperTorque;
        float zTorque = -leanTorque - antiRollTorque - rollDamperTorque;
        mBody.AddRelativeTorque(xTorque, 0f, zTorque);
    }
}