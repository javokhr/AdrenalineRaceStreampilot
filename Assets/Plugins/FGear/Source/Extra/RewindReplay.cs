// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System;
using UnityEngine;
using FGear;

public class RewindReplay : MonoBehaviour
{
    [SerializeField]
    Vehicle[] VehicleList;
    [SerializeField, Range(1f, 10f)]
    float ReplayTime = 5.0f;
    [SerializeField]
    bool ShowGUI = true;

    //components
    Rigidbody[] mBodyList;

    //state buffer
    VehicleState[,] mStateBuffers;
    int mStateSize = 1;

    //self state
    enum State
    {
        PLAY,
        REWIND,
        REPLAY
    }
    State mState = State.PLAY;

    //locals
    float mTime = 0.0f;
    float mLocalTime = 0.0f;
    int mPlayIndex = 0;
    int mRewindIndex = 0;
    int mVehicleCount = 0;

    //gui
    Rect mWindowRect = new Rect(Screen.width - 390, 5, 125, 140);
    int mWinID;

    void Start()
    {
        mWinID = Utility.winIDs++;
        mVehicleCount = VehicleList.Length;
        if (mVehicleCount == 0) return;
        
        mBodyList = new Rigidbody[mVehicleCount];
        for (int i=0; i< mVehicleCount; i++)
        {
            mBodyList[i] = VehicleList[i].GetComponent<Rigidbody>();
        }

        mLocalTime = ReplayTime;
        setReplayTime(ReplayTime);
    }

    public void setReplayTime(float time)
    {
        ReplayTime = time;
        if (mBodyList == null || mVehicleCount == 0) return;

        //calc. state buffer size acc. to replay time
        mStateSize = (int)(ReplayTime / Time.fixedDeltaTime);
        mStateBuffers = new VehicleState[mVehicleCount, mStateSize];

        //fill with current transform
        for (int i = 0; i < mVehicleCount; i++)
        {
            for (int j = 0; j < mStateSize; j++)
            {
                mStateBuffers[i, j] = new VehicleState();
                mStateBuffers[i, j].mPosition = mBodyList[i].position;
                mStateBuffers[i, j].mRotation = mBodyList[i].rotation;
                mStateBuffers[i, j].mInput = new VehicleState.InputState();
            }
        }
    }

    void FixedUpdate()
    {
        if (mState == State.PLAY)
        {
            //store state each update
            addState();
        }
        else if (mState == State.REPLAY)
        {
            //replay state each update
            replayState();
        }
        else if (mState == State.REWIND)
        {
            //rewind state each update
            rewindState();
        }
    }

    void addState()
    {
        //advance time
        mTime += Time.fixedDeltaTime;

        //store state
        for (int i = 0; i < mVehicleCount; i++)
        {
            mStateBuffers[i, mPlayIndex].mStateTime = mTime;
            VehicleList[i].saveState(ref mStateBuffers[i, mPlayIndex]);
        }

        mPlayIndex++;
        if (mPlayIndex >= mStateSize) mPlayIndex -= mStateSize;
        mRewindIndex = mPlayIndex;
        mLocalTime = ReplayTime;
    }

    void replayState()
    {
        //increment index
        mRewindIndex++;
        if (mRewindIndex >= mStateSize) mRewindIndex = 0;

        //check if done
        if (mRewindIndex == mPlayIndex)
        {
            mRewindIndex--;
            Time.timeScale = 0.0f;
            AudioListener.volume = 0.0f;
        }
        //advance time
        else
        {
            mTime += Time.fixedDeltaTime;
            mLocalTime += Time.fixedDeltaTime;
        }

        for (int i = 0; i < mVehicleCount; i++)
        {
            VehicleState state = mStateBuffers[i, mRewindIndex];
            VehicleList[i].loadState(state);
        }
    }

    void rewindState()
    {
        //decrement index
        mRewindIndex--;
        if (mRewindIndex < 0) mRewindIndex += mStateSize;

        //check if done
        if (mRewindIndex == mPlayIndex)
        {
            mRewindIndex++;
            Time.timeScale = 0.0f;
            AudioListener.volume = 0.0f;
        }
        //advance time
        else
        {
            mTime -= Time.fixedDeltaTime;
            mLocalTime -= Time.fixedDeltaTime;
        }

        for (int i = 0; i < mVehicleCount; i++)
        {
            VehicleState state = mStateBuffers[i, mRewindIndex];
            VehicleList[i].loadState(state);
        }
    }

    public void play()
    {
        if (mState == State.PLAY) return;

        mState = State.PLAY;
        Time.timeScale = 1.0f;
        AudioListener.volume = 1.0f;

        //reset vehicles states
        for (int i = 0; i < mVehicleCount; i++)
        {
            for (int j = 0; j < mStateSize; j++)
            {
                mStateBuffers[i, j].reset();
                mStateBuffers[i, j].mPosition = mBodyList[i].position;
                mStateBuffers[i, j].mRotation = mBodyList[i].rotation;
            }
        }

        for (int i = 0; i < mVehicleCount; i++)
        {
            VehicleList[i].setUpdateInputs(true);
        }
    }

    public void replay()
    {
        if (mState == State.REPLAY) return;
        mState = State.REPLAY;
        Time.timeScale = 1.0f;
        AudioListener.volume = 1.0f;

        for (int i = 0; i < mVehicleCount; i++)
        {
            VehicleList[i].setUpdateInputs(false);
        }
    }

    public void rewind()
    {
        if (mState == State.REWIND) return;
        mState = State.REWIND;
        Time.timeScale = 1.0f;
        AudioListener.volume = 1.0f;

        for (int i = 0; i < mVehicleCount; i++)
        {
            VehicleList[i].setUpdateInputs(false);
        }
    }

    void OnGUI()
    {
        if (ShowGUI) mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Rewind/Replay");
    }

    void uiWindowFunction(int windowID)
    {
        if (GUI.Button(new Rect(10, 25, 105, 25), "Rewind"))
        {
            rewind();
        }
        if (GUI.Button(new Rect(10, 55, 105, 25), "Replay"))
        {
            replay();
        }
        if (GUI.Button(new Rect(10, 85, 105, 25), "Cancel"))
        {
            play();
        }
        GUI.enabled = false;
        mLocalTime = GUI.HorizontalSlider(new Rect(10, 120, 105, 20), mLocalTime, 0, ReplayTime);
        GUI.enabled = true;
    }
}