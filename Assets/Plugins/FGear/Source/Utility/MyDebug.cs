using UnityEngine;
using FGear;

public class MyDebug : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;
    [SerializeField]
    bool ShowGUI = true;
    [SerializeField]
    float DebugForce = 15000.0f;

    Rigidbody mBody;
    Rect mWindowRect = new Rect(5, 5, 100, 100);
    int mWinID;

    public static float test1 = 0;
    public static float test2 = 0;
    public static float test3 = 0;

    void Start()
    {
        mBody = Vehicle.getBody();
        mWinID = Utility.winIDs++;
    }

    void Update()
    {
        //update debug vars
        if (Input.GetKey(KeyCode.Y)) test1 += Time.deltaTime;
        if (Input.GetKey(KeyCode.H)) test1 -= Time.deltaTime;
        if (Input.GetKey(KeyCode.U)) test2 += Time.deltaTime;
        if (Input.GetKey(KeyCode.J)) test2 -= Time.deltaTime;
        if (Input.GetKey(KeyCode.I)) test3 += Time.deltaTime;
        if (Input.GetKey(KeyCode.K)) test3 -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Keypad8)) mBody.AddRelativeForce(DebugForce * Vector3.forward);
        if (Input.GetKey(KeyCode.Keypad5)) mBody.AddRelativeForce(DebugForce * Vector3.back);
        if (Input.GetKey(KeyCode.Keypad4)) mBody.AddRelativeForce(DebugForce * Vector3.left);
        if (Input.GetKey(KeyCode.Keypad6)) mBody.AddRelativeForce(DebugForce * Vector3.right);
        if (Input.GetKey(KeyCode.Keypad7)) mBody.AddForce(DebugForce * Vector3.up);
        if (Input.GetKey(KeyCode.Keypad1)) mBody.AddForce(DebugForce * Vector3.down);
    }

    void OnGUI()
    {
        if (ShowGUI) mWindowRect = GUI.Window(mWinID, mWindowRect, uiWindowFunction, "Debug");
    }

    void uiWindowFunction(int windowID)
    {
        string t = test1.ToString();
        GUI.Label(new Rect(10, 20, 100, 25), t);
        t = test2.ToString();
        GUI.Label(new Rect(10, 40, 100, 25), t);
        t = test3.ToString();
        GUI.Label(new Rect(10, 60, 100, 25), t);
    }
}