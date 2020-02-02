using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventorySlot : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void SetItem(Item item)
    {
        if (item != null)
        {
            text.text = item.displayName;
        }
        else
        {
            text.text = "";
        }
    }
}
