using System.Collections.Generic;
using UnityEngine;


public class Location:MonoBehaviour
{
    private InstanceDescr _locationDescription;
    private Dictionary<string, GameObject> _locationChunks;
    private Octree<Chunk> _chunksOctree;
    private Vector3Int _locationBasePointCoords;
    //private DBParser parser = new DBParser();

    public enum locationSizes
    {
        tiny = 32,
        small = 64,
        medium = 128,
        big = 256
    }

    public void Initialize (InstanceDescr data, int size, List<VoxelData>voxelsData)
    {       
        _locationDescription = data;
        _locationChunks = new Dictionary<string, GameObject>();
        _locationBasePointCoords = CalculateBasePointCoords();
        //создаю octree с кусками (чанками) размером 32
       _chunksOctree = new Octree<Chunk>(_locationBasePointCoords, gameObject, size, 32);
        //создаю чанки, заполняю их вокселями
        this.CreateChunks(voxelsData);
       // DataBase.instance.AddWorldElement(data.name, "Locations");
    }

    public void CreateChunks(List<VoxelData> voxelsData)
    {
        //octree<Chunk> создает узлы-чанки 32х32х32 исходя из координат voxels
        this._chunksOctree.AddChunk(voxelsData);
        //получаю объекты созданных узлов

        //записываю чанки в БД

        //заполняю

    }
    public GameObject GetLocationInstance()
    {
        
        return gameObject;
    }
    private Vector3Int CalculateBasePointCoords()
    {
        //локация распологается в кубе 256х256х256
        //кубы располагаются по 16 в ряду, по 16 рядов и в 16 уровней
        //нижний, ближний левый угол является базовой точкой
        Vector3Int result = Vector3Int.zero;
        if (WorldGeneratorEngine.Instance.GetCurrentWorld().GetComponent<World>().GetWorldLastLocationNumber() == 0)
        {
            return result;
        }
        else
        {
            int count = WorldGeneratorEngine.Instance.GetCurrentWorld().GetComponent<World>().GetWorldLastLocationNumber();
            int row = 0;
            int level = 0;
            if (count % 16 == 0)
            {
                row++;
                if (row % 16 == 0)
                {
                    level++;
                    row = 0;
                }
            }
            result = new Vector3Int((256 * count), (256 * level), (256 * row));
        }
        return result;
    }









    public void SetupLocation(string locationName)
    {
        /* //чанк - часть локации размером 32х32х32 блоков
            //базовая точка локации - точка кратная 32 в глобальных координатах, является нижней, ближней, левой вершиной куба локации

            //получаю данны чанков локации
            //создаю чанки

            //запрашиваю данные у БД
            DataTable LocationBlocks = DB.DataBase.instance.GetUserLocationBlocksFullData(locationName);
            //формирую временный массив
            KeyValuePair<Vector3Int, VoxelData>[] locationVoxels = new KeyValuePair<Vector3Int, VoxelData>[LocationBlocks.Rows.Count];
            for (int i = 0; i < LocationBlocks.Rows.Count; i++)
            {
                DataRow row = LocationBlocks.Rows[i];
                int x = parser.ParseBlobToInt(row, "x", 1);
                int y = parser.ParseBlobToInt(row, "y", 1);
                int z = parser.ParseBlobToInt(row, "z", 1);

                int material = parser.ParseBlobToInt(row, "data", 1);
                int type = parser.ParseBlobToInt(row, "data", 2);

                locationVoxels[i] = new KeyValuePair<Vector3Int, VoxelData>(new Vector3Int(_locationBasePointCoords.x + x,
                                                                                        _locationBasePointCoords.y + y, 
                                                                                        _locationBasePointCoords.z + z), 
                                                                                        new VoxelData(material, type));
            }
            //создаю чанки
            CreateLocationChunks(locationVoxels);*/
    }
    void CreateLocationChunks(KeyValuePair<Vector3Int, VoxelData>[] locationVoxels)
    {
        //каждый чанк - узел, входящий в иерархию octree локации
        //каждый чанк содержит octree, который в свою очередь содержит данные о принадлежащих ему вокселях
        //все манипуляции с вокселями происходят через octree
        foreach (KeyValuePair<Vector3Int, VoxelData> voxel in locationVoxels)
        {
            /*  Vector3Int chunkPosition = GetChunkPosition(voxel.Key);

                if (!_locationInstance.GetComponent<Location>()._locationChunks.ContainsKey(chunkPosition))
                {
                    //создаю чанк
                    GameObject chunk = new GameObject(chunkPosition.ToString());
                    chunk.AddComponent<Chunk>();
                    chunk.GetComponent<Chunk>().InitChunk(_locationInstance.transform, chunkPosition);
                    //добавляю воксель
                    chunk.GetComponent<Chunk>().AddVoxel(voxel);
                    //сохраняю чанк
                    _locationInstance.GetComponent<Location>()._locationChunks.Add(chunkPosition, chunk.GetComponent<Chunk>().GetChunkGO());
                }*/
        }
    }

    Vector3Int GetChunkPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / WorldGeneratorEngine.chunksSize);
        int y = Mathf.FloorToInt(position.y / WorldGeneratorEngine.chunksSize);
        int z = Mathf.FloorToInt(position.z / WorldGeneratorEngine.chunksSize);

        return new Vector3Int(x, y, z);
    }

    public struct LocationData
    {
        public int id
        {
            get; set;
        }
        public string name
        {
            get; set;
        }
        public float size
        {
            get; set;
        }
    }
}
