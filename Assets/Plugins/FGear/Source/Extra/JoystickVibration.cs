// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using FGear;
//using XInputDotNetPure;

//To use this class get the XInput.Net from https://github.com/speps/XInputDotNet/releases (tested with v2017.04-2)

public class JoystickVibration : MonoBehaviour
{
    /*[SerializeField]
    Vehicle Vehicle;

    int mCurrentDeviceIndex = -1;
    GamePadState mCurrentDeviceState;
    float mMotor = 0f;

    int checkConnectedDevice()
    {
        for (int i = 0; i < 4; ++i)
        {
            PlayerIndex testPlayerIndex = (PlayerIndex)i;
            GamePadState testState = GamePad.GetState(testPlayerIndex);
            if (testState.IsConnected)
            {
                Debug.Log("GamePad found " + testPlayerIndex);
                mCurrentDeviceState = testState;
                return i;
            }
        }
        return -1;
    }

	void Update()
    {
        if (mCurrentDeviceIndex == -1 || !mCurrentDeviceState.IsConnected)
        {
            mCurrentDeviceIndex = checkConnectedDevice();
        }
        else
        {
            //body drag represents rough surface in this case
            float surfaceCoeff = Vehicle.getBody().drag * Mathf.Min(1f, Vehicle.getVelocitySize() / 10f);
            float motor = mMotor + surfaceCoeff;
            if (motor < 0.1f) motor = 0f;

            GamePad.SetVibration((PlayerIndex)mCurrentDeviceIndex, motor, motor);

            if (mMotor > 0f) mMotor -= 2f * Time.deltaTime;
            else mMotor = 0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        mMotor = 1f;
    }

    private void OnApplicationQuit()
    {
        if (mCurrentDeviceIndex > -1 && mCurrentDeviceState.IsConnected)
        {
            GamePad.SetVibration((PlayerIndex)mCurrentDeviceIndex, 0, 0);
        }
    }*/
}