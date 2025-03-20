using UnityEngine;

public class Util
{
    public static float ConvertRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
    {
        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return (newStart + ((value - originalStart) * scale));
    }

    public static int Index3dToIndex(Vector3Int i3d, int resolution)
    {
        return i3d.x + (i3d.z * resolution) + (i3d.y * (resolution * resolution));
    }

    public static Vector3Int IndexToIndex3D(int i, int resolution)
    {
        int x = i % resolution;
        int z = (i / resolution) % resolution;
        int y = ((i / resolution) / resolution) % resolution;
        return new Vector3Int(x, y, z);
    }

    public static Color RandomColor(bool includeAlpha = false)
    {
        float r = Random.value;
        float g = Random.value;
        float b = Random.value;
        float a = 1;

        if(includeAlpha)
        {
            a = Random.value;
        }

        return new Color(r, g, b, a);
    }


    public static int GetState(Voxel[] points, float isoLevel)
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

    public static Vector3 GetMidPoint(Voxel point1, Voxel point2)
    {
        return (point1.position + point2.position) / 2;
    }

    public static Vector3 LerpPoint(Voxel point1, Voxel point2, float isoLevel)
    {
        float t = (isoLevel - point1.value) / (point2.value - point1.value);
        return Vector3.Lerp(point1.position, point2.position, t);
    }

    public static Vector2[] GetUVs(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 s1 = b - a;
        Vector3 s2 = c - a;
        Vector3 norm = Vector3.Cross(s1, s2).normalized; // the normal

        norm.x = Mathf.Abs(norm.x);
        norm.y = Mathf.Abs(norm.y);
        norm.z = Mathf.Abs(norm.z);

        Vector2[] uvs = new Vector2[3];
        if (norm.x >= norm.z && norm.x >= norm.y) // x plane
        {
            uvs[0] = new Vector2(a.z, a.y);
            uvs[1] = new Vector2(b.z, b.y);
            uvs[2] = new Vector2(c.z, c.y);
        }
        else if (norm.z >= norm.x && norm.z >= norm.y) // z plane
        {
            uvs[0] = new Vector2(a.x, a.y);
            uvs[1] = new Vector2(b.x, b.y);
            uvs[2] = new Vector2(c.x, c.y);
        }
        else if (norm.y >= norm.x && norm.y >= norm.z) // y plane
        {
            uvs[0] = new Vector2(a.x, a.z);
            uvs[1] = new Vector2(b.x, b.z);
            uvs[2] = new Vector2(c.x, c.z);
        }

        return uvs;
    }
}
