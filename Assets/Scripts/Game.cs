using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : Singleton<Game>
{
    public VoyageManager voyage;
    public Ship ship;
    public PlayerScript player1;
    public PlayerScript player2;
    public PlayerScript player3;
    public PlayerScript player4;

    public GameObject water;

    private bool gameOver = false;
    private bool paused = false;
    private GameObject[] pauseObjects;
    private GameObject[] gameOverObjects;
    private GameObject[] victoryObjects;

    public int score = 0;

    private Coroutine startVoyageCoroutine;

    private void Awake()
    {
        ship = GameObject.Find("Ship").GetComponent<Ship>();
        water = GameObject.Find("environ-water");
        voyage = GetComponent<VoyageManager>();
    }

    private void Start()
    {
        UIManager.Instance.player1Inventory.AssignInventory(player1.inventory);
        UIManager.Instance.player2Inventory.AssignInventory(player2.inventory);
        //UIManager.Instance.player1Inventory.AssignInventory(player3.inventory);
        //UIManager.Instance.player1Inventory.AssignInventory(player4.inventory);
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        gameOverObjects = GameObject.FindGameObjectsWithTag("ShowOnGameOver");
        victoryObjects = GameObject.FindGameObjectsWithTag("ShowOnVictory");

        foreach (GameObject obj in gameOverObjects) {
			obj.SetActive(false);
		}
        foreach (GameObject obj in victoryObjects) {
			obj.SetActive(false);
		}
        UnPause();
        StartCoroutine(StartVoyage());
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Restart();
        }
        if (!gameOver) {
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (Time.timeScale == 1)
                {
                    Pause();
                } else if (Time.timeScale == 0)
                {
                    UnPause();
                }
            }
        }
        water.transform.position = new Vector3(ship.transform.position.x, GetWaterLevel(), ship.transform.position.z);
        if (!gameOver && ship.HullPercentage() == 1.0f)
        {
            GameOver();
        }
        if (!gameOver && voyage.GetCurrentProgress() == 1.0f)
        {
            Victory();
        }
    }

    IEnumerator StartVoyage() {
        GetComponent<ItemSpawner>().SpawnItemCount(2);
        yield return new WaitForSeconds(5.5f);
        Debug.Log("StartVoyage");
        voyage.StartVoyage();
    }

    public float GetWaterLevel()
    {
        if (ship.HullPercentage() < 0.5f)
        {
            return ship.HullPercentage() - 0.5f;
        }
        else
        {
            return ship.HullPercentage() * 2 - 1.0f;
        }
    }
    void GameOver()
    {
        gameOver = true;
        Time.timeScale = 0;
        foreach (GameObject obj in gameOverObjects) {
			obj.SetActive(true);
		}
    }

    void Victory()
    {
        Debug.Log("Victory!");
        gameOver = true;
        foreach (GameObject obj in victoryObjects) {
			obj.SetActive(true);
		}
        Time.timeScale = 0;
    }

    void Restart()
    {
        voyage.ResetVoyage();
        gameOver = false;
        Time.timeScale = 1;
        player1?.Reset();
        player2?.Reset();
        player3?.Reset();
        player4?.Reset();
        score = 0;
        PickupableFactory.Instance.Reset();
        foreach (GameObject obj in gameOverObjects) {
			obj.SetActive(false);
		}
        foreach (GameObject obj in victoryObjects) {
			obj.SetActive(false);
		}
        UnPause();
        GameObject.Find("AudioManager").GetComponent<IntroThenLoop>().Restart();
        if (startVoyageCoroutine != null) {
            StopCoroutine(startVoyageCoroutine);
        }
        startVoyageCoroutine = StartCoroutine(StartVoyage());
    }

    void Pause()
    {
        paused = true;
        Time.timeScale = 0;
        foreach (GameObject obj in pauseObjects) {
			obj.SetActive(true);
		}
    }

    void UnPause()
    {
        paused = false;
        Time.timeScale = 1;
        foreach (GameObject obj in pauseObjects) {
			obj.SetActive(false);
		}
    }
}
