using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayButtonClick()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LevelButtonClick()
    {

    }
}
