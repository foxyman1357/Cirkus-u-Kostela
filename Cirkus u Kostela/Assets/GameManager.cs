using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameLevel { Level1, Level2, Level3 }
    public GameLevel currentLevel = GameLevel.Level1;

    private float gameTime = 0f;
    private float timeLimit = 120f; // Délka kola (5 minut)

    private float moveSpeed = 1f;
    private float speedIncreaseRate = 0.1f;
    private float maxSpeed = 10f;
    public bool dvereZamceny { get; private set; } // Přístup pouze pro čtení
    

    private bool gameWon = false;
    public bool canContinue = false; // Odemyká tlačítko Continue

    private bool jsouDvereObsazene = false; // Nová proměnná pro stav dveří

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (gameWon) return;

        gameTime += Time.deltaTime;

        // Dynamické zrychlování nepřátel
        moveSpeed = 1f + (speedIncreaseRate * gameTime);
        moveSpeed = Mathf.Min(moveSpeed, maxSpeed);

        // Kontrola výhry (časový limit)
        if (gameTime >= timeLimit)
        {
            WinGame();
        }
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }

    public void SetGameTimeLimit(float newTimeLimit)
    {
        timeLimit = newTimeLimit;
    }

    public void WinGame()
    {
        if (gameWon) return;

        gameWon = true;
        canContinue = true; // Povolit tlačítko Continue
        Debug.Log("🎉 Vyhrál jsi! Přesun do menu...");

        PlayerPrefs.SetInt("CanContinue", 1); // Uložit možnost pokračovat
        SceneManager.LoadScene("Win"); // Přesměrovat na výherní obrazovku
    }

    public void LoseGame()
    {
        Debug.Log("💀 Prohrál jsi!");
        SceneManager.LoadScene("Death"); // Přesměrování na obrazovku smrti
    }

    // Metoda pro restartování hry
    public void RestartGame()
    {
        Debug.Log("Restartování hry...");
        PlayerPrefs.SetInt("CanContinue", 0); // Resetování možnosti pokračovat
        gameTime = 0f;
        gameWon = false;
        canContinue = false;
        currentLevel = GameLevel.Level1; // Nastavení na první úroveň
        SceneManager.LoadScene("Game"); // Načte hru znovu
    }

    // Metoda pro začátek nové hry
    public void NewGame()
    {
        PlayerPrefs.SetInt("CanContinue", 0); // Resetovat možnost pokračovat
        gameTime = 0f;
        gameWon = false;
        canContinue = false;
        currentLevel = GameLevel.Level1;
        SceneManager.LoadScene("Game");
    }

    // Metoda pro přechod na další level
    public void NextLevel()
    {
        if (currentLevel == GameLevel.Level1) currentLevel = GameLevel.Level2;
        else if (currentLevel == GameLevel.Level2) currentLevel = GameLevel.Level3;
        else
        {
            Debug.Log("Jsi na posledním levelu!");
            return;
        }
        timeLimit =+ 60f;
        gameTime = 0f;
        gameWon = false;
        SceneManager.LoadScene("Game");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Metody pro správu stavu dveří
    public bool JsouDvereObsazene()
    {
        return jsouDvereObsazene;
    }

    public void ObsaditDvere()
    {
        jsouDvereObsazene = true;
    }

    public void UvolnitDvere()
    {
        jsouDvereObsazene = false;
    }

    public void AktualizovatStavDveri(bool stav)
    {
        dvereZamceny = stav;
        Debug.Log("Stav dveří: " + (stav ? "Zamčeno" : "Odemčeno"));
    }

    public float GetTimeRemaining()
    {
        return gameTime;
    }
}