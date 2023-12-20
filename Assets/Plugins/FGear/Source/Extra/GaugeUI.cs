// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;
using FGear;

public class GaugeUI : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    bool ShowSpeedometer = true;
    [SerializeField]
    bool ShowTraction = true;
    [SerializeField]
    bool ShowSuspension = true;

    //wheels
    Wheel mFLW, mFRW, mRLW, mRRW;

    //ui stuff
    GameObject mKadran;
    GameObject mTraction;
    GameObject mSuspension;

    Text mSpeedText;
    Text mGearText;
    Text mFrontText;
    Text mRearText;
    RawImage mIbre;
    RawImage mGas;
    RawImage mBrake;

    RawImage mEspSign;
    RawImage mHandBrake;
    RawImage mAbsSign;
    RawImage mAsrSign;
    RawImage mWheelSign;
    RawImage mSteering;
    RawImage mFLGas;
    RawImage mFLBrake;
    RawImage mFRGas;
    RawImage mFRBrake;
    RawImage mRLGas;
    RawImage mRLBrake;
    RawImage mRRGas;
    RawImage mRRBrake;
    RawImage mFLSusp;
    RawImage mFRSusp;
    RawImage mRLSusp;
    RawImage mRRSusp;

    void Start()
    {
        mFLW = Vehicle.getAxle(0).getLeftWheel();
        mFRW = Vehicle.getAxle(0).getRightWheel();
        mRLW = Vehicle.getAxle(1).getLeftWheel();
        mRRW = Vehicle.getAxle(1).getRightWheel();

        //create uicanvas if not found
        string canvasName = "UICanvas";
        GameObject canvas = GameObject.Find(canvasName);
        if (canvas == null)
        {
            GameObject seed = Resources.Load(canvasName) as GameObject;
            if (seed != null)
            {
                canvas = GameObject.Instantiate(seed);
                canvas.name = canvasName;
            }
        }

        //get individual ui elements
        mKadran = Utility.findChild(canvas, "kadran");
        mTraction = Utility.findChild(canvas, "traction");
        mSuspension = Utility.findChild(canvas, "suspension");

        mSpeedText = Utility.findChild(canvas, "speedText").GetComponent<Text>();
        mGearText = Utility.findChild(canvas, "gearText").GetComponent<Text>();
        mFrontText = Utility.findChild(canvas, "frontShare").GetComponent<Text>();
        mRearText = Utility.findChild(canvas, "rearShare").GetComponent<Text>();
        mIbre = Utility.findChild(canvas, "ibre").GetComponent<RawImage>();
        mGas = Utility.findChild(canvas, "gas").GetComponent<RawImage>();
        mBrake = Utility.findChild(canvas, "brake").GetComponent<RawImage>();
        mEspSign = Utility.findChild(canvas, "espSign").GetComponent<RawImage>();
        mHandBrake = Utility.findChild(canvas, "handBrake").GetComponent<RawImage>();
        mAbsSign = Utility.findChild(canvas, "absSign").GetComponent<RawImage>();
        mAsrSign = Utility.findChild(canvas, "asrSign").GetComponent<RawImage>();
        mWheelSign = Utility.findChild(canvas, "wheelSign").GetComponent<RawImage>();
        mSteering = Utility.findChild(canvas, "wheel").GetComponent<RawImage>();

        mFLGas = Utility.findChild(canvas, "flgas").GetComponent<RawImage>();
        mFLBrake = Utility.findChild(canvas, "flbrake").GetComponent<RawImage>();
        mFRGas = Utility.findChild(canvas, "frgas").GetComponent<RawImage>();
        mFRBrake = Utility.findChild(canvas, "frbrake").GetComponent<RawImage>();
        mRLGas = Utility.findChild(canvas, "rlgas").GetComponent<RawImage>();
        mRLBrake = Utility.findChild(canvas, "rlbrake").GetComponent<RawImage>();
        mRRGas = Utility.findChild(canvas, "rrgas").GetComponent<RawImage>();
        mRRBrake = Utility.findChild(canvas, "rrbrake").GetComponent<RawImage>();

        mFLSusp = Utility.findChild(canvas, "flsusp").GetComponent<RawImage>();
        mFRSusp = Utility.findChild(canvas, "frsusp").GetComponent<RawImage>();
        mRLSusp = Utility.findChild(canvas, "rlsusp").GetComponent<RawImage>();
        mRRSusp = Utility.findChild(canvas, "rrsusp").GetComponent<RawImage>();

        //show/hide
        Utility.findChild(canvas, "kadran").SetActive(ShowSpeedometer);
        Utility.findChild(canvas, "traction").SetActive(ShowTraction);
        Utility.findChild(canvas, "suspension").SetActive(ShowSuspension);
    }

    void Update()
    {
        mKadran.SetActive(ShowSpeedometer);
        mTraction.SetActive(ShowTraction);
        mSuspension.SetActive(ShowSuspension);

        if (ShowSpeedometer)
        {
            float Braking = Vehicle.getAxle(0).getLeftWheel().getBraking();

            //speed text
            if (Braking != 0.0f) mSpeedText.text = ((int)Vehicle.getKMHSpeed()).ToString();
            else
            {
                int speed = (int)Vehicle.getMaxWheelKMHSpeed();
                mSpeedText.text = speed.ToString();
            }

            //gear text
            mGearText.text = "N";
            if (Vehicle.getTransmission().getCurGear() > 0) mGearText.text = Vehicle.getTransmission().getCurGear().ToString();
            else if (Vehicle.getTransmission().getCurGear() < 0) mGearText.text = "R";

            //tachometer
            mIbre.rectTransform.rotation = Quaternion.AngleAxis(Vehicle.getEngine().getRpmRatio() * -270.0f, Vector3.forward);
            mGas.rectTransform.sizeDelta = new Vector2(5.0f, Mathf.Abs(Vehicle.getEngine().getThrottle()) * 93.0f);
            mBrake.rectTransform.sizeDelta = new Vector2(5.0f, Braking * 93.0f);

            //esp, handbrake, abs, asr
            mEspSign.color = Vehicle.getESPActive() ? new Color(1f, 0.75f, 0f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);
            mHandBrake.color = Vehicle.isHandbrakeOn() ? new Color(1f, 0.75f, 0f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);
            mAbsSign.color = Vehicle.getABSActive() ? new Color(1f, 0.75f, 0f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);
            mAsrSign.color = Vehicle.getASRActive() ? new Color(1f, 0.75f, 0f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);

            //steering
            mSteering.rectTransform.localPosition = Vehicle.getNormalizedSteering() * 200.0f * Vector3.left;
            mWheelSign.color = Vehicle.getStandardInput().isSteeringAssistActive() ? new Color(1f, 0.75f, 0f) : new Color(0.5f, 0.5f, 0.5f, 0.2f);
        }

        //gas/brake for each wheel
        if (ShowTraction)
        {
            //torque share texts
            mFrontText.text = (100f * Vehicle.getAxle(0).getTorqueShare()).ToString("f0");
            mRearText.text = (100f * Vehicle.getAxle(1).getTorqueShare()).ToString("f0");

            float flgas = Vehicle.getEngine().getThrottle() * mFLW.getAxle().getTorqueShare();
            float frgas = Vehicle.getEngine().getThrottle() * mFRW.getAxle().getTorqueShare();
            float rlgas = Vehicle.getEngine().getThrottle() * mRLW.getAxle().getTorqueShare();
            float rrgas = Vehicle.getEngine().getThrottle() * mRRW.getAxle().getTorqueShare();

            mFLGas.rectTransform.localScale = new Vector3(1, flgas, 1);
            mFRGas.rectTransform.localScale = new Vector3(1, frgas, 1);
            mRLGas.rectTransform.localScale = new Vector3(1, rlgas, 1);
            mRRGas.rectTransform.localScale = new Vector3(1, rrgas, 1);

            mFLBrake.rectTransform.localScale = new Vector3(1, mFLW.getBraking(), 1);
            mFRBrake.rectTransform.localScale = new Vector3(1, mFRW.getBraking(), 1);
            mRLBrake.rectTransform.localScale = new Vector3(1, mRLW.getBraking(), 1);
            mRRBrake.rectTransform.localScale = new Vector3(1, mRRW.getBraking(), 1);
        }

        //suspension ratio for each wheel
        if (ShowSuspension)
        {
            mFLSusp.rectTransform.localScale = new Vector3(1, mFLW.getSuspensionCompressRatio(), 1);
            mFRSusp.rectTransform.localScale = new Vector3(1, mFRW.getSuspensionCompressRatio(), 1);
            mRLSusp.rectTransform.localScale = new Vector3(1, mRLW.getSuspensionCompressRatio(), 1);
            mRRSusp.rectTransform.localScale = new Vector3(1, mRRW.getSuspensionCompressRatio(), 1);

            mFLSusp.color = Color.Lerp(Color.green, Color.red, mFLW.getSuspensionCompressRatio());
            mFRSusp.color = Color.Lerp(Color.green, Color.red, mFRW.getSuspensionCompressRatio());
            mRLSusp.color = Color.Lerp(Color.green, Color.red, mRLW.getSuspensionCompressRatio());
            mRRSusp.color = Color.Lerp(Color.green, Color.red, mRRW.getSuspensionCompressRatio());
        }
    }
}
