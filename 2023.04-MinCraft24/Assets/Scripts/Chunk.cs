using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 필수 요구 컴포넌트 생성
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class Chunk : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    // voxel 데이터
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] voxelmap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    World world;

    private void Awake()
    {
        world = FindObjectOfType<World>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    // 블럭맵 생성
    void PopulateVoxelMap()
    {
        // 청크의 높이 만큼 Y값 반복
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            // 청크의 길이 만큼 X,Z 값 반복
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //여기에서 나온 숫자는 블럭ID가 된다.
                    if (y < 1) voxelmap[x, y, z] = 0;
                    else if(y == VoxelData.ChunkHeight - 1) voxelmap[x,y,z] = 3;
                    else voxelmap[x, y, z] = 1;
                }
            }
        }

    }

    // 청크 생성기
    void CreateMeshData()
    {
        // 청크의 높이 만큼 Y값 반복
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            // 청크의 길이 만큼 X,Z 값 반복
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    // 블럭맵에서 블럭의 위치값을 가져와서 블럭을 생성한다.
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    // 블럭의 면들이 붙어 있는지 확인하는 메소드.
    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // 블럭의 면들이 서로 붙어있으면 false 반환
        if(x < 0 || x > VoxelData.ChunkWidth - 1 ||
           y < 0 || y > VoxelData.ChunkHeight - 1 ||
           z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }

        // 블럭의 데이터 값 반환
        return world.blockType[voxelmap[x,y,z]].isSolid;
    }

    // pos - 블럭이 설치될 위치
    void AddVoxelDataToChunk(Vector3 pos)
    {
        // 총 6면에, 꼭짓점 갯수 6개
        for (int x = 0; x < 6; x++)
        {
            if (!CheckVoxel(pos + VoxelData.faceCheck[x]))
            {
                //블럭ID
                byte blockID = voxelmap[(int)pos.x, (int)pos.y, (int)pos.z];

                // 현재 중복되는 vertex 2개를 없에기 위해서 이렇게 만들었다.
                for(int y = 0; y<4; y++)
                {
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTriangles[x, y]]);
                    
                }
                //블럭의 ID값을 가져오고 총6면의 텍스쳐 데이터를 가져온다
                AddTexture(world.blockType[blockID].GetTextureID(x));

                // VoxelData - voxelTriangles에 보면 현재 1, 2, 2, 1, 3을 알수 있다.
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    // 이때까지 계산된 블럭의 위치값으로 매쉬 생성.
    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    // 블럭에 택스쳐 추가
    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlock;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlock);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}
