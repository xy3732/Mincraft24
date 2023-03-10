using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Attributes", menuName = "BiomeGenerator/Biome Attributes")]
public class BiomeAttributes : ScriptableObject
{
    // 바이옴 이름
    public string BiomeName;
    [Space(20)]

    //solid - 30, terrain -40이면 월드의 최대높이는 70이다.
    // 이값은 아무런 지형 없이 평탄한 지형값이다.
    public int solidGroundHeight;
    // 지형 높이.   
    public int terrainHeight;
    // 지형 스케일 값.
    public float terrainScale;

    [Header("TREE")]
    public float treeZoneScale = 1.3f;
    [Range(0.1f, 1f)]
    public float treeZoneThreshold = 0.6f;
    public float treePlacementScale = 15f;
    [Range(0.1f, 1f)]
    public float treePlacementThreshold = 0.8f;
    public int maxTreeHeight = 12;
    public int minTreeHeight = 5;


    public Lode[] lodes;
}

[System.Serializable]
public class Lode
{
    public string nodeName;
    [Space(20)]

    public byte blockID;
    [Space(20)]

    public int minHeight;
    public int maxHeight;
    [Space(20)]

    public float scale;
    public float threhold;
    public float noise; 
}
