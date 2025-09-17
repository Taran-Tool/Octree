using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ �������: ������ ������� � octree, ���, ���������.
/// </summary>
public struct VoxelData
{
    public int[] chunkIndex;
    public int type;
    public bool isEmpty => type == 0;
    public int state;

    public VoxelData(int[] _chunkIndex, int _type, int _state)
    {
        chunkIndex = _chunkIndex;
        type = _type; 
        state = _state;
    }

    /// <summary>
    /// ����������� ������ ����� � ������ (��� �������� � ���� ������).
    /// </summary>
    public string GetChunkIndexAsString()
    {
        return $"{chunkIndex[0]:D2}{chunkIndex[1]:D2}{chunkIndex[2]:D2}";
    }

    /// <summary>
    /// ����������� ������ ������� ����� � ������ int.
    /// </summary>
    public static int[] ParseChunkIndexFromString(string indexString)
    {
        if (indexString.Length != 6)
            throw new System.ArgumentException("Chunk index string must be 6 characters long.");

        return new int[]
        {
            int.Parse(indexString.Substring(0, 2)),
            int.Parse(indexString.Substring(2, 2)),
            int.Parse(indexString.Substring(4, 2))
        };
    }
}
