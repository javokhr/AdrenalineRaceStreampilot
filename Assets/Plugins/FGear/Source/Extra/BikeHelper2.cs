// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using FGear;

public class BikeHelper2 : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    float AntiRollSpring = 12500f;
    [SerializeField, Range(1f, 10f)]
    float DamperSmooth = 5.0f;
    [SerializeField]
    float MinLeanSpeed = 20f;
    [SerializeField]
    float MaxLeanAngle = 50f;
    [SerializeField]
    float MaxPitchAngle = 75f;
    [SerializeField]
    bool LeanInAir = true;

    Rigidbody mBody;
    Transform mTrans;
    float mLastPitch;

    void Start()
    {
        mBody = Vehicle.getBody();
        mTrans = mBody.transform;
        if (MinLeanSpeed <= 0.0f) MinLeanSpeed = 0.01f;
    }

    void FixedUpdate()
    {
        float antiRollTorque = 0.0f;
        float rollDamperTorque = 0.0f;
        float pitchDamperTorque = 0.0f;

        //all wheel contact?
		bool frontContact = Vehicle.getAxle(0).getLeftWheel().hasContact() && Vehicle.getAxle(0).getRightWheel().hasContact();
		bool rearContact = Vehicle.getAxle(1).getLeftWheel().hasContact() && Vehicle.getAxle(1).getRightWheel().hasContact();
        bool wheelContact = frontContact && rearContact;

        //anti roll torque
        float pitch = mTrans.localEulerAngles.x;
        float roll = mTrans.localEulerAngles.z;
        if (wheelContact || LeanInAir)
        {
            float speedCoeff = Mathf.Clamp01(Vehicle.getKMHSpeed() / MinLeanSpeed);
            float rollOffset = Vehicle.getNormalizedSteering() * speedCoeff * MaxLeanAngle;
            roll += rollOffset; MyDebug.test2 = roll;
        }

        //fix pitch/roll limits
        if (roll > 180.0f) roll = -360.0f + roll;
        if (pitch > 180.0f) pitch = -360.0f + pitch;
        if (Mathf.Abs(roll) >= 90.0f && (frontContact || rearContact)) roll += 180.0f;
        if (roll > 180.0f) roll = -360.0f + roll;
        antiRollTorque = roll / 90.0f * AntiRollSpring;
        MyDebug.test3 = roll;

        //anti roll damper
        float zVelocity = mTrans.InverseTransformVector(mBody.angularVelocity).z;
        float ra = zVelocity / Time.fixedDeltaTime;
        float rollCoeff = Mathf.Clamp01(Mathf.Abs(roll / DamperSmooth));
        rollDamperTorque = mBody.inertiaTensor.z * ra * rollCoeff;

        if (!wheelContact)
        {
            //pitch damper
            mLastPitch = pitchDamperTorque;
            if ((pitch >= 0.0f && pitch > MaxPitchAngle && pitch > mLastPitch) ||
                (pitch < 0.0f && pitch < -MaxPitchAngle && pitch < mLastPitch))
            {
                float xVelocity = mTrans.InverseTransformVector(mBody.angularVelocity).x;
                float pa = xVelocity / Time.fixedDeltaTime;
                float pitchCoeff = Mathf.Clamp01(Mathf.Abs(pitch / DamperSmooth));
                pitchDamperTorque = mBody.inertiaTensor.x * pa * pitchCoeff;
            }
        }

        //apply torques
        float xTorque = -pitchDamperTorque;
        float zTorque = -antiRollTorque - rollDamperTorque;
        mBody.AddRelativeTorque(xTorque, 0f, zTorque);
    }
}