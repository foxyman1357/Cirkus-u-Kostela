using UnityEngine;
using UnityEngine.UI;
public class DeathUI : MonoBehaviour
{
  



    public Button retryButton;
    public Button menuButton;

    private void Start()
    {
        retryButton.onClick.AddListener(RestartGame);
        menuButton.onClick.AddListener(GoToMainMenu);
    }

    private void RestartGame()
    {
        GameManager.Instance.RestartGame();
    }

    private void GoToMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
}

