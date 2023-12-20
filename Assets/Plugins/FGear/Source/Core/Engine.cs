// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System;
using UnityEngine;

namespace FGear
{
    [System.Serializable]
    public class Engine : TelemetryDrawer
    {
        [SerializeField]
        AnimationCurve mTorqueCurve;
        [SerializeField]
        float mTorqueScale = 1f;
        [SerializeField]
        float mIdleRpm = 1000f;
        [SerializeField]
        float mLimitRpm = 6500f;
        [SerializeField]
        float mLimiterTime = 100f;
        [SerializeField]
        float mEngineInertia = 0.25f;
        [SerializeField]
        float mFrictionTorque = 50f;

        [NonSerialized]
        Transmission mTransmission;
        bool mRunning = true;
        float mRpm = 0f;
        float mThrottle = 0f;
        float mLimiter = 0f;
        float mTorque = 0f;
        float mFeedbackRpm = 0f;

        #region getters & setters
        public AnimationCurve getTorqueCurve() { return mTorqueCurve; }
        public float getIdleRpm() { return mIdleRpm; }
        public float getLimitRpm() { return mLimitRpm; }
        public float getLimiterTime() { return mLimiterTime; }
        public float getEngineInertia() { return mEngineInertia; }
        public float getFrictionTorque() { return mFrictionTorque; }
        public float getRpm() { return mRpm; }
        public float getThrottle() { return mThrottle; }
        public float getTorqueScale() { return mTorqueScale; }
        public float getTorque() { return mTorque; }
        public float getRpmRatio() { return mRpm / mLimitRpm; }
        public void setTorqueCurve(AnimationCurve c) { mTorqueCurve = c; }
        public void setTorqueScale(float f) { mTorqueScale = f; }
        public void setIdleRpm(float f) { mIdleRpm = f; }
        public void setLimitRpm(float f) { mLimitRpm = f; }
        public void setLimiterTime(float f) { mLimiterTime = f; }
        public void setEngineInertia(float f) { mEngineInertia = f; }
        public void setFrictionTorque(float f) { mFrictionTorque = f; }
        public void setThrottle(float pedal) { mThrottle = mRunning ? pedal : 0.0f; }
        public void setLimiter(float l) { mLimiter = l; }
        public void setRpm(float r) { mRpm = r; }
        public void resetRpm() { mRpm = mIdleRpm; }
        public bool isRunning() { return mRunning; }
        public bool isLimiterOn() { return mLimiter > 0f; }
        #endregion

        public void init(Transmission t)
        {
            if (mTorqueCurve == null)
            {
                mTorqueCurve = new AnimationCurve();
                mTorqueCurve.AddKey(0f, 0f);
                mTorqueCurve.AddKey(1000f, 75);
                mTorqueCurve.AddKey(4000f, 150f);
                mTorqueCurve.AddKey(6000, 100);
            }

            mTransmission = t;
            mRpm = mIdleRpm;
            initTelemetryWindow();
        }

        public void myFixedUpdate(float dt)
        {
            //pedal acceleration
            float angularAcc = mTorque / mEngineInertia / dt;
            float frictionTorque = (mRunning ? getRpmRatio() : 1.0f) * mFrictionTorque;
            if (mLimiter > 0f) angularAcc = -frictionTorque / mEngineInertia / dt;
            float angularVel = angularAcc * dt;
            float rpmVel1 = angularVel * Utility.rads2Rpm;

            //update rpm
            mRpm += rpmVel1 * dt;

            //apply limits
            if (mRunning && mRpm < mIdleRpm && rpmVel1 < 0.0f) mRpm -= rpmVel1 * dt;
            if (mRpm > mLimitRpm) { mRpm = mLimitRpm; mLimiter = mLimiterTime / 1000f; }
            else if (mRpm < 0.0f) mRpm = 0.0f;
            if (mLimiter > 0f) mLimiter -= dt;

            //torque with pedal
            mTorque = mThrottle > 0f ? mThrottle * mTorqueCurve.Evaluate(mRpm) : 0f;
            
            //apply friction
            mTorque -= frictionTorque;

            //scale friction torque
            //this prevents jittery behavior when wheels are close to 0 rpm
            if (mTorque == -frictionTorque)
            {
                bool transOn = mTransmission.getCurGear() != 0 && mTransmission.getClutchPower() != 0f;
                if (transOn && mFeedbackRpm != 0f)
                {
                    mTorque *= Mathf.Clamp(mFeedbackRpm / mIdleRpm, -1f, 1f);
                }
            }

            //finalize
            mTorque *= mTorqueScale;
        }

        public void setFeedback(float dt, float rpm, float torque)
        {
            //override FeedbackTorque acc. to clutch
            float clutchPower = mTransmission.getClutchPower();
            torque *= clutchPower;

            //feedback drive
            rpm = Mathf.Clamp(rpm, mRunning ? mIdleRpm : 0.0f, mLimitRpm);
            float sign = Math.Sign(rpm - mRpm); //!signum
            float totalTorque = sign * Mathf.Abs(torque);
            float angularAcc = totalTorque / mEngineInertia / dt;
            float angularVel = angularAcc * dt;
            float rpmVel2 = angularVel * Utility.rads2Rpm;
            if (mLimiter > 0) rpmVel2 = 0f;

            //alter feedback drive to avoid overshoots and take clutch into account
            float deltaRpm = Mathf.Abs(rpm - mRpm);
            if (Mathf.Abs(rpmVel2) * dt > deltaRpm)
            {
                rpmVel2 = Mathf.Sign(rpmVel2) * (deltaRpm / dt) * clutchPower;
            }

            //update rpm
            mRpm += rpmVel2 * dt;
            mFeedbackRpm = rpm;
        }

        public void setEngineRunning(bool b)
        {
            mRunning = b;
            mThrottle = 0.0f;
        }

        //return max power in kW
        public void calculateMaxPower(out float maxPower, out float maxPowerRpm)
        {
            maxPower = maxPowerRpm = 0f;
            for (float rpm = mIdleRpm; rpm <= mLimitRpm; rpm++)
            {
                float torque = mTorqueScale * mTorqueCurve.Evaluate(rpm);
                float power = torque * rpm / 9548.8f;
                if (power > maxPower)
                {
                    maxPower = power;
                    maxPowerRpm = rpm;
                }
            }
        }

        public void saveState(ref VehicleState state)
        {
            state.mRpm = mRpm;
            state.mLimiter = mLimiter;
            state.mFeedbackRpm = mFeedbackRpm;
        }

        public void loadState(VehicleState state)
        {
            mRpm = state.mRpm;
            mLimiter = state.mLimiter;
            mFeedbackRpm = state.mFeedbackRpm;
        }

        protected override void initTelemetryWindow()
        {
            mWinID = Utility.winIDs++;
            mWindowRect = new Rect(Screen.width - 180, 185, 175, 130);
        }

        public override void drawTelemetry()
        {
            mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Engine");
        }

        void uiWindowFunction(int windowID)
        {
            //current power
            float torque = mTorque + mTorqueScale * mFrictionTorque;
            float power = (torque * mRpm / 9548.8f) * Utility.kw2hp;

            GUI.DragWindow();
            GUI.contentColor = Utility.textColor;
            GUI.Label(new Rect(10, 20, 100, 25), "Rpm: " + mRpm.ToString("f0"));
            GUI.Label(new Rect(10, 40, 100, 25), "LimitRpm: " + mLimitRpm.ToString("f0"));
            GUI.Label(new Rect(10, 60, 100, 25), "Limiter: " + isLimiterOn());
            GUI.Label(new Rect(10, 80, 100, 25), "Torque: " + torque.ToString("f0") + " Nm");
            GUI.Label(new Rect(10, 100, 100, 25), "Power: " + power.ToString("f0") + " hp");
        }
    }
}