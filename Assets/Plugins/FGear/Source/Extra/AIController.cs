// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using FGear;

public class AIController : MonoBehaviour
{
    [SerializeField]
    bool Active = false;
    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    SplineTool Spline;
    [SerializeField]
    AnimationCurve SpeedCurve;

    Rigidbody mBody;
    int mThrottleState = 0;
    float mEngineInput = 0f;
    float mBrakeInput = 0f;
    float mSteeringInput = 0f;
    float mNoVelocityTime = 0f;
    float mLateralDelta = 0f;
    float mCurrentSplineParam = -1f;
    public bool isActive() { return Active; }
    public void setActive(bool b) { Active = b; }
    public float getCurrentSplineParam() { return mCurrentSplineParam; }

    void Start()
    {
        mBody = Vehicle.getBody();

        //do not let empty curve
        if (SpeedCurve.length <= 1)
        {
            SpeedCurve = new AnimationCurve();
            SpeedCurve.AddKey(0f, 20f);
            SpeedCurve.AddKey(1f, 20f);
        }
    }

    void Update()
    {
        if (Active && Spline != null)
        {
            updateInputs();
            applyInputs();
        }
    }

    void updateInputs()
    {
        //reset all
        mThrottleState = 0;
        mEngineInput = 0f;
        mBrakeInput = 0f;
        mSteeringInput = 0f;

        //find target point
        float speedCoeff = Mathf.Max(1.0f, Vehicle.getKMHSpeed() / 40.0f);
        mCurrentSplineParam = Spline.getClosestParam(mBody.position);
        float targetPrm = mCurrentSplineParam;
        Vector3 target = Spline.moveParam(ref targetPrm, speedCoeff * 10.0f, true);
        Debug.DrawLine(mBody.position + Vector3.up, target + Vector3.up, Mathf.Abs(mLateralDelta) > 0.25f ? Color.red : Color.green);
        
        //update target if any opponent nearby
        updateRaycasts(speedCoeff);
        target += mLateralDelta * mBody.transform.right;
        
        //find steering input
        Vector3 tdir = (target - mBody.position).normalized;
        Vector3 dir = mBody.transform.forward;
        float sign = Mathf.Sign(Vector3.Cross(dir, tdir).y);
        float tsteer = sign * Vector3.Angle(dir, tdir);
        mSteeringInput = Mathf.Min(45.0f, tsteer / 90.0f);

        //find a target speed
        Vector3 side = mBody.transform.right;
        targetPrm = mCurrentSplineParam;
        Spline.moveParam(ref targetPrm, 1f + speedCoeff * 14.5f, true); //for braking look %45 further
        Vector3 futureSide = Utility.q90cw * Spline.getTangent(targetPrm).normalized;
        float angle = Vector3.Angle(side, futureSide);
        float targetSpeed = SpeedCurve.Evaluate(angle);
        float deltaSpeed = targetSpeed - Vehicle.getKMHSpeed();

        //other inputs
        mBrakeInput = deltaSpeed < -10f ? ((-deltaSpeed - 10f) / 10f) : 0f;
        if (mBrakeInput > 1f) mBrakeInput = 1f;
        mThrottleState = mBrakeInput == 0f ? 1 : 0;
        mEngineInput = deltaSpeed > 0f ? 1f : 0f;
    }

    //check 5 directions for a close opponent
    //alter lateral target if any hit occurs
    void updateRaycasts(float speedCoeff)
    {
        bool forwardObstacle = false;
        bool leftObstacle = false;
        bool rightObstacle = false;

        Vector3 fwd = mBody.transform.forward;
        Vector3 left = -mBody.transform.right;
        Vector3 right = mBody.transform.right;
        Vector3 crossLeft = (left + 1.5f * fwd).normalized;
        Vector3 crossRight = (right + 1.5f * fwd).normalized;

        float fwdDist = 2.5f + 2.5f * speedCoeff;
        float sideDist = 2.5f;
        float crossDist = 3.5f;

        Debug.DrawLine(mBody.position + 0.5f * Vector3.up, mBody.position + Vector3.up + fwdDist * fwd);
        Debug.DrawLine(mBody.position + 0.5f * Vector3.up, mBody.position + Vector3.up + sideDist * left);
        Debug.DrawLine(mBody.position + 0.5f * Vector3.up, mBody.position + Vector3.up + sideDist * right);
        Debug.DrawLine(mBody.position + 0.5f * Vector3.up, mBody.position + Vector3.up + crossDist * crossLeft);
        Debug.DrawLine(mBody.position + 0.5f * Vector3.up, mBody.position + Vector3.up + crossDist * crossRight);

        RaycastHit hit;
        if (Physics.Raycast(mBody.position + 0.5f * Vector3.up, fwd, out hit, fwdDist))
        {
            if (hit.rigidbody != null && hit.rigidbody != mBody) forwardObstacle = true;
        }
        if (Physics.Raycast(mBody.position + 0.5f * Vector3.up, left, out hit, sideDist))
        {
            if (hit.rigidbody != null && hit.rigidbody != mBody) leftObstacle = true;
        }
        else if (Physics.Raycast(mBody.position + 0.5f * Vector3.up, crossLeft, out hit, crossDist))
        {
            if (hit.rigidbody != null && hit.rigidbody != mBody) leftObstacle = true;
        }
        if (Physics.Raycast(mBody.position + 0.5f * Vector3.up, right, out hit, sideDist))
        {
            if (hit.rigidbody != null && hit.rigidbody != mBody) rightObstacle = true;
        }
        else if (Physics.Raycast(mBody.position + 0.5f * Vector3.up, crossRight, out hit, crossDist))
        {
            if (hit.rigidbody != null) rightObstacle = true;
        }

        if (forwardObstacle || leftObstacle) mLateralDelta = Mathf.Lerp(mLateralDelta, 5.0f, 1.5f * Time.deltaTime);
        else if (rightObstacle) mLateralDelta = Mathf.Lerp(mLateralDelta, -5.0f, 1.5f * Time.deltaTime);
        else mLateralDelta = Mathf.Lerp(mLateralDelta, 0.0f, 0.5f * Time.deltaTime);
    }

    void applyInputs()
    {
        //apply inputs
        Vehicle.getEngine().setThrottle(mEngineInput);
        Vehicle.getTransmission().setThrottleState(mThrottleState);
        Vehicle.setBraking(mBrakeInput, false);
        Vehicle.setSteering(mSteeringInput);

        //simple reset check : if stuck for 5 secs.
        if (Vehicle.getKMHSpeed() < 10.0f) mNoVelocityTime += Time.deltaTime;
        if (mNoVelocityTime > 5.0f)
        {
            mNoVelocityTime = 0.0f;
            Vector3 target1 = Spline.getPoint(mCurrentSplineParam);
            Vector3 target2 = Spline.getPoint(mCurrentSplineParam + 0.1f);
            Quaternion rotation = Quaternion.identity;
            rotation.SetLookRotation(target2 - target1);
            Vehicle.reset(target1, rotation);
        }
    }
}