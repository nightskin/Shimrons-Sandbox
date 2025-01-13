using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Voxelizer))]
public class VoxelizerBaker : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Voxelizer voxelizer = (Voxelizer)target;

        if (GUILayout.Button("Create Voxel Data"))
        {
            voxelizer.CreateVoxelData();
            voxelizer.CreateMeshData();
        }

    }

}
