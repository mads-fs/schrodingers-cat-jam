using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void LoadScene(int index) => SceneManager.LoadScene(index);
    public void Exit() => Application.Quit();
}
