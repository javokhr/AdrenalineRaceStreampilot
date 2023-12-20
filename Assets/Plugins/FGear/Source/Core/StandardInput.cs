// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System;
using UnityEngine;

namespace FGear
{
    [System.Serializable]
    public class StandardInput : TelemetryDrawer
    {
        public enum ControllerType
        {
            KEYBOARD,
            JOYSTICK,
            WHEEL
        }

        public enum ShifterType
        {
            AUTO,
            SEQUENTIAL,
            MANUAL
        }

        [SerializeField]
        bool mEnabled = true;
        [SerializeField]
        ControllerType mControllerType = ControllerType.KEYBOARD;
        [SerializeField]
        ShifterType mShifterType = ShifterType.AUTO;
        [SerializeField]
        bool mUseRawAxis = true;
        [SerializeField]
        bool mCombinedAxis = true;
        [SerializeField]
        string mVerticalAxis = "Vertical";
        [SerializeField]
        string mHorizontalAxis = "Horizontal";
        [SerializeField]
        string mThrottleAxis = "Vertical";
        [SerializeField]
        string mBrakeAxis = "Vertical";
        [SerializeField]
        string mLeftAxis = "Horizontal";
        [SerializeField]
        string mRightAxis = "Horizontal";
        [SerializeField]
        string mClutchAxis = "Fire1";
        [SerializeField]
        string mHandbrakeAxis = "Fire2";
        [SerializeField]
        string mShiftAxis = "Fire3";
        [SerializeField]
        string mShift1Axis = "";
        [SerializeField]
        string mShift2Axis = "";
        [SerializeField]
        string mShift3Axis = "";
        [SerializeField]
        string mShift4Axis = "";
        [SerializeField]
        string mShift5Axis = "";
        [SerializeField]
        string mShift6Axis = "";
        [SerializeField]
        string mShiftRAxis = "";

        [SerializeField, Range(0.1f, 10f)]
        float mSteerSensivity = 5f;
        [SerializeField]
        AnimationCurve mSteerSpeedCurve;
        [SerializeField]
        AnimationCurve mSteerLimitCurve;
        [SerializeField]
        float mSteerInputGravity = 1.5f;
        [SerializeField, Range(10f, 100f)]
        float mSteerRange = 100f;
        [SerializeField, Range(0f, 0.9f)]
        float mSteerDeadzone = 0.01f;

        [SerializeField, Range(0.1f, 10f)]
        float mThrottleSensivity = 3f;
        [SerializeField, Range(10f, 100f)]
        float mThrottleRange = 100f;
        [SerializeField, Range(0f, 0.9f)]
        float mThrottleDeadzone = 0.01f;

        [SerializeField, Range(0.1f, 10f)]
        float mBrakingSensivity = 3f;
        [SerializeField, Range(10f, 100f)]
        float mBrakingRange = 100f;
        [SerializeField, Range(0f, 0.9f)]
        float mBrakingDeadzone = 0.01f;

        [SerializeField, Range(0.1f, 10f)]
        float mClutchSensivity = 3f;
        [SerializeField, Range(10f, 100f)]
        float mClutchRange = 100f;
        [SerializeField, Range(0f, 0.9f)]
        float mClutchDeadzone = 0.01f;

        [SerializeField, Range(0f, 100f)]
        float mSteeringAssist = 0f;
        [SerializeField, Range(0.0f, 90.0f)]
        float mSteeringAssistThresholdAngle = 1f;

        [NonSerialized]
        Vehicle mVehicle;

        //final inputs
        float mEngineInput;
        float mBrakeInput;
        float mSteerInput;
        float mClutchInput;
        //raw inputs
        float mForward = 0f;
        float mBackward = 0f;
        float mHorizontal = 0f;
        float mClutch = 0f;
        //resulting inputs
        int mThrottleState = 0;
        float mEngineValue = 0f;    //[-1, 1]
        float mBrakeValue = 0f;     //[0, 1]
        float mClutchValue = 0f;
        bool mHandBrakeInput = false;
        int mGearboxInput = 0;
        int mLastGearboxInput = 0;
        float mTargetSteeringInput = 0f;
        float mCurrentSteeringInput = 0f;
        float mCurrentSteeringLimit = 0f;

        bool mReadInputs = true; //if set to false, inputs should be provided manually(see setInputs method)
        bool mSteerAssistActive = false;
        bool mStartGridMode = false;
        
        #region  getters & setters
        public bool isEnabled() { return mEnabled; }
        public ControllerType getControllerType() { return mControllerType; }
        public ShifterType getShifterType() { return mShifterType; }
        public string getVerticalAxis() { return mVerticalAxis; }
        public string getHorizontalAxis() { return mHorizontalAxis; }
        public string getClutchAxis() { return mClutchAxis; }
        public string getHandbrakeAxis() { return mHandbrakeAxis; }
        public string getShiftAxis() { return mShiftAxis; }
        public string getShift1Axis() { return mShift1Axis; }
        public string getShift2Axis() { return mShift2Axis; }
        public string getShift3Axis() { return mShift3Axis; }
        public string getShift4Axis() { return mShift4Axis; }
        public string getShift5Axis() { return mShift5Axis; }
        public string getShift6Axis() { return mShift6Axis; }
        public string getShiftRAxis() { return mShiftRAxis; }
        public float getSteerSensivity() { return mSteerSensivity; }
        public float getSteerInputGravity() { return mSteerInputGravity; }
        public float getSteerRange() { return mSteerRange; }
        public float getSteerDeadzone() { return mSteerDeadzone; }
        public float getThrottleSensivity() { return mThrottleSensivity; }
        public float getThrottleRange() { return mThrottleRange; }
        public float getThrottleDeadzone() { return mThrottleDeadzone; }
        public float getBrakingSensivity() { return mBrakingSensivity; }
        public float getBrakingRange() { return mBrakingRange; }
        public float getBrakingDeadzone() { return mBrakingDeadzone; }
        public float getClutchSensivity() { return mClutchSensivity; }
        public float getClutchRange() { return mClutchRange; }
        public float getClutchDeadzone() { return mClutchDeadzone; }
        public float getSteeringAssist() { return mSteeringAssist; }
        public float getSteeringAssistThresholdAngle() { return mSteeringAssistThresholdAngle; }
        public float getEngineRawInput() { return mForward; }
        public float getBrakeRawInput() { return mBackward; }
        public float getSteerRawInput() { return mHorizontal; }
        public float getClutchRawInput() { return mClutch; }
        public float getEngineFinalInput() { return mEngineInput; }
        public float getBrakeFinalInput() { return mBrakeInput; }
        public float getSteerFinalInput() { return mSteerInput; }
        public float getClutchFinalInput() { return mClutchInput; }
        public int getGearboxInput() { return mGearboxInput; }
        public int getThrottleState() { return mThrottleState; }
        public bool getHandbrakeInput() { return mHandBrakeInput; }
        public bool isSteeringAssistActive() { return mSteerAssistActive; }
        private float getSteerSpeed() { return mSteerSpeedCurve.Evaluate(Mathf.Abs(mVehicle.getKMHSpeed())); }
        private float getSteerLimit() { return mSteerLimitCurve.Evaluate(Mathf.Abs(mVehicle.getKMHSpeed())); }
        public AnimationCurve getSteerSpeedCurve() { return mSteerSpeedCurve; }
        public AnimationCurve getSteerLimitCurve() { return mSteerLimitCurve; }
        public void setEnabled(bool b) { mEnabled = b; }
        public void setStartGridMode(bool b) { mStartGridMode = b; mVehicle.getTransmission().setPauseTime(0f); }
        public void setReadInputs(bool b) { mReadInputs = b; }
        public void setControllerType(ControllerType ct) { mControllerType = ct; }
        public void setShifterType(ShifterType st) { mShifterType = st; }
        public void setVerticalAxis(string s) { mVerticalAxis = s; }
        public void setHorizontalAxis(string s) { mHorizontalAxis = s; }
        public void setClutchAxis(string s) { mClutchAxis = s; }
        public void setHandbrakeAxis(string s) { mHandbrakeAxis = s; }
        public void setShiftAxis(string s) { mShiftAxis = s; }
        public void setShift1Axis(string s) { mShift1Axis = s; }
        public void setShift2Axis(string s) { mShift2Axis = s; }
        public void setShift3Axis(string s) { mShift3Axis = s; }
        public void setShift4Axis(string s) { mShift4Axis = s; }
        public void setShift5Axis(string s) { mShift5Axis = s; }
        public void setShift6Axis(string s) { mShift6Axis = s; }
        public void setShiftRAxis(string s) { mShiftRAxis = s; }
        public void setSteerSensivity(float f) { mSteerSensivity = f; }
        public void setSteerInputGravity(float f) { mSteerInputGravity = f; }
        public void setSteerRange(float f) { mSteerRange = f; }
        public void setSteerDeadzone(float f) { mSteerDeadzone = f; }
        public void setThrottleSensivity(float f) { mThrottleSensivity = f; }
        public void setThrottleRange(float f) { mThrottleRange = f; }
        public void setThrottleDeadzone(float f) { mThrottleDeadzone = f; }
        public void setBrakingSensivity(float f) { mBrakingSensivity = f; }
        public void setBrakingRange(float f) { mBrakingRange = f; }
        public void setBrakingDeadzone(float f) { mBrakingDeadzone = f; }
        public void setClutchSensivity(float f) { mClutchSensivity = f; }
        public void setClutchRange(float f) { mClutchRange = f; }
        public void setClutchDeadzone(float f) { mClutchDeadzone = f; }
        public void setSteeringAssist(float f) { mSteeringAssist = f; }
        public void setSteeringAssistThresholdAngle(float f) { mSteeringAssistThresholdAngle = f; }
        #endregion

        public void init(Vehicle v)
        {
            //check nulls
            if (mSteerSpeedCurve == null) mSteerSpeedCurve = new AnimationCurve();
            if (mSteerLimitCurve == null) mSteerLimitCurve = new AnimationCurve();

            mVehicle = v;

            //do not leave curves empty
            if (mSteerSpeedCurve.length <= 1)
            {
                mSteerSpeedCurve = new AnimationCurve();
                mSteerSpeedCurve.AddKey(0f, 2f);
                mSteerSpeedCurve.AddKey(1f, 2f);
            }

            //do not leave curves empty
            if (mSteerLimitCurve.length <= 1)
            {
                mSteerLimitCurve = new AnimationCurve();
                mSteerLimitCurve.AddKey(0f, 100f);
                mSteerLimitCurve.AddKey(1f, 100f);
            }

            initTelemetryWindow();

            if (mEnabled)
            {
                string[] sticks = Input.GetJoystickNames();
                for (int i = 0; i < sticks.Length; i++)
                {
                    Debug.Log("StandardInput Device" + i + ":" + sticks[i]);
                }
            }
        }

        public void myUpdate(float dt, bool apply)
        {
            if (!mEnabled) return;

            if (mReadInputs) getInputs();
            updateAxisInputs(dt);
            modifyInputs(dt);
            filterInputs();
            if (apply)
            {
                applyInputs(mEngineInput, mThrottleState, mBrakeInput, mGearboxInput, mClutchInput, mHandBrakeInput, mSteerInput);
            }
        }

        float getAxis(string name)
        {
            try { return Input.GetAxis(name); }
            catch { return 0f; }
        }

        float getAxisRaw(string name)
        {
            try { return Input.GetAxisRaw(name); }
            catch { return 0f; }
        }

        //if mReadInputs is false this method should be called to provide the inputs
        //throttle [0, 1], brake [0, 1], steer [-1, 1], clutch [0, 1], hb [false, true]
        //sequential gear [-1 1], manual gear [0 7]
        public void setInputs(float throttle, float brake, float steer, float clutch, int gear, bool hb)
        {
            mForward = throttle;
            mBackward = brake;
            mHorizontal = steer;
            mClutch = clutch;
            mHandBrakeInput = hb;
            mGearboxInput = 0;
            if (mShifterType != ShifterType.MANUAL)
            {
                if (gear != mLastGearboxInput) mGearboxInput = gear;
                mLastGearboxInput = gear;
            }
            else mGearboxInput = gear;
        }

        public void resetInputs()
        {
            //final inputs
            mEngineInput = mBrakeInput = mSteerInput = mClutchInput = 0.0f;

            //raw inputs
            mForward = mBackward = mHorizontal = mClutch = 0.0f;

            //resulting inputs
            mThrottleState = mGearboxInput = mLastGearboxInput = 0;
            mEngineValue = mBrakeValue = mClutchValue = 0.0f;
            mTargetSteeringInput = mCurrentSteeringInput = mCurrentSteeringLimit = 0.0f;
            mHandBrakeInput = false;

            //vehicle itself
            if (mVehicle == null) return;
            mVehicle.getEngine().setThrottle(0.0f);
            mVehicle.getTransmission().setThrottleState(0);
            mVehicle.getTransmission().setClutchState(1.0f);
            mVehicle.setBraking(0.0f, false);
            mVehicle.setSteering(0.0f);
        }

        public void getInputs()
        {
            //axis inputs
            if (mCombinedAxis)
            {
                //combined vertical
                float vertical = mUseRawAxis ? getAxisRaw(mVerticalAxis) : getAxis(mVerticalAxis);
                mForward = vertical > 0.0f ? vertical : 0.0f;
                mBackward = vertical < 0.0f ? -vertical : 0.0f;
                //combined horizontal
                mHorizontal = mUseRawAxis ? getAxisRaw(mHorizontalAxis) : getAxis(mHorizontalAxis);
            }
            else
            {
                //seperate vertical throttle/brake
                mForward = mUseRawAxis ? getAxisRaw(mThrottleAxis) : getAxis(mThrottleAxis);
                mBackward = mUseRawAxis ? getAxisRaw(mBrakeAxis) : getAxis(mBrakeAxis);
                //seperate horizontal left/right
                float left = mUseRawAxis ? getAxisRaw(mLeftAxis) : getAxis(mLeftAxis);
                float right = mUseRawAxis ? getAxisRaw(mRightAxis) : getAxis(mRightAxis);
                mHorizontal = right - left;
            }

            //clutch
            mClutch = mUseRawAxis ? getAxisRaw(mClutchAxis) : getAxis(mClutchAxis);

            //clutch axis is between -1 and 1, scale down to [0,1]
            if (mControllerType == ControllerType.WHEEL)
            {
                mClutch = (mClutch + 1f) / 2f;
            }

            //shifter
            mGearboxInput = 0;
            if (mShifterType != ShifterType.MANUAL)
            {
                int shiftAxis = (int)getAxis(mShiftAxis);
                if (shiftAxis != mLastGearboxInput) mGearboxInput = shiftAxis;
                mLastGearboxInput = shiftAxis;
            }
            else if (mShifterType == ShifterType.MANUAL)
            {
                if (getAxis(mShift1Axis) > 0f) mGearboxInput = 1;
                else if (getAxis(mShift2Axis) > 0f) mGearboxInput = 2;
                else if (getAxis(mShift3Axis) > 0f) mGearboxInput = 3;
                else if (getAxis(mShift4Axis) > 0f) mGearboxInput = 4;
                else if (getAxis(mShift5Axis) > 0f) mGearboxInput = 5;
                else if (getAxis(mShift6Axis) > 0f) mGearboxInput = 6;
                else if (getAxis(mShiftRAxis) > 0f) mGearboxInput = 7;
            }

            //other inputs
            mHandBrakeInput = getAxis(mHandbrakeAxis) != 0f;
        }

        void updateAxisInputs(float dt)
        {
            //throttle input
            if (mForward > 0.0f) mThrottleState = 1;
            else if (mBackward > 0.0f) mThrottleState = -1;
            else mThrottleState = 0;

            //apply throttle dead zone
            float throttleVerticle = mForward - mThrottleDeadzone;
            if (throttleVerticle < 0f) throttleVerticle = 0f;
            else throttleVerticle = throttleVerticle / (1f - mThrottleDeadzone) * mForward;

            //engine input
            if (throttleVerticle > 0.0f)
            {
                if (mEngineValue < throttleVerticle)
                {
                    mEngineValue += mThrottleSensivity * dt;
                    if (mEngineValue > throttleVerticle) mEngineValue = throttleVerticle;
                }
                else if (mEngineValue > throttleVerticle)
                {
                    mEngineValue -= mThrottleSensivity * dt;
                    if (mEngineValue < throttleVerticle) mEngineValue = throttleVerticle;
                }
            }
            else if (mEngineValue > 0f)
            {
                if (mEngineValue > mThrottleRange / 100f)
                {
                    mEngineValue = mThrottleRange / 100f;
                }

                mEngineValue -= mThrottleSensivity * dt;
                if (mEngineValue < 0f) mEngineValue = 0f;
            }

            //apply brake dead zone
            float brakeVerticle = mBackward - mBrakingDeadzone;
            if (brakeVerticle < 0f) brakeVerticle = 0f;
            else brakeVerticle = brakeVerticle / (1f - mBrakingDeadzone) * mBackward;

            //brake input
            if (brakeVerticle > 0.0f)
            {
                if (mBrakeValue < brakeVerticle)
                {
                    mBrakeValue += mBrakingSensivity * dt;
                    if (mBrakeValue > brakeVerticle) mBrakeValue = brakeVerticle;
                }
                else if (mBrakeValue > brakeVerticle)
                {
                    mBrakeValue -= mBrakingSensivity * dt;
                    if (mBrakeValue < brakeVerticle) mBrakeValue = brakeVerticle;
                }
            }
            else if (mBrakeValue > 0f)
            {
                if (mBrakeValue > mBrakingRange / 100f)
                {
                    mBrakeValue = mBrakingRange / 100f;
                }

                mBrakeValue -= mBrakingSensivity * dt;
                if (mBrakeValue < 0f) mBrakeValue = 0f;
            }

            //apply clutch dead zone
            float clutchVerticle = mClutch - mClutchDeadzone;
            if (clutchVerticle < 0f) clutchVerticle = 0f;
            else clutchVerticle = clutchVerticle / (1f - mClutchDeadzone);

            //clutch input
            if (mClutchValue < clutchVerticle)
            {
                mClutchValue += mClutchSensivity * dt;
                if (mClutchValue > clutchVerticle) mClutchValue = clutchVerticle;
            }
            else if (mClutchValue > clutchVerticle)
            {
                mClutchValue -= mClutchSensivity * dt;
                if (mClutchValue < clutchVerticle) mClutchValue = clutchVerticle;
            }

            //steering input & apply dead zone
            mTargetSteeringInput = Mathf.Abs(mHorizontal) - mSteerDeadzone;
            if (mTargetSteeringInput < 0f) mTargetSteeringInput = 0f;
            else mTargetSteeringInput = mTargetSteeringInput / (1f - mSteerDeadzone) * Mathf.Sign(mHorizontal);
        }

        void modifyInputs(float dt)
        {
            //steering assist
            mSteerAssistActive = false;
            if (mSteeringAssist > 0f)
            {
                float targetAngle = 0f;
                if (mVehicle.getForwardSpeed() > 1f)
                {
                    Vector3 velDir = mVehicle.getVelocityDir();
                    Vector3 dir = mVehicle.getForwardDir();
                    float sign = Mathf.Sign(Vector3.Cross(dir, velDir).y);
                    targetAngle = sign * Vector3.Angle(dir, velDir);
                    //threshold
                    float minAngle = mSteeringAssistThresholdAngle;
                    float absTarget = Mathf.Abs(targetAngle);
                    if (absTarget < minAngle) targetAngle = 0.0f;
                    else targetAngle = Mathf.Sign(targetAngle) * (absTarget - minAngle);
                }
                //apply
                float maxAngle = mVehicle.getMaxSteeringAngle();
                mTargetSteeringInput += (0.01f * mSteeringAssist) * (targetAngle / maxAngle);
                //clamp
                mTargetSteeringInput = Mathf.Clamp(mTargetSteeringInput, -1f, 1f);
                //active?
                mSteerAssistActive = targetAngle != 0f;
            }

            //steering range limit
            mCurrentSteeringLimit = 1f;
            if (mControllerType != ControllerType.WHEEL && getSteerLimit() > 0f && mVehicle.getKMHSpeed() > 1f)
            {
                mCurrentSteeringLimit = 0.01f * getSteerLimit();
                if (Mathf.Abs(mTargetSteeringInput) > mCurrentSteeringLimit)
                {
                    mTargetSteeringInput = Mathf.Sign(mTargetSteeringInput) * mCurrentSteeringLimit;
                }
            }

            //steering speed limit
            if (mCurrentSteeringInput != mTargetSteeringInput)
            {
                float steerSpeed = mSteerSensivity;

                if (mControllerType != ControllerType.WHEEL)
                {
                    steerSpeed = getSteerSpeed();
                    //counter steer coeff.
                    if (mSteerInputGravity != 0f)
                    {
                        if (Math.Sign(mCurrentSteeringInput) != Math.Sign(mTargetSteeringInput))
                        {
                            steerSpeed *= mSteerInputGravity;
                        }
                    }
                    else if (mTargetSteeringInput == 0f) steerSpeed = 0f;
                }

                //apply
                float deltaInput = Mathf.Abs(mTargetSteeringInput - mCurrentSteeringInput);
                float steerTime = steerSpeed / deltaInput * dt;
                mCurrentSteeringInput = Mathf.Lerp(mCurrentSteeringInput, mTargetSteeringInput, steerTime);
            }
        }

        void filterInputs()
        {
            //apply engine dead zone & range
            float rangeFactor = 100f / mThrottleRange;
            mEngineInput = Mathf.Abs(mEngineValue) * rangeFactor;
            mEngineInput = Mathf.Clamp01(mEngineInput);

            //apply braking dead zone & range
            rangeFactor = 100f / mBrakingRange;
            mBrakeInput = Mathf.Abs(mBrakeValue)  * rangeFactor;
            mBrakeInput = Mathf.Clamp01(mBrakeInput);

            //apply clutch dead zone & range
            rangeFactor = 100f / mClutchRange;
            float clutchInput = Mathf.Abs(mClutchValue) * rangeFactor;
            mClutchInput = Mathf.Clamp01(clutchInput);

            //apply steering dead zone
            rangeFactor = 100f / mSteerRange;
            mSteerInput = mCurrentSteeringInput * rangeFactor;
            mSteerInput = Mathf.Clamp(mSteerInput, -mCurrentSteeringLimit, mCurrentSteeringLimit);
        }

        public void applyInputs(float e, int ts, float b, int gi, float c, bool hb, float s)
        {
            //throttle free, full brake, force neutral gear
            if (mStartGridMode)
            {
                mVehicle.getEngine().setThrottle(e);
                mVehicle.setBraking(1f, false);
                mVehicle.getTransmission().setCurrentGear(0, true);
                mVehicle.getTransmission().setPauseTime(1f);
                return;
            }

            //gearbox input
            if (mShifterType == ShifterType.SEQUENTIAL)
            {
                if (gi > 0) mVehicle.getTransmission().gearUp(false);
                else if (gi < 0) mVehicle.getTransmission().gearDown(false);
                if (!mVehicle.getTransmission().isAutoClutch())
                {
                    mVehicle.getTransmission().setClutchState(1.0f - c);
                }
            }
            else if (mShifterType == ShifterType.MANUAL)
            {
                mVehicle.getTransmission().setCurrentGear(gi, false);
                if (!mVehicle.getTransmission().isAutoClutch())
                {
                    mVehicle.getTransmission().setClutchState(1.0f - c);
                }
            }

            //invert engine/brake inputs in reverse(auto)
            bool invertVertical = mVehicle.getTransmission().isAutoChange() &&
                                  mVehicle.getTransmission().isAutoReverse() &&
                                  mVehicle.getTransmission().getCurGear() < 0;

            if (invertVertical)
            {
                float tmp = e;
                e = b;
                b = tmp;
            }

            //apply inputs
            {
                mVehicle.getEngine().setThrottle(e);
                mVehicle.getTransmission().setThrottleState(ts);
                mVehicle.setBraking(b, hb);
                mVehicle.setSteering(s);
            }
        }

        public void saveState(ref VehicleState state)
        {
            if (state.mInput == null) state.mInput = new VehicleState.InputState();
            state.mInput.mBrake = mBrakeInput;
            state.mInput.mClutch = mClutchInput;
            state.mInput.mEngine = mEngineInput;
            state.mInput.mGearbox = mGearboxInput;
            state.mInput.mHandbrake = mHandBrakeInput;
            state.mInput.mSteer = mSteerInput;
            state.mInput.mThrottle = mThrottleState;
        }

        public void loadState(VehicleState state)
        {
            mBrakeInput = state.mInput.mBrake;
            mClutchInput = state.mInput.mClutch;
            mEngineInput = state.mInput.mEngine;
            mGearboxInput = state.mInput.mGearbox;
            mHandBrakeInput = state.mInput.mHandbrake;
            mSteerInput = state.mInput.mSteer;
            mThrottleState = state.mInput.mThrottle;
            applyInputs(mEngineInput, mThrottleState, mBrakeInput, mGearboxInput, mClutchInput, mHandBrakeInput, mSteerInput);
        }

        protected override void initTelemetryWindow()
        {
            mWinID = Utility.winIDs++;
            mWindowRect = new Rect(185, Screen.height - 395, 175, 210);
        }

        public override void drawTelemetry()
        {
            mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Input");
        }

        void uiWindowFunction(int windowID)
        {
            GUI.DragWindow();
            GUI.contentColor = Utility.textColor;
            GUI.Label(new Rect(10, 20, 150, 25), "Enabled: " + mEnabled);
            GUI.Label(new Rect(10, 40, 150, 25), "Controller: " + mControllerType);
            GUI.Label(new Rect(10, 60, 150, 25), "Shifter: " + mShifterType);
            GUI.Label(new Rect(10, 80, 150, 25), "Throttle: %" + (100f * mVehicle.getEngine().getThrottle()).ToString("f0"));
            GUI.Label(new Rect(10, 100, 150, 25), "Brakes: %" + (100f * mVehicle.getAxle(0).getLeftWheel().getBraking()).ToString("f0"));
            GUI.Label(new Rect(10, 120, 150, 25), "Handbrake: " + mVehicle.isHandbrakeOn());
            GUI.Label(new Rect(10, 140, 150, 25), "Steering: %" + (100f * mVehicle.getNormalizedSteering()).ToString("f0"));
            GUI.Label(new Rect(10, 160, 150, 25), "Steer Speed: " + getSteerSpeed().ToString("f1"));
            GUI.Label(new Rect(10, 180, 150, 25), "Steer Limit: %" + getSteerLimit().ToString("f0"));
        }
    }
}