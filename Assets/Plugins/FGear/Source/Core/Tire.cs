// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;

namespace FGear
{
    public class Tire : ScriptableObject
    {
        public enum ForceCombineMode
        {
            GRIP,
            SIMPLE,
            SUM
        }
        
        [SerializeField]
        protected bool mForceSymmetry = false;
        protected ForceCombineMode mCombineMode;
        float mReferenceLoad = 3000f;
        float mReferenceTirePressure = 2f;
        
        //extremes
        float mMaxLongitudinalForce;
        float mMaxLongitudinalForceSlip;
        float mMaxLateralForce;
        float mMaxLateralForceAngle;

        //output
        protected float mLongitudinalForce;
        protected float mLateralForce;
        protected Vector3 mCombinedTorque;

        #region getters & setters
        public ForceCombineMode getCombineMode() { return mCombineMode; }
        public float getMaxLongitudinalForce() { return mMaxLongitudinalForce; }
        public float getMaxLongitudinalForceSlip() { return mMaxLongitudinalForceSlip; }
        public float getMaxLateralForce() { return mMaxLateralForce; }
        public float getMaxLateralForceAngle() { return mMaxLateralForceAngle; }
        public float getLongitudinalForce() { return mLongitudinalForce; }
        public float getLateralForce() { return mLateralForce; }
        public Vector3 getCombinedTorque() { return mCombinedTorque; }
        #endregion

        void Awake()
        {
            init();
        }

        void Reset()
        {
            init();
        }

        protected virtual void init()
        {
            calculateMaxLongitudinalForce();
            calculateMaxLateralForce();
            mLongitudinalForce = mLateralForce = 0f;
            mCombinedTorque = Vector3.zero;
        }

        public virtual void calculate(float load, float slipRatio, float slipAngle, float camber, float pressure, float radius, float Vx)
        {
            mLongitudinalForce = mLateralForce = 0f;
            mCombinedTorque = Vector3.zero;
        }

        void calculateMaxLongitudinalForce()
        {
            mMaxLongitudinalForce = -float.MaxValue;

            for (float i = 0f; i <= 1f; i += 0.01f)
            {
                calculate(mReferenceLoad, i, 0f, 0f, mReferenceTirePressure, 0f, 0f);
                if (mLongitudinalForce > mMaxLongitudinalForce)
                {
                    mMaxLongitudinalForce = mLongitudinalForce;
                    mMaxLongitudinalForceSlip = i;
                }
            }
        }

        void calculateMaxLateralForce()
        {
            mMaxLateralForce = -float.MaxValue;

            for (float i = 0f; i <= 1f; i += 0.01f)
            {
                float angle = i * 0.5f * Mathf.PI;
                calculate(mReferenceLoad, 0f, angle, 0f, mReferenceTirePressure, 0f, 0f);
                if (mLateralForce > mMaxLateralForce)
                {
                    mMaxLateralForce = mLateralForce;
                    mMaxLateralForceAngle = angle;
                }
            }
        }

        public void save(string path)
        {
            if (path.Length == 0) return;
            string json = JsonUtility.ToJson(this, true);
            System.IO.File.WriteAllText(path, json);
        }

        public void load(string path)
        {
            if (path.Length == 0) return;
            string json = System.IO.File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}