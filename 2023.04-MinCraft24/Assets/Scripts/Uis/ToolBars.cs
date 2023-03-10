using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ToolBars : MonoBehaviour
{
    World world;
    public Character player;

    public RectTransform highlight;
    public ItemSlot[] itemSlots;

    int slotIndex = 0;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

        foreach (ItemSlot slot in itemSlots)
        {

            slot.icon.sprite = world.blockType[slot.itemID].icon;
            slot.icon.enabled = true;
        }

        player.selectedBlockIndex = itemSlots[slotIndex].itemID;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0) slotIndex--;
            else slotIndex++;

            if (slotIndex > itemSlots.Length - 1) slotIndex = 0;
            if (slotIndex < 0) slotIndex = itemSlots.Length - 1;

            highlight.position = itemSlots[slotIndex].icon.transform.position - new Vector3(24, 0, 0);
            player.selectedBlockIndex = itemSlots[slotIndex].itemID;

            Debug.Log(slotIndex);
        }
    }
}

[System.Serializable]
public class ItemSlot
{

    public byte itemID;
    public Image icon;

}