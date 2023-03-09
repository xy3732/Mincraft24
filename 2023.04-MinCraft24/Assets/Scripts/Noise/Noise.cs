using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    //���� �޸� ������ ����
    public static float Get2DPerlin(Vector2 pos, float offset, float scale)
    {
        return Mathf.PerlinNoise
            (
            (pos.x + 0.1f) / VoxelData.ChunkWidth * scale + offset,
            (pos.y + 0.1f) / VoxelData.ChunkWidth * scale + offset
            );
    }

    //
    public static bool Get3DPerlin(Vector3 pos, float offset, float sclae, float threshold)
    {
        float x = (pos.x + offset + 0.1f) * sclae;
        float y = (pos.y + offset + 0.1f) * sclae;
        float z = (pos.z + offset + 0.1f) * sclae;

        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        if ((AB + BC + AC + BA + CB + CA) / 6f > threshold) return true;
        else return false;
    }
}
