using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* ВОКСЕЛЬ.
 * Данные о вершинах, гранях и сторонах вокселя.
 */

public class Voxel
{
    public static readonly Vector3[] voxelVertices = new Vector3[8]
    {
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3( 0.5f, -0.5f, -0.5f),
        new Vector3( 0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f,  0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f,  0.5f),
        new Vector3( 0.5f, -0.5f,  0.5f),
        new Vector3( 0.5f,  0.5f,  0.5f),
        new Vector3(-0.5f,  0.5f,  0.5f)
    };

    public static readonly int[,] voxelTriangles = new int[6, 6]
    {
        {3,7,2,2,7,6}, //Верх
        {0,3,1,1,3,2}, //Зад
        {5,6,4,4,6,7}, //Перед
        {4,7,0,0,7,3}, //Левая
        {1,2,5,5,2,6}, //Правая
        {1,5,0,0,5,4}  //Низ
    };

    public static readonly Vector3[] faces = new Vector3[6]
    {
        new Vector3( 0.0f,  1.0f,  0.0f), //Верх
        new Vector3( 0.0f,  0.0f, -1.0f), //Зад
        new Vector3( 0.0f,  0.0f,  1.0f), //Перед
        new Vector3(-1.0f,  0.0f,  0.0f), //Левая
        new Vector3( 1.0f,  0.0f,  0.0f), //Правая
        new Vector3( 0.0f, -1.0f,  0.0f)  //Низ
    };

    public static readonly Vector2[] voxelUVs = new Vector2[6]
    {
        new Vector2(0f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 0f),
        new Vector2(1f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 1f)
    };

    public enum Direction
    {
        Up,
        Back,
        Forward,
        Left,
        Right,
        Down
    }

    public struct DataCoordinate
    {
        public int x;
        public int y;
        public int z;

        public DataCoordinate(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public static readonly DataCoordinate[] offsets =
    {
        new DataCoordinate ( 0,  1,  0),
        new DataCoordinate ( 0,  0, -1),
        new DataCoordinate ( 0,  0,  1),
        new DataCoordinate (-1,  0,  0),
        new DataCoordinate ( 1,  0,  0),
        new DataCoordinate ( 0, -1,  0)
    };

    public struct VoxelStruct
    {
        GameObject gObject;
        VoxelData bData;

        public VoxelStruct(GameObject go, VoxelData bo)
        {
            this.gObject = go;
            this.bData = bo;
        }

        public void SetGO(GameObject newGO)
        {
            this.gObject = newGO;
        }

        public VoxelData GetBlockData()
        {
            return this.bData;
        }
    }
}

