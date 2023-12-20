// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using UnityEngine;
using FGear;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Camera))]
public class MinimapTool : MonoBehaviour
{
    [SerializeField]
    SplineTool Spline;
    [SerializeField, Range(1, 10000)]
    int DrawSteps = 100;
    [SerializeField, Range(1f, 50f)]
    float LineWidth = 1f;
    [SerializeField]
    Vehicle[] VehicleList;
    [SerializeField, Range(1f, 100f)]
    float VehicleQuadSize = 25f;

    LineRenderer mLine;
    Camera mCamera;

    void Awake()
    {
        createMinimapQuads();

        //more setttings needed for camera, should be done manually
        mCamera = GetComponent<Camera>();
        mCamera.clearFlags = CameraClearFlags.Depth;

        //create line renderer over spline
        mLine = GetComponent<LineRenderer>();
        mLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mLine.receiveShadows = false;
        mLine.allowOcclusionWhenDynamic = false;
        mLine.widthMultiplier = LineWidth;

        CubicBezierPath path = Spline.getPath();
        float step = path.GetMaxParam() / DrawSteps;
        int index = 0;
        mLine.positionCount = DrawSteps + 1;

        for (int i = 0; i <= DrawSteps; i++)
        {
            float f = i * step;
            Vector3 p = path.GetPoint(f);
            mLine.SetPosition(index++, p);
        }
    }

    //add a quad at top of each vehicle so that they can be visible in minimap
    void createMinimapQuads()
    {
        if (VehicleList == null) return;

        for (int i = 0; i < VehicleList.Length; i++)
        {
            GameObject miniSeed = Resources.Load("minimapQuad") as GameObject;
            if (miniSeed != null)
            {
                GameObject quad = GameObject.Instantiate(miniSeed);
                quad.name = "minimapQuad";
                quad.transform.localScale = VehicleQuadSize * Vector3.one;
                quad.transform.parent = VehicleList[i].transform;
                quad.transform.localPosition = Vector3.up;
            }
        }
    }
}