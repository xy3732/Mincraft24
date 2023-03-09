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

    // voxel ������
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public byte[,,] voxelmap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    World world;

    // ����Ʈ���� ���� - ���� World��ũ��Ʈ���� �ҷ��´�.
    // Coord �� ûũ�� ��ġ�̴�.
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
        //ûũ ������Ʈ�� ���� ��ġ ����.
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0.0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk" + coord.x + ", " + coord.z;
        //chunkObject.layer = 6;

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();

        //meshCollider.sharedMesh = meshFilter.mesh;
    }

    // ���� ����
    void PopulateVoxelMap()
    {
        // ûũ�� ���� ��ŭ Y�� �ݺ�
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            // ûũ�� ���� ��ŭ X,Z �� �ݺ�
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    //���⿡�� ���� ���ڴ� ��ID�� �ȴ�.
                    voxelmap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }

    }

    // ûũ ������
    void CreateMeshData()
    {
        // ûũ�� ���� ��ŭ Y�� �ݺ�
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            // ûũ�� ���� ��ŭ X,Z �� �ݺ�
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    // ���ʿ��� ���� ��ġ���� �����ͼ� Solid���� Ȯ���ϰ� ���� �����Ѵ�.
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

    // ���� ����� �پ� �ִ��� Ȯ���ϴ� �޼ҵ�.
    bool IsVoxelInChunk(int x, int y, int z)
    {
        // ���� ����� ���� �پ������� false ��ȯ
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
    
    // �� Ȯ��
    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // ���� ����� ���� �پ������� false ��ȯ
        if(!IsVoxelInChunk(x, y, z)) return world.blockType[world.GetVoxel(pos + position)].isSolid;

        // ���� ������ �� ��ȯ
        return world.blockType[voxelmap[x,y,z]].isSolid;
    }

    // pos - ���� ��ġ�� ��ġ
    void AddVoxelDataToChunk(Vector3 pos)
    {
        // �� 6�鿡, ������ ���� 6��
        for (int x = 0; x < 6; x++)
        {
            if (!CheckVoxel(pos + VoxelData.faceCheck[x]))
            {
                //��ID
                byte blockID = voxelmap[(int)pos.x, (int)pos.y, (int)pos.z];

                // ���� �ߺ��Ǵ� vertex 2���� ������ ���ؼ� �̷��� �������.
                for(int y = 0; y<4; y++)
                {
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTriangles[x, y]]);
                    
                }
                //���� ID���� �������� ��6���� �ؽ��� �����͸� �����´�
                AddTexture(world.blockType[blockID].GetTextureID(x));

                // VoxelData - voxelTriangles�� ���� ���� 1, 2, 2, 1, 3�� �˼� �ִ�.
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

    // �̶����� ���� ���� ��ġ������ �Ž� ����.
    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    // ���� �ý��� �߰�
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

    // �ٸ� ûũ�� ��ġ���� ���ٸ�.
    public bool Equals(ChunkCoord other)
    {
        if (other == null) return false;
        else if (other.x == x && other.z == z) return true;
        else return false;
    }
}
