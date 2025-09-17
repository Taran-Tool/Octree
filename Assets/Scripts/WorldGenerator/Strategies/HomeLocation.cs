using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeLocation:ILocationType
{
    public Location GenerateLocation(int locationNumber, int size, GameObject parent)
    {
        GameObject locationGo = new GameObject("Home") { transform = { parent = parent.transform } };

        // описание локации
        InstanceDescr descr = new InstanceDescr(locationNumber, "Home");
        // генерация вокселей
        List<VoxelData> voxels = GenerateBaseStructure(size);

        Location location = locationGo.AddComponent<Location>();
        location.Initialize(descr, size, voxels);

        return location;
    }

    /// <summary>
    /// Генерация базовой структуры.
    /// </summary>
    private List<VoxelData> GenerateBaseStructure(int size)
    {
        List<VoxelData> voxels = new List<VoxelData>();

        voxels.Add(new VoxelData(new int[] { 0, 0, 0, 0, 0, 0}, 1, 1));
        voxels.Add(new VoxelData(new int[] { 1, 1, 1, 1, 1, 1}, 1, 1));

        voxels.Add(new VoxelData(new int[] { 2, 2, 2, 2, 2, 2}, 1, 1));
        voxels.Add(new VoxelData(new int[] { 3, 3, 3, 3, 3, 3}, 1, 1));
        voxels.Add(new VoxelData(new int[] { 4, 4, 4, 4, 4, 4}, 1, 1));
        voxels.Add(new VoxelData(new int[] { 5, 5, 5, 5, 5, 5}, 1, 1));

        voxels.Add(new VoxelData(new int[] { 6, 6, 6, 6, 6, 6}, 1, 1));
        voxels.Add(new VoxelData(new int[] { 7, 7, 7, 7, 7, 7}, 1, 1));


        return voxels;
    }
}
