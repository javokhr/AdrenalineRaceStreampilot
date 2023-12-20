// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System;
using UnityEngine;

namespace FGear
{
    [System.Serializable]
    public class Wheel : TelemetryDrawer
    {
        [SerializeField]
        Transform mWheelTransform;
        [SerializeField]
        Mesh mConvexMesh;
        [SerializeField]
        bool mIsActive = true;

        //wheel options
        float mLatFriction;
        float mLngFriction;
        float mLatScale;
        float mLngScale;
        float mOverturnScale;
        float mRollScale;
        float mAlignScale;
        float mRadius;
        float mWidth;
        float mMass;
        float mPressure;
        float mBrakeTorque;
        float mSuspensionUpTravel;
        float mSuspensionDownTravel;
        float mSuspensionSpring;
        float mCompressionDamper;
        float mRelaxationDamper;
        float mSuspensionPreload;

        //objects
        [NonSerialized]
        Vehicle mVehicle;
        [NonSerialized]
        Axle mAxle;
        [NonSerialized]
        Tire mTire;
        Rigidbody mBody;
        RaycastHit mRayHit;
        Transform mHubTransform;
        Rigidbody mCastBody;

        //forces
        Vector3 mSpringForce;
        Vector3 mLateralForce;
        Vector3 mLongitudinalForce;
        Vector3 mCombinedTorque;
        Vector3 mLocalVelocity;
        Vector3 mGroundVelocity;

        //suspension
        Vector3 mInitialHubPos; //required to apply up travel changes at runtime
        float mSpringCoeff = 1f; //adjusted for roll stiffness
        float mCompressedLength = 0f;
        float mLastCompressLength = 0f;
        float mCompressRatio = 0f;
        float mCurrentLoad = 0f;
        bool mHasContact = false;

        //steering
        bool mIsLeft = false;
        float mCurrentSteer = 0f;
        float mToeAngle = 0f;
        float mCamberAngle = 0f;
        float mPitch = 0f;

        //friction
        float mLateralSlip;
        float mLongitudinalSlip;
        float mSlipRatio;
        float mSlipAngle;
        float mS, mA, mP; //The Physics of Racing, Part 25: Combination Grip
        float mFrictionFactor = 1f;
        float mLatFrictionForce = 0f;
        float mLngFrictionForce = 0f;
        
        //feedback
        float mFeedbackTorque;
        bool mABSActive;
        bool mASRActive;

        //others
        float mRealSpeed; //rad/s
        float mWheelTorque;
        float mCircumference;
        float mWheelRPMToKMH;
        float mBraking;
        float mSpeedOverflow;
        
        #region getters & setters
        public Axle getAxle() { return mAxle; }
        public RaycastHit getRayHit() { return mRayHit; }
        public Transform getWheelTransform() { return mWheelTransform; }
        public Vector3 getHubPosition() { return mHubTransform.position; }
        public Vector3 getWheelPosition() { return mWheelTransform.position; }
        public Vector3 getLocalHubPosition() { return mHubTransform.localPosition; }
        public Vector3 getLongitudinalForce() { return mLongitudinalForce; }
        public Vector3 getLateralForce() { return mLateralForce; }
        public Vector3 getCombinedTorque() { return mCombinedTorque; }
        public Vector3 getGroundVelocity() { return mGroundVelocity; }
        public Vector3 getGlobalUp() { return mHubTransform.up; }
        public float getRadius() { return mRadius; }
        public float getPressure() { return mPressure; }
        public float getSpeed() { return mRealSpeed; }
        public float getRpm() { return mRealSpeed * Utility.rads2Rpm; }
        public float getKMHSpeed() { return getRpm() * mWheelRPMToKMH; }
        public float getWheelRPMToKMH() { return mWheelRPMToKMH; }
        public float getLongitudinalSlip() { return mLongitudinalSlip; }
        public float getLateralSlip() { return mLateralSlip; }
        public float getSlipRatio() { return mSlipRatio; }
        public float getSlipAngle() { return mSlipAngle; }
        public float getCurrentLoad() { return mCurrentLoad; }
        public float getSuspensionSpring() { return mSuspensionSpring; }
        public float getSuspensionCompressRatio() { return mCompressRatio; }
        public float getSuspensionTotalLength() { return mSuspensionUpTravel + mSuspensionDownTravel; }
        public float getSuspensionCompressedLength() { return mCompressedLength; }
        public float getFeedbackTorque() { return mFeedbackTorque * mRadius; }
        public float getBraking() { return mBraking; }
        public float getFrictionFactor() { return mFrictionFactor; }
        public bool isActive() { return mIsActive; }
        public bool hasContact() { return mHasContact; }
        public bool hasDrive() { return mAxle.getTorqueShare() > 0.0f; }
        public bool isABSActive() { return mABSActive; }
        public bool isASRActive() { return mASRActive; }
        public void setActive(bool b) { mIsActive = b; }
        public void setSpeed(float s) { mRealSpeed = s; }
        public void setCurrentSteer(float s) { mCurrentSteer = s; }
        public void setBraking(float b) { mBraking = b; }
        public void setFrictionFactor(float f) { mFrictionFactor = f; }
        public void setSpringCoeff(float f) { mSpringCoeff = f; }
        #endregion

        public void init(Vehicle v, Axle axle)
        {
            mVehicle = v;
            mAxle = axle;
            mBody = mVehicle.getBody();
            mIsLeft = axle.getLeftWheel() == this;
            refreshParameters(null);
            initTelemetryWindow();
            checkMissingTransforms();
            reset();
        }

        //called by ui to get real time parameter update
        //axle parameter can be set when called from custom ui, otherwise keep it null
        public void refreshParameters(Axle axle)
        {
            //check axle
            Axle cAxle = mAxle;
            if (cAxle == null) cAxle = axle;
            if (cAxle == null) return;

            //get options
            mTire = cAxle.getWheelOptions().getTireModel();
            mLatFriction = cAxle.getWheelOptions().getLateralFriction();
            mLngFriction = cAxle.getWheelOptions().getLongitudinalFriction();
            mLatScale = cAxle.getWheelOptions().getLateralScale();
            mLngScale = cAxle.getWheelOptions().getLongitudinalScale();
            mOverturnScale = cAxle.getWheelOptions().getOverturnScale();
            mRollScale = cAxle.getWheelOptions().getRollMomentScale();
            mAlignScale = cAxle.getWheelOptions().getSelfAlignScale();
            mRadius = cAxle.getWheelOptions().getRadius();
            mWidth = cAxle.getWheelOptions().getWidth();
            mMass = cAxle.getWheelOptions().getMass();
            mPressure = Mathf.Max(2f * Mathf.Epsilon, cAxle.getWheelOptions().getTirePressure());
            mBrakeTorque = cAxle.getWheelOptions().getBrakeTorque();
            mSuspensionUpTravel = cAxle.getWheelOptions().getSuspensionUpTravel();
            mSuspensionDownTravel = cAxle.getWheelOptions().getSuspensionDownTravel();
            mSuspensionSpring = cAxle.getWheelOptions().getSuspensionSpring();
            mCompressionDamper = cAxle.getWheelOptions().getCompressionDamper();
            mRelaxationDamper = cAxle.getWheelOptions().getRelaxationDamper();
            mSuspensionPreload = cAxle.getWheelOptions().getSuspensionPreload();
            mToeAngle = (mIsLeft ? 1.0f : -1.0f) * cAxle.getToeAngle();
            mCamberAngle = (mIsLeft ? 1.0f : -1.0f) * cAxle.getCamberAngle();

            //extra params
            mCircumference = 2.0f * Mathf.PI * mRadius;
            mWheelRPMToKMH = mCircumference * 60.0f / 1000.0f;

            //need to update hub/caster transform for up/down tavel changes
            if (mHubTransform != null)
            {
                mHubTransform.localPosition = mInitialHubPos;
                //apply up travel offset
                mHubTransform.Translate(mSuspensionUpTravel * Vector3.up, Space.Self);
            }
        }

        //if input transforms are null, try to find them
        void checkMissingTransforms()
        {
            //try to find wheel transform by name
            if (mWheelTransform == null)
            {
                GameObject wheelObj = Utility.findChild(mVehicle.gameObject, mAxle.getIndex() + (mIsLeft ? "L" : "R") + "Wheel", false);
                if (wheelObj != null) mWheelTransform = wheelObj.transform;
            }

            //create hub transform
            if (mWheelTransform != null && mHubTransform == null)
            {
                //create & position game object
                string name = mAxle.getIndex() + (mIsLeft ? "L" : "R") + "Hub";
                GameObject hub = new GameObject(name);
                hub.transform.parent = mVehicle.transform;
                mHubTransform = hub.transform;
                mHubTransform.position = mWheelTransform.position;
                mHubTransform.rotation = mWheelTransform.rotation;
                //store for run time updates
                mInitialHubPos = mHubTransform.localPosition;
                //apply up travel offset
                mHubTransform.Translate(mSuspensionUpTravel * Vector3.up, Space.Self);
            }

            //create caster convex
            if (mVehicle.getCastType() == Vehicle.CastType.CONVEX  && mCastBody == null)
            {
                GameObject cast = null;
                string name = mAxle.getIndex() + (mIsLeft ? "L" : "R") + "Convex";

                //if not provided create a default cylinder
                if (mConvexMesh == null)
                {
                    cast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    cast.name = name;
                    //get its mesh
                    mConvexMesh = cast.GetComponent<MeshFilter>().sharedMesh;
                    //remove all components
                    foreach (var comp in cast.GetComponents<Component>())
                    {
                        if (!(comp is Transform))
                        {
                            UnityEngine.Object.DestroyImmediate(comp);
                        }
                    }
                }
                //create new game object
                else
                {
                    cast = new GameObject(name);
                }

                //position cast object
                cast.transform.parent = mVehicle.transform;
                cast.transform.position = mHubTransform.position;
                //add rigidbody and collider
                MeshCollider col = cast.AddComponent<MeshCollider>();
                col.sharedMesh = mConvexMesh;
                col.convex = true;
                col.inflateMesh = true;
                col.isTrigger = true;
                col.cookingOptions = (MeshColliderCookingOptions)(0xF);
                mCastBody = cast.AddComponent<Rigidbody>();
                mCastBody.useGravity = false;
                mCastBody.isKinematic = true;
                //apply scale to fit wheel width/radius
                Vector3 sz = mConvexMesh.bounds.size;
                Vector3 scale = new Vector3(2.0f * mRadius / sz.x, mWidth / sz.y, 2.0f * mRadius / sz.z);
                cast.transform.localScale = scale;
            }
        }

        //called when vehicle is reset
        //called when wheel is inactive
        public void reset()
        {
            //forces
            mSpringForce = mLateralForce = mLongitudinalForce = mCombinedTorque = Vector3.zero;
            mLocalVelocity = mGroundVelocity = Vector3.zero;
            //suspension
            mSpringCoeff = 1f;
            mCompressedLength = mLastCompressLength = mCompressRatio = mCurrentLoad = 0f;
            mHasContact = false;
            //steering
            mCurrentSteer = mPitch = 0f;
            //friction
            mLateralSlip = mLongitudinalSlip = mSlipRatio = mSlipAngle = 0f;
            mFrictionFactor = 1f;
            mLatFrictionForce = mLngFrictionForce = 0f;
            //feedback
            mFeedbackTorque = 0f;
            mABSActive = mASRActive = false;
            //others
            mRealSpeed = mWheelTorque = mBraking = mSpeedOverflow = 0f;
        }

        public void fixedUpdate(float dt, float dts, int step, out Vector3 force, out Vector3 torque)
        {
            if (!mIsActive)
            {
                reset();
                force = torque = Vector3.zero;
                return;
            }

            if (step == 0) //first step
            {
                updateLocalVelocity();

                suspensionUpdate(dt);

                stickyTireUpdate();
            }

            updateSlipValues();

            updateTireFrictionValues();

            updateWheelSpeed(dts);

            //rpm limit to rotation speed
            if (hasDrive()) applyRpmLimit(dt);

            updateWheelFriction(dts);

            //combination force
            Vector3 frictionForce = Vector3.zero;
            if (mTire.getCombineMode() == Tire.ForceCombineMode.SIMPLE) frictionForce = combineLegacy();
            else if (mTire.getCombineMode() == Tire.ForceCombineMode.GRIP) frictionForce = combinePOR();
            else frictionForce = combineSum();

            //scale tire forces when sticky tire is in effect
            if (mVehicle.getStickyTireState() < 1.0f)
            {
                mLateralForce *= mVehicle.getStickyTireState();
                if (mBraking > 0f || mVehicle.isHandbrakeOn())
                {
                    mLongitudinalForce *= mVehicle.getStickyTireState();
                }
            }

            //sum up with suspension
            force = mSpringForce + frictionForce;

            //torque from tire model + tire reaction torque for x component
            torque = mCombinedTorque;

            //fix rolling resistance torque sign
            if (torque.x != 0f) torque.x = Math.Sign(mRealSpeed) * Mathf.Abs(torque.x); //signum

            //add wheel torque feedback, when braking take brake disc radius into account(~%30)
            torque.x += mLongitudinalForce.magnitude * -Math.Sign(mWheelTorque) * (mBraking > 0f ? 0.3f : 1f) * mRadius; //signum

            //scale torque
            torque.x *= mRollScale;
            torque.y *= -mAlignScale;
            torque.z *= -mOverturnScale;

            //convert local torque to global
            Quaternion qs = Quaternion.Euler(0.0f, mCurrentSteer + mToeAngle, 0.0f);
            torque = qs * mHubTransform.TransformDirection(torque);
        }

        Vector3 combineLegacy()
        {
            if (mLongitudinalForce.magnitude > 0.0f && mCurrentLoad > 0.0f)
            {
                float maxLongitudinalForce = mTire.getMaxLongitudinalForce() * mFrictionFactor * mLngFriction * 2f;
                float midVal = 1.0f - Mathf.Pow(mLongitudinalForce.magnitude / maxLongitudinalForce, 2.0f);
                if (midVal > 0.0f)
                {
                    float lateralSize = mLateralForce.magnitude * Mathf.Sqrt(midVal);
                    mLateralForce = lateralSize * mLateralForce.normalized;
                }
            }
            return mLongitudinalForce + mLateralForce;
        }

        //The Physics of Racing, Part 25: Combination Grip
        Vector3 combinePOR()
        {
            if (mP == 0.0f || float.IsInfinity(mP)) mS = mA = mP = 1.0f; //p is zero/infinite for the first frame
            Vector3 totalTireForce = (mS / mP * mLongitudinalForce) + (mA / mP * mLateralForce);
            return totalTireForce;
        }

        Vector3 combineSum()
        {
            return mLongitudinalForce + mLateralForce;
        }

        public Vector3 getWheelPos()
        {
            float compress = mVehicle.getTirePenetration() ? Mathf.Max(0f, mCompressedLength) : mCompressedLength;
            return mHubTransform.position - compress * mHubTransform.up;
        }

        public void visualsUpdate(float dt)
        {
            if (mIsActive && mWheelTransform != null)
            {
                //update wheel y-pos acc. to compress length
                mWheelTransform.position = getWheelPos();
                //rotate acc. to wheel speed
                mPitch += mRealSpeed * dt;
                //clamp mpitch
                if (mPitch > Utility.pi2) mPitch -= Utility.pi2;
                else if (mPitch < -Utility.pi2) mPitch += Utility.pi2;
                Quaternion qp = Quaternion.Euler(mPitch * Mathf.Rad2Deg, 0f, 0f);
                //rotat acc. to camber angle
                Quaternion qc = Quaternion.Euler(0f, 0f, mCamberAngle);
                //rotate acc. to steering & toe
                Quaternion qst = Quaternion.Euler(0f, mCurrentSteer + mToeAngle, 0f);
                mWheelTransform.localRotation = qst * qc * qp;
            }
        }

        void suspensionUpdate(float dt)
        {
            mSpringForce = Vector3.zero;
            mCurrentLoad = 0f;

            //default wheel pos
            float totalLength = mSuspensionUpTravel + mSuspensionDownTravel;
            mCompressedLength = totalLength;

            //common
            int castMask = mVehicle.getRaycastLayer();
            float castDist = totalLength + mRadius;
            float hitCount = 0;
            float minDist = float.MaxValue;
            Vector3 avgNormal = Vector3.zero;

            //ray casting
            if (mVehicle.getCastType() == Vehicle.CastType.RAY)
            {
                //ray scan
                int rays = mVehicle.getRayCount();
                int lrays = mVehicle.getLateralRayCount();
                Quaternion steerQuat = Quaternion.AngleAxis(mCurrentSteer + mToeAngle, mHubTransform.up);
                Vector3 dirFrom = steerQuat * -mHubTransform.up;
                Vector3 dirTo = steerQuat * mHubTransform.forward;
                float maxAngle = 0f;
                float stepAngle = 1f;
                float startX = 0f;
                float stepX = 1f;
                float endX = 0f;

                if (rays > 1)
                {
                    maxAngle = 90f;
                    float yOffsetAngle = Mathf.Atan2(mCompressedLength, mRadius) * Mathf.Rad2Deg;
                    maxAngle -= yOffsetAngle;
                    stepAngle = (2f * maxAngle) / (rays - 1);
                }

                if (lrays > 1)
                {
                    startX = -0.5f * mWidth;
                    endX = 0.5f * mWidth;
                    stepX = mWidth / (lrays - 1);
                }

                //raycast
                for (float x = startX; x <= endX; x += stepX)
                {
                    Vector3 addx = x * mWheelTransform.right;
                    for (float angle = -maxAngle; angle <= (maxAngle + 0.001f); angle += stepAngle)
                    {
                        Vector3 cdir = Vector3.RotateTowards(dirFrom, dirTo, angle * Mathf.Deg2Rad, 0f);
                        Vector3 origin = mHubTransform.position + addx;
                        RaycastHit hit;
                        if (Physics.Raycast(origin, cdir, out hit, castDist, castMask, QueryTriggerInteraction.Ignore))
                        {
                            //avoid self collision
                            if (hit.rigidbody == mBody) continue;
                            //find closest
                            if (hit.distance < minDist)
                            {
                                minDist = hit.distance;
                                mRayHit = hit;
                            }
                            avgNormal += hit.normal;
                            hitCount++;
                        }
                    }
                }
            }
            //sphere cast
            else if (mVehicle.getCastType() == Vehicle.CastType.SPHERE)
            {
                Vector3 cdir = -mHubTransform.up;
                Vector3 origin = mHubTransform.position - mRadius * cdir;
                RaycastHit[] hits = Physics.SphereCastAll(origin, mRadius, cdir, castDist, castMask, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < hits.Length; i++)
                {
                    //avoid self collision
                    if (hits[i].rigidbody == mBody) continue;
                    //prevents reverse direction hits
                    if (Vector3.Dot(cdir, hits[i].normal) > 0.1f) continue;
                    //find closest
                    if (hits[i].distance < minDist)
                    {
                        minDist = hits[i].distance;
                        mRayHit = hits[i];
                    }
                    avgNormal += hits[i].normal;
                    hitCount++;
                }
            }
            //convex cast
            else if (mCastBody != null)
            {
                Vector3 cdir = -mHubTransform.up;
                Vector3 origin = mHubTransform.position - mRadius * cdir;
                Quaternion q90x = Quaternion.AngleAxis(90.0f, mHubTransform.right);
                Quaternion steerQuat = Quaternion.AngleAxis(mCurrentSteer + mToeAngle + 90.0f, mHubTransform.up);
                
                mCastBody.position = origin;
                mCastBody.rotation = steerQuat * q90x * mBody.rotation;
                
                RaycastHit[] hits = mCastBody.SweepTestAll(cdir, castDist, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < hits.Length; i++)
                {
                    //avoid self collision
                    if (hits[i].rigidbody == mBody) continue;
                    //prevents reverse direction hits
                    if (Vector3.Dot(cdir, hits[i].normal) > 0.1f) continue;
                    //find closest
                    if (hits[i].distance < minDist)
                    {
                        minDist = hits[i].distance;
                        mRayHit = hits[i];
                    }
                    avgNormal += hits[i].normal;
                    hitCount++;
                }
            }

            mHasContact = hitCount > 0;

            if (mHasContact)
            {
                //avg. dir
                avgNormal /= hitCount;
                //find compressed length & wheel pos
                mCompressedLength = minDist - mRadius;
                mCompressedLength = Mathf.Min(mCompressedLength, totalLength);
                //calc. spring force acc. to compress ratio
                mCompressRatio = (totalLength - mCompressedLength) / totalLength;
                mCompressRatio = Mathf.Clamp01(mCompressRatio);
                //normal spring + preload
                float compressMM = 1000.0f * (totalLength - mCompressedLength);
                float preloadMagnitude = (compressMM != 0.0f ? 1000.0f : 0.0f) * mSuspensionPreload * mSuspensionSpring;
                float springMagnitude = (preloadMagnitude + (compressMM * mSuspensionSpring)) * mSpringCoeff;
                Vector3 springForce = springMagnitude * avgNormal;
                //keep track of delta compress for damping
                float deltaCompress = mCompressedLength - mLastCompressLength;
                mLastCompressLength = mCompressedLength;
                //calc. damping force
                float compressSpeed = deltaCompress / dt;
                float dampingMagnitude = 0.0f;
                if (deltaCompress < 0.0f) dampingMagnitude = compressSpeed * mCompressionDamper;
                else dampingMagnitude = compressSpeed * mRelaxationDamper;
                Vector3 dampingForce = dampingMagnitude * avgNormal;
                //sum all
                mCurrentLoad = springMagnitude;
                mSpringForce = springForce - dampingForce;
                //avoid negative force if no RelaxationDownforce
                if (!mVehicle.getRelaxationDownforce() && Vector3.Dot(mSpringForce, avgNormal) < 0)
                {
                    mSpringForce = Vector3.zero;
                }
            }
            else mLastCompressLength = totalLength;
        }

        //low speed sticky tire hack
        //when below minSpeed zero out lateral components of suspension
        void stickyTireUpdate()
        {
            if (!mHasContact) return;

            if (mVehicle.getStickyTireState() < 1.0f)
            {
                //eliminate lateral spring force
                Vector3 localSpring = mSpringForce;
                float lateralSize = 0.0f;
                //inverse rotate
                float ry = mHubTransform.rotation.eulerAngles.y;
                Quaternion q = Quaternion.Euler(0, ry, 0);
                localSpring = Quaternion.Inverse(q) * localSpring;
                //zero out laterals
                lateralSize += Mathf.Abs(localSpring.x);
                localSpring.x = 0.0f;
                if (mBraking > 0f || mVehicle.isHandbrakeOn())
                {
                    lateralSize += Mathf.Abs(localSpring.z);
                    localSpring.z = 0.0f;
                }
                //back to original rotation
                mSpringForce = q * localSpring;
                //add slope factor
                float angle = Vector3.Angle(mHubTransform.up, Vector3.up);
                angle = Mathf.Clamp(angle, -80.0f, 80.0f); //clamp tangent
                float tanAngle = Mathf.Tan(angle * Mathf.Deg2Rad);
                float verticalSize = tanAngle * lateralSize;
                mSpringForce.y += verticalSize;
            }
        }

        void updateLocalVelocity()
        {
            mGroundVelocity = mBody.GetPointVelocity(mHubTransform.position);
            //detect ground is enabled and sitting on a moving object
            if (mVehicle.getDetectGround() && mRayHit.rigidbody != null)
            {
                mGroundVelocity -= mRayHit.rigidbody.GetPointVelocity(mRayHit.point);
            }
            mLocalVelocity = mHubTransform.InverseTransformVector(mGroundVelocity);
            if ((mCurrentSteer + mToeAngle) != 0f)
            {
                Quaternion qs = Quaternion.Euler(0f, -mCurrentSteer - mToeAngle, 0f);
                mLocalVelocity = qs * mLocalVelocity;
            }
        }

        void updateSlipValues()
        {
            //lateral slip
            mLateralSlip = mHasContact ? -mLocalVelocity.x : 0f;

            //longitudinal slip
            float wheelSpeed = mRealSpeed * mRadius;
            mLongitudinalSlip = mHasContact ? wheelSpeed - mLocalVelocity.z : 0f;

            //slip ratio
            mSlipRatio = mLongitudinalSlip / (mLocalVelocity.z + Mathf.Epsilon);

            //slip angle
            mSlipAngle = Mathf.Atan(mLocalVelocity.x / (Mathf.Abs(mLocalVelocity.z) + Mathf.Epsilon));

            //s - a - p
            if (mTire.getCombineMode() == Tire.ForceCombineMode.GRIP)
            {
                mS = Mathf.Abs(mSlipRatio / mTire.getMaxLongitudinalForceSlip());
                mA = Mathf.Abs(mSlipAngle / mTire.getMaxLateralForceAngle());
                mP = Mathf.Sqrt(mS * mS + mA * mA);
                mSlipRatio = Math.Sign(mSlipRatio) * mP * mTire.getMaxLongitudinalForceSlip(); //signum
                mSlipAngle = Math.Sign(mSlipAngle) * mP * mTire.getMaxLateralForceAngle(); //signum
            }

            //limit slip ratio/angle
            mSlipRatio = Mathf.Clamp(mSlipRatio, -1.0f, 1.0f);
            mSlipAngle = Mathf.Clamp(mSlipAngle, -0.475f * Mathf.PI, 0.475f * Mathf.PI); //avoid extremas
        }

        void updateTireFrictionValues()
        {
            mTire.calculate(mCurrentLoad, mSlipRatio, mSlipAngle, mCamberAngle * Mathf.Deg2Rad, mPressure, mRadius, mLocalVelocity.z);
            mLngFrictionForce = mTire.getLongitudinalForce();
            mLatFrictionForce = mTire.getLateralForce();
            mCombinedTorque = mTire.getCombinedTorque();

            //additional factors : direct friction coeff, lng/lat friction coeff, 2x magic
            mLngFrictionForce *= mFrictionFactor * mLngFriction * 2f;
            mLatFrictionForce *= mFrictionFactor * mLatFriction * 2f;
        }

        void updateWheelSpeed(float dt)
        {
            mWheelTorque = 0f;
            mASRActive = false;
            mABSActive = false;

            //magic number to overcome momentum problem of large dt's
            float momentum = 0.5f * (mMass * (mRadius * mRadius));
            float magicMomentum = (dt / 0.002f) * momentum;

            //acceleration coeff. instead of (torque / momentum)
            //this helps to spin more or less acc. to grip & load
            float absLngFriction = Mathf.Abs(mLngFrictionForce);
            float accCoeff = 1f / magicMomentum;
            if (mHasContact)
            {
                float perWheelForce = mVehicle.getMassPerWheel() * Utility.gravitySize;
                if (mLngFrictionForce == 0.0f) accCoeff = 0.0f;
                else accCoeff = (perWheelForce / absLngFriction) / magicMomentum;
            }

            //contact update
            float groundSpeed = mLocalVelocity.z / mRadius;
            if (mHasContact)
            {
                float groundDelta = groundSpeed - mRealSpeed;
                float angularAcc = Mathf.Abs(groundDelta) * absLngFriction * accCoeff;
                groundDelta = Mathf.Sign(groundDelta) * Mathf.Min(angularAcc * dt, Mathf.Abs(groundDelta));
                mRealSpeed += groundDelta;
            }

            //calc. rolling friction
            float frictionTorque = 0.1f * mVehicle.getRollingResistanceCoeff() * Mathf.Abs(mRealSpeed);

            //sign of initial speed
            float iSign = Mathf.Sign(mRealSpeed);

            //apply engine torque
            float axleTorque = mAxle.getTorque(this) / mRadius;
            if (axleTorque != 0.0f)
            {
                //engine brake if no throttle
                bool engineBrake = mVehicle.getEngine().getThrottle() == 0f;
                if (engineBrake)
                {
                    axleTorque = -iSign * Mathf.Abs(axleTorque) - iSign * frictionTorque;
                }
                else
                {
                    //apply friction torque
                    float sign = Math.Sign(axleTorque); //signum
                    axleTorque += -iSign * frictionTorque;
                    //overflow case
                    if (sign != Mathf.Sign(axleTorque)) axleTorque = 0.0f;
                }

                //done with friction
                frictionTorque = 0.0f;

                //try to match target speed
                float targetSpeed = mAxle.getCurRpm() * Utility.rpm2Rads;

                //apply only when on ground
                if (mHasContact)
                {
                    //update target speed
                    if (axleTorque > 0f && mRealSpeed > targetSpeed) targetSpeed = mRealSpeed;
                    else if (axleTorque < 0f && mRealSpeed < targetSpeed) targetSpeed = mRealSpeed;

                    float angularAcc = axleTorque * accCoeff;
                    float deltaV = angularAcc * dt;

                    //ASR
                    {
                        //this check avoids unnecessary slips over low friction surfaces at low speeds
                        bool lowSpeedCase = engineBrake && Mathf.Abs(mRealSpeed) < 1f;
                        //asr is set to inactive for engine brake case, this avoids irrelevant asr activation at low speeds
                        mASRActive = !engineBrake && mVehicle.getASR() > 0.0f && Mathf.Abs(deltaV) > Mathf.Abs(mTire.getMaxLongitudinalForceSlip() * mRealSpeed);
                        if (mASRActive || lowSpeedCase)
                        {
                            float oldDeltaV = deltaV;
                            deltaV = Mathf.Abs(mTire.getMaxLongitudinalForceSlip() * mRealSpeed) * Mathf.Sign(oldDeltaV);
                            deltaV = Mathf.Lerp(oldDeltaV, deltaV, lowSpeedCase ? 1f : mVehicle.getASR());
                        }
                    }

                    //apply
                    mWheelTorque += axleTorque;
                    mRealSpeed += deltaV;

                    //clamp with target speed
                    if ((deltaV > 0f && mRealSpeed > targetSpeed) || (deltaV < 0f && mRealSpeed < targetSpeed))
                    {
                        mRealSpeed = targetSpeed;
                    }
                }
                //just set an ideal speed or wheel gets unrealistic speed
                //engine limiter is not checked, rpm will stay fixed at limit
                else if (!mVehicle.getEngine().isLimiterOn()) mRealSpeed = targetSpeed;
            }

            //apply brake torque
            if (mBraking > 0.0f)
            {
                float brakeTorque = mBraking * mBrakeTorque;
                float angularAcc = brakeTorque * accCoeff;
                float deltaV = angularAcc * dt;
                deltaV = Mathf.Min(deltaV, Mathf.Abs(mRealSpeed));

                //ABS?
                bool cancelABS = mVehicle.isHandbrakeOn() && mAxle.hasHandBrake();
                mABSActive = mVehicle.getABS() > 0.0f && !cancelABS && deltaV > mTire.getMaxLongitudinalForceSlip() * Mathf.Abs(mRealSpeed);
                if (mABSActive)
                {
                    float oldDeltaV = deltaV;
                    deltaV = mTire.getMaxLongitudinalForceSlip() * Mathf.Abs(mRealSpeed);
                    deltaV = Mathf.Lerp(oldDeltaV, deltaV, mVehicle.getABS());
                }

                //apply
                mWheelTorque -= iSign * brakeTorque;
                if (cancelABS) mRealSpeed -= iSign * deltaV * mVehicle.getHandbrakePower();
                else mRealSpeed -= iSign * deltaV;

                //overflow case
                if (iSign != Mathf.Sign(mRealSpeed)) mRealSpeed = 0.0f;
            }

            //apply rolling friction if remains
            if (frictionTorque != 0f)
            {
                //avoid accCoeff here, causes problems with tiremodels that have vertical shifts
                float angularAcc = frictionTorque * 1.0f / magicMomentum;
                float deltaV = Mathf.Min(angularAcc * dt, Mathf.Abs(mRealSpeed));
                mWheelTorque -= iSign * frictionTorque;
                mRealSpeed -= iSign * deltaV;
                //overflow case
                if (iSign != Mathf.Sign(mRealSpeed)) mRealSpeed = 0.0f;
            }
        }

        void applyRpmLimit(float dt)
        {
            //two types of limit needed
            //1 that only limits mRealSpeed
            //1 that applies braking to limit speed
            float maxSpeedKMH = mVehicle.getWheelSpeedLimitKMH();
            if (maxSpeedKMH == 0f) return;
            float realSpeedKMH = getKMHSpeed();
            float clutch = mVehicle.getTransmission().getClutchPower();
            bool allowReverse = mVehicle.getAllowReverse();

            //if overflow is the same sign as limit, then set tire speed to limit
            if (Mathf.Sign(maxSpeedKMH) == Mathf.Sign(realSpeedKMH))
            {
                if (Mathf.Abs(realSpeedKMH) > Mathf.Abs(maxSpeedKMH))
                {
                    float targetRealSpeedKMH = Mathf.Lerp(realSpeedKMH, maxSpeedKMH, clutch);
                    mRealSpeed = targetRealSpeedKMH / (Utility.rads2Rpm * mWheelRPMToKMH);
                }
            }
            //if overflow is not the same sign as limit, then set tire speed to 0
            else if (!allowReverse) mRealSpeed = Mathf.Lerp(mRealSpeed, 0f, clutch);

            //any need for a braking limit
            mSpeedOverflow = 0f;
            float absSpeedKMH = Mathf.Abs(mLocalVelocity.z) * Utility.ms2kmh;

            //compare absSpeed with fixed dt instead of zero to avoid small impulses on flat surfaces
            if (mHasContact && absSpeedKMH > dt && maxSpeedKMH != 0f)
            {
                if (Mathf.Sign(maxSpeedKMH) == Mathf.Sign(mLocalVelocity.z))
                {
                    mSpeedOverflow = absSpeedKMH - Mathf.Abs(maxSpeedKMH);
                }
                else if (!allowReverse) mSpeedOverflow = 1f;

                if (mSpeedOverflow > 0f)
                {
                    mSpeedOverflow = clutch * Mathf.Clamp01(mSpeedOverflow);
                }
            }
        }

        void updateWheelFriction(float dt)
        {
            mLateralForce = Vector3.zero;
            mLongitudinalForce = Vector3.zero;
            if (mHasContact)
            {
                float m = mCurrentLoad / Utility.gravitySize;
                
                //lateral force
                //automatically avoids low speed jitters
                {
                    float absLatFriction = Mathf.Abs(mLatFrictionForce);
                    //required acc. to stop dv/dt
                    float a = mLateralSlip / dt;
                    //required f for acc. f=ma
                    float f = m * a * mVehicle.getSimCoeff();
                    //clamp
                    f = Mathf.Clamp(f, -absLatFriction, absLatFriction);
                    //apply
                    mLateralForce = mLatScale * f * getGlobalRight();
                }

                //longitudinal force
                //limit wheel torque with max-friction
                float absLngFriction = Mathf.Abs(mLngFrictionForce);
                float currentForce = Mathf.Clamp(mWheelTorque, -absLngFriction, absLngFriction);

                //keeps vehicle from going faster than engine rpm limit
                //this is an artificial force so take clutch power into account here
                //mSpeedOverflow is multiplied with clutch power above in applyRpmLimit
                if (mSpeedOverflow > 0f)
                {
                    currentForce = Math.Sign(-mLocalVelocity.z) * absLngFriction * mSpeedOverflow; //signum
                }

                //automatically avoids low speed jitters
                //this smooths the lng. force but using this for engine torque may result low traction
                //so this is only activated when braking
                if (mBraking > 0f)
                {
                    float absCurrentForce = Mathf.Abs(currentForce);
                    //required acc. to stop dv/dt
                    float a = mLongitudinalSlip / dt;
                    //required f for acc. f=ma
                    float f = m * a * mVehicle.getSimCoeff();
                    //clamp
                    f = Mathf.Clamp(f, -absCurrentForce, absCurrentForce);
                    //apply
                    mLongitudinalForce = mLngScale * f * getGlobalForward();
                    //set feedback torque
                    mFeedbackTorque = Mathf.Abs(mWheelTorque) + absLngFriction;
                }
                else
                {
                    mLongitudinalForce = mLngScale * currentForce * getGlobalForward();
                    //set feedback torque
                    mFeedbackTorque = Mathf.Max(absLngFriction, mCurrentLoad);
                }
            }
            else mFeedbackTorque = Mathf.Abs(mWheelTorque);
        }

        //used for slip angle calculation
        Vector3 getGlobalVelocity()
        {
            Quaternion qs = Quaternion.Euler(0.0f, mCurrentSteer + mToeAngle, 0.0f);
            Vector3 gvel = qs * mLocalVelocity;
            return mHubTransform.TransformVector(gvel);
        }

        //used from esp calculation, in vehicle space
        public Vector3 getLocalVelocity()
        {
            Quaternion qs = Quaternion.Euler(0.0f, mCurrentSteer + mToeAngle, 0.0f);
            return qs * mLocalVelocity;
        }

        //used for slip angle and longitudinal force calculation
        public Vector3 getGlobalForward()
        {
            Quaternion qs = Quaternion.Euler(0.0f, mCurrentSteer + mToeAngle, 0.0f);
            return qs * mHubTransform.forward;
        }

        //used for lateral force calculation, camber is ignored
        public Vector3 getGlobalRight()
        {
            Quaternion qs = Quaternion.Euler(0.0f, mCurrentSteer + mToeAngle, 0.0f);
            return qs * mHubTransform.right;
        }

        //used for antirollbars
        public void applyVerticalForce(float force)
        {
            if (hasContact()) mBody.AddForceAtPosition(force * mHubTransform.up, mHubTransform.position);
        }

        //set radius for individual wheels
        public void setRadius(float r)
        {
            mRadius = r;
            //extra params
            mCircumference = 2.0f * Mathf.PI * mRadius;
            mWheelRPMToKMH = mCircumference * 60.0f / 1000.0f;
        }

        //calculate wheel radius and width from meshes
        public void calculateSizes(out float r, out float w)
        {
            if (mWheelTransform != null)
            {
                Bounds b = Utility.getBounds(mWheelTransform.gameObject);
                r = b.extents.y;
                w = b.size.x;
            }
            else r = w = 0.0f;
        }

        public void saveState(ref VehicleState state)
        {
            if (state.mActive == null)
            {
                int ct = mVehicle.getAxleCount();
                state.mActive = new bool[ct, 2];
                state.mLastCompressLength = new float[ct, 2];
                state.mPitch = new float[ct, 2];
                state.mRealSpeed = new float[ct, 2];
            }
            int i = mAxle.getIndex();
            int j = mIsLeft ? 0 : 1;
            state.mActive[i, j] = mIsActive;
            state.mLastCompressLength[i, j] = mLastCompressLength;
            state.mPitch[i, j] = mPitch;
            state.mRealSpeed[i, j] = mRealSpeed;
        }

        public void loadState(VehicleState state)
        {
            int i = mAxle.getIndex();
            int j = mIsLeft ? 0 : 1;
            if (state.mActive != null)
            {
                mIsActive = state.mActive[i, j];
                mLastCompressLength = state.mLastCompressLength[i, j];
                mPitch = state.mPitch[i, j];
                mRealSpeed = state.mRealSpeed[i, j];
            }
        }

        public void uiWindowFunction(int windowID)
        {
            string lbl = mAxle.getIndex() + " " + (mIsLeft ? "L" : "R");
            int y = 40 + mAxle.getIndex() * 45 + (mIsLeft ? 0 : 20);
            GUI.Label(new Rect(10, y, 1000, 25), lbl);
            GUI.Label(new Rect(50, y, 150, 25), getKMHSpeed().ToString("f1"));
            GUI.Label(new Rect(100, y, 150, 25), mWheelTorque.ToString("f0"));
            GUI.Label(new Rect(150, y, 150, 25), mCurrentLoad.ToString("f0"));
            GUI.Label(new Rect(215, y, 150, 25), mCompressedLength.ToString("f2"));
            GUI.Label(new Rect(295, y, 150, 25), mCurrentSteer.ToString("f0"));
            GUI.Label(new Rect(375, y, 150, 25), mLatFriction.ToString("f1") + "/" + mLngFriction.ToString("f1"));
            GUI.Label(new Rect(485, y, 150, 25), mFrictionFactor.ToString("f2"));
            GUI.Label(new Rect(555, y, 150, 25), mLongitudinalForce.magnitude.ToString("f0"));
            GUI.Label(new Rect(625, y, 150, 25), mLongitudinalSlip.ToString("f2"));
            GUI.Label(new Rect(680, y, 150, 25), mSlipRatio.ToString("f2"));
            GUI.Label(new Rect(745, y, 150, 25), mLateralForce.magnitude.ToString("f0"));
            GUI.Label(new Rect(810, y, 150, 25), mLateralSlip.ToString("f2"));
            GUI.Label(new Rect(865, y, 150, 25), mSlipAngle.ToString("f2"));
        }

        public void drawGizmo()
        {
            if (!mIsActive) return;
            //draw wheel
            Gizmos.color = Color.green;
            float hw = 0.5f * mWidth;
            float radi = mVehicle == null ? 0.25f : mRadius;
            float step = Mathf.PI / 10f;
            for (float angle = 0; angle < 2f * Mathf.PI; angle += step)
            {
                Vector3 dir1 = Vector3.RotateTowards(Vector3.up, Vector3.down, angle, 0f);
                Vector3 dir2 = Vector3.RotateTowards(Vector3.up, Vector3.down, angle + step, 0f);
                Vector3 p1 = mWheelTransform.position + radi * (mWheelTransform.rotation * dir1) + hw * mWheelTransform.right;
                Vector3 p2 = mWheelTransform.position + radi * (mWheelTransform.rotation * dir2) + hw * mWheelTransform.right;
                Gizmos.DrawLine(p1, p2);
                p1 = mWheelTransform.position + radi * (mWheelTransform.rotation * dir1) - hw * mWheelTransform.right;
                p2 = mWheelTransform.position + radi * (mWheelTransform.rotation * dir2) - hw * mWheelTransform.right;
                Gizmos.DrawLine(p1, p2);
            }

            //hub to wheel
            Transform hubTrans = mHubTransform == null ? mWheelTransform : mHubTransform;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(hubTrans.position, mWheelTransform.position);

            //draw rays
            int rays = mVehicle == null ? 1 : mVehicle.getRayCount();
            int lrays = mVehicle == null ? 1 : mVehicle.getLateralRayCount();
            Quaternion steerQuat = Quaternion.AngleAxis(mCurrentSteer + mToeAngle, hubTrans.up);
            Vector3 dirFrom = steerQuat * -hubTrans.up;
            Vector3 dirTo = steerQuat * hubTrans.forward;
            float castDist = (mSuspensionUpTravel + mSuspensionDownTravel) + mRadius;
            float maxAngle = 0f;
            float stepAngle = 1f;
            float startX = 0f;
            float stepX = 1f;
            float endX = 0f;

            if (rays > 1)
            {
                maxAngle = 90f;
                float yOffsetAngle = Mathf.Atan2(mCompressedLength, mRadius) * Mathf.Rad2Deg;
                maxAngle -= yOffsetAngle;
                stepAngle = (2f * maxAngle) / (rays - 1);
            }

            if (lrays > 1)
            {
                startX = -0.5f * mWidth;
                endX = 0.5f * mWidth;
                stepX = mWidth / (lrays - 1);
            }

            Gizmos.color = Color.yellow;
            for (float x = startX; x <= endX; x += stepX)
            {
                Vector3 addx = x * mWheelTransform.right;
                for (float angle = -maxAngle; angle <= (maxAngle + 0.001f); angle += stepAngle)
                {
                    Vector3 cdir = Vector3.RotateTowards(dirFrom, dirTo, angle * Mathf.Deg2Rad, 0.0f);
                    Vector3 origin = hubTrans.position + addx;
                    Gizmos.DrawLine(origin, origin + castDist * cdir);
                }
            }
        }
    }
}