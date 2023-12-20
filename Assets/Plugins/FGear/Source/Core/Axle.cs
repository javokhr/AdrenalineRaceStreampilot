// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System;
using UnityEngine;

namespace FGear
{
    [System.Serializable]
    public class Axle : TelemetryDrawer
    {
        public enum DifferentialType
        {
            DF_OPEN,
            DF_LOCKED,
            DF_LSD
        }

        [SerializeField]
        DifferentialType mDifferentialType;
        [SerializeField]
        WheelOptions mWheelOptions;
        [SerializeField]
        Wheel mWheelLeft;
        [SerializeField]
        Wheel mWheelRight;
        [SerializeField, Range(0f, 1f)]
        float mTorqueShare = 0.5f;
        [SerializeField, Range(-90f, 90f)]
        float mMaxSteerAngle = 0f;
        [SerializeField]
        [Range(-1.0f, 1.0f)]
        float mAckermanCoeff = 1f;
        [SerializeField]
        int mAckermanReferenceIndex = 1;
        [SerializeField]
        bool mHasHandbrake = false;
        [SerializeField, Range(0f, 1f)]
        float mDiffStrength = 0.35f;
        [SerializeField]
        [Range(-10f, 10f)]
        float mCamberAngle = 0f;
        [SerializeField]
        [Range(-10f, 10f)]
        float mToeAngle = 0f;

        int mIndex = 0;
        [NonSerialized]
        Vehicle mVehicle;
        [NonSerialized]
        Engine mEngine;
        [NonSerialized]
        Transmission mTransmission;

        //torque
        float mAxleTorque = 0f;
        float mLeftTorque = 0f;
        float mRightTorque = 0f;

        //steering
        float mCurrentSteerLeft = 0f;
        float mCurrentSteerRight = 0f;
        float mTargetSteer = 0f;

        #region getters & setters
        public DifferentialType getDifferentialType() { return mDifferentialType; }
        public WheelOptions getWheelOptions() { return mWheelOptions; }
        public Wheel getLeftWheel() { return mWheelLeft; }
        public Wheel getRightWheel() { return mWheelRight; }
        public float getTorqueShare() { return mTorqueShare; }
        public float getMaxSteerAngle() { return mMaxSteerAngle; }
        public float getAckermanCoeff() { return mAckermanCoeff; }
        public int getAckermanReferenceIndex() { return mAckermanReferenceIndex; }
        public bool hasHandBrake() { return mHasHandbrake; }
        public float getDiffStrength() { return mDiffStrength; }
        public float getCamberAngle() { return mCamberAngle; }
        public float getToeAngle() { return mToeAngle; }
        public int getIndex() { return mIndex; }
        public void setDifferentialType(DifferentialType dt) { mDifferentialType = dt; }
        public void applyWheelOptions() { mWheelLeft.refreshParameters(null); mWheelRight.refreshParameters(null); } //call after modifying wheel options
        public void setTorqueShare(float f) { mTorqueShare = f; }
        public void setMaxSteerAngle(float f) { mMaxSteerAngle = f; }
        public void setAckermanCoeff(float f) { mAckermanCoeff = f; }
        public void setAckermanReferenceIndex(int i) { mAckermanReferenceIndex = i; }
        public void setHasHandBrake(bool b) { mHasHandbrake = b; }
        public void setDiffStrength(float f) { mDiffStrength = f; }
        public void setCamberAngle(float f) { mCamberAngle = f; }
        public void setToeAngle(float f) { mToeAngle = f; }
        #endregion

        public void init(Vehicle v, int index)
        {
            //check nulls
            if (mWheelOptions == null)
            {
                mWheelOptions = new WheelOptions();
                mWheelOptions.reset();
            }
            if (mWheelLeft == null) mWheelLeft = new Wheel();
            if (mWheelRight == null) mWheelRight = new Wheel();

            mIndex = index;
            mVehicle = v;
            mEngine = v.getEngine();
            mTransmission = v.getTransmission();
            mWheelOptions.checkNoTireState(); //check before wheels
            mWheelLeft.init(v, this);
            mWheelRight.init(v, this);
            initTelemetryWindow();
        }

        //called from Vehicle.Reset
        //used for custom ui to get default values
        public void reset()
        {
            if (mWheelOptions == null)
            {
                mWheelOptions = new WheelOptions();
            }
            mWheelOptions.reset();
            mTorqueShare = 0.5f;
            mMaxSteerAngle = 0f;
            mAckermanCoeff = 1.0f;
            mAckermanReferenceIndex = 1;
            mHasHandbrake = false;
            mDiffStrength = 0.25f;
            mCamberAngle = 0f;
            mToeAngle = 0f;
        }

        public void preWheelupdate(bool firstStep)
        {
            if (firstStep) steeringUpdate();
            calculateAxleTorque();
        }

        public void postWheelupdate()
        {
            //force same speed of dominating wheel
            if (mDifferentialType == DifferentialType.DF_LOCKED)
            {
                float leftFeed = mWheelLeft.getFeedbackTorque();
                float rightFeed = mWheelRight.getFeedbackTorque();
                if (leftFeed > rightFeed) mWheelRight.setSpeed(mWheelLeft.getSpeed());
                else if (leftFeed < rightFeed) mWheelLeft.setSpeed(mWheelRight.getSpeed());
            }
        }

        void steeringUpdate()
        {
            if (hasSteering())
            {
                mCurrentSteerLeft = mTargetSteer;
                mCurrentSteerRight = mTargetSteer;

                //ackerman
                bool outerLeft = mCurrentSteerLeft != 0.0f && Mathf.Sign(mWheelLeft.getLocalHubPosition().x) != Mathf.Sign(mCurrentSteerLeft);
                bool outerRight = mCurrentSteerRight != 0.0f && Mathf.Sign(mWheelRight.getLocalHubPosition().x) != Mathf.Sign(mCurrentSteerRight);
                if (mAckermanCoeff != 0.0f && (outerLeft || outerRight))
                {
                    //reversed case
                    if (mMaxSteerAngle < 0.0f)
                    {
                        outerLeft = !outerLeft;
                        outerRight = !outerRight;
                    }
                    float localTargetSteer = outerLeft ? mCurrentSteerLeft : mCurrentSteerRight;
                    float d = getTrackWidth();
                    float l = mVehicle.getAxleDistance(mIndex, mAckermanReferenceIndex);
                    float absTarget = Mathf.Abs(localTargetSteer) * Mathf.Deg2Rad;
                    float turnRadius = l / Mathf.Tan(absTarget);
                    float Y = Mathf.Atan2(l, d + turnRadius) * Mathf.Rad2Deg;
                    localTargetSteer = Mathf.Sign(localTargetSteer) * Mathf.Abs(Y);
                    if (outerLeft) mCurrentSteerLeft = Mathf.Lerp(mTargetSteer, localTargetSteer, mAckermanCoeff);
                    else mCurrentSteerRight = Mathf.Lerp(mTargetSteer, localTargetSteer, mAckermanCoeff);
                }

                //apply
                mWheelLeft.setCurrentSteer(mCurrentSteerLeft);
                mWheelRight.setCurrentSteer(mCurrentSteerRight);
            }
        }

        void calculateAxleTorque()
        {
            float engineTorque = mEngine.getTorque();
            float transRatio = mTransmission.getTransmissionRatio() * mTransmission.getClutchPower();
            mAxleTorque = transRatio * engineTorque * mTorqueShare;
            mLeftTorque = mRightTorque = 0f;
            if (mAxleTorque == 0f) return;

            if (mDifferentialType == DifferentialType.DF_OPEN)
            {
                mLeftTorque = mRightTorque = getOpenDiffTorque();
            }
            else if (mDifferentialType == DifferentialType.DF_LSD)
            {
                float openTorque = getOpenDiffTorque();
                mLeftTorque = Mathf.Lerp(openTorque, getLockedDiffTorque(mWheelLeft), mDiffStrength);
                mRightTorque = Mathf.Lerp(openTorque, getLockedDiffTorque(mWheelRight), mDiffStrength);
            }
            else if (mDifferentialType == DifferentialType.DF_LOCKED)
            {
                mLeftTorque = getLockedDiffTorque(mWheelLeft);
                mRightTorque = getLockedDiffTorque(mWheelRight);
            }
        }

        private float getOpenDiffTorque()
        {
            float leftSpeed = Mathf.Abs(mWheelLeft.getSpeed());
            float rightSpeed = Mathf.Abs(mWheelRight.getSpeed());
            
            float leftFeed = mWheelLeft.hasContact() ? mWheelLeft.getFeedbackTorque() : 1f;
            float rightFeed = mWheelRight.hasContact() ? mWheelRight.getFeedbackTorque() : 1f;

            if (leftSpeed > rightSpeed)
            {
                if (mAxleTorque > 0) return Mathf.Min(leftFeed, 0.5f * mAxleTorque);
                else return Mathf.Max(-leftFeed, 0.5f * mAxleTorque);
            }
            else if (rightSpeed > leftSpeed)
            {
                if (mAxleTorque > 0) return Mathf.Min(rightFeed, 0.5f * mAxleTorque);
                else return Mathf.Max(-rightFeed, 0.5f * mAxleTorque);
            }
            else return 0.5f * mAxleTorque;
        }

        private float getLockedDiffTorque(Wheel w)
        {
            float leftFeed = mWheelLeft.hasContact() ? mWheelLeft.getFeedbackTorque() : 1f;
            float rightFeed = mWheelRight.hasContact() ? mWheelRight.getFeedbackTorque() : 1f;
            float totalFeed = leftFeed + rightFeed;
            float leftRatio = 0.5f;
            float rightRatio = 0.5f;
            if (totalFeed != 0f)
            {
                leftRatio = leftFeed / totalFeed;
                rightRatio = rightFeed / totalFeed;
            }

            if (mAxleTorque > 0)
            {
                if (w == mWheelLeft) return Mathf.Min(leftFeed, leftRatio * mAxleTorque);
                if (w == mWheelRight) return Mathf.Min(rightFeed, rightRatio * mAxleTorque);
            }
            else if (mAxleTorque < 0)
            {
                if (w == mWheelLeft) return Mathf.Max(-leftFeed, leftRatio * mAxleTorque);
                if (w == mWheelRight) return Mathf.Max(-rightFeed, rightRatio * mAxleTorque);
            }

            return 0f;
        }

        public float getTorque(Wheel w)
        {
            if (w == mWheelLeft) return mLeftTorque;
            if (w == mWheelRight) return mRightTorque;
            return 0f;
        }

        public float getCurRpm()
        {
            float transRatio = mTransmission.getTransmissionRatio();
            if (mTorqueShare == 0f || transRatio == 0f) return 0f;
            return mEngine.getRpm() / transRatio;
        }

        public float getMaxRpm()
        {
            float transRatio = mTransmission.getTransmissionRatio();
            if (mTorqueShare == 0f || transRatio == 0f) return 0f;
            return mEngine.getLimitRpm() / transRatio;
        }

        public float getFeedbackTorque()
        {
            float transRatio = mTransmission.getTransmissionRatio();
            if (mTorqueShare == 0.0f || transRatio == 0.0f) return 0.0f;
            return (mWheelLeft.getFeedbackTorque() + mWheelRight.getFeedbackTorque()) / transRatio;
        }

        public float getWheelRPMToKMH()
        {
            return 0.5f * (mWheelLeft.getWheelRPMToKMH() + mWheelRight.getWheelRPMToKMH());
        }

        public float getNormalizedSteering()
        {
            if (mCurrentSteerLeft + mCurrentSteerRight > 0.0f) return mCurrentSteerRight / mMaxSteerAngle;
            else return mCurrentSteerLeft / mMaxSteerAngle;
        }

        public Vector3 getLocalCenter()
        {
            return 0.5f * (mWheelLeft.getLocalHubPosition() + mWheelRight.getLocalHubPosition());

        }

        public Vector3 getGlobalCenter()
        {
            return 0.5f * (mWheelLeft.getHubPosition() + mWheelRight.getHubPosition());
        }

        public float getRPM()
        {
            if (!mWheelLeft.isActive()) return mWheelRight.getRpm();
            if (!mWheelRight.isActive()) return mWheelLeft.getRpm();
            return 0.5f * (mWheelLeft.getRpm() + mWheelRight.getRpm());
        }

        public float getTrackWidth()
        {
            return Mathf.Abs(mWheelLeft.getLocalHubPosition().x - mWheelRight.getLocalHubPosition().x);
        }

        public bool hasSteering()
        {
            return mMaxSteerAngle != 0f;
        }

        public void setTargetSteer(float s)
        {
            if (hasSteering()) mTargetSteer = s * mMaxSteerAngle;
        }

        public void saveState(ref VehicleState state)
        {
            mWheelLeft.saveState(ref state);
            mWheelRight.saveState(ref state);
        }

        public void loadState(VehicleState state)
        {
            mWheelLeft.loadState(state);
            mWheelRight.loadState(state);
        }

        protected override void initTelemetryWindow()
        {
            mWinID = Utility.winIDs++;
            mWindowRect = new Rect(5 + mIndex * 180, Screen.height - 180, 175, 175);
        }

        public override void drawTelemetry()
        {
            mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Axle " + mIndex);
        }

        void uiWindowFunction(int windowID)
        {
            GUI.DragWindow();
            GUI.contentColor = Utility.textColor;
            GUI.Label(new Rect(10, 20, 175, 25), "TireModel: " + mWheelOptions.getTireModel().name);
            GUI.Label(new Rect(10, 40, 150, 25), "Differential: " + mDifferentialType.ToString().Substring(3));
            GUI.Label(new Rect(10, 60, 150, 25), "Has Power: " + (mTorqueShare != 0f));
            GUI.Label(new Rect(10, 80, 150, 25), "Has Steering: " + hasSteering());
            GUI.Label(new Rect(10, 100, 150, 25), "Torque Share: %" + (100f * mTorqueShare).ToString("f0"));
            GUI.Label(new Rect(10, 120, 150, 25), "Camber Angle: " + mCamberAngle.ToString("f2"));
            GUI.Label(new Rect(10, 140, 150, 25), "Toe Angle: " + mToeAngle.ToString("f2"));
        }

        public void drawGizmo()
        {
            if (mWheelLeft.getWheelTransform() == null || mWheelRight.getWheelTransform() == null) return;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(mWheelLeft.getWheelPosition(), mWheelRight.getWheelPosition());
            if (mWheelLeft.getWheelTransform() != null) mWheelLeft.drawGizmo();
            if (mWheelRight.getWheelTransform() != null) mWheelRight.drawGizmo();
        }
    }
}