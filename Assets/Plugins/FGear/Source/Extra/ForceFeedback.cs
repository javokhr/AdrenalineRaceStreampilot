// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

//#define LOGI_INSTALLED
using UnityEngine;
using FGear;

//To use this class 
//1-get the latest logitech gaming sdk(tested with LogitechSteeringWheelSDK_8.75.30)
//2-include the c# wrapper class LogitechGSDK
//3-put the LogitechSteeringWheelEnginesWrapper.dll in the project.
//4-uncomment #define LOGI_INSTALLED

public class ForceFeedback : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    bool UseDefaultCurves = true;
    [SerializeField]
    bool ShowGUI = true;

    [Space]
    [SerializeField, Range(40, 900)]
    int OperatingRange = 450;

    [Space]
    [SerializeField, Range(0, 100)]
    int AlignForce = 50;
    [SerializeField]
    AnimationCurve AlignCurve;

    [Space]
    [SerializeField, Range(0, 100)]
    int ConstantForce = 25;
    [SerializeField]
    AnimationCurve ConstantCurve;

    [Space]
    [SerializeField, Range(0, 100)]
    int DamperForce = 100;
    [SerializeField]
    AnimationCurve DamperCurve;

    [Space]
    [SerializeField, Range(0, 100)]
    int RoughnessForce = 25;

    [Space]
    [SerializeField, Range(0, 100)]
    int CollisionForce = 50;

    float mMaxMoment = 0f;
    float mReferenceLoad = 0f;
    int mWheelID = -1;
    int mSpringForce = 0;
    int mConstantForce = 0;
    int mDamperForce = 0;
    int mRoughnessForce = 0;
    float mFrontCollisionForce = 0f; //keep it float for fade out
    float mSideCollisionForce = 0f; //keep it float for fade out

    Rect mWindowRect = new Rect(240, 5, 125, 170);
    int mWinID;

    void Start()
    {
#if (LOGI_INSTALLED)
        if (!LogitechGSDK.LogiSteeringInitialize(true))
        {
            Debug.Log("LogitechGSDK init failed!");
        }
        else
        {
            for (int i = 0; LogitechGSDK.LogiIsConnected(i); i++)
            {
                if (LogitechGSDK.LogiIsConnected(i) && LogitechGSDK.LogiIsDeviceConnected(i, LogitechGSDK.LOGI_DEVICE_TYPE_WHEEL))
                {
                    mWheelID = i;
                    LogitechGSDK.LogiSetOperatingRange(mWheelID, OperatingRange);
                    Debug.Log("LogitechGSDK wheel id:" + mWheelID);
                    break;
                }
            }
        }
#endif

        if (UseDefaultCurves) createDefaultCurves();

        //calc. max moment
        for (int i=0; i< AlignCurve.length; i++)
        {
            if (AlignCurve.keys[i].value > mMaxMoment) mMaxMoment = AlignCurve.keys[i].value;
        }

        //calc. ref load
        mReferenceLoad = Vehicle.getMassPerWheel() * Utility.gravitySize;

        mWinID = Utility.winIDs++;
    }

    void createDefaultCurves()
    {
        //default AlignCurve
        Keyframe[] keys = new Keyframe[5];
        keys[0] = new Keyframe(-1.5f, 1f, 0f, 0f);
        keys[1] = new Keyframe(-0.1f, -8f, 0f, 0f);
        keys[2] = new Keyframe(0f, 0f, 0f, 0f);
        keys[3] = new Keyframe(0.1f, 8f, 0f, 0f);
        keys[4] = new Keyframe(1.5f, -1f, 0f, 0f);
        AlignCurve = new AnimationCurve(keys);

        //default ConstantCurve
        keys = new Keyframe[2];
        keys[0] = new Keyframe(0f, 0f, 0f, 0f, 0f, 0f);
        keys[1] = new Keyframe(10f, 1f, 0f, 0f, 0f, 0f);
        ConstantCurve = new AnimationCurve(keys);

        //default DamperCurve
        keys = new Keyframe[4];
        keys[0] = new Keyframe(0f, 1f, 0f, 0f);
        keys[1] = new Keyframe(10f, 0.25f, 0f, 0f);
        keys[2] = new Keyframe(100f, 0.5f, 0f, 0f);
        keys[3] = new Keyframe(200f, 1f, 0f, 0f);
        DamperCurve = new AnimationCurve(keys);
    }

    void OnDestroy()
    {
#if (LOGI_INSTALLED)
        LogitechGSDK.LogiSteeringShutdown();
#endif
    }

    void FixedUpdate()
    {
        if (mWheelID == -1) return;

#if (LOGI_INSTALLED)
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(mWheelID))
        {
            Engine e = Vehicle.getEngine();
            LogitechGSDK.LogiPlayLeds(mWheelID, e.getRpm(), 0.75f * e.getLimitRpm(), e.getLimitRpm());

            //LogitechGSDK.LOGI_FORCE_SPRING
            {
                float slipL = Vehicle.getAxle(0).getLeftWheel().getSlipAngle();
                float slipR = Vehicle.getAxle(0).getRightWheel().getSlipAngle();
                float momentL = AlignCurve.Evaluate(slipL);
                float momentR = AlignCurve.Evaluate(slipR);
                float ratio = (0.5f * (momentL + momentR)) / mMaxMoment;
                ratio = Mathf.Clamp(Mathf.Abs(ratio), 0.2f, 1f);
                mSpringForce = (int)(ratio * AlignForce);

                //find target steer
                Vector3 dir1 = Vehicle.getForwardDir(); dir1.y = 0f; dir1.Normalize();
                Vector3 vel = Vehicle.getBody().GetPointVelocity(Vehicle.getAxle(0).getGlobalCenter());
                Vector3 dir2 = vel.normalized; dir2.y = 0f; dir2.Normalize();
                float angle = Vector3.SignedAngle(dir1, dir2, Vector3.up);
                if (Mathf.Abs(angle) > 90f) angle = Mathf.Sign(angle) * (Mathf.Abs(angle) - 180f);
                int offset = (int)(angle / Vehicle.getMaxSteeringAngle() * 100);

                //surface effect does not work when spring is on so play spring only if surface effect is off
                if (mSpringForce > 0 && mRoughnessForce <= 0) LogitechGSDK.LogiPlaySpringForce(mWheelID, offset, mSpringForce, 50);
                else LogitechGSDK.LogiStopSpringForce(mWheelID);
            }

            //LogitechGSDK.LOGI_FORCE_CONSTANT
            {
                float speed = Vehicle.getKMHSpeed();
                float speedRatio = ConstantCurve.Evaluate(speed);
                float sign = 10f * Vehicle.getAxle(0).getNormalizedSteering();
                sign = Mathf.Clamp(sign, -1f, 1f);
                mConstantForce = (int)(sign * speedRatio * ConstantForce);

                if (mConstantForce != 0) LogitechGSDK.LogiPlayConstantForce(mWheelID, mConstantForce);
                else LogitechGSDK.LogiStopConstantForce(mWheelID);
            }

            //LogitechGSDK.LOGI_FORCE_DAMPER
            {
                float loadL = Vehicle.getAxle(0).getLeftWheel().getCurrentLoad();
                float loadR = Vehicle.getAxle(0).getRightWheel().getCurrentLoad();
                float loadRatio = -1f + 2f * (0.5f * (loadL + loadR)) / mReferenceLoad;
                loadRatio = Mathf.Clamp(loadRatio, -1f, 1f);
                float speed = Vehicle.getKMHSpeed();
                float speedRatio = DamperCurve.Evaluate(speed);
                mDamperForce = (int)(loadRatio * speedRatio * DamperForce);

                if (mDamperForce != 0) LogitechGSDK.LogiPlayDamperForce(mWheelID, mDamperForce);
                else LogitechGSDK.LogiStopDamperForce(mWheelID);
            }

            //LogitechGSDK.LOGI_FORCE_CAR_AIRBORNE
            {
                bool airBorne = !Vehicle.getAxle(0).getLeftWheel().hasContact() && !Vehicle.getAxle(0).getRightWheel().hasContact();
                if (airBorne) LogitechGSDK.LogiPlayCarAirborne(mWheelID);
                else LogitechGSDK.LogiStopCarAirborne(mWheelID);
            }

            //LogitechGSDK.LOGI_FORCE_SURFACE_EFFECT
            {
                float surfaceCoeff = getSurfaceRoughness();
                float speed = Mathf.Abs(Vehicle.getKMHSpeed());
                float speedRatio = Mathf.Clamp01(speed / 30f);
                mRoughnessForce = (int)(surfaceCoeff * speedRatio * RoughnessForce);
                if (mRoughnessForce > 0f)
                {
                    LogitechGSDK.LogiPlaySurfaceEffect(mWheelID, LogitechGSDK.LOGI_PERIODICTYPE_TRIANGLE, mRoughnessForce, 60);
                }
                else LogitechGSDK.LogiStopSurfaceEffect(mWheelID);
            }

            //LogitechGSDK.LOGI_FORCE_FRONTAL_COLLISION
            {
                LogitechGSDK.LogiPlayFrontalCollisionForce(mWheelID, (int)mFrontCollisionForce);

                //fade out collision force
                if (mFrontCollisionForce != 0f)
                {
                    mFrontCollisionForce = Mathf.Lerp(mFrontCollisionForce, 0f, 5f * Time.fixedDeltaTime);
                }
            }

            //LogitechGSDK.LOGI_FORCE_SIDE_COLLISION
            {
                LogitechGSDK.LogiPlaySideCollisionForce(mWheelID, (int)mSideCollisionForce);

                //fade out collision force
                if (mSideCollisionForce != 0f)
                {
                    mSideCollisionForce = Mathf.Lerp(mSideCollisionForce, 0f, 5f * Time.fixedDeltaTime);
                }
            }
        }
#endif
    }
    
    void OnCollisionEnter(Collision collision)
    {
        float impact = collision.relativeVelocity.magnitude * Utility.ms2kmh;
        float impactRatio = Mathf.Clamp01(impact / 30f);
        float dot1 = -Vector3.Dot(Vehicle.getForwardDir(), collision.contacts[0].normal);
        float dot2 = -Vector3.Dot(Vehicle.transform.right, collision.contacts[0].normal);
        mFrontCollisionForce = (int)(dot1 * impactRatio * CollisionForce);
        mSideCollisionForce = (int)(dot2 * impactRatio * CollisionForce);
    }

    //return a value for roughness, ex. positive for bumpy roads
    float getSurfaceRoughness()
    {
        float surfaceCoeff = 0f;
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            RaycastHit hit = Vehicle.getAxle(i).getLeftWheel().getRayHit();
            if (hit.collider != null && hit.collider.sharedMaterial != null && hit.collider.sharedMaterial.name != "asphalt") surfaceCoeff += 0.25f;
            hit = Vehicle.getAxle(i).getRightWheel().getRayHit();
            if (hit.collider != null && hit.collider.sharedMaterial != null && hit.collider.sharedMaterial.name != "asphalt") surfaceCoeff += 0.25f;
        }
        return Mathf.Clamp01(surfaceCoeff);
    }

    void OnGUI()
    {
        if (ShowGUI)
        {
            mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "ForceFeedback");
        }
    }

    void uiWindowFunction(int windowID)
    {
#if (LOGI_INSTALLED)
        string t = "Device:" + (LogitechGSDK.LogiIsConnected(mWheelID) ? "Active" : "Off");
        GUI.Label(new Rect(10, 20, 200, 30), t);
        t = "Spring %" + mSpringForce;
        if (!LogitechGSDK.LogiIsPlaying(mWheelID, LogitechGSDK.LOGI_FORCE_SPRING)) t = "Spring Off";
        GUI.Label(new Rect(10, 40, 200, 30), t);
        t = "Constant %" + mConstantForce;
        if (!LogitechGSDK.LogiIsPlaying(mWheelID, LogitechGSDK.LOGI_FORCE_CONSTANT)) t = "Constant Off";
        GUI.Label(new Rect(10, 60, 200, 30), t);
        t = "Damper %" + mDamperForce;
        if (!LogitechGSDK.LogiIsPlaying(mWheelID, LogitechGSDK.LOGI_FORCE_DAMPER)) t = "Damper Off";
        GUI.Label(new Rect(10, 80, 200, 30), t);
        t = "Roughness %" + mRoughnessForce;
        if (!LogitechGSDK.LogiIsPlaying(mWheelID, LogitechGSDK.LOGI_FORCE_SURFACE_EFFECT)) t = "Roughness Off";
        GUI.Label(new Rect(10, 100, 200, 30), t);
        t = "F-Collision %" + (int)mFrontCollisionForce;
        if (!LogitechGSDK.LogiIsPlaying(mWheelID, LogitechGSDK.LOGI_FORCE_FRONTAL_COLLISION)) t = "F-Collision Off";
        GUI.Label(new Rect(10, 120, 200, 30), t);
        t = "S-Collision %" + (int)mSideCollisionForce;
        if (!LogitechGSDK.LogiIsPlaying(mWheelID, LogitechGSDK.LOGI_FORCE_SIDE_COLLISION)) t = "S-Collision Off";
        GUI.Label(new Rect(10, 140, 200, 30), t);
#else
        string t = "No Logitech SDK";
        GUI.Label(new Rect(10, 20, 200, 30), t);
#endif
    }
}