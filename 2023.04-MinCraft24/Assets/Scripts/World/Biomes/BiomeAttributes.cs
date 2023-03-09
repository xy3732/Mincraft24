using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Attributes", menuName = "BiomeGenerator/Biome Attributes")]
public class BiomeAttributes : ScriptableObject
{
    // ���̿� �̸�
    public string BiomeName;
    [Space(20)]

    //solid - 30, terrain -40�̸� ������ �ִ���̴� 70�̴�.
    // �̰��� �ƹ��� ���� ���� ��ź�� �������̴�.
    public int solidGroundHeight;
    // ���� ����.   
    public int terrainHeight;
    // ���� ������ ��.
    public float terrainScale;

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
