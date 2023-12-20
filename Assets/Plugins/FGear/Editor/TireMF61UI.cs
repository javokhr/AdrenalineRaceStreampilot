// Copyright (C) Yunus Kara 2019-2020. All Rights Reserved.

using UnityEngine;
using UnityEditor;
using FGear;

[CustomEditor(typeof(TireMF61))]
[CanEditMultipleObjects]
public class TireMF61UI : Editor
{
    TireMF61 mTire;
    SerializedProperty mForceSymmetry;

    //pacejka 96 longitudinal
    SerializedProperty  PCX1,
                        PDX1, PDX2, PDX3,
                        PEX1, PEX2, PEX3, PEX4,
                        PKX1, PKX2, PKX3,
                        PHX1, PHX2,
                        PVX1, PVX2,
                        PPX1, PPX2, PPX3, PPX4;

    //pacejka 96 lateral
    SerializedProperty  PCY1,
                        PDY1, PDY2, PDY3,
                        PEY1, PEY2, PEY3, PEY4, PEY5,
                        PKY1, PKY2, PKY3, PKY4, PKY5, PKY6, PKY7,
                        PHY1, PHY2,
                        PVY1, PVY2, PVY3, PVY4,
                        PPY1, PPY2, PPY3, PPY4, PPY5;


    //pacejka 96 combined longitudinal
    SerializedProperty  RBX1, RBX2, RBX3,
                        RCX1,
                        REX1, REX2,
                        RHX1;

    //pacejka 96 combined lateral
    SerializedProperty  RBY1, RBY2, RBY3, RBY4,
                        RCY1,
                        REY1, REY2,
                        RHY1, RHY2,
                        RVY1, RVY2, RVY3, RVY4, RVY5, RVY6;

    //pacejka 96 overturning moment
    SerializedProperty  QSX1, QSX2, QSX3, QSX4, QSX5,
                        QSX6, QSX7, QSX8, QSX9, QSX10,
                        QSX11, QSX12, QSX13, QSX14, PPMX1;

    //pacejka 96 rolling resistance moment
    SerializedProperty  QSY1, QSY2, QSY3, QSY4, QSY5,
                        QSY6, QSY7, QSY8, VREF;

    //pacejka 96 aligning moment
    SerializedProperty  QBZ1, QBZ2, QBZ3, QBZ4, QBZ5,
                        QBZ9, QBZ10,
                        QCZ1,
                        QDZ1, QDZ2, QDZ3, QDZ4,
                        QDZ6, QDZ7, QDZ8, QDZ9, QDZ10, QDZ11,
                        QEZ1, QEZ2, QEZ3, QEZ4, QEZ5,
                        QHZ1, QHZ2, QHZ3, QHZ4,
                        PPZ1, PPZ2;

    //pacejka 96 combined aligning moment
    SerializedProperty SSZ1, SSZ2, SSZ3, SSZ4;

    SerializedProperty NML, NMP;

    static float mReferenceLoad = 3000f;
    static float mReferenceTirePressure = 2f;
    static float mReferenceCamber = 0f;
    static bool mShowLng = false;
    static bool mShowLat = false;
    static bool mShowOverTurn = false;
    static bool mShowRollResist = false;
    static bool mShowSelfAlign = false;
    static bool mShowCmb = false;
    static bool mShowSaveLoad;

    public void OnEnable()
    {
        mTire = (TireMF61)target;

        mForceSymmetry = serializedObject.FindProperty("mForceSymmetry");
        NML = serializedObject.FindProperty("mNominalLoad");
        NMP = serializedObject.FindProperty("mNominalPressure");

        PCX1 = serializedObject.FindProperty("PCX1");
        PDX1 = serializedObject.FindProperty("PDX1");
        PDX2 = serializedObject.FindProperty("PDX2");
        PDX3 = serializedObject.FindProperty("PDX3");
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
        PPX1 = serializedObject.FindProperty("PPX1");
        PPX2 = serializedObject.FindProperty("PPX2");
        PPX3 = serializedObject.FindProperty("PPX3");
        PPX4 = serializedObject.FindProperty("PPX4");

        PCY1 = serializedObject.FindProperty("PCY1");
        PDY1 = serializedObject.FindProperty("PDY1");
        PDY2 = serializedObject.FindProperty("PDY2");
        PDY3 = serializedObject.FindProperty("PDY3");
        PEY1 = serializedObject.FindProperty("PEY1");
        PEY2 = serializedObject.FindProperty("PEY2");
        PEY3 = serializedObject.FindProperty("PEY3");
        PEY4 = serializedObject.FindProperty("PEY4");
        PEY5 = serializedObject.FindProperty("PEY5");
        PKY1 = serializedObject.FindProperty("PKY1");
        PKY2 = serializedObject.FindProperty("PKY2");
        PKY3 = serializedObject.FindProperty("PKY3");
        PKY4 = serializedObject.FindProperty("PKY4");
        PKY5 = serializedObject.FindProperty("PKY5");
        PKY6 = serializedObject.FindProperty("PKY6");
        PKY7 = serializedObject.FindProperty("PKY7");
        PHY1 = serializedObject.FindProperty("PHY1");
        PHY2 = serializedObject.FindProperty("PHY2");
        PVY1 = serializedObject.FindProperty("PVY1");
        PVY2 = serializedObject.FindProperty("PVY2");
        PVY3 = serializedObject.FindProperty("PVY3");
        PVY4 = serializedObject.FindProperty("PVY4");
        PPY1 = serializedObject.FindProperty("PPY1");
        PPY2 = serializedObject.FindProperty("PPY2");
        PPY3 = serializedObject.FindProperty("PPY3");
        PPY4 = serializedObject.FindProperty("PPY4");
        PPY5 = serializedObject.FindProperty("PPY5");

        RBX1 = serializedObject.FindProperty("RBX1");
        RBX2 = serializedObject.FindProperty("RBX2");
        RBX3 = serializedObject.FindProperty("RBX3");
        RCX1 = serializedObject.FindProperty("RCX1");
        REX1 = serializedObject.FindProperty("REX1");
        REX2 = serializedObject.FindProperty("REX2");
        RHX1 = serializedObject.FindProperty("RHX1");

        RBY1 = serializedObject.FindProperty("RBY1");
        RBY2 = serializedObject.FindProperty("RBY2");
        RBY3 = serializedObject.FindProperty("RBY3");
        RBY4 = serializedObject.FindProperty("RBY4");
        RCY1 = serializedObject.FindProperty("RCY1");
        REY1 = serializedObject.FindProperty("REY1");
        REY2 = serializedObject.FindProperty("REY2");
        RHY1 = serializedObject.FindProperty("RHY1");
        RHY2 = serializedObject.FindProperty("RHY2");
        RVY1 = serializedObject.FindProperty("RVY1");
        RVY2 = serializedObject.FindProperty("RVY2");
        RVY3 = serializedObject.FindProperty("RVY3");
        RVY4 = serializedObject.FindProperty("RVY4");
        RVY5 = serializedObject.FindProperty("RVY5");
        RVY6 = serializedObject.FindProperty("RVY6");

        QSX1 = serializedObject.FindProperty("QSX1");
        QSX2 = serializedObject.FindProperty("QSX2");
        QSX3 = serializedObject.FindProperty("QSX3");
        QSX4 = serializedObject.FindProperty("QSX4");
        QSX5 = serializedObject.FindProperty("QSX5");
        QSX6 = serializedObject.FindProperty("QSX6");
        QSX7 = serializedObject.FindProperty("QSX7");
        QSX8 = serializedObject.FindProperty("QSX8");
        QSX9 = serializedObject.FindProperty("QSX9");
        QSX10 = serializedObject.FindProperty("QSX10");
        QSX11 = serializedObject.FindProperty("QSX11");
        QSX12 = serializedObject.FindProperty("QSX12");
        QSX13 = serializedObject.FindProperty("QSX13");
        QSX14 = serializedObject.FindProperty("QSX14");
        PPMX1 = serializedObject.FindProperty("PPMX1");

        QSY1 = serializedObject.FindProperty("QSY1");
        QSY2 = serializedObject.FindProperty("QSY2");
        QSY3 = serializedObject.FindProperty("QSY3");
        QSY4 = serializedObject.FindProperty("QSY4");
        QSY5 = serializedObject.FindProperty("QSY5");
        QSY6 = serializedObject.FindProperty("QSY6");
        QSY7 = serializedObject.FindProperty("QSY7");
        QSY8 = serializedObject.FindProperty("QSY8");
        VREF = serializedObject.FindProperty("VREF");

        QBZ1 = serializedObject.FindProperty("QBZ1");
        QBZ2 = serializedObject.FindProperty("QBZ2");
        QBZ3 = serializedObject.FindProperty("QBZ3");
        QBZ4 = serializedObject.FindProperty("QBZ4");
        QBZ5 = serializedObject.FindProperty("QBZ5");
        QBZ9 = serializedObject.FindProperty("QBZ9");
        QBZ10 = serializedObject.FindProperty("QBZ10");
        QCZ1 = serializedObject.FindProperty("QCZ1");
        QDZ1 = serializedObject.FindProperty("QDZ1");
        QDZ2 = serializedObject.FindProperty("QDZ2");
        QDZ3 = serializedObject.FindProperty("QDZ3");
        QDZ4 = serializedObject.FindProperty("QDZ4");
        QDZ6 = serializedObject.FindProperty("QDZ6");
        QDZ7 = serializedObject.FindProperty("QDZ7");
        QDZ8 = serializedObject.FindProperty("QDZ8");
        QDZ9 = serializedObject.FindProperty("QDZ9");
        QDZ10 = serializedObject.FindProperty("QDZ10");
        QDZ11 = serializedObject.FindProperty("QDZ11");
        QEZ1 = serializedObject.FindProperty("QEZ1");
        QEZ2 = serializedObject.FindProperty("QEZ2");
        QEZ3 = serializedObject.FindProperty("QEZ3");
        QEZ4 = serializedObject.FindProperty("QEZ4");
        QEZ5 = serializedObject.FindProperty("QEZ5");
        QHZ1 = serializedObject.FindProperty("QHZ1");
        QHZ2 = serializedObject.FindProperty("QHZ2");
        QHZ3 = serializedObject.FindProperty("QHZ3");
        QHZ4 = serializedObject.FindProperty("QHZ4");
        PPZ1 = serializedObject.FindProperty("PPZ1");
        PPZ2 = serializedObject.FindProperty("PPZ2");

        SSZ1 = serializedObject.FindProperty("SSZ1");
        SSZ2 = serializedObject.FindProperty("SSZ2");
        SSZ3 = serializedObject.FindProperty("SSZ3");
        SSZ4 = serializedObject.FindProperty("SSZ4");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.Space();
        mReferenceLoad = EditorGUILayout.Slider("Reference Load", mReferenceLoad, 10.0f, 10000.0f);
        mReferenceCamber = EditorGUILayout.Slider("Reference Camber", mReferenceCamber, -10.0f, 10.0f);
        mReferenceTirePressure = EditorGUILayout.Slider("Reference Tire Pressure", mReferenceTirePressure, 0.1f, 10.0f);
        
        EditorGUILayout.Space();
        NML.floatValue = EditorGUILayout.Slider("Nominal Load", NML.floatValue, 10.0f, 10000.0f);
        NMP.floatValue = EditorGUILayout.Slider("Nominal Tire Pressure", NMP.floatValue, 1.0f, 10.0f);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(mForceSymmetry, new GUIContent("Force Symmetry"));

        EditorGUILayout.Space();
        if (GUILayout.Button("Longitudinal Friction Curve Parameters")) mShowLng = !mShowLng;
        if (mShowLng)
        {
            EditorGUILayout.PropertyField(PCX1);
            EditorGUILayout.PropertyField(PDX1);
            EditorGUILayout.PropertyField(PDX2);
            EditorGUILayout.PropertyField(PDX3);
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
            EditorGUILayout.PropertyField(PPX1);
            EditorGUILayout.PropertyField(PPX2);
            EditorGUILayout.PropertyField(PPX3);
            EditorGUILayout.PropertyField(PPX4);

            if (GUILayout.Button("Reset Longitudinal Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    PCX1.floatValue = 1.579f;
                    PDX1.floatValue = 1.0422f;
                    PDX2.floatValue = -0.08285f;
                    PDX3.floatValue = 0f;
                    PEX1.floatValue = 0.11113f;
                    PEX2.floatValue = 0.3143f;
                    PEX3.floatValue = 0f;
                    PEX4.floatValue = 0.001719f;
                    PKX1.floatValue = 21.687f;
                    PKX2.floatValue = 13.728f;
                    PKX3.floatValue = -0.4098f;
                    PHX1.floatValue = 0.00021615f;
                    PHX2.floatValue = 0.0011598f;
                    PVX1.floatValue = 0.000020283f;
                    PVX2.floatValue = 0.00010568f;
                    PPX1.floatValue = -0.3485f;
                    PPX2.floatValue = 0.37824f;
                    PPX3.floatValue = -0.09603f;
                    PPX4.floatValue = 0.06518f;
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
            EditorGUILayout.PropertyField(PEY5);
            EditorGUILayout.PropertyField(PKY1);
            EditorGUILayout.PropertyField(PKY2);
            EditorGUILayout.PropertyField(PKY3);
            EditorGUILayout.PropertyField(PKY4);
            EditorGUILayout.PropertyField(PKY5);
            EditorGUILayout.PropertyField(PKY6);
            EditorGUILayout.PropertyField(PKY7);
            EditorGUILayout.PropertyField(PHY1);
            EditorGUILayout.PropertyField(PHY2);
            EditorGUILayout.PropertyField(PVY1);
            EditorGUILayout.PropertyField(PVY2);
            EditorGUILayout.PropertyField(PVY3);
            EditorGUILayout.PropertyField(PVY4);
            EditorGUILayout.PropertyField(PPY1);
            EditorGUILayout.PropertyField(PPY2);
            EditorGUILayout.PropertyField(PPY3);
            EditorGUILayout.PropertyField(PPY4);
            EditorGUILayout.PropertyField(PPY5);

            if (GUILayout.Button("Reset Lateral Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    PCY1.floatValue = 1.338f;
                    PDY1.floatValue = 0.8785f;
                    PDY2.floatValue = -0.06452f;
                    PDY3.floatValue = 0f;
                    PEY1.floatValue = -0.8057f;
                    PEY2.floatValue = -0.6046f;
                    PEY3.floatValue = 0.09854f;
                    PEY4.floatValue = -6.697f;
                    PEY5.floatValue = 0f;
                    PKY1.floatValue = -15.324f;
                    PKY2.floatValue = 1.715f;
                    PKY3.floatValue = 0.3695f;
                    PKY4.floatValue = 2.0005f;
                    PKY5.floatValue = 0f;
                    PKY6.floatValue = -0.8987f;
                    PKY7.floatValue = -0.23303f;
                    PHY1.floatValue = -0.001806f;
                    PHY2.floatValue = 0.0035f;
                    PVY1.floatValue = -0.00661f;
                    PVY2.floatValue = 0.03592f;
                    PVY3.floatValue = -0.162f;
                    PVY4.floatValue = -0.4864f;
                    PPY1.floatValue = -0.6255f;
                    PPY2.floatValue = -0.06523f;
                    PPY3.floatValue = -0.16666f;
                    PPY4.floatValue = 0.2811f;
                    PPY5.floatValue = 0f;
                }
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Over Turning Moment Parameters")) mShowOverTurn = !mShowOverTurn;
        if (mShowOverTurn)
        {
            EditorGUILayout.PropertyField(QSX1);
            EditorGUILayout.PropertyField(QSX2);
            EditorGUILayout.PropertyField(QSX3);
            EditorGUILayout.PropertyField(QSX4);
            EditorGUILayout.PropertyField(QSX5);
            EditorGUILayout.PropertyField(QSX6);
            EditorGUILayout.PropertyField(QSX7);
            EditorGUILayout.PropertyField(QSX8);
            EditorGUILayout.PropertyField(QSX9);
            EditorGUILayout.PropertyField(QSX10);
            EditorGUILayout.PropertyField(QSX11);
            EditorGUILayout.PropertyField(QSX12);
            EditorGUILayout.PropertyField(QSX13);
            EditorGUILayout.PropertyField(QSX14);
            EditorGUILayout.PropertyField(PPMX1);

            if (GUILayout.Button("Reset Over Turning Moment Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    QSX1.floatValue = -0.007764f;
                    QSX2.floatValue = 1.1915f;
                    QSX3.floatValue = 0.013948f;
                    QSX4.floatValue = 4.912f;
                    QSX5.floatValue = 1.02f;
                    QSX6.floatValue = 22.83f;
                    QSX7.floatValue = 0.7104f;
                    QSX8.floatValue = -0.023393f;
                    QSX9.floatValue = 0.6581f;
                    QSX10.floatValue = 0.2824f;
                    QSX11.floatValue = 5.349f;
                    QSX12.floatValue = 0.0f;
                    QSX13.floatValue = 0.0f;
                    QSX14.floatValue = 0.0f;
                    PPMX1.floatValue = 0f;
                }
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Rolling Resistance Moment Parameters")) mShowRollResist = !mShowRollResist;
        if (mShowRollResist)
        {
            EditorGUILayout.PropertyField(QSY1);
            EditorGUILayout.PropertyField(QSY2);
            EditorGUILayout.PropertyField(QSY3);
            EditorGUILayout.PropertyField(QSY4);
            EditorGUILayout.PropertyField(QSY5);
            EditorGUILayout.PropertyField(QSY6);
            EditorGUILayout.PropertyField(QSY7);
            EditorGUILayout.PropertyField(QSY8);
            EditorGUILayout.PropertyField(VREF);

            if (GUILayout.Button("Reset Rolling Resistance Moment Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    QSY1.floatValue = 0.00702f;
                    QSY2.floatValue = 0f;
                    QSY3.floatValue = 0.001515f;
                    QSY4.floatValue = 0.00008514f;
                    QSY5.floatValue = 0f;
                    QSY6.floatValue = 0f;
                    QSY7.floatValue = 0.9008f;
                    QSY8.floatValue = -0.4089f;
                    VREF.floatValue = 16.67f;
                }
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Self Aligning Moment Parameters")) mShowSelfAlign = !mShowSelfAlign;
        if (mShowSelfAlign)
        {
            EditorGUILayout.PropertyField(QBZ1);
            EditorGUILayout.PropertyField(QBZ2);
            EditorGUILayout.PropertyField(QBZ3);
            EditorGUILayout.PropertyField(QBZ4);
            EditorGUILayout.PropertyField(QBZ5);
            EditorGUILayout.PropertyField(QBZ9);
            EditorGUILayout.PropertyField(QBZ10);
            EditorGUILayout.PropertyField(QCZ1);
            EditorGUILayout.PropertyField(QDZ1);
            EditorGUILayout.PropertyField(QDZ2);
            EditorGUILayout.PropertyField(QDZ3);
            EditorGUILayout.PropertyField(QDZ4);
            EditorGUILayout.PropertyField(QDZ6);
            EditorGUILayout.PropertyField(QDZ7);
            EditorGUILayout.PropertyField(QDZ8);
            EditorGUILayout.PropertyField(QDZ9);
            EditorGUILayout.PropertyField(QDZ10);
            EditorGUILayout.PropertyField(QDZ11);
            EditorGUILayout.PropertyField(QEZ1);
            EditorGUILayout.PropertyField(QEZ2);
            EditorGUILayout.PropertyField(QEZ3);
            EditorGUILayout.PropertyField(QEZ4);
            EditorGUILayout.PropertyField(QEZ5);
            EditorGUILayout.PropertyField(QHZ1);
            EditorGUILayout.PropertyField(QHZ2);
            EditorGUILayout.PropertyField(QHZ3);
            EditorGUILayout.PropertyField(QHZ4);
            EditorGUILayout.PropertyField(PPZ1);
            EditorGUILayout.PropertyField(PPZ2);

            if (GUILayout.Button("Reset Self Aligning Moment Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    QBZ1.floatValue = 12.035f;
                    QBZ2.floatValue = -1.33f;
                    QBZ3.floatValue = 0f;
                    QBZ4.floatValue = 0.176f;
                    QBZ5.floatValue = -0.14853f;
                    QBZ9.floatValue = 34.5f;
                    QBZ10.floatValue = 0f;
                    QCZ1.floatValue = 1.2923f;
                    QDZ1.floatValue = 0.09068f;
                    QDZ2.floatValue = -0.00565f;
                    QDZ3.floatValue = 0.3778f;
                    QDZ4.floatValue = 0f;
                    QDZ6.floatValue = 0.0017015f;
                    QDZ7.floatValue = -0.002091f;
                    QDZ8.floatValue = -0.1428f;
                    QDZ9.floatValue = 0.00915f;
                    QDZ10.floatValue = 0f;
                    QDZ11.floatValue = 0f;
                    QEZ1.floatValue = -1.7924f;
                    QEZ2.floatValue = 0.8975f;
                    QEZ3.floatValue = 0f;
                    QEZ4.floatValue = 0.2895f;
                    QEZ5.floatValue = -0.6786f;
                    QHZ1.floatValue = 0.0014333f;
                    QHZ2.floatValue = 0.0024087f;
                    QHZ3.floatValue = 0.24973f;
                    QHZ4.floatValue = -0.21205f;
                    PPZ1.floatValue = -0.4408f;
                    PPZ2.floatValue = 0f;
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
            EditorGUILayout.PropertyField(RBX3);
            EditorGUILayout.PropertyField(RCX1);
            EditorGUILayout.PropertyField(REX1);
            EditorGUILayout.PropertyField(REX2);
            EditorGUILayout.PropertyField(RHX1);

            EditorGUILayout.LabelField("Lateral");
            EditorGUILayout.PropertyField(RBY1);
            EditorGUILayout.PropertyField(RBY2);
            EditorGUILayout.PropertyField(RBY3);
            EditorGUILayout.PropertyField(RBY4);
            EditorGUILayout.PropertyField(RCY1);
            EditorGUILayout.PropertyField(REY1);
            EditorGUILayout.PropertyField(REY2);
            EditorGUILayout.PropertyField(RHY1);
            EditorGUILayout.PropertyField(RHY2);
            EditorGUILayout.PropertyField(RVY1);
            EditorGUILayout.PropertyField(RVY2);
            EditorGUILayout.PropertyField(RVY3);
            EditorGUILayout.PropertyField(RVY4);
            EditorGUILayout.PropertyField(RVY5);
            EditorGUILayout.PropertyField(RVY6);

            EditorGUILayout.LabelField("Self Align");
            EditorGUILayout.PropertyField(SSZ1);
            EditorGUILayout.PropertyField(SSZ2);
            EditorGUILayout.PropertyField(SSZ3);
            EditorGUILayout.PropertyField(SSZ4);

            if (GUILayout.Button("Reset Combined Parameters", EditorStyles.miniButton))
            {
                bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
                if (yes)
                {
                    //Longitudinal
                    RBX1.floatValue = 13.046f;
                    RBX2.floatValue = 9.718f;
                    RBX3.floatValue = 0f;
                    RCX1.floatValue = 0.9995f;
                    REX1.floatValue = -0.4403f;
                    REX2.floatValue = -0.4663f;
                    RHX1.floatValue = -0.00009968f;

                    //Lateral
                    RBY1.floatValue = 10.622f;
                    RBY2.floatValue = 7.82f;
                    RBY3.floatValue = 0.002037f;
                    RBY4.floatValue = 0f;
                    RCY1.floatValue = 1.0587f;
                    REY1.floatValue = 0.3148f;
                    REY2.floatValue = 0.004867f;
                    RHY1.floatValue = 0.009472f;
                    RHY2.floatValue = 0.009754f;
                    RVY1.floatValue = 0.05187f;
                    RVY2.floatValue = 0.0004853f;
                    RVY3.floatValue = 0f;
                    RVY4.floatValue = 94.63f;
                    RVY5.floatValue = 1.8914f;
                    RVY6.floatValue = 23.8f;

                    //Self Align
                    SSZ1.floatValue = 0.00918f;
                    SSZ2.floatValue = 0.03869f;
                    SSZ3.floatValue = 0f;
                    SSZ4.floatValue = 0f;
                }
            }
        }

        AnimationCurve lngCurve = new AnimationCurve();
        AnimationCurve latCurve = new AnimationCurve();
        AnimationCurve mzCurve = new AnimationCurve();

        for (int i = -100; i <= 100; i++)
        {
            float fi = i / 100f;
            float angle = fi * 0.3f * Mathf.PI;
            mTire.calculate(mReferenceLoad, fi, angle, mReferenceCamber * Mathf.Deg2Rad, mReferenceTirePressure, 0.25f, 16f);
            lngCurve.AddKey(fi, mTire.getLongitudinalForce());
            latCurve.AddKey(angle, mTire.getLateralForce());
            mzCurve.AddKey(angle, mTire.getCombinedTorque().y);
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
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Align Moment Curve");
        EditorGUILayout.CurveField(mzCurve, GUILayout.Height(100));

        serializedObject.ApplyModifiedProperties();
    }
}