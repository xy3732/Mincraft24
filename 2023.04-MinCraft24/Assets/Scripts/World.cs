using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    // 플레이어
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
        // 스폰 장소를 먼저 계산 하고 맵을 생성 해야 한다.
        spawnPos = new Vector3((VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 2f, (VoxelData.WorldSizeCchunk * VoxelData.ChunkWidth) / 2f);

        // 청크 생성
        GenerateWorld();

        // 플레이어가 최근에 있던 청크를 저장한다.
        playerLastChunkCoord = GetChunkFromVector3(player.position);
    }

    private void Update()
    {
        //플레이어 현재 청크를 가져온다.
        playerChunkCoord = GetChunkFromVector3(player.position);
        //  최근에 저장했던 청크랑 현재 청크랑 같지 않다면 청크를 업데이트를 한다.
        if(!playerChunkCoord.Equals(playerLastChunkCoord)) CheckViewDistance();
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
                CreateNewChunk(x, z);
            }
        }

        // 플레이어 생성
        player.position = spawnPos;
    }

    ChunkCoord GetChunkFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);

        return new ChunkCoord(x, z);
    }

    // 플레이어 시야기반으로 청크 업데이트.
    void CheckViewDistance()
    {
        //플레이어 기준으로 vector값 계산
        ChunkCoord coord = GetChunkFromVector3(player.position);

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
                    if (chunks[x, z] == null) CreateNewChunk(x, z);
                    
                    // 청크를 이미 생성한적이 있다면 그 청크를 다시 활성화 처리를 한다.
                    else if(!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(new ChunkCoord(x, z));
                    }
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

    // 블럭이 어떤 블럭인지 반환하는 메소드.
    public byte GetVoxel(Vector3 pos)
    {
        if (!isVoxelInWorld(pos)) return 0;
        else if (pos.y < 1) return 1;
        else if (pos.y == VoxelData.ChunkHeight - 1) return 3;
        else return 2;
    }

    // 청크 생성기
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

    // 월드 블럭에 있는 면들이 붙어 있는지 확인하는 메소드.
    bool isVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeVoxels &&
           pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
           pos.z >= 0 && pos.z < VoxelData.WorldSizeVoxels) return true;
        else return false;
    }

}

//블럭 타입
[System.Serializable]
public class BlockType
{
    public string BlockName;
    public bool isSolid;

    [Header("텍스쳐값")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    public int GetTextureID(int faceIndex)
    {
        //뒤, 앞, 위, 밑, 왼, 오
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
                Debug.Log("인덱스값 에러");
                return 0;
        }
    }
}

