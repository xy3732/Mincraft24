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
    public Material transparentMaterial;
    public BlockTypes[] blockType;

    //�� ûũ�� ũ��
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeCchunk, VoxelData.WorldSizeCchunk];

    // �ε����� �ִ� ûũ
    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    //
    List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunkToDraw = new Queue<Chunk>();  
    bool applyingModification = false;
    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    List<ChunkCoord> chunkToCreate = new List<ChunkCoord>();

    public GameObject debugScreen; 
    private void Start()
    {
        // 60������ Ÿ�� ����
        Application.targetFrameRate = 60;

        // �õ尪 ����
        Random.InitState(seed);

        // ���� ��Ҹ� ���� ��� �ϰ� ���� ���� �ؾ� �Ѵ�.
        spawnPos = new Vector3((VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight -50f, (VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f);

        // ûũ ����
        GenerateWorld();

        // �÷��̾ �ֱٿ� �ִ� ûũ�� �����Ѵ�.
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        // �÷��̾� ���� ûũ�� �����´�.
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        // �ֱٿ� �����ߴ� ûũ�� ���� ûũ�� ���� �ʴٸ� ûũ�� ������Ʈ�� �Ѵ�.
        if(!playerChunkCoord.Equals(playerLastChunkCoord)) CheckViewDistance();

        if (!applyingModification) ApplyModifications(); 

        if (chunkToCreate.Count > 0) CreateChunk();

        if (chunksToUpdate.Count > 0) UpdateChunk();

        if (chunkToDraw.Count > 0)
        { 
            lock (chunkToDraw)
            {
                if (chunkToDraw.Peek().isEditable) chunkToDraw.Dequeue().CreateMesh();
            }
        }
        // ����� �ؽ�Ʈ
        if (Input.GetKeyDown(KeyCode.F3)) debugScreen.SetActive(!debugScreen.activeSelf);
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
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x,z));
            }
        }
        

        // �÷��̾� ����
        player.position = spawnPos;
    }

    void CreateChunk()
    {
        ChunkCoord c = chunkToCreate[0];
        chunkToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.z].init();
    }

    void UpdateChunk()
    {
        bool updated = false;
        int index = 0;

        while (!updated && index < chunksToUpdate.Count - 1)
        {
            if(chunksToUpdate[index].isEditable)
            {
                chunksToUpdate[index].UpdateChunk();
                chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
            {
                index++;
            }
        }
    }

    void ApplyModifications()
    {
        applyingModification = true;

        while(modifications.Count > 0)
        {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while(queue.Count > 0)
            {
                VoxelMod v = queue.Dequeue();

                ChunkCoord c = GetChunkCoordFromVector3(v.pos);

                if (chunks[c.x, c.z] == null)
                {
                    chunks[c.x, c.z] = new Chunk(c, this, true);
                    activeChunks.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);

                if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
                {
                    chunksToUpdate.Add(chunks[c.x, c.z]);
                }
            }
        }
        applyingModification = false;
    }


    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return chunks[x, z];
    }


    // �÷��̾� �þ߱������ ûũ ������Ʈ.
    void CheckViewDistance()
    {
        //�÷��̾� �������� vector�� ���
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

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
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunkToCreate.Add(new ChunkCoord(x, z));
                    }
                    // ûũ�� �̹� ���������� �ִٸ� �� ûũ�� �ٽ� Ȱ��ȭ ó���� �Ѵ�.
                    else if (!chunks[x, z].isActive) chunks[x, z].isActive = true;
                    
                    activeChunks.Add(new ChunkCoord(x, z));
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

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if(!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight) return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
            return blockType[chunks[thisChunk.x, thisChunk.z].GecVoxelFromGlobalVector3(pos)].isSolid;

        return blockType[GetVoxel(pos)].isSolid;
    }

    public bool CheckForVoxelTransparent(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight) return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
            return blockType[chunks[thisChunk.x, thisChunk.z].GecVoxelFromGlobalVector3(pos)].isTransparent;

        return blockType[GetVoxel(pos)].isTransparent;
    }


    // ���� � ������ ��ȯ�ϴ� �޼ҵ�.
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /* �Һ� Pass */

        // ���� ������ �U���� ���� ������ ����.
        if (!isVoxelInWorld(pos)) return 0;
        // �ٴڿ� �ִ� ���� BedBlock���� ����.
        if (yPos == 0) return 1;

        /* ���� Pass */

        // �޸� ������� ���� ����.
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelvalue = 0;

        // ǥ��� ���� ���̸� �ܵ� ������ ����
        if (yPos == terrainHeight) voxelvalue = 3;
        // ǥ���� �Ʒ� 4�� ���� ǥ�� �� ���� ������ �� ����.
        else if (yPos < terrainHeight && yPos > terrainHeight - 4) voxelvalue = 5;
        // ǥ�� ���� ���� ������ ���� ������ ��ü
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

        /* ���� PASS */

        if( yPos == terrainHeight)
        {
            // ������ ������ ������� ����.
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treeZoneScale) > biome.treeZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treePlacementScale) > biome.treePlacementThreshold)
                {
                    modifications.Enqueue(structures.MakeTree(pos, biome.minTreeHeight, biome.maxTreeHeight));
                }
            }
        }

        return voxelvalue;
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



