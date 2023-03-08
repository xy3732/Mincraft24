using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    // �÷��̾�
    public Transform player;
    public Vector3 spawnPos;

    public Material material;
    public BlockType[] blockType;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeCchunk, VoxelData.WorldSizeCchunk];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start()
    {
        // ���� ��Ҹ� ���� ��� �ϰ� ���� ���� �ؾ� �Ѵ�.
        spawnPos = new Vector3((VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 2f, (VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f);

        // ûũ ����
        GenerateWorld();

        // �÷��̾ �ֱٿ� �ִ� ûũ�� �����Ѵ�.
        playerLastChunkCoord = GetChunkFromVector3(player.position);
    }

    private void Update()
    {
        //�÷��̾� ���� ûũ�� �����´�.
        playerChunkCoord = GetChunkFromVector3(player.position);
        //  �ֱٿ� �����ߴ� ûũ�� ���� ûũ�� ���� �ʴٸ� ûũ�� ������Ʈ�� �Ѵ�.
        if(!playerChunkCoord.Equals(playerLastChunkCoord)) CheckViewDistance();
    }

    // ���� ������
    void GenerateWorld()
    {
        // �� ûũ�� ũ�⸸ŭ x,z ���� �ݺ��Ѵ�.
        // ����ũ����Ʈ�� �ִ� ûũ ���� ������� �����Ŵ�.
        // ViewDistanceInChunks�� ����ŭ �� ���� �ְ� ����.
        for (int x = (VoxelData.WorldSizeCchunk / 2 ) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeCchunk / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeCchunk / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeCchunk / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }

        // �÷��̾� ����
        player.position = spawnPos;
    }

    ChunkCoord GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    // �÷��̾� �þ߱������ ûũ ������Ʈ.
    void CheckViewDistance()
    {
        //�÷��̾� �������� vector�� ���
        ChunkCoord coord = GetChunkFromVector3(player.position);

        // ���� �����ִ� ��� ûũ���� ����Ʈȭ �ؼ� �����Ѵ�.
        List<ChunkCoord> previouseActiveChunk = new List<ChunkCoord>(activeChunks);

        // �÷��̾� �þ߿� �ִ��� Ȯ��
        for(int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                // ���� ûũ ���� ���� �Ÿ����� ���� ���� ûũ �Ÿ��� ª�ٸ� �����Ѵ�.
                if(IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    // ���� ���� ûũ�� �ѹ��̶� ûũ�� �������� �ʾҴٸ� ûũ�� �����Ѵ�.
                    if (chunks[x, z] == null) CreateNewChunk(x, z);
                    
                    // ûũ�� �̹� ���������� �ִٸ� �� ûũ�� �ٽ� Ȱ��ȭ ó���� �Ѵ�.
                    else if(!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
                }

                // ���࿡ ���� �þ߿� �ִ� ûũ��� ����ó���� ���� �ʴ´�.
                for(int i = 0; i<previouseActiveChunk.Count; i++)
                {
                    if(previouseActiveChunk[i].Equals(new ChunkCoord(x,z)))
                    {
                        previouseActiveChunk.RemoveAt(i);
                    }
                }
            }
        }

        // ���� ���ִ� ��� ûũ���� ���δ� ��Ȱ��ȭ ��Ų��.
        foreach (ChunkCoord chunk in previouseActiveChunk)
        {
            chunks[chunk.x, chunk.z].isActive = false;
        }
    }

    // ���� � ������ ��ȯ�ϴ� �޼ҵ�.
    public byte GetVoxel(Vector3 pos)
    {
        if (!isVoxelInWorld(pos)) return 0;
        else if (pos.y < 1) return 1;
        else if (pos.y == VoxelData.ChunkHeight - 1) return 3;
        else return 2;
    }

    // ûũ ������
    void CreateNewChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        activeChunks.Add(new ChunkCoord(x, z));
    }

    bool IsChunkInWorld(ChunkCoord _coord)
    {
        if (_coord.x > 0 && _coord.x < VoxelData.WorldSizeCchunk - 1 &&
            _coord.z > 0 && _coord.z < VoxelData.WorldSizeCchunk - 1) return true;
        else return false;
    }

    // ���� ���� �ִ� ����� �پ� �ִ��� Ȯ���ϴ� �޼ҵ�.
    bool isVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeVoxels &&
           pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
           pos.z >= 0 && pos.z < VoxelData.WorldSizeVoxels) return true;
        else return false;
    }

}

//�� Ÿ��
[System.Serializable]
public class BlockType
{
    public string BlockName;
    public bool isSolid;

    [Header("�ؽ��İ�")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureID(int faceIndex)
    {
        //��, ��, ��, ��, ��, ��
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("�ε����� ����");
                return 0;
        }
    }
}

