using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    // 플레이어
    public Transform player;
    public Vector3 spawnPos;
    [Space(20)]

    // 맵 생성 시드값. - 마인크래프트의 시드랑 같은것이다.
    public int seed;
    // 지형 종류
    public BiomeAttributes biome;
    [Space(20)]

    public Material material;
    public Material transparentMaterial;
    public BlockTypes[] blockType;

    //총 청크의 크기
    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeCchunk, VoxelData.WorldSizeCchunk];

    // 로딩한적 있는 청크
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
        // 60프레임 타겟 고정
        Application.targetFrameRate = 60;

        // 시드값 랜덤
        Random.InitState(seed);

        // 스폰 장소를 먼저 계산 하고 맵을 생성 해야 한다.
        spawnPos = new Vector3((VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight -50f, (VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f);

        // 청크 생성
        GenerateWorld();

        // 플레이어가 최근에 있던 청크를 저장한다.
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update()
    {
        // 플레이어 현재 청크를 가져온다.
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        // 최근에 저장했던 청크랑 현재 청크랑 같지 않다면 청크를 업데이트를 한다.
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
        // 디버그 텍스트
        if (Input.GetKeyDown(KeyCode.F3)) debugScreen.SetActive(!debugScreen.activeSelf);
    }

    // 월드 생성기
    void GenerateWorld()
    {
        // 총 청크의 크기만큼 x,z 값을 반복한다.
        // 마인크래프트에 있는 청크 랜더 사이즈랑 같은거다.
        // ViewDistanceInChunks의 값만큼 맵 볼수 있게 생성.
        for (int x = (VoxelData.WorldSizeCchunk / 2 ) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeCchunk / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeCchunk / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeCchunk / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x,z));
            }
        }
        

        // 플레이어 생성
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


    // 플레이어 시야기반으로 청크 업데이트.
    void CheckViewDistance()
    {
        //플레이어 기준으로 vector값 계산
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        // 현재 켜져있는 모든 청크들은 리스트화 해서 보관한다.
        List<ChunkCoord> previouseActiveChunk = new List<ChunkCoord>(activeChunks);

        // 플레이어 시야에 있는지 확인
        for(int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                // 현재 청크 생성 가능 거리보다 현재 맵의 청크 거리가 짧다면 실행한다.
                if(IsChunkInWorld(new ChunkCoord(x,z)))
                {
                    // 만약 현재 청크에 한번이라도 청크를 생성하지 않았다면 청크를 생성한다.
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunkToCreate.Add(new ChunkCoord(x, z));
                    }
                    // 청크를 이미 생성한적이 있다면 그 청크를 다시 활성화 처리를 한다.
                    else if (!chunks[x, z].isActive) chunks[x, z].isActive = true;
                    
                    activeChunks.Add(new ChunkCoord(x, z));
                }

                // 만약에 현재 시야에 있는 청크라면 보관처리를 하지 않는다.
                for(int i = 0; i<previouseActiveChunk.Count; i++)
                {
                    if(previouseActiveChunk[i].Equals(new ChunkCoord(x,z)))
                    {
                        previouseActiveChunk.RemoveAt(i);
                    }
                }
            }
        }

        // 보관 되있던 모든 청크들을 전부다 비활성화 시킨다.
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


    // 블럭이 어떤 블럭인지 반환하는 메소드.
    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        /* 불변 Pass */

        // 월드 사이즈 밬에는 공기 블럭으로 생성.
        if (!isVoxelInWorld(pos)) return 0;
        // 바닥에 있는 블럭은 BedBlock으로 생성.
        if (yPos == 0) return 1;

        /* 지형 Pass */

        // 펄린 노이즈로 지형 생성.
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelvalue = 0;

        // 표면과 같은 값이면 잔디 블럭으로 생성
        if (yPos == terrainHeight) voxelvalue = 3;
        // 표면의 아래 4블럭 부터 표면 전 까지 흙으로 블럭 생성.
        else if (yPos < terrainHeight && yPos > terrainHeight - 4) voxelvalue = 5;
        // 표면 보다 위에 있으면 공기 블럭으로 대체
        else if (yPos > terrainHeight) return 0;
        else voxelvalue =  2;

        /* 2번째 지형 PASS */
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

        /* 나무 PASS */

        if( yPos == terrainHeight)
        {
            // 나무가 존재할 노이즈맵 생성.
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

    // 월드 블럭에 있는 면들이 붙어 있는지 확인하는 메소드.
    bool isVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeVoxels &&
           pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
           pos.z >= 0 && pos.z < VoxelData.WorldSizeVoxels) return true;
        else return false;
    }

}



