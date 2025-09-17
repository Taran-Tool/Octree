using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Octree для управления чанками.
/// </summary>
public class Octree<Type>
{
    private Node<Type> root;
    private GameObject parentObject;
    private int octreeSize;
    private int leafSize;
    private Vector3Int octreeBasePoint;
    private Vector3Int center;
    private Dictionary<string, GameObject> chunks;
    private Dictionary<Vector3, GameObject> voxelsDict;
    private List<Vector3> voxels;

    public Octree(Vector3Int basePoint, GameObject parent, int oSize, int lSize)
    {
        octreeBasePoint = basePoint;
        parentObject = parent;
        octreeSize = oSize;
        leafSize = lSize;
        center = new Vector3Int(octreeSize / 2, octreeSize / 2, octreeSize / 2);
        root = new Node<Type>(octreeSize, octreeBasePoint, center);
        if (typeof(Type) == typeof(Octree<Chunk>))
        {
            chunks = new Dictionary<string, GameObject>();
        }
        if (typeof(Type) == typeof(Octree<Voxel>))
        {
            voxelsDict = new Dictionary<Vector3, GameObject>();
            voxels = new List<Vector3>();
        }
    }

    private class Node<Type>
    {
        public Node<Type>[] subNodes;
        public Node<Type> parent;
        public int nSize;
        public Vector3Int nBasePoint;
        public Vector3 nCenter;
        public bool isLeaf;
        public VoxelData voxelData;
        public Vector3Int coordsOffsets = Vector3Int.zero;
        public GameObject leafObject;
        public Node(int size, Vector3Int basePoint, Vector3 center)
        {
            nSize = size;
            nBasePoint = basePoint;
            nCenter = center;
            isLeaf = false;
            
        }
    }

    public void AddChunk(List<VoxelData> voxelsData)
    {
         foreach (VoxelData voxel in voxelsData)
         {
             BuildChunkOrVoxelByIndex(root, voxel.chunkIndex, voxel, 0);
         }
    }

    public void AddVoxel(VoxelData data)
    {
       BuildChunkOrVoxelByIndex(root, data.chunkIndex.Skip(1).ToArray(), data, 0);

    }

    public void InsertVoxels(List<VoxelData> voxels)
    {
        //определяю индекс octree узла в который вставится воксель

        foreach (VoxelData voxel in voxels)
        {
            BuildChunkOrVoxelByIndex(root, voxel.chunkIndex.Skip(1).ToArray(), voxel, 0);
        }
    }

    private void BuildChunkOrVoxelByIndex(Node<Type> node, int[] index, VoxelData data, int depth)
    {
        // Если достигли листового узла
        if (depth == 3 && this.GetType() == typeof(Octree<Chunk>))
        {
            node.isLeaf = true;
            node.coordsOffsets = Vector3Int.zero;

            Debug.Log("you creater chunk" + depth); //#123
            //проверяю chunks
            string chunkName = data.chunkIndex[0] + "" + data.chunkIndex[1] + "" + data.chunkIndex[2];
            //если в словаре нет чанка с названием chunkName, то создаю...
            //вместо int.Parse(chunkName) нужно вставить string в Chunk
            InstanceDescr chunkDesr = new InstanceDescr(int.Parse(chunkName), chunkName);
            GameObject chunkGO = new GameObject(chunkName);
            Chunk chunk = chunkGO.AddComponent<Chunk>();
            chunk.Initialize(chunkDesr, node.nBasePoint, parentObject);
            //   Chunk chunk = new Chunk(parentObject, chunkDesr, node.nBasePoint);
            //добавляю воксели
            chunk.AddVoxel(data);
            // chunk.GetComponent<Chunk>().InsertVoxel(data);
            //иначе - просто добавляю воксели в чанк с названием chunkName
            DrawBounds(node.nBasePoint, node.nSize, depth);
            return;
        }
        else if (depth == index.Length && this.GetType() == typeof(Octree<Voxel>))
        {

            Debug.Log("you created voxel");

            node.leafObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            node.voxelData = data;
            node.leafObject.gameObject.AddComponent<VoxelDataEntity>();
            node.leafObject.gameObject.GetComponent<VoxelDataEntity>().ApplyData(data);
            node.leafObject.transform.parent = parentObject.transform;
            node.leafObject.transform.position = node.nBasePoint;
            
            Debug.Log(node.leafObject.transform.localPosition);

            string octalIndex = System.Convert.ToString(GetIndexFromArray(index), 8);
            if (octalIndex.Length == 1)
            {
                octalIndex = "0" + octalIndex;
            }
            node.leafObject.name = "Voxel: " + octalIndex;
            DrawBounds(node.nBasePoint, node.nSize, depth);
            return;
        }


            // Если узел еще не разделен, разделяем его на подузлы
            if (node.subNodes == null)
        {
            int subNodeSize = node.nSize / 2;
            node.subNodes = new Node<Type>[8];
            Vector3Int[] subNodesOffsets = new Vector3Int[]
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(subNodeSize , 0, 0),
                new Vector3Int(0, 0, subNodeSize),
                new Vector3Int(subNodeSize, 0, subNodeSize),

                new Vector3Int(0, subNodeSize, 0),
                new Vector3Int(subNodeSize, subNodeSize, 0),
                new Vector3Int(0, subNodeSize, subNodeSize),
                new Vector3Int(subNodeSize, subNodeSize, subNodeSize)
            };

            for (int i = 0; i < node.subNodes.Length; i++)
            {
                Vector3Int subNodeBasePoint = node.nBasePoint + subNodesOffsets[i];
                Vector3 subNodeCenter = subNodeBasePoint + new Vector3(subNodeSize, subNodeSize, subNodeSize) * 0.5f;

                    node.subNodes[i] = new Node<Type>(subNodeSize, subNodeBasePoint, subNodeCenter) { parent = node };
            }
            node.isLeaf = false;
        }
        // Переходим к следующему уровню глубины
        BuildChunkOrVoxelByIndex(node.subNodes[index[depth]], index, data, depth + 1);
        DrawBounds(node.nBasePoint, node.nSize, depth);
    }

    private int GetIndexFromArray(int[] index)
    {
        int result = 0;
        for (int i = 0; i < index.Length; i++)
        {
            result |= index[i] << (3 * i);
        }
        return result;
    }




    public void RemoveVoxel(Vector3Int voxelPosition)
    {
        RemoveVoxelByCoord(voxelPosition);
    }

    public void UpdateVoxel(Vector3Int voxelPosition, VoxelData data)
    {
        UpdateVoxelByCoord(voxelPosition, data);
    }

    private void UpdateVoxelByCoord(Vector3Int voxelPosition, VoxelData data)
    {
        int[] index = GetIndexesArrayFromCoords(voxelPosition, 32, 2);
        Node<Type> current = GetNodeByIndex(index);
        //...воксель узла
        current.voxelData = data;
        //...gameobject
        current.leafObject.gameObject.GetComponent<VoxelDataEntity>().ApplyData(data);
        //...индекс в octree
        if (voxelsDict.ContainsKey(voxelPosition))
        {
            voxelsDict[voxelPosition].gameObject.GetComponent<VoxelDataEntity>().ApplyData(data);
        }
    }

    private void RemoveVoxelByCoord(Vector3Int voxelPosition)
    {
        //Очистка:          
        int[] index = GetIndexesArrayFromCoords(voxelPosition, 32, 2);
        Node<Type> current = GetNodeByIndex(index);
        //...gameobject
        GameObject.Destroy(current.leafObject.gameObject);
        //...воксель узла
        current.leafObject = null;
        //...индекс в octree
        if (voxelsDict.ContainsKey(voxelPosition))
        {
            voxelsDict.Remove(voxelPosition);
        }
        //...пустые узлы. Если в родительском узле все дочерние узлы не имеют вокселей и своих дочерних узлов - удаляю дочерние узлы родительского узла.
        RemoveEmptyNodes(current);
    }

    private void RemoveEmptyNodes(Node<Type> node)
    {
        //если у узла нет родителя (он является root), выхожу
        if (node == null)
        {
            return;
        }
        //...если у узла нет дочерних узлов
        bool hasSubNodes = false;
        if (node.subNodes != null)
        {
            for (int i = 0; i < node.subNodes.Length; i++)
            {
                if (node.subNodes[i] != null)
                {
                    hasSubNodes = true;
                    break;
                }
            }
        }
        //...если у узла нет вокселя
        bool hasVoxels = node.leafObject != null;
        //если выполняются оба условия, то...
        if (!hasSubNodes && !hasVoxels)
        {
            if (node.parent != null)
            {
                //если у родительского узла все дочерние узлы не имеют вокселей или своих дочерних узлов...
                bool anySubnodeHasData = false;
                for (int i = 0; i < node.parent.subNodes.Length; i++)
                {
                    if (node.parent.subNodes[i] != null &&
                        (node.parent.subNodes[i].subNodes != null || node.parent.subNodes[i].leafObject != null))
                    {
                        anySubnodeHasData = true;
                        break;
                    }
                }
                //очищаю массив дочерних узлов додительского узла
                if (!anySubnodeHasData)
                {
                    node.parent.subNodes = null;
                }
            }
            //иду вверх от листа к корню
            RemoveEmptyNodes(node.parent);
        }
    }

    private Node<Type> GetNodeByIndex(int[] index)
    {
        Node<Type> current = root;
        foreach (var i in index)
        {
            current = current.subNodes[i];
        }
        return current;
    }


    private void InsertRecursive(Node<Type> node, Vector3Int voxelPosiiton, VoxelData data, int size, int depth, Vector3 offset, ushort curIndex)
    {

        //если размер узла равен leafSize, то считаю что он "лист" и выхожу из рекурсии
        if (size == leafSize)
        {
            // Достигнут листовой узел
            node.isLeaf = true;
            node.coordsOffsets = Vector3Int.zero;
            //создаю объект
            if (node is Node<Chunk>)
            {
                string octalIndex = System.Convert.ToString(curIndex, 8);
                if (octalIndex.Length < 3)
                {
                    octalIndex = "0" + octalIndex;
                }
                //создаю чанк                    
                if (this.voxelsDict.Values.Any(obj => obj.name == octalIndex))
                {
                    //если в словаре есть чанк (GmaObject) с таким индексом, то прошу его создать воксель с обновленными координатами
                    Debug.Log("Чанк с индексом: " + octalIndex + " ранее был создан. Создаю в нем воксель с координатами: " + voxelPosiiton + "/32");
                }
                else
                {
                    //иначе, создаю чанк, добавляю его в словарь, прошу создать воксель с обновленными координатами
                    Debug.Log("Чанк с индексом: " + octalIndex + " еще не был создан. Создаю чанк. Записываю его данные в словарь. Создаю в нем воксель с координатами: " + voxelPosiiton + "/32");
                    //GameObject chuncGO = new GameObject(octalIndex) { transform = {parent = parentObject.gameObject.transform} };
                    //считаю номер чанка
                    InstanceDescr chunkDesr = new InstanceDescr(int.Parse(octalIndex), octalIndex);
                }


                //сохраняю чанк
                //создаю octree в chunk
                //создаю воксели
            }
            else
            {
                node.leafObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                node.voxelData = data;
                node.leafObject.gameObject.AddComponent<VoxelDataEntity>();
                node.leafObject.gameObject.GetComponent<VoxelDataEntity>().ApplyData(data);
                node.leafObject.transform.position = voxelPosiiton;
                node.leafObject.transform.parent = parentObject.transform;
                string octalIndex = System.Convert.ToString(curIndex, 8);
                if (octalIndex.Length == 1)
                {
                    octalIndex = "0" + octalIndex;
                }
                node.leafObject.name = octalIndex;

                //добавляю индекс вокселя
                voxelsDict.Add(voxelPosiiton, node.leafObject);
            }

            return;
        }

        int subNodeSize = size / 2;
        if (node.subNodes == null)
        {
            // Если узел еще не разделен, разделяем его на подузлы
            node.subNodes = new Node<Type>[8];
            Vector3Int[] subNodesOffsets = new Vector3Int[]
            {
                new Vector3Int(0, 0, 0),
                new Vector3Int(subNodeSize , 0, 0),
                new Vector3Int(0, 0, subNodeSize),
                new Vector3Int(subNodeSize, 0, subNodeSize),

                new Vector3Int(0, subNodeSize, 0),
                new Vector3Int(subNodeSize, subNodeSize, 0),
                new Vector3Int(0, subNodeSize, subNodeSize),
                new Vector3Int(subNodeSize, subNodeSize, subNodeSize)
            };

            for (int i = 0; i < node.subNodes.Length; i++)
            {
                Vector3Int subNodeBasePoint = node.nBasePoint + subNodesOffsets[i];
                Vector3 subNodeCenter = subNodeBasePoint + new Vector3(subNodeSize, subNodeSize, subNodeSize) * 0.5f;

                node.subNodes[i] = new Node<Type>(subNodeSize, subNodeBasePoint, subNodeCenter) { parent = node };
            }
            node.isLeaf = false;
        }
        //определяею индекс подузла для вставки
        PositionParameters subPosition = GetPositionParameters(voxelPosiiton, size, offset);
        ushort newIndex = (ushort) (curIndex << 3 | subPosition.index);
        //рекурсивно вставляю воксель в соответствующий подузел 
        InsertRecursive(node.subNodes[subPosition.index], voxelPosiiton, data, size / 2, depth + 1, subPosition.offset, newIndex);

        //DrawBounds(node.subNodes[subPosition.index], depth + 1);
    }

    private PositionParameters GetPositionParameters(Vector3Int voxelPositionInt, int nodeSize, Vector3 offset)
    {
        PositionParameters posParams;
        int index = 0;
        // верх куба
        // | 6 | 7 |
        // | 4 | 5 |
        // низ куба
        // | 2 | 3 |
        // | 0 | 1 |

        float middle = 0.5f * nodeSize;

        //X | 1
        if (voxelPositionInt.x == nodeSize)
        {
            index |= 1;
        }

        if (voxelPositionInt.x < nodeSize && voxelPositionInt.x > middle)
        {
            index |= 1;
        }

        if (voxelPositionInt.x > nodeSize)
        {
            if (voxelPositionInt.x - offset.x > nodeSize)
            {
                offset.x += nodeSize;
                if (voxelPositionInt.x - offset.x < nodeSize && voxelPositionInt.x - offset.x > middle)
                {
                    index |= 1;
                }
            }
            if (voxelPositionInt.x - offset.x < nodeSize)
            {
                if (voxelPositionInt.x - offset.x > middle)
                {
                    index |= 1;
                }
            }
            if (voxelPositionInt.x - offset.x == nodeSize)
            {
                index |= 1;
            }
        }

        //Y | 4
        if (voxelPositionInt.y == nodeSize)
        {
            index |= 4;
        }

        if (voxelPositionInt.y < nodeSize && voxelPositionInt.y > middle)
        {
            index |= 4;
        }

        if (voxelPositionInt.y > nodeSize)
        {
            if (voxelPositionInt.y - offset.y > nodeSize)
            {
                offset.y += nodeSize;
                if (voxelPositionInt.y - offset.y < nodeSize && voxelPositionInt.y - offset.y > middle)
                {
                    index |= 4;
                }
            }
            if (voxelPositionInt.y - offset.y < nodeSize)
            {
                if (voxelPositionInt.y - offset.y > middle)
                {
                    index |= 4;
                }
            }
            if (voxelPositionInt.y - offset.y == nodeSize)
            {
                index |= 4;
            }
        }

        //Z | 2
        if (voxelPositionInt.z == nodeSize)
        {
            index |= 2;
        }

        if (voxelPositionInt.z < nodeSize && voxelPositionInt.z > middle)
        {
            index |= 2;
        }

        if (voxelPositionInt.z > nodeSize)
        {
            if (voxelPositionInt.z - offset.z > nodeSize)
            {
                offset.z += nodeSize;
                if (voxelPositionInt.z - offset.z < nodeSize && voxelPositionInt.z - offset.z > middle)
                {
                    index |= 2;
                }
            }
            if (voxelPositionInt.z - offset.z < nodeSize)
            {
                if (voxelPositionInt.z - offset.z > middle)
                {
                    index |= 2;
                }
            }
            if (voxelPositionInt.z - offset.z == nodeSize)
            {
                index |= 2;
            }
        }
        posParams.index = index;
        posParams.offset = offset;
        return posParams;
    }

    private int[] GetIndexesArrayFromCoords(Vector3Int point, int nodeMaxSize, int nodeSecondToLastSize)
    {
        int[] size = createArray(nodeMaxSize, nodeSecondToLastSize);
        int[] indexes = new int[size.Length];
        Vector3 offset = Vector3.zero;
        for (int i = 0; i < size.Length; i++)
        {
            PositionParameters posParameters = GetPositionParameters(point, size[i], offset);
            offset = posParameters.offset;
            indexes[i] = posParameters.index;
        }
        return indexes;
    }

    public static int[] createArray(int startValue, int endValue)
    {
        // Рассчитываю размер массива
        int size = (int) (Math.Log(startValue / endValue) / Mathf.Log(2)) + 1;
        int[] array = new int[size];
        //Заполняю массив
        for (int i = 0; i < size; i++)
        {
            array[i] = startValue;
            startValue /= 2;
        }
        return array;
    }

    public void DrawBounds(Vector3Int basePoint, int size, int depth)
    {
        Vector3 half = new Vector3(0.5f, 0.5f, 0.5f);
        Color color = Color.red;

        switch (depth)
        {
            case 5:
            color = Color.cyan;
            break;
            case 4:
            color = Color.magenta;
            break;
            case 3:
            color = Color.yellow;
            break;
            case 2:
            color = Color.blue;
            break;
            case 1:
            color = Color.green;
            break;
        }

        Vector3 p1 = new Vector3Int(0, 0, 0) + basePoint - half;
        Vector3 p2 = new Vector3Int(0, 0,  size) + basePoint - half;
        Vector3 p3 = new Vector3Int(size, 0,  size) + basePoint - half;
        Vector3 p4 = new Vector3Int(size, 0, 0) + basePoint - half;
        Vector3 p5 = new Vector3Int(0, size, 0) + basePoint - half;
        Vector3 p6 = new Vector3Int(0, size, size) + basePoint - half;
        Vector3 p7 = new Vector3Int(size, size, size) + basePoint - half;
        Vector3 p8 = new Vector3Int(size, size, 0) + basePoint - half;

        Debug.DrawLine(p1, p2, color, 30f);
        Debug.DrawLine(p2, p3, color, 30f);
        Debug.DrawLine(p3, p4, color, 30f);
        Debug.DrawLine(p4, p1, color, 30f);
        Debug.DrawLine(p5, p6, color, 30f);
        Debug.DrawLine(p6, p7, color, 30f);
        Debug.DrawLine(p7, p8, color, 30f);
        Debug.DrawLine(p8, p5, color, 30f);
        Debug.DrawLine(p1, p5, color, 30f);
        Debug.DrawLine(p2, p6, color, 30f);
        Debug.DrawLine(p3, p7, color, 30f);
        Debug.DrawLine(p4, p8, color, 30f);
    }


    private struct PositionParameters
    {
        public int index;
        public Vector3 offset;
    }
}

public enum SectorIndex
{
    LowerLeftFront = 0,
    LowerRightFront = 1,
    LowerRightBack = 2,
    LowerLeftBack = 3,
    UpperLeftFront = 4,
    UpperRightFront = 5,
    UpperRightBack = 6,
    UpperLeftBack = 7
}

