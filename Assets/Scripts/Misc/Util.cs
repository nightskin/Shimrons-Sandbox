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

}
