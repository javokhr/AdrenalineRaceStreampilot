// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using UnityEditor;

namespace FGear
{
    public class Utility
    {
        public static float rads2Rpm = 60.0f / (2.0f * Mathf.PI);
        public static float rpm2Rads = (2.0f * Mathf.PI) / 60.0f;
        public static float ms2kmh = 3.6f;
        public static float kmh2ms = 0.277778f;
        public static float kw2hp = 1.34102f;
        public static float gravitySize = Physics.gravity.magnitude;
        public static float pi2 = 2.0f * Mathf.PI;
        public static Quaternion q90cw = Quaternion.AngleAxis(90.0f, Vector3.up); //clockwise 90 degree
        public static int winIDs = 0;
        public static Color textColor = Color.white;

        public static string formatTime(float f)
        {
            int secs = (int)f % 60;
            int mins = (int)f / 60;
            return (mins < 10 ? "0" : "") + mins + ":" + (secs < 10 ? "0" : "") + secs;
        }

        public static GameObject findChild(GameObject parent, string name, bool caseSensitive = true)
        {
            GameObject ret = null;
            string givenName = caseSensitive ? name : name.ToLowerInvariant();
            foreach (Transform node in parent.transform)
            {
                string nodeName = caseSensitive ? node.gameObject.name : node.gameObject.name.ToLowerInvariant();

                if (nodeName == givenName)
                {
                    return node.gameObject;
                }
                else
                {
                    GameObject result = findChild(node.gameObject, name, caseSensitive);
                    if (result != null) ret = result;
                }
            }
            return ret;
        }

        public static Bounds getBounds(GameObject obj)
        {
            Bounds bounds = new Bounds(Vector3.zero, 0.1f * Vector3.one);
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return bounds;
            bounds = renderers[0].bounds;
            
            foreach (Renderer r in renderers)
            {
                if (r.enabled) bounds.Encapsulate(r.bounds);
            }

            return bounds;
        }

        public static void addTorqueAtPosition(Rigidbody body, Vector3 position, Vector3 torque)
        {
            Vector3 torqueAxis = torque.normalized;
            Vector3 ortho = Vector3.right;

            //prevent torqueAxis and ortho from pointing in the same direction
            if ((torqueAxis - ortho).sqrMagnitude < float.Epsilon)
            {
                ortho = Vector3.up;
            }

            //calculate force
            Vector3.OrthoNormalize(ref torqueAxis, ref ortho);
            Vector3 force = Vector3.Cross(0.5f * torque, ortho);
            body.AddForceAtPosition(force, position + ortho);
            body.AddForceAtPosition(-force, position - ortho);
        }

        public static void rotateQuat(ref Quaternion rot, Vector3 dv)
        {
            Quaternion q = new Quaternion(dv.x, dv.y, dv.z, 0.0f);
            q *= rot;
            rot.x += 0.5f * q.x;
            rot.y += 0.5f * q.y;
            rot.z += 0.5f * q.z;
            rot.w += 0.5f * q.w;
            rot.Normalize();
        }

        public static string[] getAxisList()
        {
#if UNITY_EDITOR
            var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            SerializedObject obj = new SerializedObject(inputManager);
            SerializedProperty axisArray = obj.FindProperty("m_Axes");

            if (axisArray.arraySize == 0) return null;

            string[] axes = new string[axisArray.arraySize];

            for (int i = 0; i < axisArray.arraySize; ++i)
            {
                var axis = axisArray.GetArrayElementAtIndex(i);
                axes[i] = axis.FindPropertyRelative("m_Name").stringValue;
            }

            return axes;
#else
            return null;
#endif
        }
    }
}