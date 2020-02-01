using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventorySlot : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI text;

    public void SetItem(Item item)
    {
        if (item != null)
        {
            itemIcon.color = Color.white;
            itemIcon.sprite = item.itemIcon;
            text.text = item.displayName;
        }
        else
        {
            itemIcon.color = Color.clear;
            text.text = "";
        }
    }
}
