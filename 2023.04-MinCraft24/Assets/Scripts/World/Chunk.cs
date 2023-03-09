using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    // voxel 데이터
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public byte[,,] voxelmap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    World world;

    // 컨스트럭터 생성 - 현재 World스크립트에서 불러온다.
    // Coord 는 청크의 위치이다.
    public Chunk(ChunkCoord _coord, World _world)
    {
        coord = _coord;
        world = _world;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        //meshCollider = chunkObject.AddComponent<MeshCollider>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        //청크 오브젝트의 월드 위치 설정.
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0.0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk" + coord.x + ", " + coord.z;
        //chunkObject.layer = 6;

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();

        //meshCollider.sharedMesh = meshFilter.mesh;
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
                    voxelmap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
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
                    // 블럭맵에서 블럭의 위치값을 가져와서 Solid인지 확인하고 블럭을 생성한다.
                    if (world.blockType[voxelmap[x, y, z]].isSolid) AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }

    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }

    // 블럭의 면들이 붙어 있는지 확인하는 메소드.
    bool IsVoxelInChunk(int x, int y, int z)
    {
        // 블럭의 면들이 서로 붙어있으면 false 반환
        if (x < 0 || x > VoxelData.ChunkWidth - 1 ||
           y < 0 || y > VoxelData.ChunkHeight - 1 ||
           z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    
    // 블럭 확인
    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // 블럭의 면들이 서로 붙어있으면 false 반환
        if(!IsVoxelInChunk(x, y, z)) return world.blockType[world.GetVoxel(pos + position)].isSolid;

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

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    // 다른 청크랑 위치값이 같다면.
    public bool Equals(ChunkCoord other)
    {
        if (other == null) return false;
        else if (other.x == x && other.z == z) return true;
        else return false;
    }
}
