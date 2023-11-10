using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Color color = new Color();
    public bool active = false;
    public float height = 0;
    public Vector3 position = new Vector3();
    public int x;
    public int y;
    public int z;

    public Point()
    {

    }
    public Point(Vector3 pos)
    {
        position = pos;
    }
}
