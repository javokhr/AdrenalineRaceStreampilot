// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;

namespace FGear
{
    [System.Serializable]
    public class WheelOptions
    {
        [SerializeField]
        Tire mTireModel;
        [SerializeField]
        float mLateralFriction;
        [SerializeField]
        float mLongitudinalFriction;
        [SerializeField]
        float mLateralScale;
        [SerializeField]
        float mLongitudinalScale;
        [SerializeField]
        float mOverturnScale;
        [SerializeField]
        float mRollMomentScale;
        [SerializeField]
        float mSelfAlignScale;
        [SerializeField]
        float mRadius;
        [SerializeField]
        float mWidth;
        [SerializeField]
        float mMass;
        [SerializeField]
        float mTirePressure;
        [SerializeField]
        float mBrakeTorque;
        [SerializeField]
        float mSuspensionUpTravel;
        [SerializeField]
        float mSuspensionDownTravel;
        [SerializeField]
        float mSuspensionSpring;
        [SerializeField]
        float mCompressionDamper;
        [SerializeField]
        float mRelaxationDamper;
        [SerializeField]
        float mSuspensionPreload;

        #region getters & setters
        public Tire getTireModel() { return mTireModel; }
        public float getLateralFriction() { return mLateralFriction; }
        public float getLongitudinalFriction() { return mLongitudinalFriction; }
        public float getLateralScale() { return mLateralScale; }
        public float getLongitudinalScale() { return mLongitudinalScale; }
        public float getOverturnScale() { return mOverturnScale; }
        public float getRollMomentScale() { return mRollMomentScale; }
        public float getSelfAlignScale() { return mSelfAlignScale; }
        public float getRadius() { return mRadius; }
        public float getWidth() { return mWidth; }
        public float getMass() { return mMass; }
        public float getTirePressure() { return mTirePressure; }
        public float getBrakeTorque() { return mBrakeTorque; }
        public float getSuspensionUpTravel() { return mSuspensionUpTravel; }
        public float getSuspensionDownTravel() { return mSuspensionDownTravel; }
        public float getSuspensionSpring() { return mSuspensionSpring; }
        public float getCompressionDamper() { return mCompressionDamper; }
        public float getRelaxationDamper() { return mRelaxationDamper; }
        public float getSuspensionPreload() { return mSuspensionPreload; }
        public void setTireModel(Tire t) { mTireModel = t; }
        public void setLateralFriction(float f) { mLateralFriction = f; }
        public void setLongitudinalFriction(float f) { mLongitudinalFriction = f; }
        public void setLateralScale(float f) { mLateralScale = f; }
        public void setLongitudinalScale(float f) { mLongitudinalScale = f; }
        public void setOverturnScale(float f) { mOverturnScale = f; }
        public void setRollMomentScale(float f) { mRollMomentScale = f; }
        public void setSelfAlignScale(float f) { mSelfAlignScale = f; }
        public void setRadius(float f) { mRadius = f; }
        public void setWidth(float f) { mWidth = f; }
        public void setMass(float f) { mMass = f; }
        public void setTirePressure(float f) { mTirePressure = f; }
        public void setBrakeTorque(float f) { mBrakeTorque = f; }
        public void setSuspensionUpTravel(float f) { mSuspensionUpTravel = f; }
        public void setSuspensionDownTravel(float f) { mSuspensionDownTravel = f; }
        public void setSuspensionSpring(float f) { mSuspensionSpring = f; }
        public void setCompressionDamper(float f) { mCompressionDamper = f; }
        public void setRelaxationDamper(float f) { mRelaxationDamper = f; }
        public void setSuspensionPreload(float f) { mSuspensionPreload = f; }
        #endregion

        //called for default values in editor
        public void reset()
        {
            mLateralFriction = 1f;
            mLongitudinalFriction = 1f;
            mLateralScale = 1f;
            mLongitudinalScale = 1f;
            mOverturnScale = 0f;
            mRollMomentScale = 0f;
            mSelfAlignScale = 0f;
            mRadius = 0.25f;
            mWidth = 0.2f;
            mMass = 10f;
            mTirePressure = 2.0f;
            mBrakeTorque = 1000f;
            mSuspensionUpTravel = 0.1f;
            mSuspensionDownTravel = 0.1f;
            mSuspensionSpring = 20f;
            mCompressionDamper = 1000f;
            mRelaxationDamper = 1000f;
            mSuspensionPreload = 0.0f;
            checkNoTireState();
        }

        public void checkNoTireState()
        {
            if (mTireModel == null)
            {
                mTireModel = Resources.Load("Tires/Tire96Basic") as Tire;
                if (mTireModel == null) Debug.LogError("Failed to find a tire model, try adding manually from \"Create/FGear\" menu!");
            }
        }
    }
}