// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;

namespace FGear
{
    public class TelemetryDrawer
    {
        //telemetry window
        protected int mWinID;
        protected Rect mWindowRect;

        protected virtual void initTelemetryWindow()
        {
        }

        public virtual void drawTelemetry()
        {
        }
    }
}