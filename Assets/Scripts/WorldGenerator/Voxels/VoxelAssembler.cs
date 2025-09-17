using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelAssembler : MonoBehaviour
{
   /* public void CreateVoxel(Vector3Int position, VoxelData data, string name)
    {
        GameObject voxel = new GameObject(name);
        voxel.transform.position = position;

        voxel.AddComponent<MeshFilter>();
        voxel.AddComponent<MeshRenderer>();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        int vertexIndex = 0;

        Mesh mesh = new Mesh();
        mesh.name = name;
        MeshFilter meshFilter = voxel.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = voxel.GetComponent<MeshRenderer>();

        Material material = Resources.Load("Materials/" + data.material.ToString(), typeof(Material)) as Material;
        meshRenderer.material = material;

        for (int j = 0; j < 6; j++)
        {
            //провер€ю соседние воксели
            if (!CheckBlockNeighbor(position, j))
            {
                for (int k = 0; k < 6; k++)
                {
                    int triangleIndex = Voxel.voxelTriangles[j, k];

                    vertices.Add((Voxel.voxelVertices[triangleIndex]));
                    triangles.Add(vertexIndex);
                    uvs.Add(Voxel.voxelUVs[k]);
                    vertexIndex++;
                }
            }
        }
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private bool CheckBlockNeighbor(Vector3 position, int direction)
    {
        Voxel.DataCoordinate offsetToCheck = Voxel.offsets[direction];
        Voxel.DataCoordinate neighborCoord = new Voxel.DataCoordinate((int) position.x + offsetToCheck.x, (int) position.y + offsetToCheck.y, (int) position.z + offsetToCheck.z);
        return GetBlock(new Vector3Int(neighborCoord.x, neighborCoord.y, neighborCoord.z));
    }
   */
   /* private bool GetBlock(Vector3Int coord)
    {
       
    }*/
}
