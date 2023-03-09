using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData : MonoBehaviour
{
    // �� ûũ�� ũ��
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    //�� ûũ ũ��
    public static readonly int WorldSizeCchunk = 20;

    // �� ������ ���� ��
    public static int WorldSizeVoxels
    {
        get { return WorldSizeCchunk * ChunkWidth; }
    }

    public static readonly int ViewDistanceInChunks = 5;

    //���� �� ��������Ʈ �ȿ� ���� 4��, ���� 4���� ���� ���� �ִ�. 
    public static readonly int TextureAtlasSizeInBlock = 4;
    public static float NormalizedBlockTextureSize
    {
        // 1�� ũ��� �븻������ �����ִ� �۾��̴�.
        get { return 1f / (float)TextureAtlasSizeInBlock; }
    }

    // ���簢�� ������Ʈ�� ���鋚 �������� �� 8�� �̴�. �׷��� �� 8�� �迭�� ������ �ȴ�.
    public static readonly Vector3[] voxelVerts =
        new Vector3[8]
        {
            new Vector3(0.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(1.0f, 0.0f, 1.0f),
            new Vector3(1.0f, 1.0f, 1.0f),
            new Vector3(0.0f, 1.0f, 1.0f),
        };

    // �� ��� �ﰢ���� 2���� �ִ�. �� �ﰢ���� �������� ���ؼ� �� 6���̴�.
    // �׷��Ƿ� �� 6�鿡, ������ ���� 6�� �̹Ƿ� [6,6]�迭�� ���´�.
    public static readonly int[,] voxelTriangles =
        new int[6, 4]
        {
            // 0 1 2 2 1 3
            {0, 3, 1, 2}, //����
            {5, 6, 4, 7}, //����
            {3, 7, 2, 6}, //����
            {1, 5, 0, 4}, //����
            {4, 7, 0, 3}, //����
            {1, 2, 5, 6}  //������
        };

    // �Ѹ鿡 �ִ� �ﰢ�� 2���� uv�� ������.
    public static readonly Vector2[] voxelUvs =
        new Vector2[4]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f)
        };

    // ���� ���� �پ��ִ��� Ȯ���ϱ� ���� vector��.
    public static readonly Vector3[] faceCheck =
        new Vector3[6]
        {
            new Vector3(0.0f, 0.0f, -1.0f),
            new Vector3(0.0f, 0.0f, 1.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, -1.0f, 0.0f),
            new Vector3(-1.0f, 0.0f, 0.0f),
            new Vector3(1.0f, 0.0f, 0.0f)
        };

}
