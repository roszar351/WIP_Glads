﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;

    public void LoadNextLevelWithStory()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1, true));
    }

    public void LoadNextLevelWithNoStory()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1, false));
    }

    public void LoadSpecificLevel(int buildIndex, bool withStory)
    {
        StartCoroutine(LoadLevel(buildIndex, withStory));
    }

    public void GoBackToMenu()
    {
        StartCoroutine(LoadLevel(0, false));
    }

    IEnumerator LoadLevel(int levelIndex, bool withStory)
    {
        // Disable player input while level is loading
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            PlayerManager.instance.StopPlayerInput();
        }

        if (withStory)
        {
            // Can be used to add a cutscene, story text or something else while the level is being loaded.
        }
        else
        {
            transition.SetTrigger("StartFade");
            yield return new WaitForSeconds(.9f);
        }

        SceneManager.LoadScene(levelIndex);
    }
}