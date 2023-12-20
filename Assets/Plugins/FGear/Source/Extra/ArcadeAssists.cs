// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using FGear;

public class ArcadeAssists : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;

    //recover assists
    [SerializeField]
    bool EnableRecoverAssists = false;
    [SerializeField]
    float UnderSteerRecoverPower = 0f;
    [SerializeField]
    float OverSteerRecoverPower = 0f;
    [SerializeField, Range(0f, 100f)]
    float MinRecoverAssistSpeed = 5f;
    [SerializeField, Range(1f, 100f)]
    float FullRecoverAssistSpeed = 50f;

    //antirollbars
    [SerializeField]
    bool EnableAntiRollBars = false;
    [SerializeField, Range(0f, 100f)]
    float FrontAntiRollPower = 0f;
    [SerializeField, Range(0f, 100f)]
    float RearAntiRollPower = 0f;

    //traction assist
    [SerializeField]
    bool EnableTractionAssist = false;
    [SerializeField, Range(0f, 200f)]
    float TractionAssistRatio = 0f;
    [SerializeField, Range(1f, 50f)]
    float TractionAssistMaxSpeed = 15f;

    //drift assist
    [SerializeField]
    bool EnableDriftAssist = false;
    [SerializeField, Range(-100f, 100f)]
    float DriftAssistFrontRatio = 0f;
    [SerializeField, Range(-100f, 100f)]
    float DriftAssistRearRatio = 0f;

    //torque splitter
    [SerializeField]
    bool EnableTorqueSplitter = false;
    [SerializeField, Range(0f, 100f)]
    float TorqueSplitterRatio = 0f;
    [SerializeField, Range(1f, 1000f)]
    float TorqueSplitterResponseTime = 100f;

    Transform mTrans;
    Rigidbody mBody;
    Wheel mFlw, mFrw, mRlw, mRrw;
    float[] mFrictionCoeffs;
    float[] mTorqueShares;
    
    void Start()
    {
        mTrans = Vehicle.transform;
        mBody = Vehicle.getBody();
        mFlw = Vehicle.getAxle(0).getLeftWheel();
        mFrw = Vehicle.getAxle(0).getRightWheel();
        mRlw = Vehicle.getAxle(1).getLeftWheel();
        mRrw = Vehicle.getAxle(1).getRightWheel();

        //grab a copy of wheel friction factors -> axleCt * 2(lng/lat)
        mFrictionCoeffs = new float[Vehicle.getAxleCount() * 2];
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            mFrictionCoeffs[i * 2] = Vehicle.getAxle(i).getWheelOptions().getLongitudinalFriction();
            mFrictionCoeffs[i * 2 + 1] = Vehicle.getAxle(i).getWheelOptions().getLateralFriction();
        }

        //grab a copy of axle torque shares -> axleCt
        mTorqueShares = new float[Vehicle.getAxleCount()];
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            mTorqueShares[i] = Vehicle.getAxle(i).getTorqueShare();
        }
    }

    void FixedUpdate()
    {
        if (EnableRecoverAssists) updateRecoverAssists();
        if (EnableAntiRollBars) updateAntiRollBars();
        if (EnableTractionAssist) updateTractionAssist();
        if (EnableDriftAssist) updateDriftAssist();
        if (EnableTorqueSplitter) updateTorqueSplitter();
    }

    void updateRecoverAssists()
    {
        //check min. speed
        Vector3 dir2 = mTrans.forward;
        float dot1 = Vector3.Dot(dir2, Utility.ms2kmh * Vehicle.getVelocity());
        if (dot1 < MinRecoverAssistSpeed) return;

        //find speed coeff.
        bool frontContact = mFlw.hasContact() && mFrw.hasContact();
        bool rearContact = mRlw.hasContact() && mRrw.hasContact();
        float spdCoeff = 1.0f;
        if (FullRecoverAssistSpeed > 0.0f)
        {
            float limit = FullRecoverAssistSpeed - MinRecoverAssistSpeed;
            if (limit > 0f) spdCoeff = Mathf.Min(1f, (Vehicle.getKMHSpeed() - MinRecoverAssistSpeed) / limit);
        }

        if (frontContact)
        {
            Vector3 dir1 = 0.5f * (Vehicle.getAxle(0).getLeftWheel().getGlobalForward() + Vehicle.getAxle(0).getRightWheel().getGlobalForward());
            float angle = Vector3.SignedAngle(dir1, dir2, Vector3.up);
            float targetAngle = 0.5f * angle;
            float delta = targetAngle - angle;
            mBody.AddRelativeTorque(0.0f, 10f * spdCoeff * UnderSteerRecoverPower * delta, 0.0f);
        }

        if (rearContact)
        {
            float angleRatio = Mathf.Min(1f, Mathf.Abs(Vehicle.getSteerDeltaAngle(false) / 90f));
            mBody.AddRelativeTorque(0.0f, -20f * spdCoeff * OverSteerRecoverPower * mBody.angularVelocity.y * angleRatio, 0.0f);
        }
    }

    void updateAntiRollBars()
    {
        float flCompress = mFlw.getSuspensionCompressRatio();
        float frCompress = mFrw.getSuspensionCompressRatio();
        float rlCompress = mRlw.getSuspensionCompressRatio();
        float rrCompress = mRrw.getSuspensionCompressRatio();

        float frontAntiRoll = Mathf.Abs(flCompress - frCompress) * 0.01f * FrontAntiRollPower;
        float rearAntiRoll = Mathf.Abs(rlCompress - rrCompress) * 0.01f * RearAntiRollPower;
        
        //apply to front
        if (flCompress > frCompress)
        {
            float springLength = 1000.0f * mFlw.getSuspensionTotalLength();
            mFrw.applyVerticalForce(-rearAntiRoll * springLength * mFlw.getSuspensionSpring());
        }
        else if (frCompress > flCompress)
        {
            float springLength = 1000.0f * mFrw.getSuspensionTotalLength();
            mFlw.applyVerticalForce(-rearAntiRoll * springLength * mFrw.getSuspensionSpring());
        }

        //apply to rear
        if (rlCompress > rrCompress)
        {
            float springLength = 1000.0f * mRlw.getSuspensionTotalLength();
            mRrw.applyVerticalForce(-frontAntiRoll * springLength * mRlw.getSuspensionSpring());
        }
        else if (rrCompress > rlCompress)
        {
            float springLength = 1000.0f * mRrw.getSuspensionTotalLength();
            mRlw.applyVerticalForce(-frontAntiRoll * springLength * mRrw.getSuspensionSpring());
        }
    }

    void updateTractionAssist()
    {
        //only works when drift assist is off
        if (DriftAssistFrontRatio != 0f || DriftAssistRearRatio != 0f) return;

        //reset friction factors
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            Vehicle.getAxle(i).getWheelOptions().setLongitudinalFriction(mFrictionCoeffs[i * 2]);
            Vehicle.getAxle(i).getWheelOptions().setLateralFriction(mFrictionCoeffs[i * 2 + 1]);
        }

        if (TractionAssistRatio != 0f && Mathf.Abs(Vehicle.getKMHSpeed()) < TractionAssistMaxSpeed)
        {
            float magnitude = (TractionAssistMaxSpeed - Mathf.Abs(Vehicle.getKMHSpeed())) / TractionAssistMaxSpeed;
            magnitude *= 0.01f * TractionAssistRatio;
            for (int i = 0; i < Vehicle.getAxleCount(); i++)
            {
                Vehicle.getAxle(i).getWheelOptions().setLongitudinalFriction((1f + magnitude) * mFrictionCoeffs[i * 2]);
                Vehicle.getAxle(i).getWheelOptions().setLateralFriction((1f + magnitude) * mFrictionCoeffs[i * 2 + 1]);
            }
        }

        //apply
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            Vehicle.getAxle(i).applyWheelOptions();
        }
    }

    void updateDriftAssist()
    {
        //only works when traction assist is off
        if (TractionAssistRatio != 0f) return;

        //reset assist factors
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            Vehicle.getAxle(i).getWheelOptions().setLateralFriction(mFrictionCoeffs[i * 2 + 1]);
        }

        if (Vehicle.getForwardSpeed() < 1f) return;

        if (DriftAssistFrontRatio != 0f || DriftAssistRearRatio != 0f)
        {
            float angleRatio = Mathf.Min(1f, Mathf.Abs(Vehicle.getSteerDeltaAngle(false) / 90f));
            Vehicle.getAxle(0).getWheelOptions().setLateralFriction((1f + 0.01f * DriftAssistFrontRatio * angleRatio) * mFrictionCoeffs[1]);
            Vehicle.getAxle(1).getWheelOptions().setLateralFriction((1f + 0.01f * DriftAssistRearRatio * angleRatio) * mFrictionCoeffs[3]);
        }

        //apply
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            Vehicle.getAxle(i).applyWheelOptions();
        }
    }

    void updateTorqueSplitter()
    {
        if (TorqueSplitterRatio == 0f) return;

        float frontSpeed = Mathf.Abs(Vehicle.getAxle(0).getRPM());
        float rearSpeed = Mathf.Abs(Vehicle.getAxle(1).getRPM());
        float splitRatio = TorqueSplitterRatio / 100f;
        float frontShare = mTorqueShares[0];
        float rearShare = mTorqueShares[1];

        if (frontSpeed > rearSpeed)
        {
            float frontRatio = rearSpeed / frontSpeed;
            float rearRatio = 1f - frontRatio;
            frontShare = mTorqueShares[0] - splitRatio * rearRatio * mTorqueShares[0];
            rearShare = 1f - frontShare;
        }
        else if (rearSpeed > frontSpeed)
        {
            float rearRatio = frontSpeed / rearSpeed;
            float frontRatio = 1f - rearRatio;
            rearShare = mTorqueShares[1] - splitRatio * frontRatio * mTorqueShares[1];
            frontShare = 1f - rearShare;
        }

        float curFrontShare = Vehicle.getAxle(0).getTorqueShare();
        float curRearShare = Vehicle.getAxle(1).getTorqueShare();
        float time = Time.fixedDeltaTime / (0.001f * TorqueSplitterResponseTime);
        Vehicle.getAxle(0).setTorqueShare(Mathf.Lerp(curFrontShare, frontShare, time));
        Vehicle.getAxle(1).setTorqueShare(Mathf.Lerp(curRearShare, rearShare, time));
    }

    //set instant kmh speed
    public void setCruiseSpeed(float kmhSpeed, int gear)
    {
        //set rigidbody velocity
        float speed = Utility.kmh2ms * kmhSpeed;
        mBody.velocity = speed * Vehicle.getForwardDir();

        //set current gear
        if (gear >= -1 && gear <= Vehicle.getTransmission().getMaxGear())
        {
            Vehicle.getTransmission().setCurrentGear(gear, true);
        }

        //sync engine rpm, find fastest wheel rpm
        float wheelRPMToKMH = Vehicle.getMaxWheelRPMToKMH();
        float targetRpm = wheelRPMToKMH * Vehicle.getTransmission().getTransmissionRatio();
        Vehicle.getEngine().setRpm(targetRpm);
    }
}