using UnityEngine;
using System.Collections.Generic;
using FGear;

public class SuspensionConstraint : MonoBehaviour
{
    [SerializeField]
    Vehicle Vehicle;

    //list of created joints
    List<ConfigurableJoint> mJTS = new List<ConfigurableJoint>();

    void Start ()
    {
        //this fixes hard contact bounces
        int vs = Vehicle.getBody().solverVelocityIterations;
        Vehicle.getBody().solverVelocityIterations = Mathf.Max(vs, 2);

        //add joint for each axle
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            ConfigurableJoint jtL = Vehicle.gameObject.AddComponent<ConfigurableJoint>();
            ConfigurableJoint jtR = Vehicle.gameObject.AddComponent<ConfigurableJoint>();

            Vector3 ptL = Vehicle.getAxle(i).getLeftWheel().getLocalHubPosition();
            Vector3 ptR = Vehicle.getAxle(i).getRightWheel().getLocalHubPosition();

            //configure joints
            jtL.anchor = ptL;
            jtR.anchor = ptR;

            jtL.configuredInWorldSpace = true;
            jtR.configuredInWorldSpace = true;

            jtL.autoConfigureConnectedAnchor = false;
            jtR.autoConfigureConnectedAnchor = false;

            jtL.yMotion = ConfigurableJointMotion.Limited;
            jtR.yMotion = ConfigurableJointMotion.Limited;

            SoftJointLimit limitL = jtL.linearLimit;
            SoftJointLimit limitR = jtR.linearLimit;

            limitL.limit = Vehicle.getAxle(i).getWheelOptions().getSuspensionUpTravel();
            limitR.limit = Vehicle.getAxle(i).getWheelOptions().getSuspensionUpTravel();

            jtL.linearLimit = limitL;
            jtR.linearLimit = limitR;

            //store in list
            mJTS.Add(jtL);
            mJTS.Add(jtR);
        }
    }

    void FixedUpdate()
    {
        //update limits when wheels have contact
        for (int i = 0; i < Vehicle.getAxleCount(); i++)
        {
            int iL = i * 2;
            int iR = i * 2 + 1;

            Axle ia = Vehicle.getAxle(i);
            Wheel wL = ia.getLeftWheel();
            Wheel wR = ia.getRightWheel();

            if (wL.hasContact())
            {
                mJTS[iL].yMotion = ConfigurableJointMotion.Limited;
                //update axis
                mJTS[iL].axis = wL.getGlobalRight();
                mJTS[iL].secondaryAxis = wL.getGlobalUp();
                //anchor limit pos
                Vector3 wLP = wL.getHubPosition();
                float upTravel = Vehicle.getAxle(i).getWheelOptions().getSuspensionUpTravel();
                Vector3 wLPLimit = wL.getWheelPos() + upTravel * wL.getGlobalUp();
                //limit condition
                Vector3 delta = wLP - wLPLimit;
                delta = Vector3.Project(delta, wL.getGlobalUp());
                if (Vector3.Dot(delta, wL.getGlobalUp()) < 0.0f) wLP -= delta;
                //apply
                mJTS[iL].connectedAnchor = wLP;
            }
            else mJTS[iL].yMotion = ConfigurableJointMotion.Free;

            if (wR.hasContact())
            {
                mJTS[iR].yMotion = ConfigurableJointMotion.Limited;
                //update axis
                mJTS[iR].axis = wR.getGlobalRight();
                mJTS[iR].secondaryAxis = wR.getGlobalUp();
                //anchor limit pos
                Vector3 wRP = wR.getHubPosition();
                float upTravel = Vehicle.getAxle(i).getWheelOptions().getSuspensionUpTravel();
                Vector3 wRPLimit = wR.getWheelPos() + upTravel * wR.getGlobalUp();
                //limit condition
                Vector3 delta = wRP - wRPLimit;
                delta = Vector3.Project(delta, wR.getGlobalUp());
                if (Vector3.Dot(delta, wR.getGlobalUp()) < 0.0f) wRP -= delta;
                //apply
                mJTS[iR].connectedAnchor = wRP;
            }
            else mJTS[iR].yMotion = ConfigurableJointMotion.Free;
        }
    }
}
