// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

//#define PUN2_INSTALLED
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
//4-also check NetworkSync.cs

#if (PUN2_INSTALLED)
public class NetworkSample : MonoBehaviourPunCallbacks
#else
public class NetworkSample : MonoBehaviour
#endif
{
#if (PUN2_INSTALLED)
    [SerializeField]
    int SendRate = 50;
    [SerializeField]
    string VehiclePrefabName = "hbFwdNetwork";

    string mUserName = "Player";

    //stats
    Rect mWindowRect = new Rect(10, 10, 125, 150);
    int mWinID;
    float mTime;
    float mInPerSec;
    float mOutPerSec;
    float mLatency;

    void Awake()
    {
        mWinID = Utility.winIDs++;
        Application.targetFrameRate = 60;
        
        //this makes sure we can use PhotonNetwork.LoadLevel() on the master client
        //and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
        //set sendrates
        PhotonNetwork.SendRate = SendRate;
        PhotonNetwork.SerializationRate = SendRate;
    }

    void connect()
    {
        //we check if we are connected or not, we join if we are
        //else we initiate the connection to the server
        if (PhotonNetwork.IsConnected)
        {
            //try to join test room or create the room if not available
            PhotonNetwork.JoinOrCreateRoom("testRoom", new RoomOptions(), TypedLobby.Default);
        }
        else
        {
            //we must first and foremost connect to Photon Online Server
            PhotonNetwork.GameVersion = "1";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void addVehicle()
    {
        //instantiate vehicle prefab
        Vector3 position = new Vector3(-35.0f + PhotonNetwork.PlayerList.Length * 3.0f, 0.05f, 0.0f);
        PhotonNetwork.Instantiate(VehiclePrefabName, position, Quaternion.identity, 0);
    }

    public override void OnConnectedToMaster()
    {
        Debug.LogFormat("OnConnectedToMaster user {0}", PhotonNetwork.NickName);
        //connection established
        //try to join test room or create the room if not available
        PhotonNetwork.JoinOrCreateRoom("testRoom", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogFormat("OnDisconnected with reason {0}", cause);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogFormat("OnJoinRoomFailed code {0} message {1}", returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        Debug.LogFormat("OnJoinedRoom room {0}", PhotonNetwork.CurrentRoom.Name);
        addVehicle();
    }

    void OnGUI()
    {
        if (PhotonNetwork.InRoom)
        {
            mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Network");
            return;
        }

        GUI.enabled = !PhotonNetwork.IsConnected;
        mUserName = GUI.TextField(new Rect(0.5f * Screen.width - 50, 0.5f * Screen.height - 35, 150, 35), mUserName, 10);
        if (GUI.Button(new Rect(0.5f * Screen.width - 50, 0.5f * Screen.height, 150, 50), "Join"))
        {
            PhotonNetwork.NickName = mUserName;
            connect();
        }
    }

    void uiWindowFunction(int windowID)
    {
        mTime += Time.deltaTime;
        if (mTime >= 1.0f)
        {
            mTime -= 1.0f;
            mInPerSec = PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsIncoming.TotalPacketBytes;
            mOutPerSec = PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsOutgoing.TotalPacketBytes;
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsReset();
            PhotonNetwork.NetworkingClient.LoadBalancingPeer.TrafficStatsEnabled = true;
        }

        //stats
        GUI.Label(new Rect(10, 20, 100, 30), "Ping:" + PhotonNetwork.GetPing() + " ms");
        GUI.Label(new Rect(10, 40, 100, 30), "In:" + (mInPerSec / 1024).ToString("f1") + " kb/s");
        GUI.Label(new Rect(10, 60, 100, 30), "Out:" + (mOutPerSec / 1024).ToString("f1") + " kb/s");
        //lag sim
        GUI.Label(new Rect(10, 85, 100, 30), "ExtraLag:" + (mLatency >= 1 ? ((int)mLatency + " ms") : "-"));
        mLatency = GUI.HorizontalSlider(new Rect(10, 110, 100, 20), mLatency, 0, 500);
        LoadBalancingPeer peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
        peer.IsSimulationEnabled = mLatency >= 1;
        peer.NetworkSimulationSettings.IncomingLag = (int)(mLatency / 2);
        peer.NetworkSimulationSettings.OutgoingLag = (int)(mLatency / 2);
    }
#endif
}