// Copyright (C) Yunus Kara 2019-2020. All Rights Reserved.

using UnityEngine;
using UnityEditor;
using FGear;

[CustomEditor(typeof(Tire96))]
[CanEditMultipleObjects]
public class Tire96UI : Editor
{
    Tire96 mTire;
    SerializedProperty mForceSymmetry;
    SerializedProperty PCX1, PDX1, PDX2, PEX1, PEX2, PEX3, PEX4, PKX1, PKX2, PKX3, PHX1, PHX2, PVX1, PVX2;
    SerializedProperty PCY1, PDY1, PDY2, PDY3, PEY1, PEY2, PEY3, PEY4, PKY1, PKY2, PKY3, PHY1, PHY2, PHY3, PVY1, PVY2, PVY3, PVY4;
    SerializedProperty RBX1, RBX2, RCX1, REX1, REX2, RHX1;
    SerializedProperty RBY1, RBY2, RBY3, RCY1, REY1, REY2, RHY1, RVY1, RVY2, RVY3, RVY4, RVY5, RVY6;
    SerializedProperty NML;
    static float mReferenceLoad = 3000f;
    static float mReferenceCamber = 0f;
    static bool mShowLng = false;
    static bool mShowLat = false;
    static bool mShowCmb = false;
    static bool mShowSaveLoad;

    public void OnEnable()
    {
        mTire = (Tire96)target;

        mForceSymmetry = serializedObject.FindProperty("mForceSymmetry");
        NML = serializedObject.FindProperty("mNominalLoad");

        PCX1 = serializedObject.FindProperty("PCX1");
        PDX1 = serializedObject.FindProperty("PDX1");
        PDX2 = serializedObject.FindProperty("PDX2");
        PEX1 = serializedObject.FindProperty("PEX1");
        PEX2 = serializedObject.FindProperty("PEX2");
        PEX3 = serializedObject.FindProperty("PEX3");
        PEX4 = serializedObject.FindProperty("PEX4");
        PKX1 = serializedObject.FindProperty("PKX1");
        PKX2 = serializedObject.FindProperty("PKX2");
        PKX3 = serializedObject.FindProperty("PKX3");
        PHX1 = serializedObject.FindProperty("PHX1");
        PHX2 = serializedObject.FindProperty("PHX2");
        PVX1 = serializedObject.FindProperty("PVX1");
        PVX2 = serializedObject.FindProperty("PVX2");

        PCY1 = serializedObject.FindProperty("PCY1");
        PDY1 = serializedObject.FindProperty("PDY1");
        PDY2 = serializedObject.FindProperty("PDY2");
        PDY3 = serializedObject.FindProperty("PDY3");
        PEY1 = serializedObject.FindProperty("PEY1");
        PEY2 = serializedObject.FindProperty("PEY2");
        PEY3 = serializedObject.FindProperty("PEY3");
        PEY4 = serializedObject.FindProperty("PEY4");
        PKY1 = serializedObject.FindProperty("PKY1");
        PKY2 = serializedObject.FindProperty("PKY2");
        PKY3 = serializedObject.FindProperty("PKY3");
        PHY1 = serializedObject.FindProperty("PHY1");
        PHY2 = serializedObject.FindProperty("PHY2");
        PHY3 = serializedObject.FindProperty("PHY3");
        PVY1 = serializedObject.FindProperty("PVY1");
        PVY2 = serializedObject.FindProperty("PVY2");
        PVY3 = serializedObject.FindProperty("PVY3");
        PVY4 = serializedObject.FindProperty("PVY4");

        RBX1 = serializedObject.FindProperty("RBX1");
        RBX2 = serializedObject.FindProperty("RBX2");
        RCX1 = serializedObject.FindProperty("RCX1");
        REX1 = serializedObject.FindProperty("REX1");
        REX2 = serializedObject.FindProperty("REX2");
        RHX1 = serializedObject.FindProperty("RHX1");

        RBY1 = serializedObject.FindProperty("RBY1");
        RBY2 = serializedObject.FindProperty("RBY2");
        RBY3 = serializedObject.FindProperty("RBY3");
        RCY1 = serializedObject.FindProperty("RCY1");
        REY1 = serializedObject.FindProperty("REY1");
        REY2 = serializedObject.FindProperty("REY2");
        RHY1 = serializedObject.FindProperty("RHY1");
        RVY1 = serializedObject.FindProperty("RVY1");
        RVY2 = serializedObject.FindProperty("RVY2");
        RVY3 = serializedObject.FindProperty("RVY3");
        RVY4 = serializedObject.FindProperty("RVY4");
        RVY5 = serializedObject.FindProperty("RVY5");
        RVY6 = serializedObject.FindProperty("RVY6");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.Space();
        mReferenceLoad = EditorGUILayout.Slider("Reference Load", mReferenceLoad, 10.0f, 10000.0f);
        NML.floatValue = EditorGUILayout.Slider("Nominal Load", NML.floatValue, 10.0f, 10000.0f);
        mReferenceCamber = EditorGUILayout.Slider("Reference Camber", mReferenceCamber, -10.0f, 10.0f);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(mForceSymmetry, new GUIContent("Force Symmetry"));
        EditorGUILayout.Space();
        if (GUILayout.Button("Longitudinal Friction Curve Parameters")) mShowLng = !mShowLng;
        if (mShowLng)
        {
            EditorGUILayout.PropertyField(PCX1);
            EditorGUILayout.PropertyField(PDX1);
            EditorGUILayout.PropertyField(PDX2);
            EditorGUILayout.PropertyField(PEX1);
            EditorGUILayout.PropertyField(PEX2);
            EditorGUILayout.PropertyField(PEX3);
            EditorGUILayout.PropertyField(PEX4);
            EditorGUILayout.PropertyField(PKX1);
            EditorGUILayout.PropertyField(PKX2);
            EditorGUILayout.PropertyField(PKX3);
            EditorGUILayout.PropertyField(PHX1);
            EditorGUILayout.PropertyField(PHX2);
            EditorGUILayout.PropertyField(PVX1);
            EditorGUILayout.PropertyField(PVX2);

            if (GUILayout.Button("Reset Longitudinal Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    PCX1.floatValue = 1.685f;
                    PDX1.floatValue = 1.210f;
                    PDX2.floatValue = -0.037f;
                    PEX1.floatValue = 0.344f;
                    PEX2.floatValue = 0.095f;
                    PEX3.floatValue = -0.020f;
                    PEX4.floatValue = 0f;
                    PKX1.floatValue = 21.51f;
                    PKX2.floatValue = -0.163f;
                    PKX3.floatValue = 0.245f;
                    PHX1.floatValue = -0.002f;
                    PHX2.floatValue = 0.002f;
                    PVX1.floatValue = 0f;
                    PVX2.floatValue = 0f;
                }
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Lateral Friction Curve Parameters")) mShowLat = !mShowLat;
        if (mShowLat)
        {
            EditorGUILayout.PropertyField(PCY1);
            EditorGUILayout.PropertyField(PDY1);
            EditorGUILayout.PropertyField(PDY2);
            EditorGUILayout.PropertyField(PDY3);
            EditorGUILayout.PropertyField(PEY1);
            EditorGUILayout.PropertyField(PEY2);
            EditorGUILayout.PropertyField(PEY3);
            EditorGUILayout.PropertyField(PEY4);
            EditorGUILayout.PropertyField(PKY1);
            EditorGUILayout.PropertyField(PKY2);
            EditorGUILayout.PropertyField(PKY3);
            EditorGUILayout.PropertyField(PHY1);
            EditorGUILayout.PropertyField(PHY2);
            EditorGUILayout.PropertyField(PHY3);
            EditorGUILayout.PropertyField(PVY1);
            EditorGUILayout.PropertyField(PVY2);
            EditorGUILayout.PropertyField(PVY3);
            EditorGUILayout.PropertyField(PVY4);

            if (GUILayout.Button("Reset Lateral Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    PCY1.floatValue = 1.193f;
                    PDY1.floatValue = -0.990f;
                    PDY2.floatValue = 0.145f;
                    PDY3.floatValue = -11.23f;
                    PEY1.floatValue = -1.003f;
                    PEY2.floatValue = -0.537f;
                    PEY3.floatValue = -0.083f;
                    PEY4.floatValue = -4.787f;
                    PKY1.floatValue = -14.95f;
                    PKY2.floatValue = 2.130f;
                    PKY3.floatValue = -0.028f;
                    PHY1.floatValue = 0.003f;
                    PHY2.floatValue = -0.001f;
                    PHY3.floatValue = 0.075f;
                    PVY1.floatValue = 0.045f;
                    PVY2.floatValue = -0.024f;
                    PVY3.floatValue = -0.532f;
                    PVY4.floatValue = 0.039f;
                }
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Combined Force Parameters")) mShowCmb = !mShowCmb;

        if (mShowCmb)
        {
            EditorGUILayout.LabelField("Longitudinal");
            EditorGUILayout.PropertyField(RBX1);
            EditorGUILayout.PropertyField(RBX2);
            EditorGUILayout.PropertyField(RCX1);
            EditorGUILayout.PropertyField(REX1);
            EditorGUILayout.PropertyField(REX2);
            EditorGUILayout.PropertyField(RHX1);

            EditorGUILayout.LabelField("Lateral");
            EditorGUILayout.PropertyField(RBY1);
            EditorGUILayout.PropertyField(RBY2);
            EditorGUILayout.PropertyField(RBY3);
            EditorGUILayout.PropertyField(RCY1);
            EditorGUILayout.PropertyField(REY1);
            EditorGUILayout.PropertyField(REY2);
            EditorGUILayout.PropertyField(RHY1);
            EditorGUILayout.PropertyField(RVY1);
            EditorGUILayout.PropertyField(RVY2);
            EditorGUILayout.PropertyField(RVY3);
            EditorGUILayout.PropertyField(RVY4);
            EditorGUILayout.PropertyField(RVY5);
            EditorGUILayout.PropertyField(RVY6);

            if (GUILayout.Button("Reset Combined Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    RBX1.floatValue = 12.35f;
                    RBX2.floatValue = -10.77f;
                    RCX1.floatValue = 1.092f;
                    REX1.floatValue = 0.0f;
                    REX2.floatValue = 0.0f;
                    RHX1.floatValue = 0.007f;

                    RBY1.floatValue = 6.461f;
                    RBY2.floatValue = 4.196f;
                    RBY3.floatValue = -0.015f;
                    RCY1.floatValue = 1.081f;
                    REY1.floatValue = 0.0f;
                    REY2.floatValue = 0.0f;
                    RHY1.floatValue = 0.009f;
                    RVY1.floatValue = 0.053f;
                    RVY2.floatValue = -0.073f;
                    RVY3.floatValue = 0.517f;
                    RVY4.floatValue = 35.44f;
                    RVY5.floatValue = 1.9f;
                    RVY6.floatValue = -10.71f;
                }
            }
        }

        AnimationCurve lngCurve = new AnimationCurve();
        AnimationCurve latCurve = new AnimationCurve();

        for (int i = -100; i <= 100; i++)
        {
            float fi = i / 100f;
            float angle = fi * 0.3f * Mathf.PI;
            mTire.calculate(mReferenceLoad, fi, angle, mReferenceCamber * Mathf.Deg2Rad, 0f, 0f, 0f);
            lngCurve.AddKey(fi, mTire.getLongitudinalForce());
            latCurve.AddKey(angle, mTire.getLateralForce());
        }

        EditorGUILayout.Space();
        mShowSaveLoad = GUILayout.Toggle(mShowSaveLoad, "Load/Save", "Button");

        if (mShowSaveLoad)
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Load Settings", EditorStyles.miniButton))
            {
                string path = EditorUtility.OpenFilePanel("Load tire from json", Application.dataPath, "json");
                mTire.load(path);
                EditorUtility.SetDirty(mTire);
            }
            if (GUILayout.Button("Save Settings", EditorStyles.miniButton))
            {
                string path = EditorUtility.SaveFilePanel("Save tire as json", Application.dataPath, mTire.name + ".json", "json");
                mTire.save(path);
            }
            EditorGUILayout.Space();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Longitudinal Friction Curve");
        EditorGUILayout.CurveField(lngCurve, GUILayout.Height(100));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lateral Friction Curve");
        EditorGUILayout.CurveField(latCurve, GUILayout.Height(100));

        serializedObject.ApplyModifiedProperties();
    }
}