// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System;
using UnityEngine;

namespace FGear
{
    [System.Serializable]
    public class Transmission : TelemetryDrawer
    {
        [SerializeField]
        float[] mGearRatios;
        [SerializeField]
        float mFinalGearRatio = 3f;
        [SerializeField]
        bool mAutoChange = true;
        [SerializeField]
        bool mAutoReverse = true;
        [SerializeField]
        bool mAutoClutch = true;
        [SerializeField, Range(1f, 1000f)]
        float mChangeTime = 100f;
        [SerializeField, Range(1f, 1000f)]
        float mClutchTime = 100f;
        [SerializeField, Range(0f, 100f)]
        float mGearUpRatio = 90f;
        [SerializeField, Range(0f, 100f)]
        float mGearDownRatio = 80f;
        [SerializeField]
        AnimationCurve mClutchEngagement;
        [SerializeField, Range(10f, 1000f)]
        float mClutchScale = 100f;

        [NonSerialized]
        Vehicle mVehicle;
        float[] mMaxSpeeds;
        int mCurGear = 0;
        int mNextGear = 0;
        int mMaxGear = 0;
        int mThrottleState = 0;
        float mPauseTime = 0f;
        float mUpSpeed = 0f;
        float mDownSpeed = 0f;
        float mClutchState = 1.0f;

        #region getters & setters
        public float getFinalGearRatio() { return mFinalGearRatio; }
        public bool isAutoChange() { return mAutoChange; }
        public bool isAutoReverse() { return mAutoReverse; }
        public bool isAutoClutch() { return mAutoClutch; }
        public float getChangeTime() { return mChangeTime; }
        public float getClutchTime() { return mClutchTime; }
        public float getGearUpRatio() { return mGearUpRatio; }
        public float getGearDownRatio() { return mGearDownRatio; }
        public float getClutchScale() { return mClutchScale; }
        public float getClutchPower() { return mClutchEngagement.Evaluate(mClutchState); }
        public float[] getMaxSpeeds() { return mMaxSpeeds; }
        public AnimationCurve getCluthEngagementCurve() { return mClutchEngagement; }
        public int getCurGear() { return mCurGear; }
        public int getMaxGear() { return mMaxGear; }
        public int getThrottleState() { return mThrottleState; }
        public void setGearCount(int size) { mGearRatios = new float[size]; }
        public void setGearRatio(int gear, float ratio) { mGearRatios[gear] = ratio; }
        public void setFinalGearRatio(float f) { mFinalGearRatio = f; }
        public void setAutoChange(bool b) { mAutoChange = b; }
        public void setAutoReverse(bool b) { mAutoReverse = b; }
        public void setAutoClutch(bool b) { mAutoClutch = b; }
        public void setChangeTime(float t) { mChangeTime = t; }
        public void setClutchTime(float t) { mClutchTime = t; }
        public void setGearUpRatio(float f) { mGearUpRatio = f; }
        public void setGearDownRatio(float f) { mGearDownRatio = f; }
        public void setThrottleState(int ts) { mThrottleState = ts; }
        public void setClutchState(float cs) { mClutchState = cs; }
        public void setPauseTime(float f) { mPauseTime = f; }
        public bool isChanging() { return mPauseTime > 0f; }
        #endregion

        public void init(Vehicle v)
        {
            //check nulls
            if (mGearRatios == null)
            {
                mGearRatios = new float[2];
                mGearRatios[0] = 5f;
                mGearRatios[1] = 3f;
            }

            if (mClutchEngagement == null) mClutchEngagement = new AnimationCurve();

            //do not leave curves empty
            if (mClutchEngagement.length <= 1)
            {
                mClutchEngagement = new AnimationCurve();
                mClutchEngagement.AddKey(0f, 0f);
                mClutchEngagement.AddKey(1f, 1f);
            }

            //start at 1st gear if auto
            if (mAutoChange)
            {
                mCurGear = mNextGear = 1;
            }

            mVehicle = v;
            refreshParameters(null);
            initTelemetryWindow();
        }

        //called by ui to get real time parameter update for gear ratios
        //vehicle parameter can be set when called from custom ui, otherwise keep it null
        public void refreshParameters(Vehicle v)
        {
            mMaxGear = mGearRatios.Length - 1; //-1 rear
            mMaxSpeeds = new float[mMaxGear + 1]; //+1 rear

            //check vehicle
            Vehicle cv = mVehicle;
            if (cv == null) cv = v;
            if (cv == null) return;

            //calculate max speed for each gear
            Engine e = cv.getEngine();
            float limitRpm = e != null ? e.getLimitRpm() : 5000f;
            for (int i = 0; i < mMaxSpeeds.Length; i++)
            {
                float gearRatio = (i == mMaxSpeeds.Length - 1) ? getGearRatio(-1) : getGearRatio(i + 1);
                float wheelRpm = limitRpm / (gearRatio * mFinalGearRatio);
                mMaxSpeeds[i] = wheelRpm * cv.getMaxWheelRPMToKMH();
            }
        }

        public void myFixedUpdate(float dt)
        {
            if (mPauseTime > 0f)
            {
                mPauseTime -= dt;
                if (mPauseTime < 0f) mPauseTime = 0f;
                return;
            }
            else if (mCurGear != mNextGear)
            {
                mCurGear = mNextGear;
            }

            //re-engage clutch
            if ((mAutoChange || mAutoClutch) && mClutchState < 1f)
            {
                mClutchState += dt / (0.001f * mClutchTime);
                if (mClutchState > 1f) mClutchState = 1f;
            }

            if (!mAutoChange) return;

            //do not stay N if auto
            if (mCurGear == 0 && mPauseTime == 0f)
            {
                gearUp(true);
                return;
            }

            mUpSpeed = float.PositiveInfinity;
            mDownSpeed = 0f;
            float wspeed = mVehicle.getMinWheelKMHSpeed();
            float speed = mVehicle.getKMHSpeed();

            if (mCurGear > 0 && mCurGear < mMaxGear)
            {
                mUpSpeed = mMaxSpeeds[mCurGear - 1] * (0.01f * mGearUpRatio);
            }

            if (mCurGear > 1)
            {
                mDownSpeed = mMaxSpeeds[mCurGear - 2] * (0.01f * mGearDownRatio);
            }

            if (mCurGear < 0)
            {
                mUpSpeed = 0f;
                mDownSpeed = float.NegativeInfinity;
            }

            //R -> 1
            if (mAutoReverse && mCurGear < 0 && wspeed > -1.0f && speed > -1.0f && mThrottleState > 0)
            {
                gearUp(true);
            }
            //1 -> R
            else if (mAutoReverse && mCurGear == 1 && wspeed < 1.0f && speed < 1.0f && mThrottleState < 0)
            {
                gearDown(true);
            }
            //X -> Y
            else if (speed > mUpSpeed && mCurGear > 0)
            {
                gearUp(true);
            }
            else if (speed < mDownSpeed && mCurGear > 1)
            {
                gearDown(true);
            }
        }

        public void setCurrentGear(int gear, bool force)
        {
            if (force)
            {
                mCurGear = gear;
                mNextGear = gear;
            }
            else if (gear != mCurGear && (mAutoClutch || mClutchState < 1f))
            {
                mNextGear = gear;
                //R
                if (mNextGear == 7) mNextGear = -1;
                //check overflow
                if (mNextGear > mMaxGear) mNextGear = 0;
                if (mAutoClutch) mClutchState = 0f;
            }
        }

        public void gearUp(bool skipN)
        {
            if (!mAutoChange && !mAutoClutch && mClutchState >= 1f) return;
            if (skipN && mCurGear == -1) mNextGear = 0;
            mNextGear++;
            if (mNextGear > mMaxGear)
            {
                mNextGear = mMaxGear;
                return;
            }
            mCurGear = 0;
            mClutchState = 0f;
            mPauseTime = mChangeTime / 1000f; //to secs.
            if (mNextGear > 1) mVehicle.getEngine().setLimiter(mPauseTime);
        }

        public void gearDown(bool skipN)
        {
            if (!mAutoChange && !mAutoClutch && mClutchState >= 1f) return;
            if (skipN && mCurGear == 1) mNextGear = 0;
            mNextGear--;
            if (mNextGear < -1)
            {
                mNextGear = -1;
                return;
            }
            mCurGear = 0;
            mClutchState = 0f;
            mPauseTime = mChangeTime / 1000f; //to secs.
            if (mNextGear > -1) mVehicle.getEngine().setLimiter(mPauseTime);
        }

        public float getTransmissionRatio()
        {
            return mFinalGearRatio * getGearRatio(mCurGear);
        }

        float getGearRatio(int gear)
        {
            //N
            if (gear == 0) return 0.0f;
            //R
            if (gear == -1) return -mGearRatios[mMaxGear];
            //D
            return mGearRatios[gear - 1];
        }

        public void saveState(ref VehicleState state)
        {
            state.mCurGear = mCurGear;
            state.mNextGear = mNextGear;
            state.mPauseTime = mPauseTime;
        }

        public void loadState(VehicleState state)
        {
            mCurGear = state.mCurGear;
            mNextGear = state.mNextGear;
            mPauseTime = state.mPauseTime;
        }

        protected override void initTelemetryWindow()
        {
            mWinID = Utility.winIDs++;
            mWindowRect = new Rect(Screen.width - 180, 320, 175, 170);
        }

        public override void drawTelemetry()
        {
            mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Transmission");
        }

        void uiWindowFunction(int windowID)
        {
            GUI.DragWindow();
            GUI.contentColor = Utility.textColor;
            GUI.Label(new Rect(10, 20, 150, 25), "Automatic: " + mAutoChange);
            GUI.Label(new Rect(10, 40, 150, 25), "Max Gear: " + mMaxGear);
            GUI.Label(new Rect(10, 60, 150, 25), "Cur Gear: " + (mCurGear < 0 ? "R" : (mCurGear == 0 ? "N" : mCurGear.ToString())));
            GUI.Label(new Rect(10, 80, 150, 25), "Change Time: " + (mPauseTime == 0f ? mChangeTime : (mChangeTime - 1000f * mPauseTime)).ToString("f0") + " ms");
            GUI.Label(new Rect(10, 100, 150, 25), "Up Speed: " + mUpSpeed.ToString("f0") + " kmh");
            GUI.Label(new Rect(10, 120, 150, 25), "Down Speed: " + mDownSpeed.ToString("f0") + " kmh");
            GUI.Label(new Rect(10, 140, 150, 25), "Clutch State: %" + (mClutchState * 100f).ToString("f0"));
        }
    }
}