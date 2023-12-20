// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using FGear;
using GamePush;
public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform Target;
    [SerializeField]
    bool ShowGUI = true;
    [SerializeField]
    bool DriftMode = false;
    [SerializeField]
    float Spring = 0.0f;
    [SerializeField]
    float Distance = 7.5f;
    [SerializeField]
    float TargetYOffset = 1.0f;
    [SerializeField]
    float TiltAngle = 10.0f;

    Camera mCamera;
    Vector3 mDeltaDir = new Vector3(0, 0, -1);
    Vector3 mLastPos;
    float mRotationY = -50.0f;
    float mSensivity = 200.0f;
    float mRotationOffset = 0f;
    float mCurrentTilt = 0f;
    Rect mWindowRect = new Rect(Screen.width - 130, 5, 125, 170);
    int mWinID;
    bool mFreeze = false;
    bool mIsMobile = false;
    bool mOrbitMode = false;
    public void setTarget(Transform t) { Target = t; }
    public void setSpring(float s) { Spring = s; }
    public void setDistance(float d) { Distance = d; }

    void Start()
    {
        //QualitySettings.vSyncCount = 0;  // VSync must be disabled
        //Application.targetFrameRate = 1000;
        mWinID = Utility.winIDs++;
        mIsMobile = GP_Device.IsMobile();
        mCamera = GetComponent<Camera>();
        if (mCamera == null) mCamera = Camera.main;
        mLastPos = Target.position;
    }

    void Update()
    {

        //distance to target
        Distance -= Input.GetAxis("Mouse ScrollWheel") * mSensivity * Time.deltaTime;
        mOrbitMode = Input.GetMouseButton(1);

        //look back
        mRotationOffset = 0f;
        if (Input.GetKey(KeyCode.B))
        {
            mRotationOffset = 180f;
        }

        if (!mIsMobile && mOrbitMode)
        {
            TiltAngle -= Input.GetAxis("Mouse Y") * mSensivity * Time.deltaTime;
            mRotationY += Input.GetAxis("Mouse X") * mSensivity * Time.deltaTime;
            //clamp rotY
            TiltAngle = Mathf.Clamp(TiltAngle, 0f, 80f);
        }

        float cspring = mOrbitMode ? 0f : Spring;

        //position update
        //also update rotation when spring is zero
        if (Target != null)
        {
            Quaternion rot = Quaternion.Euler(mCurrentTilt, mRotationY + mRotationOffset, 0f);
            Vector3 targetPos = Target.position + TargetYOffset * Vector3.up;
            Vector3 camPos = targetPos + Distance * (rot * Vector3.back);
            if (cspring <= 0f) camPos = targetPos + Distance * (rot * mDeltaDir);
            if (!mFreeze) mCamera.transform.position = camPos;
            if (cspring <= 0f) mCamera.transform.rotation = Quaternion.LookRotation(targetPos - camPos);
        }
    }

    void FixedUpdate()
    {
        float cspring = mOrbitMode ? 0f : Spring;
        //rotation update when spring is non zero
        if (Target != null)
        {
            //lerp towards current tilt angle
            float tilt = Target.rotation.eulerAngles.x + TiltAngle;
            if (cspring > 0f) mCurrentTilt = Mathf.LerpAngle(mCurrentTilt, tilt, cspring * Time.fixedDeltaTime);
            else mCurrentTilt = TiltAngle;
            //lerp towards current yaw angle
            float targetAngle = Target.rotation.eulerAngles.y;
            if (DriftMode)
            {
                Vector3 delta = Target.position - mLastPos;
                float speed = (delta.magnitude / Time.fixedDeltaTime) * Utility.ms2kmh;
                if (speed > 5f) targetAngle = Quaternion.LookRotation(delta).eulerAngles.y;
            }
            mRotationY = Mathf.LerpAngle(mRotationY, targetAngle, cspring * Time.fixedDeltaTime);
            Quaternion rot = Quaternion.Euler(mCurrentTilt, mRotationY + mRotationOffset, 0f);
            Vector3 targetPos = Target.position + TargetYOffset * Vector3.up;
            Vector3 camPos = targetPos + Distance * (rot * Vector3.back);
            if (mFreeze) camPos = mCamera.transform.position;
            if (cspring > 0f || mFreeze) mCamera.transform.rotation = Quaternion.LookRotation(targetPos - camPos);
            mLastPos = Target.position;
        }
    }

    void OnGUI()
    {
        if (ShowGUI) mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Camera");
    }

    void uiWindowFunction(int windowID)
    {
        GUI.Label(new Rect(10, 20, 100, 20), "Spring");
        Spring = GUI.HorizontalSlider(new Rect(10, 40, 100, 20), Spring, 0f, 15f);
        GUI.Label(new Rect(10, 60, 100, 20), "Distance");
        Distance = GUI.HorizontalSlider(new Rect(10, 80, 100, 20), Distance, 2f, 20f);
        GUI.Label(new Rect(10, 100, 100, 20), "Tilt");
        float prevTilt = TiltAngle;
        TiltAngle = GUI.HorizontalSlider(new Rect(10, 120, 100, 20), TiltAngle, 0f, 50f);
        if (Spring == 0f) TiltAngle = prevTilt;
        mFreeze = GUI.Toggle(new Rect(10, 140, 100, 20), mFreeze, "Freeze");
    }
}