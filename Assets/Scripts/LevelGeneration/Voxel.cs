using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Voxel
{
    public float value;
    public Vector3 position;

    public static int GetState(Voxel[] points,float isoLevel)
    {
        int state = 0;
        if (points[0].value > isoLevel) state |= 1;
        if (points[1].value > isoLevel) state |= 2;
        if (points[2].value > isoLevel) state |= 4;
        if (points[3].value > isoLevel) state |= 8;
        if (points[4].value > isoLevel) state |= 16;
        if (points[5].value > isoLevel) state |= 32;
        if (points[6].value > isoLevel) state |= 64;
        if (points[7].value > isoLevel) state |= 128;
        return state;
    }

    public static Vector3 LerpPoint(Voxel point1, Voxel point2, float isoLevel) 
    {
        float t = (isoLevel - point1.value) / (point2.value - point1.value);
        return Vector3.Lerp(point1.position, point2.position, t);
    }

    public static Vector3 MidPoint(Voxel point1, Voxel point2)
    {
        return (point1.position + point2.position) / 2;
    }

}

