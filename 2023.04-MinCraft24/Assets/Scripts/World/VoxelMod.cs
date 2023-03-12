using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMod 
{
    public Vector3 pos;
    public byte id;

    public VoxelMod()
    {
        pos = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _pos, byte _id)
    {
        pos = _pos;
        id = _id;
    }
}
