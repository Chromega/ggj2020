using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public TextMeshProUGUI progressText;

    public UIInventory player1Inventory;
    public UIInventory player2Inventory;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        progressText.text = $"Voyage progress {(Game.Instance.voyage.GetCurrentProgress() * 100).ToString("0")}%. Score: {Game.Instance.score}";
    }
}
