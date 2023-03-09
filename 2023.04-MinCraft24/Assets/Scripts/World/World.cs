using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    // �÷��̾�
    public Transform player;
    public Vector3 spawnPos;
    [Space(20)]

    // �� ���� �õ尪. - ����ũ����Ʈ�� �õ�� �������̴�.
    public int seed;
    // ���� ����
    public BiomeAttributes biome;
    [Space(20)]

    public Material material;
    public BlockTypes[] blockType;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeCchunk, VoxelData.WorldSizeCchunk];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    private void Start()
    {
        // �õ尪 ����
        Random.InitState(seed);

        // ���� ��Ҹ� ���� ��� �ϰ� ���� ���� �ؾ� �Ѵ�.
        spawnPos = new Vector3((VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight -50f, (VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f);

        // ûũ ����
        GenerateWorld();

        // �÷��̾ �ֱٿ� �ִ� ûũ�� �����Ѵ�.
        playerLastChunkCoord = GetChunkFromVector3(player.position);
    }

    private void Update()
    {
        // �÷��̾� ���� ûũ�� �����´�.
        playerChunkCoord = GetChunkFromVector3(player.position);

        // �ֱٿ� �����ߴ� ûũ�� ���� ûũ�� ���� �ʴٸ� ûũ�� ������Ʈ�� �Ѵ�.
        //if(!playerChunkCoord.Equals(playerLastChunkCoord)) CheckViewDistance();
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

    public bool CheckForVoxel(float _x, float _y, float _z)
    {
        int xCheck = Mathf.FloorToInt(_x);
        int yCheck = Mathf.FloorToInt(_y);
        int zCheck = Mathf.FloorToInt(_z);

        int xChunk = xCheck / VoxelData.ChunkWidth;
        int zChunk = zCheck / VoxelData.ChunkWidth;

        xCheck -= (xChunk * VoxelData.ChunkWidth);
        zCheck -= (zChunk * VoxelData.ChunkWidth);

        return blockType[chunks[xChunk, zChunk].voxelmap[xCheck, yCheck, zCheck]].isSolid;
    }


    // ������ � �������� ��ȯ�ϴ� �޼ҵ�.
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /* �Һ� Pass */

        // ���� ������ �U���� ���� �������� ����.
        if (!isVoxelInWorld(pos)) return 0;
        // �ٴڿ� �ִ� ������ BedBlock���� ����.
        if (yPos == 0) return 1;

        /* ���� Pass */

        // �޸� ������� ���� ����.
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelvalue = 0;

        // ǥ��� ���� ���̸� �ܵ� �������� ����
        if (yPos == terrainHeight) voxelvalue = 3;
        // ǥ���� �Ʒ� 4���� ���� ǥ�� �� ���� ������ ���� ����.
        else if (yPos < terrainHeight && yPos > terrainHeight - 4) voxelvalue = 5;
        // ǥ�� ���� ���� ������ ���� �������� ��ü
        else if (yPos > terrainHeight) return 0;
        else voxelvalue =  2;

        /* 2��° ���� PASS */
        if (voxelvalue == 2)
        {
            foreach (var lode in biome.lodes)
            {
                if(yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(pos, lode.noise, lode.scale, lode.threhold)) voxelvalue = lode.blockID;
                }
            }
        }

        return voxelvalue;
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

    // ���� ������ �ִ� ����� �پ� �ִ��� Ȯ���ϴ� �޼ҵ�.
    bool isVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeVoxels &&
           pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
           pos.z >= 0 && pos.z < VoxelData.WorldSizeVoxels) return true;
        else return false;
    }

}


