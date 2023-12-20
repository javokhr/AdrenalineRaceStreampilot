// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using FGear;

public class Statistics : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    SplineTool Spline;
    [SerializeField]
    bool ShowGUI = true;
    [SerializeField]
    bool ShowDriftScore = true;
    [SerializeField]
    float DriftThreshold = 20f;

    float mZero100Time = 0.0f;
    float mZero200Time = 0.0f;
    float mHundredZeroDistance = 0.0f;
    float mLapTime = 0.0f;
    float mLastLapTime = 0.0f;
    float mLastTF = 0.0f;
    float mCurrentDrift = 0f;
    float mTotalDrift = 0f;
    Vector3 mLastDriftPos;

    bool mGot100Result;
    bool mGot200Result;
    bool mGotZeroResult = true;

    Rect mWindowRect = new Rect(110, 5, 125, 150);
    int mWinID;

    void Start()
    {
        mWinID = Utility.winIDs++;
        mLastDriftPos = Vehicle.getPosition();
    }

    void Update()
    {
        //reset stats if stopped or going backwards
        if (Vehicle.getKMHSpeed() <= 0.0f)
        {
            mZero100Time = 0.0f;
            mZero200Time = 0.0f;
            mGot100Result = false;
            mGot200Result = false;
            return;
        }

        //0-100
        if (!mGot100Result && Vehicle.getTransmission().getCurGear() > 0)
        {
            if (Vehicle.getKMHSpeed() < 100.0f) mZero100Time += Time.deltaTime;
            else mGot100Result = true;
        }

        //0-200
        if (!mGot200Result && Vehicle.getTransmission().getCurGear() > 0)
        {
            if (Vehicle.getKMHSpeed() < 200.0f) mZero200Time += Time.deltaTime;
            else mGot200Result = true;
        }

        //reset braking stat
        if (Vehicle.getKMHSpeed() >= 100.0f)
        {
            mHundredZeroDistance = 0.0f;
            mGotZeroResult = false;
        }

        //100-0 braking
        if (!mGotZeroResult)
        {
            if (Vehicle.getKMHSpeed() < 100.0f && Vehicle.getKMHSpeed() > 0.0f)
            {
                mHundredZeroDistance += Vehicle.getKMHSpeed() * Utility.kmh2ms * Time.deltaTime;
            }
            else if (Vehicle.getKMHSpeed() <= 0.0f) mGotZeroResult = true;
        }

        //drift
        float speed = Vehicle.getVelocitySize();
        float angle = Mathf.Abs(Vehicle.getSteerDeltaAngle(false));
        if (speed > 3f && angle >= DriftThreshold)
        {
            float angleRatio = Mathf.Min(1f, Mathf.Abs(Vehicle.getSteerDeltaAngle(false) / 90f));
            mTotalDrift += angleRatio * speed;
            Vector3 cpos = Vehicle.getPosition();
            float dist = Vector3.Distance(cpos, mLastDriftPos);
            mCurrentDrift += dist;
            mLastDriftPos = cpos;
        }
        else mCurrentDrift = 0f;

        //lap time
        if (Spline != null)
        {
            float tf = Spline.getClosestParam(Vehicle.transform.position);
            if (mLastTF - tf > 0.9f)
            {
                mLastLapTime = mLapTime;
                mLapTime = 0.0f;
            }
            else mLapTime += Time.deltaTime;
            mLastTF = tf;
        }
    }

    void OnGUI()
    {
        if (ShowGUI)
        {
            mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Statistics");
        }

        if (ShowDriftScore)
        {
            //drift score
            GUI.skin.label.fontSize = mCurrentDrift != 0f ? 48 : 42;
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            string t = mTotalDrift.ToString("f0");
            GUI.Label(new Rect(Screen.width - 300, 20, 250, 50), t);

            //current drift distance
            if (mCurrentDrift != 0f)
            {
                GUI.skin.label.fontSize = 24;
                t = mCurrentDrift.ToString("f0") + " m";
                GUI.Label(new Rect(Screen.width - 300, 70, 250, 50), t);
            }

            GUI.skin.label.fontSize = 12;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
        }
    }

    void uiWindowFunction(int windowID)
    {
        string t = "V:" + (Vehicle.getForwardSpeed() * Utility.ms2kmh).ToString("0.0");
        GUI.Label(new Rect(10, 20, 200, 30), t);
        t = "0-100:" + mZero100Time.ToString("0.0") + " s";
        GUI.Label(new Rect(10, 40, 200, 30), t);
        t = "0-200:" + mZero200Time.ToString("0.0") + " s";
        GUI.Label(new Rect(10, 60, 200, 30), t);
        t = "100-0:" + mHundredZeroDistance.ToString("0.0") + " m";
        GUI.Label(new Rect(10, 80, 200, 30), t);
        t = "Cur Lap:" + Utility.formatTime(mLapTime);
        GUI.Label(new Rect(10, 100, 200, 30), t);
        t = "Last Lap:" + Utility.formatTime(mLastLapTime);
        GUI.Label(new Rect(10, 120, 200, 30), t);
    }
}