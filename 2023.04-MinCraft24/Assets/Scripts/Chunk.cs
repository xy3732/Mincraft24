using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �ʼ� �䱸 ������Ʈ ����
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]

public class Chunk : MonoBehaviour
{
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    // voxel ������
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
                    if (y < 1) voxelmap[x, y, z] = 0;
                    else if(y == VoxelData.ChunkHeight - 1) voxelmap[x,y,z] = 3;
                    else voxelmap[x, y, z] = 1;
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
                    // ���ʿ��� ���� ��ġ���� �����ͼ� ���� �����Ѵ�.
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    // ���� ����� �پ� �ִ��� Ȯ���ϴ� �޼ҵ�.
    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // ���� ����� ���� �پ������� false ��ȯ
        if(x < 0 || x > VoxelData.ChunkWidth - 1 ||
           y < 0 || y > VoxelData.ChunkHeight - 1 ||
           z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }

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
