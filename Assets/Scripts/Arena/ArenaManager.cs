﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArenaManager : MonoBehaviour
{
    // Using gameobject over transform provides more flexibility for future if wanted to add an spawn animation, effect, sprite or something similar
    // First index i.e. spawnPoints[0] is always the boss spawn point
    public GameObject[] spawnPoints;
    // Keep track of traps present in the arena to be activated, currently not sure if it will be used
    public GameObject[] arenaTraps;
    // Currently using one array for enemies, but might need to split them to have more dangerous/rare enemies be part of a different array
    public GameObject[] enemies;
    public GameObject[] bosses;

    public TextMeshProUGUI levelText;

    private int level = 0;
    private float levelTimer = 0; // might be used for score or some other stat
    private int enemiesLeft = 0;
    private int enemiesKilled = 0;
    private bool isRewardUIOpen = false;
    private bool bossFight = false;

    [SerializeField]
    private GameObject enemiesParent;

    [SerializeField]
    private so_GameEvent onRewardUIOpen;
    
    [SerializeField]
    private so_GameEvent onRewardUIClose;

    private void Start()
    {
        levelText.SetText("Level: " + (level+1));
    }

    private void Update()
    {
        if(enemiesLeft <= 0 && !isRewardUIOpen)
        {
            NextLevel();
        }
    }

    public void EnemyDied()
    {
        enemiesLeft -= 1;
        enemiesKilled++;

        if(enemiesLeft <= 0 && bossFight)
        {
            bossFight = false;
            onRewardUIOpen.Raise();
            AudioManager.instance.PlayRandomMusic(MusicType.GameMusic);
        }
        else if(enemiesLeft <= 0 && enemiesKilled / 5 > 0)
        {
            onRewardUIOpen.Raise();
        }
    }

    public void RewardUIToggled(bool openOrClose)
    {
        // TODO: currently reward UI is based on enemies killed and after each boss
        isRewardUIOpen = openOrClose;
        enemiesKilled = 0;
    }

    public void UpdateMaxLevel()
    {
        if (PlayerPrefs.HasKey("MaxLevel"))
        {
            if (level > PlayerPrefs.GetInt("MaxLevel"))
            {
                PlayerPrefs.SetInt("MaxLevel", level);
            }
        }
        else
        {
            PlayerPrefs.SetInt("MaxLevel", level);
        }

        PlayerPrefs.Save();
    }

    private void NextLevel()
    {
        AudioManager.instance.PlayOneShotSound("NextLevel");
        level++;
        levelText.SetText("Level: " + level);
        if (level % 5 == 0)
        {
            Debug.LogWarning("BOSS");
            SpawnBoss();
        }
        else
        {
            SpawnEnemies();
        }
    }

    private void SpawnEnemies()
    {
        //TODO: spawn enemies, rework spawn system
        // for each enemy to spawn, spawn enemy, enemiesLeft++
        int howMany = 2 + (int)(1.5 * level);
        int whichEnemy, whichPoint;
        Vector3 randomOffset;

        for (int i = 0; i < howMany; ++i)
        {
            whichEnemy = Random.Range(0, enemies.Length);
            whichPoint = Random.Range(0, spawnPoints.Length);

            randomOffset = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0f);

            //spawnPoints[whichPoint].SetActive(true);
            Instantiate(enemies[whichEnemy], spawnPoints[whichPoint].transform.position + randomOffset, Quaternion.identity, enemiesParent.transform);
            enemiesLeft++;
        }
    }

    private void SpawnBoss()
    {
        bossFight = true;

        int howMany = level / 5;
        Vector3 randomOffset;

        for (int i = 0; i < howMany; ++i)
        {
            enemiesLeft++;

            randomOffset = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0f);

            Instantiate(bosses[0], spawnPoints[0].transform.position + randomOffset * 2, Quaternion.identity, enemiesParent.transform);
        }

        AudioManager.instance.PlayRandomMusic(MusicType.BossMusic);
    }

    private void ActivateTraps()
    {
        //TODO: activate traps, maybe add a method to disable as well or just leave activated for rest of the game
    }
}
