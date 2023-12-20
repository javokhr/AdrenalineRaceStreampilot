// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;

namespace FGear
{
    public class VehicleState
    {
        public float mStateTime;

        //kinematic state
        public Vector3 mPosition;
        public Quaternion mRotation;
        public Vector3 mVelocity;
        public Vector3 mAngularVelocity;

        //vehicle state
        public float mVehicleTime;

        //engine state
        public float mRpm;
        public float mLimiter;
        public float mFeedbackRpm;

        //transmission state
        public int mCurGear;
        public int mNextGear;
        public float mPauseTime;

        //input state
        public InputState mInput;

        //wheel states
        public bool[,] mActive;
        public float[,] mLastCompressLength;
        public float[,] mPitch;
        public float[,] mRealSpeed;

        public void reset()
        {
            mStateTime = mPauseTime = 0.0f;
            mPosition = mVelocity = mAngularVelocity = Vector3.zero;
            mRotation = Quaternion.identity;
            mVehicleTime = mRpm = mLimiter = mFeedbackRpm = 0.0f;
            mCurGear = mNextGear = 0;
            mInput.reset();
            if (mActive == null) return;
            for (int i=0; i<mActive.GetLength(0); i++)
            {
                for (int j = 0; j<mActive.GetLength(1); j++)
                {
                    mActive[i, j] = true;
                    mLastCompressLength[i, j] = 0.0f;
                    mPitch[i, j] = mRealSpeed[i, j] = 0.0f;
                }
            }
        }

        public class InputState
        {
            public InputState(int t, float ei, float bi, float si, float ci, int gi, bool hbi)
            {
                mTick = t;
                mEngine = ei;
                mBrake = bi;
                mSteer = si;
                mClutch = ci;
                mGearbox = gi;
                mHandbrake = hbi;
            }

            public InputState()
            {
            }

            public bool isSame(InputState s)
            {
                return mEngine == s.mEngine && mBrake == s.mBrake && mSteer == s.mSteer &&
                       mClutch == s.mClutch && mGearbox == s.mGearbox && mHandbrake == s.mHandbrake;
            }

            public void reset()
            {
                mTick = mLife = 0;
                mEngine = mBrake = 0f;
                mSteer = mClutch = 0f;
                mGearbox = mThrottle = 0;
                mHandbrake = false;
            }

            public int mTick = 0;
            public int mLife = 0;
            public float mEngine = 0f;
            public float mBrake = 0f;
            public float mSteer = 0f;
            public float mClutch = 0f;
            public int mGearbox = 0;
            public int mThrottle = 0;
            public bool mHandbrake = false;
        }
    }
}