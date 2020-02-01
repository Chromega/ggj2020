using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public TextMeshProUGUI progressText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        progressText.text = $"Voyage progress {(Game.Instance.voyage.GetCurrentProgress() * 100).ToString("0.00")}%";
    }
}
