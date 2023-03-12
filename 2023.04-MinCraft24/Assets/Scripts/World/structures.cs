using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class structures
{
    public static Queue<VoxelMod> MakeTree(Vector3 _pos, int _minTrunkHeight, int _maxTrunkHeight)
    {
        Queue<VoxelMod> _queue = new Queue<VoxelMod>();

        int height = (int)(_maxTrunkHeight * Noise.Get2DPerlin(new Vector2(_pos.x, _pos.z), 250f, 3f));

        if(height < _minTrunkHeight)
        {
            height = _minTrunkHeight;
        }    

        for(int i = 1; i<height; i++)
        {
            _queue.Enqueue(new VoxelMod(new Vector3(_pos.x, _pos.y + i, _pos.z),6));
        }

        for (int x = -2; x < 3; x++)
        {
            for (int z = -2; z < 3; z++)
            {
                _queue.Enqueue(new VoxelMod(new Vector3(_pos.x + x, _pos.y + height - 2, _pos.z + z), 11));
                _queue.Enqueue(new VoxelMod(new Vector3(_pos.x + x, _pos.y + height - 3, _pos.z + z), 11));
            }
        }

        for (int x = -1; x < 2; x++)
        {
            for (int z = -1; z < 2; z++)
            {
                _queue.Enqueue(new VoxelMod(new Vector3(_pos.x + x, _pos.y + height - 1, _pos.z + z), 11));
            }
        }
        for (int x = -1; x < 2; x++)
        {
            if (x == 0)
                for (int z = -1; z < 2; z++)
                {
                    _queue.Enqueue(new VoxelMod(new Vector3(_pos.x + x, _pos.y + height, _pos.z + z), 11));
                }
            else
                _queue.Enqueue(new VoxelMod(new Vector3(_pos.x + x, _pos.y + height, _pos.z), 11));
        }

        return _queue;
    }
}
