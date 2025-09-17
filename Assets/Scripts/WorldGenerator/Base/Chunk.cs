using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk:MonoBehaviour
{
  //  private GameObject _chunkInstance;
    private Octree<Voxel> _voxelOctree;
    private Vector3Int _chunkBasePointCoords;
   // private GameObject parentLocation;
    private InstanceDescr _chunkDescription;

    public void Initialize (InstanceDescr data, Vector3Int position, GameObject parent)
    {
        Debug.Log("Chunk created: " + data.name + "- (" + data.id + ")" + position);
        _chunkBasePointCoords = position;
        _chunkDescription = data;
        this.gameObject.transform.position = _chunkBasePointCoords;
        this.transform.parent = parent.transform;
        _voxelOctree = new Octree<Voxel>(_chunkBasePointCoords, gameObject, 32, 1);

    }

    public void AddVoxel(VoxelData data)
    {
       _voxelOctree.AddVoxel(data);
    }

    public void InitChunk(Transform parent, Vector3Int coord)
    {

    }

    public GameObject GetChunkGO()
    {
        return gameObject;
    }

    public void AddVoxel(KeyValuePair<Vector3Int, VoxelData> voxel)
    {

    }
}

