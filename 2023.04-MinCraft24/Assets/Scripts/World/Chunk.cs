using System;
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
    List<int> transparentTriangles = new List<int>();
    Material[] materials = new Material[2];

    List<Vector2> uvs = new List<Vector2>();
     
    public byte[,,] voxelmap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    public Queue<VoxelMod> modifications = new Queue<VoxelMod>();

    World world;

    private bool _isActive;
    public bool isVoxelMapPopulated = false;
    // ����Ʈ���� ���� - ���� World��ũ��Ʈ���� �ҷ��´�.
    // Coord �� ûũ�� ��ġ�̴�.
    public Chunk(ChunkCoord _coord, World _world, bool generateOnLoad)
    {
        coord = _coord;
        world = _world;

        isActive = true;

        if (generateOnLoad) init();
    }

    public void init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        //meshCollider = chunkObject.AddComponent<MeshCollider>();

        // ���� ���׸��� �̶� �ָ��� ���׸��� ��ġ��
        materials[0] = world.material;
        materials[1] = world.transparentMaterial;
        meshRenderer.materials = materials;

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        //ûũ ������Ʈ�� ���� ��ġ ����.
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0.0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk" + coord.x + ", " + coord.z;
        //chunkObject.layer = 6;

        PopulateVoxelMap();
        UpdateChunk();

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
        isVoxelMapPopulated = true;
    }

    // ûũ ������
    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            Vector3 pos = v.pos -= position;
            voxelmap[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;    
        }

        // �Ž������� �ʱ�ȭ
        ClearMeshData();

        // ûũ�� ���� ��ŭ Y�� �ݺ�
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            // ûũ�� ���� ��ŭ X,Z �� �ݺ�
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    // ���ʿ��� ���� ��ġ���� �����ͼ� Solid���� Ȯ���ϰ� ���� �����Ѵ�.
                    if (world.blockType[voxelmap[x, y, z]].isSolid) UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }
        CreateMesh();
    }

    // �Ž������� �ʱ�ȭ
    void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
    }

    public bool isActive
    {
        get { return _isActive; }
        set 
        {
            _isActive = value;
            if (chunkObject != null) chunkObject.SetActive(value);
        }
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

    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        voxelmap[xCheck, yCheck, zCheck] = newID;

        UpdateSetSurroundingVoxels(xCheck, yCheck, zCheck);

        UpdateChunk();
    }

    // ��ó ûũ ������Ʈ
    void UpdateSetSurroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);
 
        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceCheck[p];

            if(!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.GetChunkFromVector3(currentVoxel + position).UpdateChunk();
            }
        }
    }
    
    // �� Ȯ��
    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // ���� ����� ���� �پ������� false ��ȯ
        if(!IsVoxelInChunk(x, y, z)) return world.CheckForVoxelTransparent(pos + position);

        // ���� ������ �� ��ȯ
        return world.blockType[voxelmap[x,y,z]].isTransparent;
    }

    public byte GecVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelmap[xCheck, yCheck, zCheck];
    }

    // pos - ���� ��ġ�� ��ġ
    void UpdateMeshData(Vector3 pos)
    {
        //��ID
        byte blockID = voxelmap[(int)pos.x, (int)pos.y, (int)pos.z];
        bool isTransparent = world.blockType[blockID].isTransparent;

        // �� 6�鿡, ������ ���� 6��
        for (int x = 0; x < 6; x++)
        {
            if (CheckVoxel(pos + VoxelData.faceCheck[x]))
            {
                // ���� �ߺ��Ǵ� vertex 2���� ������ ���ؼ� �̷��� �������.
                for(int y = 0; y<4; y++)
                {
                    vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTriangles[x, y]]);
                    
                }
                //���� ID���� �������� ��6���� �ؽ��� �����͸� �����´�
                AddTexture(world.blockType[blockID].GetTextureID(x));

                // VoxelData - voxelTriangles�� ���� ���� 1, 2, 2, 1, 3�� �˼� �ִ�.
                // ���� ������ ������� �ƴ϶�� �ָ��� ������ ����, �Ǵ� ���� ������ ����.
                if (!isTransparent)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 3);
                }
                else
                {
                    transparentTriangles.Add(vertexIndex);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 2);
                    transparentTriangles.Add(vertexIndex + 1);
                    transparentTriangles.Add(vertexIndex + 3);
                }
                vertexIndex += 4;
            }
        }
    }

    // �̶����� ���� ���� ��ġ������ �Ž� ����.
    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);

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

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    // �ٸ� ûũ�� ��ġ���� ���ٸ�.
    public bool Equals(ChunkCoord other)
    {
        if (other == null) return false;
        else if (other.x == x && other.z == z) return true;
        else return false;
    }
}
