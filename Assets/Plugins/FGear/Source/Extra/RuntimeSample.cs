// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeSample : MonoBehaviour
{
    FGear.Vehicle mVehicle;
    GameObject mWheelFL, mWheelFR, mWheelRL, mWheelRR;
    float mSuspensionLength = 0.3f;
    float mWheelRadius = 0.25f;
    float mPowerScale = 1.0f;
    float mPowerFront = 0.5f;
    float mPowerRear = 0.5f;
    bool mFLActive = true, mFRActive = true, mRLActive = true, mRRActive = true;

    void Start ()
    {
        //create vehicle body
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.parent = gameObject.transform;
        body.name = "BodyVisual";
        
        //scale & position body
        body.transform.localScale = new Vector3(2, 1, 4);
        body.transform.localPosition = new Vector3(0, 1, 0);
        
        //set body material
        body.GetComponent<Renderer>().material = Resources.Load("Materials/grey") as Material;
        
        //create wheel bodies
        mWheelFL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mWheelFR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mWheelRL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mWheelRR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mWheelFL.transform.parent = gameObject.transform;
        mWheelFR.transform.parent = gameObject.transform;
        mWheelRL.transform.parent = gameObject.transform;
        mWheelRR.transform.parent = gameObject.transform;
        mWheelFL.name = "0LWheel";
        mWheelFR.name = "0RWheel";
        mWheelRL.name = "1LWheel";
        mWheelRR.name = "1RWheel";
        
        //remove colliders from wheels
        Destroy(mWheelFL.GetComponent<Collider>());
        Destroy(mWheelFR.GetComponent<Collider>());
        Destroy(mWheelRL.GetComponent<Collider>());
        Destroy(mWheelRR.GetComponent<Collider>());
        
        //scale wheels
        mWheelFL.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        mWheelFR.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        mWheelRL.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        mWheelRR.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        //position wheels
        mWheelFL.transform.localPosition = new Vector3(-1.25f, 0.40f,  1.7f);
        mWheelFR.transform.localPosition = new Vector3( 1.25f, 0.40f,  1.7f);
        mWheelRL.transform.localPosition = new Vector3(-1.25f, 0.40f, -1.7f);
        mWheelRR.transform.localPosition = new Vector3( 1.25f, 0.40f, -1.7f);
        
        //set wheel materials
        Material wheelMat = Resources.Load("Materials/checker1") as Material;
        mWheelFL.GetComponent<Renderer>().material = wheelMat;
        mWheelFR.GetComponent<Renderer>().material = wheelMat;
        mWheelRL.GetComponent<Renderer>().material = wheelMat;
        mWheelRR.GetComponent<Renderer>().material = wheelMat;

        //add vehicle script
        mVehicle = gameObject.AddComponent<FGear.Vehicle>();

        //make it steerable
        mVehicle.getAxle(0).setMaxSteerAngle(30f);

        //add more gears
        mVehicle.getTransmission().setGearCount(4);
        mVehicle.getTransmission().setGearRatio(0, 5f); //1st
        mVehicle.getTransmission().setGearRatio(1, 3.5f); //2nd
        mVehicle.getTransmission().setGearRatio(2, 2f); //3rd
        mVehicle.getTransmission().setGearRatio(3, 4.5f); //R
        mVehicle.getTransmission().refreshParameters(null);
    }

    void OnGUI()
    {
        //UI
        {
            GUI.Box(new Rect(0, 0, 175, 400), "");

            //suspension length slider
            GUI.Label(new Rect(10, 20, 150, 20), "Suspension Length " + mSuspensionLength.ToString("0.00"));
            mSuspensionLength = GUI.HorizontalSlider(new Rect(10, 40, 150, 20), mSuspensionLength, 0.1f, 0.5f);

            //wheel radius slider
            GUI.Label(new Rect(10, 65, 150, 20), "Wheel Radius " + mWheelRadius.ToString("0.00"));
            mWheelRadius = GUI.HorizontalSlider(new Rect(10, 85, 150, 20), mWheelRadius, 0.2f, 0.4f);

            //engine power slider
            GUI.Label(new Rect(10, 110, 150, 20), "Engine Power Scale " + mPowerScale.ToString("0.00"));
            mPowerScale = GUI.HorizontalSlider(new Rect(10, 130, 150, 20), mPowerScale, 0.5f, 2f);

            //front power slider
            GUI.Label(new Rect(10, 155, 150, 20), "Front Power Ratio " + mPowerFront.ToString("0.00"));
            mPowerFront = GUI.HorizontalSlider(new Rect(10, 175, 150, 20), mPowerFront, 0f, 1f);
            //total ratio should be 1
            if (GUI.changed) mPowerRear = 1f - mPowerFront;

            //rear power slider
            GUI.Label(new Rect(10, 200, 150, 20), "Rear Power Ratio " + mPowerRear.ToString("0.00"));
            mPowerRear = GUI.HorizontalSlider(new Rect(10, 220, 150, 20), mPowerRear, 0f, 1f);
            //total ratio should be 1
            if (GUI.changed) mPowerFront = 1f - mPowerRear;

            //FL wheel active
            mFLActive = GUI.Toggle(new Rect(10, 250, 150, 20), mFLActive, "FL Active");
            if (GUI.changed)
            {
                mVehicle.getAxle(0).getLeftWheel().setActive(mFLActive); //set active
                mVehicle.getAxle(0).getLeftWheel().getWheelTransform().gameObject.SetActive(mFLActive); //hide/show
            }

            //FR wheel active
            mFRActive = GUI.Toggle(new Rect(10, 285, 150, 20), mFRActive, "FR Active");
            if (GUI.changed)
            {
                mVehicle.getAxle(0).getRightWheel().setActive(mFRActive); //set active
                mVehicle.getAxle(0).getRightWheel().getWheelTransform().gameObject.SetActive(mFRActive); //hide/show
            }

            //RL wheel active
            mRLActive = GUI.Toggle(new Rect(10, 320, 150, 20), mRLActive, "RL Active");
            if (GUI.changed)
            {
                mVehicle.getAxle(1).getLeftWheel().setActive(mRLActive); //set active
                mVehicle.getAxle(1).getLeftWheel().getWheelTransform().gameObject.SetActive(mRLActive); //hide/show
            }

            //RR wheel active
            mRRActive = GUI.Toggle(new Rect(10, 355, 150, 20), mRRActive, "RR Active");
            if (GUI.changed)
            {
                mVehicle.getAxle(1).getRightWheel().setActive(mRRActive); //set active
                mVehicle.getAxle(1).getRightWheel().getWheelTransform().gameObject.SetActive(mRRActive); //hide/show
            }
        }

        //apply settings
        //not a good idea to update these every frame but this is just a demonstration
        {
            //set wheel options
            for (int i = 0; i < mVehicle.getAxleCount(); i++)
            {
                mVehicle.getAxle(i).getWheelOptions().setRadius(mWheelRadius);
                mVehicle.getAxle(i).getWheelOptions().setSuspensionUpTravel(0.5f * mSuspensionLength);
                mVehicle.getAxle(i).getWheelOptions().setSuspensionDownTravel(0.5f * mSuspensionLength);
                mVehicle.getAxle(i).applyWheelOptions();
            }

            //scale engine power
            mVehicle.getEngine().setTorqueScale(mPowerScale);

            //set front/rear power distribution
            mVehicle.getAxle(0).setTorqueShare(mPowerFront);
            mVehicle.getAxle(1).setTorqueShare(mPowerRear);

            //update wheel visuals
            mWheelFL.transform.localScale = 2f * mWheelRadius * Vector3.one;
            mWheelFR.transform.localScale = 2f * mWheelRadius * Vector3.one;
            mWheelRL.transform.localScale = 2f * mWheelRadius * Vector3.one;
            mWheelRR.transform.localScale = 2f * mWheelRadius * Vector3.one;
        }
    }
}