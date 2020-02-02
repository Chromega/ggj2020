using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public TextMeshProUGUI scoreText;

    public UIInventory player1Inventory;
    public UIInventory player2Inventory;

    public Image progressBarFill;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        progressBarFill.transform.localScale = new Vector3(Game.Instance.voyage.GetCurrentProgress(), 1f, 1f);
        scoreText.text = $"Score: {Game.Instance.score}";
    }
}
