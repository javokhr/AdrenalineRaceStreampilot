// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;

namespace FGear
{
    [RequireComponent(typeof(Rigidbody)), DisallowMultipleComponent]
    public class Vehicle : MonoBehaviour
    {
        [System.Serializable]
        class TelemetryOptions
        {
            public Color TextColor = Color.black;
            public bool Show = false;
            public bool ShowAeroDynamics = false;
            public bool ShowAxles = false;
            public bool ShowEngine = false;
            public bool ShowInput = false;
            public bool ShowTransmission = false;
            public bool ShowVehicle = false;
            public bool ShowWheels = false;
        }

        public enum CastType
        {
            RAY,
            SPHERE,
            CONVEX
        }

        [SerializeField]
        Engine mEngine;
        [SerializeField]
        Transmission mTransmission;
        [SerializeField]
        Axle[] mAxleList;
        [SerializeField]
        StandardInput mStandardInput;
        [SerializeField]
        AeroDynamics mAeroDynamics;
        [SerializeField]
        TelemetryOptions mTelemetry;
        [SerializeField, Range(10, 1000)]
        int mUpdateRate = 250;
        [SerializeField]
        float mMass = 1000f;
        [SerializeField]
        Vector3 mInertiaScale = Vector3.one;
        [SerializeField]
        Vector3 mCenterofMass = Vector3.zero;
        [SerializeField]
        CastType mCastType = CastType.RAY;
        [SerializeField, Range(1, 49)]
        int mRays = 1;
        [SerializeField, Range(1, 3)]
        int mLateralRays = 1;
        [SerializeField]
        LayerMask mRaycastLayer = 1;
        [SerializeField, Range(1f, 100f)]
        float mHandBrakePower = 1f;
        [SerializeField]
        float mRollingResistanceCoeff = 1.0f;
        [SerializeField]
        bool mAllowReverse = true;
        [SerializeField]
        bool mHardContact = false;
        [SerializeField]
        bool mRelaxationDownforce = false;
        [SerializeField]
        bool mTirePenetration = true;
        [SerializeField]
        bool mDetectGround = false;
        [SerializeField, Range(0f, 1f)]
        float mReactionScale = 0.0f;
        [SerializeField]
        AnimationCurve mBrakeEngagement;
        [SerializeField, Range(0f, 100f)]
        float mABS = 0f;
        [SerializeField, Range(0f, 100f)]
        float mASR = 0f;
        [SerializeField, Range(0f, 100f)]
        float mUnderSteerAssist = 0f;
        [SerializeField, Range(1f, 90f)]
        float mMinUnderSteerAngle = 10f;
        [SerializeField, Range(0f, 100f)]
        float mOverSteerAssist = 0f;
        [SerializeField, Range(1f, 90f)]
        float mMinOverSteerAngle = 20f;
        [SerializeField, Range(0f, 100f)]
        float mFrontAntiRollPower = 0f;
        [SerializeField, Range(0f, 100f)]
        float mRearAntiRollPower = 0f;
        [SerializeField, Range(0f, 5f)]
        float mStickyTireSpeed = 0f;

        Rigidbody mBody;
        Transform mTrans;
        Vector3[] mWheelForces;
        Vector3[] mWheelTorques;
        bool mHandbrakeOn;
        bool mEspActive;
        bool mUnderSteering;
        bool mOverSteering;
        float mStickyTireState;

        //update
        float mTime;
        float mSimCoeff;
        bool mManualUpdate = false;
        bool mUpdateInputs = true;

        //telemetry wins
        Rect mWindowRectVehicle;
        Rect mWindowRectWheel;
        int mWinIDV, mWinIDW;

        #region getters & setters
        public int getUpdateRate() { return mUpdateRate; }
        public float getMass() { return mMass; }
        public Vector3 getInertiaScale() { return mInertiaScale; }
        public Vector3 getCenterofMass() { return mCenterofMass; }
        public CastType getCastType() { return mCastType; }
        public int getRayCount() { return mRays; }
        public int getRaycastLayer() { return mRaycastLayer; }
        public int getLateralRayCount() { return mLateralRays; }
        public float getHandbrakePower() { return mHandBrakePower; }
        public float getRollingResistanceCoeff() { return mRollingResistanceCoeff; }
        public bool getAllowReverse() { return mAllowReverse; }
        public bool getHardContact() { return mHardContact; }
        public bool getRelaxationDownforce() { return mRelaxationDownforce; }
        public bool getTirePenetration() { return mTirePenetration; }
        public bool getDetectGround() { return mDetectGround; }
        public float getReactionScale() { return mReactionScale; }
        public float getABS() { return 0.01f * mABS; }
        public float getASR() { return 0.01f * mASR; }
        public Rigidbody getBody() { return mBody; }
        public Engine getEngine() { return mEngine; }
        public Transmission getTransmission() { return mTransmission; }
        public StandardInput getStandardInput() { return mStandardInput; }
        public AeroDynamics getAeroDynamics() { return mAeroDynamics; }
        public Axle getAxle(int i) { return mAxleList[i]; }
        public Vector3 getPosition() { return mTrans.position; }
        public Quaternion getRotation() { return mTrans.rotation; }
        public Vector3 getForwardDir() { return mTrans.forward; }
        public Vector3 getUpDir() { return mTrans.up; }
        public Vector3 getVelocityDir() { return getVelocity().normalized; }
        public int getWheelCount() { return 2 * mAxleList.Length; }
        public int getAxleCount() { return mAxleList.Length; }
        public float getForwardSpeed() { return mTrans.InverseTransformVector(getVelocity()).z; }
        public float getRightSpeed() { return mTrans.InverseTransformVector(getVelocity()).x; }
        public float getKMHSpeed() { return getForwardSpeed() * Utility.ms2kmh; }
        public float getVelocitySize() { return getVelocity().magnitude; }
        public float getMassPerWheel() { return mBody.mass / getWheelCount(); }
        public float getSimCoeff() { return mSimCoeff; }
        public bool isHandbrakeOn() { return mHandbrakeOn; }
        public bool getESPActive() { return mEspActive; }
        public bool isOverSteering() { return mOverSteering; }
        public bool isUnderSteering() { return mUnderSteering; }
        public float getStickyTireState() { return mStickyTireState; }
        public void setUpdateRate(int i) { mUpdateRate = Mathf.Clamp(i, 10, 1000); }
        public void setCastType(CastType ct) { mCastType = ct; }
        public void setRayCount(int ct) { mRays = ct; }
        public void setRaycastLayer(int l) { mRaycastLayer = l; }
        public void setLateralRayCount(int ct) { mLateralRays = ct; }
        public void setHandbrakePower(float f) { mHandBrakePower = f; }
        public void setRollingResistanceCoeff(float f) { mRollingResistanceCoeff = f; }
        public void setAllowReverse(bool allow) { mAllowReverse = allow; }
        public void setHardContact(bool hc) { mHardContact = hc; }
        public void setRelaxationDownforce(bool rd) { mRelaxationDownforce = rd; }
        public void setTirePenetration(bool tp) { mTirePenetration = tp; }
        public void setDetectGround(bool dg) { mDetectGround = dg; }
        public void setReactionScale(float f) { mReactionScale = f; }
        public void setABS(float f) { mABS = f; }
        public void setASR(float f) { mASR = f; }
        public void setUnderSteerAssist(float f) { mUnderSteerAssist = f; }
        public void setMinUnderSteerAngle(float f) { mMinUnderSteerAngle = f; }
        public void setOverSteerAssist(float f) { mOverSteerAssist = f; }
        public void setMinOverSteerAngle(float f) { mMinOverSteerAngle = f; }
        public void setFrontAntiRollPower(float f) { mFrontAntiRollPower = f; }
        public void setRearAntiRollPower(float f) { mRearAntiRollPower = f; }
        public void setStickySpeed(float f) { mStickyTireSpeed = f; }
        public void setManualUpdate(bool b) { mManualUpdate = b; }
        public void setUpdateInputs(bool b) { mUpdateInputs = b; }
        #endregion

        void Awake()
        {
            //check nulls
            if (mAxleList == null)
            {
                mAxleList = new Axle[2];
                mAxleList[0] = new Axle();
                mAxleList[1] = new Axle();
            }
            if (mTelemetry == null) mTelemetry = new TelemetryOptions();
            if (mEngine == null) mEngine = new Engine();
            if (mTransmission == null) mTransmission = new Transmission();
            if (mStandardInput == null) mStandardInput = new StandardInput();
            if (mAeroDynamics == null) mAeroDynamics = new AeroDynamics();

            mBody = GetComponent<Rigidbody>();
            mBody.mass = mMass;
            mBody.centerOfMass += mCenterofMass;
            Vector3 inertia = mBody.inertiaTensor;
            inertia.Scale(mInertiaScale);
            mBody.inertiaTensor = inertia;
            mWheelForces = new Vector3[2 * mAxleList.Length];
            mWheelTorques = new Vector3[2 * mAxleList.Length];

            if (mBrakeEngagement == null) mBrakeEngagement = new AnimationCurve();

            //do not leave curves empty
            if (mBrakeEngagement.length <= 1)
            {
                mBrakeEngagement = new AnimationCurve();
                mBrakeEngagement.AddKey(0f, 0f);
                mBrakeEngagement.AddKey(1f, 1f);
            }

            //do this after inertia tensor calculation
            activateColliders();

            initTelemetryWindows();

            mTrans = GetComponent<Transform>();
            mEngine.init(mTransmission);
            for (int i = 0; i < mAxleList.Length; i++)
            {
                mAxleList[i].init(this, i);
            }
            mTransmission.init(this);
            mStandardInput.init(this);
            mAeroDynamics.init(this);
        }

        //disable inertia colliders
        //activate real colliders
        void activateColliders()
        {
            GameObject collisionObject = Utility.findChild(gameObject, "collision", false);
            GameObject inertiaObject = Utility.findChild(gameObject, "inertia", false);

            if (collisionObject != null && inertiaObject != null)
            {
                collisionObject.SetActive(true);
                inertiaObject.SetActive(false);
            }
        }

        public void reset(Vector3 position, Quaternion rotation)
        {
            mTrans.position = position;
            mTrans.rotation = rotation;
            mBody.velocity = Vector3.zero;
            mBody.angularVelocity = Vector3.zero;
            mEngine.resetRpm();
            mTransmission.setCurrentGear(0, true);

            //reset all wheel speed
            for (int i = 0; i < mAxleList.Length; i++)
            {
                mAxleList[i].getLeftWheel().reset();
                mAxleList[i].getRightWheel().reset();
            }
        }

        //for default values in custom editor
        void Reset()
        {
            if (mAxleList == null)
            {
                mAxleList = new Axle[2];
                mAxleList[0] = new Axle();
                mAxleList[1] = new Axle();
                mAxleList[0].reset();
                mAxleList[1].reset();
            }
        }

        void Update()
        {
            myUpdate(Time.deltaTime);
        }

        void FixedUpdate()
        {
            if (!mManualUpdate)
            {
                myFixedUpdate(Time.fixedDeltaTime);
            }
        }

        public void myUpdate(float dt)
        {
            //update inputs
            if (mUpdateInputs) mStandardInput.myUpdate(dt, true);

            //update wheels
            for (int i = 0; i < mAxleList.Length; i++)
            {
                mAxleList[i].getLeftWheel().visualsUpdate(dt);
                mAxleList[i].getRightWheel().visualsUpdate(dt);
            }
        }

        public void myFixedUpdate(float dt)
        {
            //reset wheel forces/torques
            for (int i = 0; i < mAxleList.Length; i++)
            {
                mWheelForces[i * 2] = Vector3.zero;
                mWheelForces[i * 2 + 1] = Vector3.zero;
                mWheelTorques[i * 2] = Vector3.zero;
                mWheelTorques[i * 2 + 1] = Vector3.zero;
            }

            //check stick tire state condition, see Wheel.stickyTireUpdate()
            mStickyTireState = 1.0f;
            if (mStickyTireSpeed > 0.0f)
            {
                float angularVelocity = mBody.angularVelocity.magnitude;
                float lateralVelocity = Mathf.Abs(getForwardSpeed()) + Mathf.Abs(getRightSpeed());
                float totalSpeed = (lateralVelocity + angularVelocity) * Utility.ms2kmh;
                mStickyTireState = Mathf.Clamp01(totalSpeed / mStickyTireSpeed);
            }
            
            //update @ mUpdateRate Hz
            {
                mTime += dt;
                float dts = 1f / mUpdateRate;
                //mUpdateRate should not be lower then physics tick rate
                //no sub steps in stick state
                if (dts > dt || mStickyTireState < 1.0f) dts = dt;
                mSimCoeff = dts / dt;
                int i = 0;
                while (mTime >= dts)
                {
                    mTime -= dts;
                    substepUpdate(i++, dt, dts);
                }
            }

            //apply wheel forces & torques & reaction forces
            for (int i = 0; i < mAxleList.Length; i++)
            {
                mBody.AddForceAtPosition(mWheelForces[2 * i], mAxleList[i].getLeftWheel().getHubPosition());
                mBody.AddForceAtPosition(mWheelForces[2 * i + 1], mAxleList[i].getRightWheel().getHubPosition());

                Utility.addTorqueAtPosition(mBody, mAxleList[i].getLeftWheel().getHubPosition(), mWheelTorques[2 * i]);
                Utility.addTorqueAtPosition(mBody, mAxleList[i].getRightWheel().getHubPosition(), mWheelTorques[2 * i + 1]);

                //optional force to be applied back to dynamic colliders
                if (mReactionScale > 0.0f)
                {
                    RaycastHit hitL = mAxleList[i].getLeftWheel().getRayHit();
                    RaycastHit hitR = mAxleList[i].getRightWheel().getRayHit();

                    if (hitL.rigidbody != null && !hitL.rigidbody.isKinematic)
                    {
                        hitL.rigidbody.AddForceAtPosition(-mReactionScale * mWheelForces[2 * i], hitL.point);
                    }

                    if (hitR.rigidbody != null && !hitR.rigidbody.isKinematic)
                    {
                        hitR.rigidbody.AddForceAtPosition(-mReactionScale * mWheelForces[2 * i + 1], hitR.point);
                    }
                }
            }

            //handle hard contacts
            if (mHardContact) handleHardContacts();
            
            //update assists
            updateESP();
            updateAntiRollBar();
            mAeroDynamics.myFixedUpdate();
        }

        void substepUpdate(int i, float dt, float dts)
        {
            //update engine/transmission with substeps
            mEngine.myFixedUpdate(dts);
            mTransmission.myFixedUpdate(dts);

            //pre update axles
            for (int j = 0; j < mAxleList.Length; j++)
            {
                mAxleList[j].preWheelupdate(i == 0);
            }

            //update wheels
            for (int j = 0; j < mAxleList.Length; j++)
            {
                Vector3 force, torque;

                mAxleList[j].getLeftWheel().fixedUpdate(dt, dts, i, out force, out torque);
                mWheelForces[j * 2] += mSimCoeff * force;
                mWheelTorques[j * 2] += mSimCoeff * torque;

                mAxleList[j].getRightWheel().fixedUpdate(dt, dts, i, out force, out torque);
                mWheelForces[j * 2 + 1] += mSimCoeff * force;
                mWheelTorques[j * 2 + 1] += mSimCoeff * torque;
            }

            //post update axles
            for (int j = 0; j < mAxleList.Length; j++)
            {
                mAxleList[j].postWheelupdate();
            }

            //find engine feedback
            {
                //find final rpm, find the slowest
                float finalRpm = float.MaxValue;
                for (int j = 0; j < mAxleList.Length; j++)
                {
                    float share = mAxleList[j].getTorqueShare();
                    float rpm = mAxleList[j].getRPM();
                    if (share > 0f) finalRpm = Mathf.Min(finalRpm, rpm);
                }

                //apply engine rpm feedback
                //feedback torque is multiplied with clutch inside engine
                float feedbackTorque = 0f;
                for (int j = 0; j < mAxleList.Length; j++)
                {
                    float share = mAxleList[j].getTorqueShare();
                    feedbackTorque += share * mAxleList[j].getFeedbackTorque();
                }
                float targetRpm = finalRpm * mTransmission.getTransmissionRatio();
                float clutchScale = 0.01f * mTransmission.getClutchScale(); //artificial
                mEngine.setFeedback(dts, targetRpm, feedbackTorque * clutchScale);
            }
        }

        public void setBraking(float brakeInput, bool handbrake)
        {
            brakeInput = mBrakeEngagement.Evaluate(brakeInput);
            mHandbrakeOn = handbrake;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                bool handBrake = mHandbrakeOn && mAxleList[i].hasHandBrake();
                mAxleList[i].getLeftWheel().setBraking(handBrake ? 1.0f : brakeInput);
                mAxleList[i].getRightWheel().setBraking(handBrake ? 1.0f : brakeInput);
            }
        }

        public void setSteering(float steerInput)
        {
            for (int i = 0; i < mAxleList.Length; i++)
            {
                if (mAxleList[i].hasSteering())
                {
                    mAxleList[i].setTargetSteer(steerInput);
                }
            }
        }

        public float getNormalizedSteering()
        {
            for (int i = 0; i < mAxleList.Length; i++)
            {
                if (mAxleList[i].hasSteering())
                {
                    return mAxleList[i].getNormalizedSteering();
                }
            }
            return 0f;
        }

        //valid only for 4 wheel vehicles
        float getCurrentSteering()
        {
            for (int i = 0; i < mAxleList.Length; i++)
            {
                if (mAxleList[i].hasSteering())
                {
                    return mAxleList[i].getNormalizedSteering() * mAxleList[i].getMaxSteerAngle();
                }
            }
            return 0f;
        }

        //valid only for 4 wheel vehicles
        public float getSteerDeltaAngle(bool front)
        {
            if (getAxleCount() < 2) return 0f;
            Axle frontAxle = getAxle(0);
            Axle rearAxle = getAxle(1);

            Quaternion qs = Quaternion.Euler(0f, getCurrentSteering(), 0f);
            Vector3 dir1 = qs * mTrans.forward; dir1.y = 0f; dir1.Normalize();
            Vector3 vel = mBody.GetPointVelocity(front ? frontAxle.getGlobalCenter() : rearAxle.getGlobalCenter());
            Vector3 dir2 = vel.normalized; dir2.y = 0f; dir2.Normalize();
            Vector3 dir3 = mTrans.forward; dir3.y = 0f; dir3.Normalize();

            float angle = front ? Vector3.SignedAngle(dir2, dir1, Vector3.up) : Vector3.SignedAngle(dir2, dir3, Vector3.up);
            if (angle > 90f) return 90f;
            else if (angle < -90f) return -90f;
            return angle;
        }

        //designed only for 4 wheel vehicles
        void updateAntiRollBar()
        {
            if (getAxleCount() != 2) return;

            if (mFrontAntiRollPower > 0f)
            {
                Wheel fl = getAxle(0).getLeftWheel();
                Wheel fr = getAxle(0).getRightWheel();

                //reset
                fl.setSpringCoeff(1f);
                fr.setSpringCoeff(1f);

                //calculate
                float flCompress = fl.getSuspensionCompressRatio();
                float frCompress = fr.getSuspensionCompressRatio();
                float frontAntiRoll = (flCompress - frCompress) * 0.01f * mFrontAntiRollPower;

                //apply
                fl.setSpringCoeff(1f + frontAntiRoll);
                fr.setSpringCoeff(1f - frontAntiRoll);
            }

            if (mRearAntiRollPower > 0f)
            {
                Wheel rl = getAxle(1).getLeftWheel();
                Wheel rr = getAxle(1).getRightWheel();

                //reset
                rl.setSpringCoeff(1f);
                rr.setSpringCoeff(1f);

                //calculate
                float rlCompress = rl.getSuspensionCompressRatio();
                float rrCompress = rr.getSuspensionCompressRatio();
                float rearAntiRoll = (rlCompress - rrCompress) * 0.01f * mRearAntiRollPower;

                //apply
                rl.setSpringCoeff(1f + rearAntiRoll);
                rr.setSpringCoeff(1f - rearAntiRoll);
            }
        }

        //designed only for 4 wheel vehicles
        void updateESP()
        {
            mEspActive = false;
            mUnderSteering = false;
            mOverSteering = false;

            if (getAxleCount() != 2 || getForwardSpeed() < 0f) return;

            Wheel fl = getAxle(0).getLeftWheel();
            Wheel fr = getAxle(0).getRightWheel();
            Wheel rl = getAxle(1).getLeftWheel();
            Wheel rr = getAxle(1).getRightWheel();

            float maxSteerAngle = getAxle(0).getMaxSteerAngle();
            float speedCoeff = Mathf.Min(1f, getVelocitySize() / 10f);

            //oversteer, brake front wheels
            if (mOverSteerAssist > 0.0f)
            {
                float angle = getSteerDeltaAngle(false);

                //turn left
                if (angle < -mMinOverSteerAngle)
                {
                    float power = speedCoeff * (0.01f * mOverSteerAssist) * -angle / maxSteerAngle;
                    fr.setBraking(Mathf.Min(1f, power));
                    mEspActive = true;
                    mOverSteering = true;
                }
                //turn right
                if (angle > mMinOverSteerAngle)
                {
                    float power = speedCoeff * (0.01f * mOverSteerAssist) * angle / maxSteerAngle;
                    fl.setBraking(Mathf.Min(1f, power));
                    mEspActive = true;
                    mOverSteering = true;
                }
            }

            //understeer, brake rear wheels
            //skip understeer when oversteer
            if (mUnderSteerAssist > 0.0f)
            {
                float angle = getSteerDeltaAngle(true);

                //turn left
                if (angle < -mMinUnderSteerAngle)
                {
                    float power = speedCoeff * Mathf.Min(1.0f, (0.01f * mUnderSteerAssist) * -angle / maxSteerAngle);
                    rl.setBraking(Mathf.Min(1f, power));
                    mEspActive = true;
                    mUnderSteering = true;
                }
                //turn right
                if (angle > mMinUnderSteerAngle)
                {
                    float power = speedCoeff * Mathf.Min(1.0f, (0.01f * mUnderSteerAssist) * angle / maxSteerAngle);
                    rr.setBraking(Mathf.Min(1f, power));
                    mEspActive = true;
                    mUnderSteering = true;
                }
            }
        }

        void handleHardContacts()
        {
            //check penetrations
            float maxPenetration = 0.0f;
            float maxDistance = 0.0f;
            Vector3 maxNormal = Vector3.up;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                float p1 = mAxleList[i].getLeftWheel().getSuspensionCompressedLength();
                float p2 = mAxleList[i].getRightWheel().getSuspensionCompressedLength();
                if (p1 < maxPenetration || p2 < maxPenetration)
                {
                    maxPenetration = p1 < p2 ? p1 : p2;
                    RaycastHit mhit = (p1 < p2 ? mAxleList[i].getLeftWheel() : mAxleList[i].getRightWheel()).getRayHit();
                    maxNormal = mhit.normal;
                    maxDistance = mhit.distance;
                }
            }
            
            //handle only largest
            if (maxPenetration < 0.0f && maxDistance > 0.0f)
            {
                //stopper impulse
                Vector3 move = mBody.velocity;
                if (Vector3.Dot(move, Physics.gravity) > 0.0f)
                {
                    move = mTrans.InverseTransformVector(move);
                    move.y = 0.0f;
                    move = mTrans.TransformVector(move);
                    mBody.velocity = move;
                }

                //resolve penetration
                mBody.MovePosition(mBody.position - maxPenetration * maxNormal);
            }
        }

        //get velocity from wheels or directly from rigidbody
        //if the vehicle is on a moving platform mDetectGround should be set
        public Vector3 getVelocity()
        {
            if (!mDetectGround) return mBody.velocity;

            Vector3 vel = Vector3.zero;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                vel += mAxleList[i].getLeftWheel().getGroundVelocity();
                vel += mAxleList[i].getRightWheel().getGroundVelocity();
            }
            //zero axle case not checked
            return vel / (2 * mAxleList.Length);
        }

        //return the speed of fastest wheel
        public float getMaxWheelKMHSpeed()
        {
            float speed = 0f;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                speed = Mathf.Max(speed, Mathf.Abs(mAxleList[i].getLeftWheel().getKMHSpeed()));
                speed = Mathf.Max(speed, Mathf.Abs(mAxleList[i].getRightWheel().getKMHSpeed()));
            }
            return speed;
        }

        //return the speed of slowest wheel
        public float getMinWheelKMHSpeed()
        {
            float speed = float.MaxValue;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                speed = Mathf.Min(speed, Mathf.Abs(mAxleList[i].getLeftWheel().getKMHSpeed()));
                speed = Mathf.Min(speed, Mathf.Abs(mAxleList[i].getRightWheel().getKMHSpeed()));
            }
            return speed;
        }

        //used for engine braking
        public float getWheelSpeedLimitKMH()
        {
            float limit = 0.0f;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                float leftSpeed = Mathf.Abs(mAxleList[i].getMaxRpm()) * mAxleList[i].getLeftWheel().getWheelRPMToKMH();
                float rightSpeed = Mathf.Abs(mAxleList[i].getMaxRpm()) * mAxleList[i].getRightWheel().getWheelRPMToKMH();
                if (leftSpeed > limit) limit = leftSpeed;
                if (rightSpeed > limit) limit = rightSpeed;
            }
            return limit * Mathf.Sign(mTransmission.getTransmissionRatio());
        }

        //used for max speed calculations
        public float getMaxWheelRPMToKMH()
        {
            float maxRatio = 0.0f;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                if (mAxleList[i].getTorqueShare() > 0.0f)
                {
                    maxRatio = Mathf.Max(maxRatio, mAxleList[i].getWheelRPMToKMH());
                }
            }
            return maxRatio;
        }

        //used in VehicleInput->limit steering angle
        public float getMaxSteeringAngle()
        {
            float maxSteer = 0.0f;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                maxSteer = Mathf.Max(maxSteer, mAxleList[i].getMaxSteerAngle());
            }
            return maxSteer;
        }

        //used in ackerman calculations
        public float getAxleDistance(int axle1, int axle2)
        {
            return Mathf.Abs(mAxleList[axle1].getLocalCenter().z - mAxleList[axle2].getLocalCenter().z);
        }

        //used for hard contact simulation of suspensions
        public float getTotalWheelLoad()
        {
            float loadSum = 0.0f;
            for (int i = 0; i < mAxleList.Length; i++)
            {
                loadSum += mAxleList[i].getLeftWheel().getCurrentLoad();
                loadSum += mAxleList[i].getRightWheel().getCurrentLoad();
            }
            return loadSum;
        }

        //if any wheel has active abs then return true
        public bool getABSActive()
        {
            for (int i = 0; i < mAxleList.Length; i++)
            {
                if (mAxleList[i].getLeftWheel().isABSActive() || mAxleList[i].getRightWheel().isABSActive()) return true;
            }
            return false;
        }

        //if any wheel has active asr then return true
        public bool getASRActive()
        {
            for (int i = 0; i < mAxleList.Length; i++)
            {
                if (mAxleList[i].getLeftWheel().isASRActive() || mAxleList[i].getRightWheel().isASRActive()) return true;
            }
            return false;
        }

        public void saveState(ref VehicleState state)
        {
            //self state
            state.mVehicleTime = mTime;
            state.mPosition = mBody.position;
            state.mRotation = mBody.rotation;
            state.mVelocity = mBody.velocity;
            state.mAngularVelocity = mBody.angularVelocity;

            //component states
            mEngine.saveState(ref state);
            mTransmission.saveState(ref state);
            mStandardInput.saveState(ref state);

            //axle/wheel states
            for (int i = 0; i < mAxleList.Length; i++)
            {
                mAxleList[i].saveState(ref state);
            }
        }

        public void loadState(VehicleState state)
        {
            //self state
            mTime = state.mVehicleTime;
            mBody.position = state.mPosition;
            mBody.rotation = state.mRotation;
            mBody.velocity = state.mVelocity;
            mBody.angularVelocity = state.mAngularVelocity;
            mTrans.SetPositionAndRotation(state.mPosition, state.mRotation);

            //component states
            mEngine.loadState(state);
            mTransmission.loadState(state);
            mStandardInput.loadState(state);
            for (int i = 0; i < mAxleList.Length; i++)
            {
                mAxleList[i].loadState(state);
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

        void OnDrawGizmos()
        {
            //wheel gizmos
            if (mAxleList != null)
            {
                for (int i = 0; i < mAxleList.Length; i++)
                {
                    mAxleList[i].drawGizmo();
                }
            }
        }

        void OnGUI()
        {
            if (mTelemetry.Show)
            {
                Utility.textColor = mTelemetry.TextColor;
                if (mTelemetry.ShowVehicle) drawTelemetryV();
                if (mTelemetry.ShowWheels) drawTelemetryW();
                if (mTelemetry.ShowAeroDynamics) mAeroDynamics.drawTelemetry();
                if (mTelemetry.ShowInput) mStandardInput.drawTelemetry();
                if (mTelemetry.ShowEngine) mEngine.drawTelemetry();
                if (mTelemetry.ShowTransmission) mTransmission.drawTelemetry();
                if (mTelemetry.ShowAxles)
                {
                    for (int i = 0; i < mAxleList.Length; i++)
                    {
                        mAxleList[i].drawTelemetry();
                    }
                }
            }
        }

        void initTelemetryWindows()
        {
            mWinIDV = Utility.winIDs++;
            mWinIDW = Utility.winIDs++;
            mWindowRectVehicle = new Rect(5, Screen.height - 510, 175, 250);
            mWindowRectWheel = new Rect(240, 5, 935, 80 + mAxleList.Length * 37.5f);
        }

        void drawTelemetryV()
        {
            mWindowRectVehicle = GUI.Window(mWinIDV, mWindowRectVehicle, uiWindowFunctionV, "Vehicle");
        }

        void drawTelemetryW()
        {
            mWindowRectWheel = GUI.Window(mWinIDW, mWindowRectWheel, uiWindowFunctionW, "Wheels");
        }

        void uiWindowFunctionV(int windowID)
        {
            GUI.DragWindow();
            GUI.contentColor = Utility.textColor;
            GUI.Label(new Rect(10, 20, 150, 25), "Mass: " + mMass.ToString("f0") + " kg");
            GUI.Label(new Rect(10, 40, 150, 25), "Speed: " + getKMHSpeed().ToString("f1") + " kmh");
            GUI.Label(new Rect(10, 60, 150, 25), "Update Rate: " + mUpdateRate + " hz");
            GUI.Label(new Rect(10, 80, 150, 25), "Total Raycasts: " + mLateralRays * mRays);
            GUI.Label(new Rect(10, 100, 150, 25), "ABS: %" + mABS.ToString("f0"));
            GUI.Label(new Rect(10, 120, 150, 25), "ASR: %" + mASR.ToString("f0"));
            GUI.Label(new Rect(10, 140, 150, 25), "Steering Assist: %" + mStandardInput.getSteeringAssist().ToString("f0"));
            GUI.Label(new Rect(10, 160, 150, 25), "Oversteer Assist: %" + mOverSteerAssist.ToString("f0"));
            GUI.Label(new Rect(10, 180, 150, 25), "Understeer Assist: %" + mUnderSteerAssist.ToString("f0"));
            GUI.Label(new Rect(10, 200, 150, 25), "Front Antiroll: %" + mFrontAntiRollPower.ToString("f0"));
            GUI.Label(new Rect(10, 220, 150, 25), "Rear Antiroll: %" + mRearAntiRollPower.ToString("f0"));
        }

        void uiWindowFunctionW(int windowID)
        {
            GUI.DragWindow();
            GUI.contentColor = Utility.textColor;
            GUI.Label(new Rect(50, 20, 75, 25), "Speed");
            GUI.Label(new Rect(100, 20, 75, 25), "Torque");
            GUI.Label(new Rect(150, 20, 75, 25), "Load");
            GUI.Label(new Rect(195, 20, 80, 25), "RideHeight");
            GUI.Label(new Rect(275, 20, 80, 25), "SteerAngle");
            GUI.Label(new Rect(350, 20, 95, 25), "TireFriction(F/R)");
            GUI.Label(new Rect(455, 20, 90, 25), "ContactFriction");
            GUI.Label(new Rect(555, 20, 75, 25), "LngForce");
            GUI.Label(new Rect(625, 20, 75, 25), "LngSlip");
            GUI.Label(new Rect(680, 20, 75, 25), "SlipRatio");
            GUI.Label(new Rect(745, 20, 75, 25), "LatForce");
            GUI.Label(new Rect(810, 20, 75, 25), "LatSlip");
            GUI.Label(new Rect(865, 20, 75, 25), "SlipAngle");

            for (int i = 0; i < mAxleList.Length; i++)
            {
                mAxleList[i].getLeftWheel().uiWindowFunction(windowID);
                mAxleList[i].getRightWheel().uiWindowFunction(windowID);
            }
        }
    }
}