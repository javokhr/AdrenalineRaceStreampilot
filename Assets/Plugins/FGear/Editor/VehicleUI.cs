// Copyright (C) Yunus Kara 2019-2020. All Rights Reserved.

using UnityEngine;
using UnityEditor;
using FGear;

[CustomEditor(typeof(Vehicle))]
[CanEditMultipleObjects]
public class VehicleUI : Editor
{
    Vehicle mVehicle;
    static bool mShowGeneral;
    static bool mShowEngine;
    static bool mShowTransmission;
    static bool mShowAxles;
    static bool[] mShowAxle = new bool[8];
    static bool mShowWheels;
    static bool[] mShowWheel = new bool[8];
    static bool mShowSuspension;
    static bool mShowAero;
    static bool mShowInput;
    static bool mShowTelemetry;
    static bool mShowAssists;
    static bool mShowSaveLoad;
    static bool mShowForceScalers;

    public static GUIStyle Foldout
    {
        get
        {
            if (mFoldout == null)
            {
                mFoldout = new GUIStyle(EditorStyles.foldout);
                mFoldout.fontStyle = FontStyle.Bold;
            }
            return mFoldout;
        }
    }
    static GUIStyle mFoldout;

    //general
    SerializedProperty mMass;
    SerializedProperty mInertiaScale;
    SerializedProperty mCenterofMass;
    SerializedProperty mUpdateRate;
    SerializedProperty mCastType;
    SerializedProperty mRays;
    SerializedProperty mLateralRays;
    SerializedProperty mRaycastLayer;
    SerializedProperty mHandBrakePower;
    SerializedProperty mRollingResistanceCoeff;
    SerializedProperty mAllowReverse;
    SerializedProperty mBrakeEngagement;
    SerializedProperty mHardContact;
    SerializedProperty mRelaxationDownforce;
    SerializedProperty mTirePenetration;
    SerializedProperty mDetectGround;
    SerializedProperty mReactionScale;
    SerializedProperty mStickyTireSpeed;

    //engine
    SerializedProperty mTorqueCurve;
    SerializedProperty mTorqueScale;
    SerializedProperty mIdleRpm;
    SerializedProperty mLimitRpm;
    SerializedProperty mLimiterTime;
    SerializedProperty mEngineInertia;
    SerializedProperty mFrictionTorque;

    //transmission
    SerializedProperty mGearRatios;
    SerializedProperty mFinalGearRatio;
    SerializedProperty mAutoChange;
    SerializedProperty mAutoReverse;
    SerializedProperty mAutoClutch;
    SerializedProperty mChangeTime;
    SerializedProperty mClutchTime;
    SerializedProperty mGearUpRatio;
    SerializedProperty mGearDownRatio;
    SerializedProperty mClutchEngagement;
    SerializedProperty mClutchScale;

    //axles & wheels
    SerializedProperty mAxleList;

    //aero dynamics
    SerializedProperty mDragCoefficient;
    SerializedProperty mDownForceCoefficient;
    SerializedProperty mDownForceZOffset;

    //input
    SerializedProperty mEnabled;
    SerializedProperty mUseRawAxis;
    SerializedProperty mCombinedAxis;
    SerializedProperty mControllerType;
    SerializedProperty mShifterType;
    SerializedProperty mSteerSensivity;
    SerializedProperty mSteerSpeedCurve;
    SerializedProperty mSteerLimitCurve;
    SerializedProperty mSteerDeadZone;
    SerializedProperty mSteerRange;
    SerializedProperty mThrottleSensivity;
    SerializedProperty mThrottleDeadzone;
    SerializedProperty mThrottleRange;
    SerializedProperty mBrakingSensivity;
    SerializedProperty mBrakingDeadzone;
    SerializedProperty mBrakingRange;
    SerializedProperty mClutchSensivity;
    SerializedProperty mClutchDeadzone;
    SerializedProperty mClutchRange;
    SerializedProperty mSteeringAssist;
    SerializedProperty mSteeringAssistThresholdAngle;
    string[] mAxisList;
    int mHorizontalAxisIndex = 0;
    int mVerticalAxisIndex = 1;
    int mThrottleAxisIndex = 1;
    int mBrakeAxisIndex = 1;
    int mLeftAxisIndex = 0;
    int mRightAxisIndex = 0;
    int mClutchAxisIndex = 2;
    int mHandbrakeIndex = 3;
    int mShiftIndex = 4;
    int mShift1Index = 5;
    int mShift2Index = 6;
    int mShift3Index = 7;
    int mShift4Index = 8;
    int mShift5Index = 9;
    int mShift6Index = 10;
    int mShiftRIndex = 11;

    //telemetry
    SerializedProperty mTextColor;
    SerializedProperty mTShow;
    SerializedProperty mTShowAeroDynamics;
    SerializedProperty mTShowAxles;
    SerializedProperty mTShowEngine;
    SerializedProperty mTShowInput;
    SerializedProperty mTShowTransmission;
    SerializedProperty mTShowVehicle;
    SerializedProperty mTShowWheels;

    //assists
    SerializedProperty mAbs;
    SerializedProperty mAsr;
    SerializedProperty mUnderSteerAssist;
    SerializedProperty mMinUnderSteerAngle;
    SerializedProperty mOverSteerAssist;
    SerializedProperty mMinOverSteerAngle;
    SerializedProperty mFrontAntiRollPower;
    SerializedProperty mRearAntiRollPower;

    public void OnEnable()
    {
        mVehicle = (Vehicle)target;

        //general
        mMass = serializedObject.FindProperty("mMass");
        mInertiaScale = serializedObject.FindProperty("mInertiaScale");
        mCenterofMass = serializedObject.FindProperty("mCenterofMass");
        mUpdateRate = serializedObject.FindProperty("mUpdateRate");
        mCastType = serializedObject.FindProperty("mCastType");
        mRays = serializedObject.FindProperty("mRays");
        mLateralRays = serializedObject.FindProperty("mLateralRays");
        mRaycastLayer = serializedObject.FindProperty("mRaycastLayer");
        mHandBrakePower = serializedObject.FindProperty("mHandBrakePower");
        mRollingResistanceCoeff = serializedObject.FindProperty("mRollingResistanceCoeff");
        mAllowReverse = serializedObject.FindProperty("mAllowReverse");
        mBrakeEngagement = serializedObject.FindProperty("mBrakeEngagement");
        mHardContact = serializedObject.FindProperty("mHardContact");
        mRelaxationDownforce = serializedObject.FindProperty("mRelaxationDownforce");
        mTirePenetration = serializedObject.FindProperty("mTirePenetration");
        mDetectGround = serializedObject.FindProperty("mDetectGround");
        mReactionScale = serializedObject.FindProperty("mReactionScale");
        mStickyTireSpeed = serializedObject.FindProperty("mStickyTireSpeed");

        //engine
        mTorqueCurve = serializedObject.FindProperty("mEngine.mTorqueCurve");
        mTorqueScale = serializedObject.FindProperty("mEngine.mTorqueScale");
        mIdleRpm = serializedObject.FindProperty("mEngine.mIdleRpm");
        mLimitRpm = serializedObject.FindProperty("mEngine.mLimitRpm");
        mLimiterTime = serializedObject.FindProperty("mEngine.mLimiterTime");
        mEngineInertia = serializedObject.FindProperty("mEngine.mEngineInertia");
        mFrictionTorque = serializedObject.FindProperty("mEngine.mFrictionTorque");

        //transmission
        mGearRatios = serializedObject.FindProperty("mTransmission.mGearRatios");
        mFinalGearRatio = serializedObject.FindProperty("mTransmission.mFinalGearRatio");
        mAutoChange = serializedObject.FindProperty("mTransmission.mAutoChange");
        mAutoReverse = serializedObject.FindProperty("mTransmission.mAutoReverse");
        mAutoClutch = serializedObject.FindProperty("mTransmission.mAutoClutch");
        mChangeTime = serializedObject.FindProperty("mTransmission.mChangeTime");
        mClutchTime = serializedObject.FindProperty("mTransmission.mClutchTime");
        mGearUpRatio = serializedObject.FindProperty("mTransmission.mGearUpRatio");
        mGearDownRatio = serializedObject.FindProperty("mTransmission.mGearDownRatio");
        mClutchEngagement = serializedObject.FindProperty("mTransmission.mClutchEngagement");
        mClutchScale = serializedObject.FindProperty("mTransmission.mClutchScale");

        //axles/wheels
        mAxleList = serializedObject.FindProperty("mAxleList");
        
        //aero dynamics
        mDragCoefficient = serializedObject.FindProperty("mAeroDynamics.mDragCoefficient");
        mDownForceCoefficient = serializedObject.FindProperty("mAeroDynamics.mDownForceCoefficient");
        mDownForceZOffset = serializedObject.FindProperty("mAeroDynamics.mDownForceZOffset");

        //input
        mEnabled = serializedObject.FindProperty("mStandardInput.mEnabled");
        mUseRawAxis = serializedObject.FindProperty("mStandardInput.mUseRawAxis");
        mCombinedAxis = serializedObject.FindProperty("mStandardInput.mCombinedAxis");
        mControllerType = serializedObject.FindProperty("mStandardInput.mControllerType");
        mShifterType = serializedObject.FindProperty("mStandardInput.mShifterType");
        mSteerSensivity = serializedObject.FindProperty("mStandardInput.mSteerSensivity");
        mSteerSpeedCurve = serializedObject.FindProperty("mStandardInput.mSteerSpeedCurve");
        mSteerLimitCurve = serializedObject.FindProperty("mStandardInput.mSteerLimitCurve");
        mSteerDeadZone = serializedObject.FindProperty("mStandardInput.mSteerDeadzone");
        mSteerRange = serializedObject.FindProperty("mStandardInput.mSteerRange");
        mThrottleSensivity = serializedObject.FindProperty("mStandardInput.mThrottleSensivity");
        mThrottleDeadzone = serializedObject.FindProperty("mStandardInput.mThrottleDeadzone");
        mThrottleRange = serializedObject.FindProperty("mStandardInput.mThrottleRange");
        mBrakingSensivity = serializedObject.FindProperty("mStandardInput.mBrakingSensivity");
        mBrakingDeadzone = serializedObject.FindProperty("mStandardInput.mBrakingDeadzone");
        mBrakingRange = serializedObject.FindProperty("mStandardInput.mBrakingRange");
        mClutchSensivity = serializedObject.FindProperty("mStandardInput.mClutchSensivity");
        mClutchDeadzone = serializedObject.FindProperty("mStandardInput.mClutchDeadzone");
        mClutchRange = serializedObject.FindProperty("mStandardInput.mClutchRange");
        mSteeringAssist = serializedObject.FindProperty("mStandardInput.mSteeringAssist");
        mSteeringAssistThresholdAngle = serializedObject.FindProperty("mStandardInput.mSteeringAssistThresholdAngle");
        mAxisList = Utility.getAxisList();

        //telemetry
        mTextColor = serializedObject.FindProperty("mTelemetry.TextColor");
        mTShow = serializedObject.FindProperty("mTelemetry.Show");
        mTShowAeroDynamics = serializedObject.FindProperty("mTelemetry.ShowAeroDynamics");
        mTShowAxles = serializedObject.FindProperty("mTelemetry.ShowAxles");
        mTShowEngine = serializedObject.FindProperty("mTelemetry.ShowEngine");
        mTShowInput = serializedObject.FindProperty("mTelemetry.ShowInput");
        mTShowTransmission = serializedObject.FindProperty("mTelemetry.ShowTransmission");
        mTShowVehicle = serializedObject.FindProperty("mTelemetry.ShowVehicle");
        mTShowWheels = serializedObject.FindProperty("mTelemetry.ShowWheels");

        //assists
        mAbs = serializedObject.FindProperty("mABS");
        mAsr = serializedObject.FindProperty("mASR");
        mUnderSteerAssist = serializedObject.FindProperty("mUnderSteerAssist");
        mMinUnderSteerAngle = serializedObject.FindProperty("mMinUnderSteerAngle");
        mOverSteerAssist = serializedObject.FindProperty("mOverSteerAssist");
        mMinOverSteerAngle = serializedObject.FindProperty("mMinOverSteerAngle");
        mFrontAntiRollPower = serializedObject.FindProperty("mFrontAntiRollPower");
        mRearAntiRollPower = serializedObject.FindProperty("mRearAntiRollPower");

        postEnable();
    }

    void postEnable()
    {
        //general
        if (mRays.intValue % 2 == 0) mRays.intValue++; //keep odd number
        mStickyTireSpeed.floatValue = Mathf.Round(mStickyTireSpeed.floatValue);

        //if brake engagement curve is empty, create a default curve
        if (mBrakeEngagement.animationCurveValue.length <= 1)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(1f, 1f);
            mBrakeEngagement.animationCurveValue = curve;
        }

        //engine
        //if engine torque curve is empty, create a default curve
        if (mTorqueCurve.animationCurveValue.length <= 1)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(1000f, 75);
            curve.AddKey(4000f, 150f);
            curve.AddKey(6000, 100);
            mTorqueCurve.animationCurveValue = curve;
        }

        //transmission
        //if gear ratios array is empty, create a default one
        if (mGearRatios.arraySize < 2)
        {
            mGearRatios.arraySize = 2;
            mGearRatios.GetArrayElementAtIndex(0).floatValue = 5f;
            mGearRatios.GetArrayElementAtIndex(1).floatValue = 3f;
        }
        //if clutch engagement curve is empty, create a default curve
        if (mClutchEngagement.animationCurveValue.length <= 1)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(1f, 1f);
            mClutchEngagement.animationCurveValue = curve;
        }

        //axles
        if (mAxleList.arraySize < 2) mAxleList.arraySize = 2;

        //input
        //if steer speed curve is empty, create a default curve
        if (mSteerSpeedCurve.animationCurveValue.length <= 1)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 2f);
            curve.AddKey(1f, 2f);
            mSteerSpeedCurve.animationCurveValue = curve;
        }
        //if steer limit curve is empty, create a default curve
        if (mSteerLimitCurve.animationCurveValue.length <= 1)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 100f);
            curve.AddKey(1f, 100f);
            mSteerLimitCurve.animationCurveValue = curve;
        }

        //finalize
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        mShowGeneral = GUILayout.Toggle(mShowGeneral, "General Settings", "Button");

        if (mShowGeneral)
        {
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.PropertyField(mMass, new GUIContent("Mass"));
            EditorGUILayout.PropertyField(mInertiaScale, new GUIContent("Inertia Scale"));
            EditorGUILayout.PropertyField(mCenterofMass, new GUIContent("Center of Mass Offset"));
            EditorGUI.EndDisabledGroup();
            mUpdateRate.intValue = EditorGUILayout.IntSlider("Solver Update Rate(Hz)", mUpdateRate.intValue, 50, 1000);
            mUpdateRate.intValue = mUpdateRate.intValue / 10 * 10;
            EditorGUILayout.PropertyField(mCastType, new GUIContent("Cast Type"));
            EditorGUI.BeginDisabledGroup(mCastType.enumValueIndex != 0);
            EditorGUILayout.PropertyField(mRays, new GUIContent("Ray Count"));
            if (mRays.intValue % 2 == 0) mRays.intValue++; //keep odd number
            EditorGUILayout.PropertyField(mLateralRays, new GUIContent("Lateral Ray Count"));
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(mCastType.enumValueIndex == 2);
            EditorGUILayout.PropertyField(mRaycastLayer, new GUIContent("Raycast Layer Mask"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(mHandBrakePower, new GUIContent("HandBrake Power"));
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowEngine = GUILayout.Toggle(mShowEngine, "Engine Settings", "Button");

        if (mShowEngine)
        {
            //if engine torque curve is empty, create a default curve
            if (mTorqueCurve.animationCurveValue.length <= 1)
            {
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0f, 0f);
                curve.AddKey(1000f, 75);
                curve.AddKey(4000f, 150f);
                curve.AddKey(6000, 100);
                mTorqueCurve.animationCurveValue = curve;
            }

            //calc. max power and display
            float maxPower, maxPowerRpm;
            mVehicle.getEngine().calculateMaxPower(out maxPower, out maxPowerRpm);
            maxPower *= Utility.kw2hp;

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mTorqueCurve, new GUIContent("Torque Curve"));
            EditorGUILayout.PropertyField(mTorqueScale, new GUIContent("Torque Scale"));
            EditorGUILayout.LabelField("*Max Power " + maxPower.ToString("f0") + "hp @" + maxPowerRpm + "rpm");
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUILayout.PropertyField(mIdleRpm, new GUIContent("Idle Rpm"));
            EditorGUILayout.PropertyField(mLimitRpm, new GUIContent("Limit Rpm"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(mLimiterTime, new GUIContent("Limiter Time (ms)"));
            EditorGUILayout.PropertyField(mEngineInertia, new GUIContent("Engine Inertia"));
            EditorGUILayout.PropertyField(mFrictionTorque, new GUIContent("Friction Torque"));
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowTransmission = GUILayout.Toggle(mShowTransmission, "Transmission Settings", "Button");

        if (mShowTransmission)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mAutoChange, new GUIContent("Auto Change"));
            EditorGUI.BeginDisabledGroup(!mAutoChange.boolValue);
            EditorGUILayout.PropertyField(mAutoReverse, new GUIContent("Auto Reverse"));
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(mAutoChange.boolValue);
            EditorGUILayout.PropertyField(mAutoClutch, new GUIContent("Auto Clutch"));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(mChangeTime, new GUIContent("Change Time (ms)"));
            EditorGUILayout.PropertyField(mClutchTime, new GUIContent("Clutch Engage Time (ms)"));
            EditorGUILayout.PropertyField(mGearUpRatio, new GUIContent("Gear Up Ratio (%)"));
            EditorGUILayout.PropertyField(mGearDownRatio, new GUIContent("Gear Down Ratio (%)"));
            EditorGUILayout.PropertyField(mClutchEngagement, new GUIContent("Clutch Engagement"));
            EditorGUILayout.PropertyField(mClutchScale, new GUIContent("Clutch Power Scale (%)"));

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(mFinalGearRatio, new GUIContent("Final Gear Ratio"));

            //if gear ratios array is empty, create a default one
            if (mGearRatios.arraySize < 2)
            {
                mGearRatios.arraySize = 2;
                mGearRatios.GetArrayElementAtIndex(0).floatValue = 5f;
                mGearRatios.GetArrayElementAtIndex(1).floatValue = 3f;
            }

            EditorGUILayout.PropertyField(mGearRatios, new GUIContent("Gear Ratios"), true);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                //wheels should be updated before for max speed calculations
                if (mGearRatios.isExpanded)
                {
                    for (int i = 0; i < mVehicle.getAxleCount(); i++)
                    {
                        mVehicle.getAxle(i).getLeftWheel().refreshParameters(mVehicle.getAxle(i));
                        mVehicle.getAxle(i).getRightWheel().refreshParameters(mVehicle.getAxle(i));
                    }
                }

                mVehicle.getTransmission().refreshParameters(mVehicle);
            }

            if (mGearRatios.isExpanded)
            {
                //get max speeds and generate ui
                float[] maxSpeeds = mVehicle.getTransmission().getMaxSpeeds();

                if (maxSpeeds != null)
                {
                    int currentIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = currentIndent + 1;
                    EditorGUILayout.Space();
                    for (int i = 0; i < maxSpeeds.Length; i++)
                    {
                        string gearStr = (i == maxSpeeds.Length - 1) ? "R" : (i + 1).ToString();
                        EditorGUILayout.LabelField("*Gear " + gearStr + " TopSpeed : " + maxSpeeds[i].ToString("0.0") + " kmh");
                    }
                    EditorGUI.indentLevel = currentIndent;
                }
            }

            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowAxles = GUILayout.Toggle(mShowAxles, "Axle Settings", "Button");

        if (mShowAxles)
        {
            EditorGUILayout.Space();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            mAxleList.arraySize = EditorGUILayout.IntSlider("Axle Count", mAxleList.arraySize, 2, 8);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();

            //check total torque share
            float totalShare = 0f;

            for (int i = 0; i < mAxleList.arraySize; i++)
            {
                GUIContent content = new GUIContent("Axle - " + i);
                Rect r = GUILayoutUtility.GetRect(content, Foldout);
                mShowAxle[i] = GUI.Toggle(r, mShowAxle[i], content, Foldout);
                if (mShowAxle[i])
                {
                    int currentIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = currentIndent + 1;

                    SerializedProperty prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mTorqueShare");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Torque Share"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mHasHandbrake");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Has Handbrake"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mMaxSteerAngle");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Max Steer Angle"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mAckermanCoeff");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Ackerman Coefficient"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mAckermanReferenceIndex");
                    prop.intValue = EditorGUILayout.IntSlider("Ackerman Reference Axle", prop.intValue, 0, mAxleList.arraySize - 1);
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mCamberAngle");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Camber Angle"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mToeAngle");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Toe Angle"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mDifferentialType");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Differential Type"));
                    Axle.DifferentialType type = (Axle.DifferentialType)prop.enumValueIndex;
                    if (type == Axle.DifferentialType.DF_LSD)
                    {
                        prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mDiffStrength");
                        EditorGUILayout.PropertyField(prop, new GUIContent("Differential Strength"));
                    }

                    EditorGUI.indentLevel = currentIndent;
                }
                //add torque share
                totalShare += mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mTorqueShare").floatValue;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                for (int i = 0; i < mVehicle.getAxleCount(); i++)
                {
                    mVehicle.getAxle(i).getLeftWheel().refreshParameters(null);
                    mVehicle.getAxle(i).getRightWheel().refreshParameters(null);
                }
            }

            //if total torque share is not 1 show warning
            if (totalShare != 1f)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Warning : Total torque share of axles is not equal to 1, this may cause problems.", EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowWheels = GUILayout.Toggle(mShowWheels, "Wheel Settings", "Button");

        if (mShowWheels)
        {
            EditorGUILayout.Space();
            mAllowReverse.boolValue = GUILayout.Toggle(mAllowReverse.boolValue, " Allow Wheel Rotation Against Engine");
            mDetectGround.boolValue = GUILayout.Toggle(mDetectGround.boolValue, " Detect Ground Velocity");
            EditorGUILayout.PropertyField(mRollingResistanceCoeff, new GUIContent("Rolling Resistance Coefficient"));
            EditorGUILayout.PropertyField(mStickyTireSpeed, new GUIContent("Sticky Tire Max Speed"));
            mStickyTireSpeed.floatValue = Mathf.Round(mStickyTireSpeed.floatValue); //keep rounded to int
            mReactionScale.floatValue = EditorGUILayout.Slider("Wheel Reaction Force Scale", mReactionScale.floatValue, 0f, 1f);
            EditorGUILayout.PropertyField(mBrakeEngagement, new GUIContent("Brake Engagement"));
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            if (GUILayout.Button("Calculate Wheel Sizes", EditorStyles.miniButton))
            {
                SerializedProperty prop = null;
                for (int i = 0; i < mAxleList.arraySize; i++)
                {
                    float radius, width;
                    mVehicle.getAxle(i).getLeftWheel().calculateSizes(out radius, out width);
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mRadius");
                    if (radius > 0.0f) prop.floatValue = radius;
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mWidth");
                    if (width > 0.0f) prop.floatValue = width;
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            for (int i = 0; i < mAxleList.arraySize; i++)
            {
                GUIContent content = new GUIContent("Axle - " + i);
                Rect r = GUILayoutUtility.GetRect(content, Foldout);
                mShowWheel[i] = GUI.Toggle(r, mShowWheel[i], content, Foldout);
                if (mShowWheel[i])
                {
                    int currentIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = currentIndent + 1;

                    SerializedProperty prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelLeft.mIsActive");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Left Active"), true);
                    bool leftActive = prop.boolValue;
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelRight.mIsActive");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Right Active"), true);
                    bool rightActive = prop.boolValue;

                    EditorGUI.BeginDisabledGroup(Application.isPlaying || !leftActive);
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelLeft.mWheelTransform");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Left Wheel"), true);
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(Application.isPlaying || !rightActive);
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelRight.mWheelTransform");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Right Wheel"), true);
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(Application.isPlaying || !leftActive || mCastType.enumValueIndex != 2);
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelLeft.mConvexMesh");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Left Mesh"), true);
                    EditorGUI.EndDisabledGroup();

                    EditorGUI.BeginDisabledGroup(Application.isPlaying || !rightActive || mCastType.enumValueIndex != 2);
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelRight.mConvexMesh");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Right Mesh"), true);
                    EditorGUI.EndDisabledGroup();

                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mTireModel");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Tire Model"), true);
                    
                    mShowForceScalers = EditorGUILayout.Foldout(mShowForceScalers, "Tire Force/Moment Scalers");

                    if (mShowForceScalers)
                    {
                        int subIndent = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = subIndent + 1;

                        prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mLongitudinalScale");
                        EditorGUILayout.PropertyField(prop, new GUIContent("(Fx)Longitudinal Force Scale"));
                        prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mLateralScale");
                        EditorGUILayout.PropertyField(prop, new GUIContent("(Fy)Lateral Force Scale"));

                        prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mOverturnScale");
                        EditorGUILayout.PropertyField(prop, new GUIContent("(Mx)Over-Turn Moment Scale"));
                        prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mRollMomentScale");
                        EditorGUILayout.PropertyField(prop, new GUIContent("(My)Wheel-Roll Moment Scale"));
                        prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mSelfAlignScale");
                        EditorGUILayout.PropertyField(prop, new GUIContent("(Mz)Self-Align Moment Scale"));
                        EditorGUI.indentLevel = subIndent;
                    }

                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mLongitudinalFriction");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Longitudinal Friction"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mLateralFriction");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Lateral Friction"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mRadius");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Radius"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mWidth");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Width"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mMass");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Mass"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mTirePressure");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Tire Pressure"));
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mBrakeTorque");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Brake Torque"));

                    EditorGUI.indentLevel = currentIndent;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                for (int i=0; i<mVehicle.getAxleCount();i ++)
                {
                    mVehicle.getAxle(i).getLeftWheel().refreshParameters(null);
                    mVehicle.getAxle(i).getRightWheel().refreshParameters(null);
                }
            }

            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowSuspension = GUILayout.Toggle(mShowSuspension, "Suspension Settings", "Button");

        if (mShowSuspension)
        {
            EditorGUILayout.Space();
            mHardContact.boolValue = GUILayout.Toggle(mHardContact.boolValue, " Simulate Hard Contact");
            mRelaxationDownforce.boolValue = GUILayout.Toggle(mRelaxationDownforce.boolValue, " Relaxation Damper Downforce");
            mTirePenetration.boolValue = GUILayout.Toggle(mTirePenetration.boolValue, " Tire Ground Penetration");
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < mAxleList.arraySize; i++)
            {
                GUIContent content = new GUIContent("Axle - " + i);
                Rect r = GUILayoutUtility.GetRect(content, Foldout);
                mShowWheel[i] = GUI.Toggle(r, mShowWheel[i], content, Foldout);
                if (mShowWheel[i])
                {
                    int currentIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = currentIndent + 1;

                    SerializedProperty prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mSuspensionSpring");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Spring Rate (N/mm)"));
                    float springRate = prop.floatValue;

                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mSuspensionUpTravel");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Up Travel (m)"));
                    float upLength = prop.floatValue;

                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mSuspensionDownTravel");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Down Travel (m)"));
                    float downLength = prop.floatValue;

                    //convert to percentage and reconvert back to meters
                    float springLength = upLength + downLength;
                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mSuspensionPreload");
                    float percent = 100.0f * prop.floatValue / springLength;
                    float preload = springLength * EditorGUILayout.Slider("Preload (%)", percent, 0.0f, 100.0f) / 100.0f;
                    prop.floatValue = float.IsNaN(preload) ? 0.0f : preload;

                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mCompressionDamper");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Compression Damper (N/ms)"));

                    prop = mAxleList.GetArrayElementAtIndex(i).FindPropertyRelative("mWheelOptions.mRelaxationDamper");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Relaxation Damper (N/ms)"));

                    float springForce = 1000.0f * springLength * springRate;
                    float preloadForce = 1000.0f * preload * springRate;
                    EditorGUILayout.LabelField("*Total Length " + springLength + "m");
                    EditorGUILayout.LabelField("*Max Spring Force " + springForce + "N");
                    EditorGUILayout.LabelField("*Preload Length " + (1000.0f * preload).ToString("f1") + "mm - Force " + preloadForce.ToString("f1") + "N");
                    EditorGUI.indentLevel = currentIndent;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                for (int i = 0; i < mVehicle.getAxleCount(); i++)
                {
                    mVehicle.getAxle(i).getLeftWheel().refreshParameters(null);
                    mVehicle.getAxle(i).getRightWheel().refreshParameters(null);
                }
            }

            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowAero = GUILayout.Toggle(mShowAero, "AeroDynamics Settings", "Button");

        if (mShowAero)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mDragCoefficient, new GUIContent("Drag Coefficient"));
            EditorGUILayout.PropertyField(mDownForceCoefficient, new GUIContent("DownForce Coefficient"));
            EditorGUILayout.PropertyField(mDownForceZOffset, new GUIContent("DownForce Z-Offset"));
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowInput = GUILayout.Toggle(mShowInput, "Input Settings", "Button");

        if (mShowInput)
        {
            //if steer speed curve is empty, create a default curve
            if (mSteerSpeedCurve.animationCurveValue.length <= 1)
            {
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0f, 2f);
                curve.AddKey(1f, 2f);
                mSteerSpeedCurve.animationCurveValue = curve;
            }
            //if steer limit curve is empty, create a default curve
            if (mSteerLimitCurve.animationCurveValue.length <= 1)
            {
                AnimationCurve curve = new AnimationCurve();
                curve.AddKey(0f, 100f);
                curve.AddKey(1f, 100f);
                mSteerLimitCurve.animationCurveValue = curve;
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mEnabled, new GUIContent("Active"));
            bool active = mEnabled.boolValue;
            if (active)
            {
                EditorGUILayout.PropertyField(mUseRawAxis, new GUIContent("Use Raw Axis"));
                EditorGUILayout.PropertyField(mCombinedAxis, new GUIContent("Use Combined Axis"));
                EditorGUILayout.PropertyField(mControllerType, new GUIContent("Controller"));
                StandardInput.ControllerType ctype = (StandardInput.ControllerType)mControllerType.enumValueIndex;
                EditorGUILayout.PropertyField(mShifterType, new GUIContent("Shifter"));
                StandardInput.ShifterType stype = (StandardInput.ShifterType)mShifterType.enumValueIndex;

                //input axis
                {
                    SerializedProperty prop = null;

                    //combined axis
                    if (mCombinedAxis.boolValue)
                    {
                        prop = serializedObject.FindProperty("mStandardInput.mHorizontalAxis");
                        mHorizontalAxisIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mHorizontalAxisIndex = EditorGUILayout.Popup(new GUIContent("Horizontal Axis Name"), mHorizontalAxisIndex, mAxisList);
                        prop.stringValue = mAxisList[mHorizontalAxisIndex];

                        prop = serializedObject.FindProperty("mStandardInput.mVerticalAxis");
                        mVerticalAxisIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mVerticalAxisIndex = EditorGUILayout.Popup(new GUIContent("Vertical Axis Name"), mVerticalAxisIndex, mAxisList);
                        prop.stringValue = mAxisList[mVerticalAxisIndex];
                    }
                    //seperate axis
                    else
                    {
                        prop = serializedObject.FindProperty("mStandardInput.mThrottleAxis");
                        mThrottleAxisIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mThrottleAxisIndex = EditorGUILayout.Popup(new GUIContent("Throttle Axis Name"), mThrottleAxisIndex, mAxisList);
                        prop.stringValue = mAxisList[mThrottleAxisIndex];

                        prop = serializedObject.FindProperty("mStandardInput.mBrakeAxis");
                        mBrakeAxisIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mBrakeAxisIndex = EditorGUILayout.Popup(new GUIContent("Brake Axis Name"), mBrakeAxisIndex, mAxisList);
                        prop.stringValue = mAxisList[mBrakeAxisIndex];

                        prop = serializedObject.FindProperty("mStandardInput.mLeftAxis");
                        mLeftAxisIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mLeftAxisIndex = EditorGUILayout.Popup(new GUIContent("Left Axis Name"), mLeftAxisIndex, mAxisList);
                        prop.stringValue = mAxisList[mLeftAxisIndex];

                        prop = serializedObject.FindProperty("mStandardInput.mRightAxis");
                        mRightAxisIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mRightAxisIndex = EditorGUILayout.Popup(new GUIContent("Right Axis Name"), mRightAxisIndex, mAxisList);
                        prop.stringValue = mAxisList[mRightAxisIndex];
                    }

                    prop = serializedObject.FindProperty("mStandardInput.mClutchAxis");
                    mClutchAxisIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                    mClutchAxisIndex = EditorGUILayout.Popup(new GUIContent("Clutch Axis Name"), mClutchAxisIndex, mAxisList);
                    prop.stringValue = mAxisList[mClutchAxisIndex];

                    prop = serializedObject.FindProperty("mStandardInput.mHandbrakeAxis");
                    mHandbrakeIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                    mHandbrakeIndex = EditorGUILayout.Popup(new GUIContent("Handbrake Axis Name"), mHandbrakeIndex, mAxisList);
                    prop.stringValue = mAxisList[mHandbrakeIndex];

                    if (stype == StandardInput.ShifterType.SEQUENTIAL)
                    {
                        prop = serializedObject.FindProperty("mStandardInput.mShiftAxis");
                        mShiftIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mShiftIndex = EditorGUILayout.Popup(new GUIContent("Shift Axis Name"), mShiftIndex, mAxisList);
                        prop.stringValue = mAxisList[mShiftIndex];
                    }
                    else if (stype == StandardInput.ShifterType.MANUAL)
                    {
                        prop = serializedObject.FindProperty("mStandardInput.mShift1Axis");
                        mShift1Index = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mShift1Index = EditorGUILayout.Popup(new GUIContent("Shift 1 Axis"), mShift1Index, mAxisList);
                        prop.stringValue = mAxisList[mShift1Index];

                        prop = serializedObject.FindProperty("mStandardInput.mShift2Axis");
                        mShift2Index = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mShift2Index = EditorGUILayout.Popup(new GUIContent("Shift 2 Axis"), mShift2Index, mAxisList);
                        prop.stringValue = mAxisList[mShift2Index];

                        prop = serializedObject.FindProperty("mStandardInput.mShift3Axis");
                        mShift3Index = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mShift3Index = EditorGUILayout.Popup(new GUIContent("Shift 3 Axis"), mShift3Index, mAxisList);
                        prop.stringValue = mAxisList[mShift3Index];

                        prop = serializedObject.FindProperty("mStandardInput.mShift4Axis");
                        mShift4Index = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mShift4Index = EditorGUILayout.Popup(new GUIContent("Shift 4 Axis"), mShift4Index, mAxisList);
                        prop.stringValue = mAxisList[mShift4Index];

                        prop = serializedObject.FindProperty("mStandardInput.mShift5Axis");
                        mShift5Index = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mShift5Index = EditorGUILayout.Popup(new GUIContent("Shift 5 Axis"), mShift5Index, mAxisList);
                        prop.stringValue = mAxisList[mShift5Index];

                        prop = serializedObject.FindProperty("mStandardInput.mShift6Axis");
                        mShift6Index = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mShift6Index = EditorGUILayout.Popup(new GUIContent("Shift 6 Axis"), mShift6Index, mAxisList);
                        prop.stringValue = mAxisList[mShift6Index];

                        prop = serializedObject.FindProperty("mStandardInput.mShiftRAxis");
                        mShiftRIndex = Mathf.Max(0, System.Array.IndexOf(mAxisList, prop.stringValue));
                        mShiftRIndex = EditorGUILayout.Popup(new GUIContent("Shift R Axis"), mShiftRIndex, mAxisList);
                        prop.stringValue = mAxisList[mShiftRIndex];
                    }
                }

                if (ctype != StandardInput.ControllerType.WHEEL)
                {
                    EditorGUILayout.PropertyField(mSteerSpeedCurve, new GUIContent("Steering Sensivity Curve"));
                    EditorGUILayout.PropertyField(mSteerLimitCurve, new GUIContent("Steering Limit Curve"));
                    SerializedProperty prop = serializedObject.FindProperty("mStandardInput.mSteerInputGravity");
                    EditorGUILayout.PropertyField(prop, new GUIContent("Steering Input Gravity"));
                }
                else
                {
                    EditorGUILayout.PropertyField(mSteerSensivity, new GUIContent("Steering Sensivity"));
                }

                EditorGUILayout.PropertyField(mSteerRange, new GUIContent("Steering Range"));
                EditorGUILayout.PropertyField(mSteerDeadZone, new GUIContent("Steering Deadzone"));

                EditorGUILayout.PropertyField(mThrottleSensivity, new GUIContent("Throttle Sensivity"));
                EditorGUILayout.PropertyField(mThrottleRange, new GUIContent("Throttle Range"));
                EditorGUILayout.PropertyField(mThrottleDeadzone, new GUIContent("Throttle Deadzone"));

                EditorGUILayout.PropertyField(mBrakingSensivity, new GUIContent("Braking Sensivity"));
                EditorGUILayout.PropertyField(mBrakingRange, new GUIContent("Braking Range"));
                EditorGUILayout.PropertyField(mBrakingDeadzone, new GUIContent("Braking Deadzone"));

                EditorGUILayout.PropertyField(mClutchSensivity, new GUIContent("Clutch Sensivity"));
                EditorGUILayout.PropertyField(mClutchRange, new GUIContent("Clutch Range"));
                EditorGUILayout.PropertyField(mClutchDeadzone, new GUIContent("Clutch Deadzone"));
            }
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowTelemetry = GUILayout.Toggle(mShowTelemetry, "Telemetry Settings", "Button");

        if (mShowTelemetry)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mTextColor, new GUIContent("Text Color"));
            EditorGUILayout.PropertyField(mTShow, new GUIContent("Show"));
            GUI.enabled = mTShow.boolValue;
            EditorGUILayout.PropertyField(mTShowAeroDynamics, new GUIContent("Show AeroDynamics"));
            EditorGUILayout.PropertyField(mTShowAxles, new GUIContent("Show Axles"));
            EditorGUILayout.PropertyField(mTShowEngine, new GUIContent("Show Engine"));
            EditorGUILayout.PropertyField(mTShowInput, new GUIContent("Show Input"));
            EditorGUILayout.PropertyField(mTShowTransmission, new GUIContent("Show Transmission"));
            EditorGUILayout.PropertyField(mTShowVehicle, new GUIContent("Show Vehicle"));
            EditorGUILayout.PropertyField(mTShowWheels, new GUIContent("Show Wheels"));
            GUI.enabled = true;
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowAssists = GUILayout.Toggle(mShowAssists, "Driving Assists Settings", "Button");

        if (mShowAssists)
        {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mAbs, new GUIContent("ABS (%)"));
            EditorGUILayout.PropertyField(mAsr, new GUIContent("ASR (%)"));
            EditorGUILayout.PropertyField(mSteeringAssist, new GUIContent("Steering Assist (%)"));
            EditorGUILayout.PropertyField(mSteeringAssistThresholdAngle, new GUIContent("Steering Assist Threshold Angle"));
            EditorGUILayout.PropertyField(mUnderSteerAssist, new GUIContent("UnderSteer Assist (%)"));
            EditorGUILayout.PropertyField(mMinUnderSteerAngle, new GUIContent("Min UnderSteer Angle"));
            EditorGUILayout.PropertyField(mOverSteerAssist, new GUIContent("OverSteer Assist (%)"));
            EditorGUILayout.PropertyField(mMinOverSteerAngle, new GUIContent("Min OverSteer Angle"));
            EditorGUILayout.PropertyField(mFrontAntiRollPower, new GUIContent("Front AntiRoll (%)"));
            EditorGUILayout.PropertyField(mRearAntiRollPower, new GUIContent("Rear AntiRoll (%)"));
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        mShowSaveLoad = GUILayout.Toggle(mShowSaveLoad, "Load/Save", "Button");

        if (mShowSaveLoad)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Load Settings", EditorStyles.miniButton))
            {
                string path = EditorUtility.OpenFilePanel("Load vehicle from json", Application.dataPath, "json");
                mVehicle.load(path);
                EditorUtility.SetDirty(mVehicle);
            }
            if (GUILayout.Button("Save Settings", EditorStyles.miniButton))
            {
                string path = EditorUtility.SaveFilePanel("Save vehicle as json", Application.dataPath, mVehicle.name + ".json", "json");
                mVehicle.save(path);
            }
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }
}