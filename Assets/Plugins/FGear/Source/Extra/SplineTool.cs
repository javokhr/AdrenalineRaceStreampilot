// Copyright (C) Yunus Kara 2019-2021. All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

public class SplineTool : MonoBehaviour
{
    [SerializeField]
    CubicBezierPath.Type Type;
    [SerializeField, Range(1, 10000)]
    int DrawSteps = 100;

    CubicBezierPath mPath;
    float mLength;
    float mThreshold = 0f;
    List<float> mLengthList = new List<float>();
    public CubicBezierPath getPath() { return mPath; }

    void Awake()
    {
        updatePoints();
        calculateLengthList();
        gameObject.SetActive(false);
        Debug.Log("Spline Length:" + mLength);
    }

    //get list of children transforms and
    //crerate bezier path from locations
    void updatePoints()
    {
        List<Vector3> knots = new List<Vector3>();
        foreach (Transform node in gameObject.transform)
        {
            Vector3 pos = node.gameObject.transform.position;
            knots.Add(pos);
        }

        if (knots.Count > 1)
        {
            mPath = new CubicBezierPath(knots.ToArray(), Type);
            mLength = mPath.ComputeApproxLength();
        }
    }

    //store distances between 1 unit parameter increments
    //used for moveParam calculaitons
    void calculateLengthList()
    {
        if (mPath == null) return;

        for (float f = 1f; f <= mPath.GetMaxParam(); f++)
        {
            Vector3 p1 = mPath.GetPoint(f - 1f);
            Vector3 p2 = mPath.GetPoint(f);
            mLengthList.Add(Vector3.Distance(p1, p2));
        }
    }

    //get spline param from position
    public float getClosestParam(Vector3 p)
    {
        if (mThreshold == 0f) mThreshold = 0.1f * mPath.ComputeApproxParamPerUnitLength();
        return mPath.ComputeClosestParam(p, mThreshold);
    }

    //try to move on spline and return new param position
    public Vector3 moveParam(ref float p, float distance, bool forward)
    {
        int maxIndex = (int)mPath.GetMaxParam();
        int index = Mathf.FloorToInt(p);
        float sectorLength = mLengthList[index];
        float remainToNext = Mathf.Ceil(p + 0.00001f) - p;
        float remainDist = remainToNext * sectorLength;

        while (distance > 0f)
        {
            //check if can reach target
            if (distance <= remainDist)
            {
                p += (distance / remainDist) * remainToNext;
                break;
            }

            //go on
            distance -= remainDist;
            p += remainToNext;
            
            //go to next point
            index++;
            if (index >= maxIndex)
            {
                if (Type == CubicBezierPath.Type.Open) index = maxIndex - 1;
                else index -= maxIndex;
            }

            sectorLength = mLengthList[index];
            remainToNext = 1f;
            remainDist = remainToNext * sectorLength;
        }

        return mPath.GetPoint(p);
    }

    public Vector3 getPoint(float p)
    {
        return mPath.GetPoint(p);
    }

    public Vector3 getTangent(float p)
    {
        return mPath.GetTangent(p);
    }

    void OnDrawGizmos()
    {
        updatePoints();
        if (mPath == null) return;

        Gizmos.color = Color.yellow;
        float step = mPath.GetMaxParam() / DrawSteps;
        for (float f = step; f <= mPath.GetMaxParam(); f += step)
        {
            Vector3 p1 = mPath.GetPoint(f - step);
            Vector3 p2 = mPath.GetPoint(f);
            Gizmos.DrawLine(p1, p2);
        }
    }
}
