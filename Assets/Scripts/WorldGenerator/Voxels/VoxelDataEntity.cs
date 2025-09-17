using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDataEntity:MonoBehaviour
{
    public void ApplyData(VoxelData data)
    {
        Debug.Log("Applying this voxel data: \n" + "State: " + data.state + "Type: " + data.type);

    }

}