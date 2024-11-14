using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    public enum Preset
    {
        MOUNTAINS,
        CITY,
        TUNNELS,
        SPACE
    }
    public string seed = "";
    public int voxelsInSingleChunk = 64;
    public float voxelSize = 1;
    public Noise noise;


    public float noiseScale = 0.01f;
    public float strength = 0.25f;
    public float baseRoughness = 1;
    public float roughness = 2;
    public float persistance = 0.5f;
    public float minValue = 0.25f;
    public int layers = 2;


    GameObject level;
    

    void OnValidate()
    {
        level = transform.Find("Level").gameObject;
        noise = new Noise(seed.GetHashCode());
        level.GetComponent<Level>().Reload();     
    }

    public float Evaluate2D(Vector3 point)
    {
        point = new Vector3(point.x, 0, point.z) * noiseScale;
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;


        for (int i = 0; i < layers; i++)
        {
            float v = noise.Evaluate(point * frequency);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= roughness;
            amplitude *= persistance;
        }

        noiseValue = Mathf.Max(minValue, noiseValue - minValue);
        return noiseValue * strength;
    }

    public float Evaluate3D(Vector3 point)
    {
        point = point * noiseScale;
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;


        for (int i = 0; i < layers; i++)
        {
            float v = noise.Evaluate(point * frequency);
            noiseValue += (v + 1) * 0.5f * amplitude;
            frequency *= roughness;
            amplitude *= persistance;
        }

        noiseValue = Mathf.Max(minValue, noiseValue - minValue);
        return noiseValue * strength;
    }

    public Vector3Int PositionToIndex3D(Vector3 pos)
    {
        float x = pos.x / voxelSize / voxelsInSingleChunk;
        float y = pos.y / voxelSize / voxelsInSingleChunk;
        float z = pos.z / voxelSize / voxelsInSingleChunk;

        x = Mathf.Abs(ConvertRange(0, 1, 0, voxelsInSingleChunk, x));
        y = Mathf.Abs(ConvertRange(0, 1, 0, voxelsInSingleChunk, y));
        z = Mathf.Abs(ConvertRange(0, 1, 0, voxelsInSingleChunk, z));

        return new Vector3Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
    }

    public Vector3Int IndexToIndex3D(int index)
    {
        int x = index % voxelsInSingleChunk;
        int z = (index / voxelsInSingleChunk) % voxelsInSingleChunk;
        int y = ((index / voxelsInSingleChunk) / voxelsInSingleChunk) % voxelsInSingleChunk;

        return new Vector3Int(x, y, z);
    }

    public int Index3DToIndex(Vector3Int pos)
    {
        return pos.x + (pos.z * voxelsInSingleChunk) + (pos.y * (voxelsInSingleChunk * voxelsInSingleChunk));
    }

    public float ConvertRange(float originalStart, float originalEnd, float newStart, float newEnd, float value)
    {
        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return (newStart + ((value - originalStart) * scale));
    }
}
