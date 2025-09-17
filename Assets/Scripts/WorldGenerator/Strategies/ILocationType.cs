using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocationType
{
    Location GenerateLocation(int locationNumber, int size, GameObject parent);
}

