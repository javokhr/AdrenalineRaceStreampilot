// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;

namespace FGear
{
    [System.Serializable]
    public class AeroDynamics : TelemetryDrawer
    {
        [SerializeField]
        float mDragCoefficient = 0.8f;
        [SerializeField]
        float mDownForceCoefficient = 0.2f;
        [SerializeField]
        [Range(-10f, 10f)]
        float mDownForceZOffset = 0f;

        Rigidbody mBody;
        float mDragForce;
        float mDownForce;
        const float mAirDensity = 1.225f; //kg/m3

        #region getters & setters
        public float getDragCoefficient() { return mDragCoefficient; }
        public float getDownForceCoefficient() { return mDownForceCoefficient; }
        public float getDownForceZOffset() { return mDownForceZOffset; }
        public void setDragCoefficient(float f) { mDragCoefficient = f; }
        public void setDownForceCoefficient(float f) { mDownForceCoefficient = f; }
        public void setDownForceZOffset(float f) { mDownForceZOffset = f; }
        #endregion

        public void init(Vehicle v)
        {
            mBody = v.getBody();
            initTelemetryWindow();
        }

        public void myFixedUpdate()
        {
            float v = mBody.velocity.magnitude;

            //frontal drag
            mDragForce = -0.5f * mAirDensity * (v * v) * mDragCoefficient;
            Vector3 velDir = mBody.velocity.normalized;
            mBody.AddForce(mDragForce * velDir);

            //down force
            mDownForce = 0.5f * mAirDensity * (v * v) * mDownForceCoefficient;
            mBody.AddForceAtPosition(mDownForce * Vector3.down, mBody.position + mBody.rotation * (mDownForceZOffset * Vector3.forward));
        }

        protected override void initTelemetryWindow()
        {
            mWinID = Utility.winIDs++;
            mWindowRect = new Rect(5, Screen.height - 255, 175, 70);
        }

        public override void drawTelemetry()
        {
            mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "AeroDynamics");
        }

        void uiWindowFunction(int windowID)
        {
            GUI.DragWindow();
            GUI.contentColor = Utility.textColor;
            GUI.Label(new Rect(10, 20, 150, 25), "Drag Force: " + mDragForce.ToString("f0") + " Nm");
            GUI.Label(new Rect(10, 40, 150, 25), "Down Force: " + mDownForce.ToString("f0") + " Nm");
        }
    }
}