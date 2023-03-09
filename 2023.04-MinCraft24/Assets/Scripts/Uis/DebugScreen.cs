using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
public class DebugScreen : MonoBehaviour
{
    World world;
    TextMeshProUGUI text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;
    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<TextMeshProUGUI>();

        halfWorldSizeInVoxels = VoxelData.WorldSizeVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeCchunk / 2;

    }

    private void Update()
    {
        string debugText = "Debug text test";
        debugText += "\n";
        debugText += frameRate + " FPS";
        debugText += "\n";
        debugText += "XYZ : " + (Mathf.FloorToInt(world.player.transform.position.x) - halfWorldSizeInVoxels) + " / " + Mathf.FloorToInt(world.player.transform.position.y) + " / " + (Mathf.FloorToInt(world.player.transform.position.z) - halfWorldSizeInVoxels);
       

        text.text = debugText;

        if (timer > 1f)
        { 
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;
        }
        else
        {
            timer += Time.deltaTime; 
        }

    }
}
