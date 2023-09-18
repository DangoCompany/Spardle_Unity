using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeTransition : MonoBehaviour
{
    public void LoadStoryModeScene()
    {
        SceneManager.LoadScene("StoryMode");
    }
}
