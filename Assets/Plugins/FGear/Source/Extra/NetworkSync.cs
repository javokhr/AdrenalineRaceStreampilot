// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

//#define PUN2_INSTALLED
using System.Collections.Generic;
using UnityEngine;
using FGear;
#if (PUN2_INSTALLED)
using Photon.Pun;
using Photon.Realtime;
#endif

//To use this class 
//1-install "PUN2-Free" from assetstore (tested with v2.15)
//2-create a pun account or use existing one -> setup pun-project after install
//3-uncomment line3 : #define PUN2_INSTALLED
//4-also check NetworkSample.cs

#if (PUN2_INSTALLED)
[RequireComponent(typeof(PhotonView)), DisallowMultipleComponent]
public class NetworkSync : MonoBehaviourPunCallbacks, IPunObservable
#else
public class NetworkSync : MonoBehaviour
#endif
{
    class State
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 vel;
        public Vector3 avel;
    }

    public class InputState
    {
        public InputState(float ei, float bi, float si, float ci, int gi, bool hbi)
        {
            engine = ei;
            brake = bi;
            steer = si;
            clutch = ci;
            gearbox = gi;
            handbrake = hbi;
        }

        public bool isSame(InputState s)
        {
            return engine == s.engine && brake == s.brake && steer == s.steer &&
                   clutch == s.clutch && gearbox == s.gearbox && handbrake == s.handbrake;
        }

        public float engine = 0f;
        public float brake = 0f;
        public float steer = 0f;
        public float clutch = 0f;
        public int gearbox = 0;
        public bool handbrake = false;
    }

    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    float InterpolationSpeed = 5.0f;

#if (PUN2_INSTALLED)
    //components
    Rigidbody mBody;

    //network serialized input
    InputState mLastInput;

    //snapshot for interpolation
    State mLastSnap;

    //getters & setters
    public Vehicle getVehicle() { return Vehicle; }
    public Rigidbody getBody() { return mBody; }
    public InputState getLastInput() { return mLastInput; }

    void Awake()
    {
        photonView.Owner.TagObject = this;
        mBody = GetComponent<Rigidbody>();
        
        //input for sending and receiving
        mLastInput = new InputState(0.0f, 0.0f, 0.0f, 0.0f, 0, false);

        //snapshot for interpolation
        if (!photonView.IsMine)
        {
            mLastSnap = new State();
            mLastSnap.pos = mBody.position;
            mLastSnap.rot = mBody.rotation;
        }

        //no ui for client vehicle
        GetComponent<GaugeUI>().enabled = photonView.IsMine;

        //we will manually update inputs
        Vehicle.setUpdateInputs(false);

        //no read input, we will read from buffers
        Vehicle.getStandardInput().setReadInputs(false);

        //attach camera to own car
        if (photonView.IsMine)
        {
            OrbitCamera cam = gameObject.GetComponent<OrbitCamera>();
            if (cam == null) cam = gameObject.AddComponent<OrbitCamera>();
            cam.setTarget(transform);
            cam.setSpring(2.0f);
            cam.setDistance(5.0f);
        }

        //set name text
        GameObject nameObj = Utility.findChild(gameObject, "nameText");
        if (nameObj != null)
        {
            TextMesh nameText = nameObj.GetComponent<TextMesh>();
            nameText.text = photonView.Owner.NickName;
        }
    }

    void Update()
    {
        //reset vehicle and call reset on others
        if (Input.GetKeyDown(KeyCode.R))
        {
            photonView.RPC("resetVehicle", RpcTarget.All);
        }
    }

    void FixedUpdate()
    {
        syncInputs();

        //interpolation
        if (!photonView.IsMine)
        {
            //use extrapolated transform
            float lag = 0.001f * PhotonNetwork.GetPing();
            Vector3 pos = mLastSnap.pos + lag * mLastSnap.vel;
            Quaternion rot = mLastSnap.rot;
            Utility.rotateQuat(ref rot, lag * mLastSnap.avel);
            
            //interpolate
            pos = Vector3.Lerp(mBody.position, pos, InterpolationSpeed * Time.fixedDeltaTime);
            rot = Quaternion.Slerp(mBody.rotation, rot, InterpolationSpeed * Time.fixedDeltaTime);

            //apply
            mBody.MovePosition(pos);
            mBody.MoveRotation(rot);
        }
    }

    void syncInputs()
    {
        //store owners last input every tick
        if (photonView.IsMine)
        {
            //read inputs
            Vehicle.getStandardInput().getInputs();
            //store
            mLastInput.brake = Vehicle.getStandardInput().getBrakeRawInput();
            mLastInput.clutch = Vehicle.getStandardInput().getClutchRawInput();
            mLastInput.engine = Vehicle.getStandardInput().getEngineRawInput();
            mLastInput.gearbox = Vehicle.getStandardInput().getGearboxInput();
            mLastInput.handbrake = Vehicle.getStandardInput().getHandbrakeInput();
            mLastInput.steer = Vehicle.getStandardInput().getSteerRawInput();
        }
        //others read last input from network
        else
        {
            //use from last input
            InputState s = mLastInput;
            Vehicle.getStandardInput().setInputs(s.engine, s.brake, s.steer, s.clutch, s.gearbox, s.handbrake);
        }

        //update input
        Vehicle.getStandardInput().myUpdate(Time.fixedDeltaTime, true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //owner sends input/transform/velocity
            if (photonView.IsMine)
            {
                NetworkSync ns = (NetworkSync)PhotonNetwork.LocalPlayer.TagObject;
                if (ns != null)
                {
                    //send input
                    InputState s = ns.getLastInput();
                    stream.SendNext(s.engine);
                    stream.SendNext(s.brake);
                    stream.SendNext(s.steer);
                    stream.SendNext(s.clutch);
                    stream.SendNext(s.gearbox);
                    stream.SendNext(s.handbrake);

                    //send transform
                    stream.SendNext(ns.getBody().position);
                    stream.SendNext(ns.getBody().rotation);

                    //send linear/angular velocity
                    stream.SendNext(ns.getBody().velocity);
                    stream.SendNext(ns.getBody().angularVelocity);
                }
            }
        }
        else if (stream.IsReading)
        {
            //others receive input/transform/velocity
            if (!photonView.IsMine)
            {
                NetworkSync ns = (NetworkSync)info.Sender.TagObject;
                if (ns != null)
                {
                    //receive input
                    float ei = (float)stream.ReceiveNext();
                    float bi = (float)stream.ReceiveNext();
                    float si = (float)stream.ReceiveNext();
                    float ci = (float)stream.ReceiveNext();
                    int gi = (int)stream.ReceiveNext();
                    bool hbi = (bool)stream.ReceiveNext();
                    ns.setLastInput(ei, bi, si, ci, gi, hbi);

                    //get pos/rot
                    Vector3 pos = (Vector3)stream.ReceiveNext();
                    Quaternion rot = (Quaternion)stream.ReceiveNext();

                    //get linear/angular velocity
                    Vector3 vel = (Vector3)stream.ReceiveNext();
                    Vector3 avel = (Vector3)stream.ReceiveNext();

                    //set received transform
                    ns.setLastSnap(pos, rot, vel, avel);
                }
            }
        }
    }

    void setLastSnap(Vector3 p, Quaternion q, Vector3 v, Vector3 av)
    {
        mLastSnap.pos = p;
        mLastSnap.rot = q;
        mLastSnap.vel = v;
        mLastSnap.avel = av;
    }

    void setLastInput(float ei, float bi, float si, float ci, int gi, bool hbi)
    {
        mLastInput.engine = ei;
        mLastInput.brake = bi;
        mLastInput.steer = si;
        mLastInput.clutch = ci;
        mLastInput.gearbox = gi;
        mLastInput.handbrake = hbi;
    }

    [PunRPC]
    void resetVehicle()
    {
        Vector3 pos = Vehicle.getPosition();
        Quaternion rot = Vehicle.getRotation();
        rot.eulerAngles = new Vector3(0, rot.eulerAngles.y, 0);
        Vehicle.reset(pos, rot);
    }
#endif
}