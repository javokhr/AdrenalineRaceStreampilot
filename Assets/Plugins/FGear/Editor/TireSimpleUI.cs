// Copyright (C) Yunus Kara 2019-2020. All Rights Reserved.

using UnityEngine;
using UnityEditor;
using FGear;

[CustomEditor(typeof(TireSimple))]
[CanEditMultipleObjects]
public class TireSimpleUI : Editor
{
    TireSimple mTire;
    SerializedProperty pLnb, pLnc, pLnd, pLne;
    SerializedProperty pLtb, pLtc, pLtd, pLte;
    static bool mShowSaveLoad;

    public void OnEnable()
    {
        mTire = (TireSimple)target;
        
        pLnb = serializedObject.FindProperty("Lnb");
        pLnc = serializedObject.FindProperty("Lnc");
        pLnd = serializedObject.FindProperty("Lnd");
        pLne = serializedObject.FindProperty("Lne");

        pLtb = serializedObject.FindProperty("Ltb");
        pLtc = serializedObject.FindProperty("Ltc");
        pLtd = serializedObject.FindProperty("Ltd");
        pLte = serializedObject.FindProperty("Lte");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Reference Load is 3kN");
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Longitudinal Friction Curve Parameters");
        EditorGUILayout.PropertyField(pLnb, new GUIContent("B"));
        EditorGUILayout.PropertyField(pLnc, new GUIContent("C"));
        EditorGUILayout.PropertyField(pLnd, new GUIContent("D"));
        EditorGUILayout.PropertyField(pLne, new GUIContent("E"));

        if (GUILayout.Button("Reset Longitudinal Parameters", EditorStyles.miniButton))
        {
            bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
            if (yes)
            {
                pLnb.floatValue = 15.0f;
                pLnc.floatValue = 2.0f;
                pLnd.floatValue = 1.0f;
                pLne.floatValue = 0.95f;
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lateral Friction Curve Parameters");
        EditorGUILayout.PropertyField(pLtb, new GUIContent("B"));
        EditorGUILayout.PropertyField(pLtc, new GUIContent("C"));
        EditorGUILayout.PropertyField(pLtd, new GUIContent("D"));
        EditorGUILayout.PropertyField(pLte, new GUIContent("E"));

        if (GUILayout.Button("Reset Lateral Parameters", EditorStyles.miniButton))
        {
            bool yes = EditorUtility.DisplayDialog("Confirm Reset", "Are you sure you want to reset all parameters?", "Yes", "No");
            if (yes)
            {
                pLtb.floatValue = 5.0f;
                pLtc.floatValue = 2.0f;
                pLtd.floatValue = 1.0f;
                pLte.floatValue = 0.9f;
            }
        }

        AnimationCurve lngCurve = new AnimationCurve();
        AnimationCurve latCurve = new AnimationCurve();

        for (int i = -100; i <= 100; i++)
        {
            float fi = i / 100f;
            float angle = fi * 0.3f * Mathf.PI;
            mTire.calculate(3000f, fi, angle, 0f, 0f, 0f, 0f);
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