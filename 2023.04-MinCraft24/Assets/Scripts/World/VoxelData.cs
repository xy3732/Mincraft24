using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelData : MonoBehaviour
{
    // 총 청크의 크기
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    //총 청크 크기
    public static readonly int WorldSizeCchunk = 20;

    // 한 길이의 블럭의 수
    public static int WorldSizeVoxels
    {
        get { return WorldSizeCchunk * ChunkWidth; }
    }

    public static readonly int ViewDistanceInChunks = 5;

    //현재 블럭 스프라이트 안에 가로 4개, 세로 4개의 블럭을 볼수 있다. 
    public static readonly int TextureAtlasSizeInBlock = 4;
    public static float NormalizedBlockTextureSize
    {
        // 1의 크기로 노말라이즈 시켜주는 작업이다.
        get { return 1f / (float)TextureAtlasSizeInBlock; }
    }

    // 정사각형 오브젝트를 만들떄 꼭짓점이 총 8개 이다. 그래서 총 8의 배열을 가지게 된다.
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

    // 한 면당 삼각형이 2개씩 있다. 그 삼각형의 꼭짓점은 합해서 총 6개이다.
    // 그러므로 총 6면에, 꼭짓점 갯수 6개 이므로 [6,6]배열이 나온다.
    public static readonly int[,] voxelTriangles =
        new int[6, 4]
        {
            // 0 1 2 2 1 3
            {0, 3, 1, 2}, //뒤쪽
            {5, 6, 4, 7}, //앞쪽
            {3, 7, 2, 6}, //위쪽
            {1, 5, 0, 4}, //밑쪽
            {4, 7, 0, 3}, //왼쪽
            {1, 2, 5, 6}  //오른쪽
        };

    // 한면에 있는 삼각형 2개에 uv를 입힌다.
    public static readonly Vector2[] voxelUvs =
        new Vector2[4]
        {
            new Vector2(0.0f, 0.0f),
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f)
        };

    // 블럭이 서로 붙어있는지 확인하기 위한 vector값.
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
