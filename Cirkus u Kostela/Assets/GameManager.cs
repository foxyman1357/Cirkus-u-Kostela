using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameLevel { Level1, Level2, Level3 }
    public GameLevel currentLevel = GameLevel.Level1;

    public int sunday = 1; // Aktuální noc (1 = první noc, 2 = druhá noc, atd.) // UPRAVA

    private float gameTime = 0f;
    private float timeLimit = 120f; // Délka kola (5 minut)

    private float moveSpeed = 1f;
    private float speedIncreaseRate = 0.1f;
    private float maxSpeed = 10f;
    public bool dvereZamceny { get; private set; } // Přístup pouze pro čtení

    private bool gameWon = false;
    public bool canContinue = false; // Odemyká tlačítko Continue

    private bool jsouDvereObsazene = false; // Nová proměnná pro stav dveří

    public enum TypProhry
    {
        Klasická,
        FoxyJumpscare,
        MyvalJumpscare
    }

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

        // 🎮 Klávesová zkratka: Ctrl + S → přeskočí level
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("⏩ Přeskakuji level pomocí Ctrl+S");
            NextLevel();
            return;
        }

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
        canContinue = true;
        Debug.Log("🎉 Vyhrál jsi! Přesun do menu...");

        PlayerPrefs.SetInt("CanContinue", 1);
        SceneManager.LoadScene("Win");
    }

    public void LoseGame()
    {
        Debug.Log("💀 Prohrál jsi!");
        SceneManager.LoadScene("Death");
    }
    public void LoseGameFoxy()
    {
        Debug.Log("💀 Prohrál jsi!");
        SceneManager.LoadScene("Death 1");
    }
    public void LosegameMyval()
    {
        Debug.Log("💀 Prohrál jsi!");
        SceneManager.LoadScene("Death 2");
    }

    public void RestartGame()
    {
        Debug.Log("Restartování hry...");
        PlayerPrefs.SetInt("CanContinue", 0);
        gameTime = 0f;
        gameWon = false;
        canContinue = false;
        currentLevel = GameLevel.Level1;
        sunday = 1; // resetujeme noc na 1 // UPRAVA
        SceneManager.LoadScene("Game");
    }

    public void NewGame()
    {
        PlayerPrefs.SetInt("CanContinue", 0);
        gameTime = 0f;
        gameWon = false;
        canContinue = false;
        currentLevel = GameLevel.Level1;
        sunday = 1; // resetujeme noc na 1 // UPRAVA
        SceneManager.LoadScene("Game");
    }

    // Přidána metoda pro přechod na další noc
    public void NextNight() // UPRAVA
    {
        sunday++;
        gameTime = 0f;
        Debug.Log("Přechod na noc: " + sunday);
    }

    // Upravena metoda NextLevel - nastaví noc na 1
    public void NextLevel()
    {
        if (currentLevel == GameLevel.Level1)
        {
            currentLevel = GameLevel.Level2;
            sunday = 1;
        }
        else if (currentLevel == GameLevel.Level2)
        {
            currentLevel = GameLevel.Level3;
        }
        else
        {
            Debug.Log("Jsi na posledním levelu!");
            return;
        }

        PlayerPrefs.SetInt("CanContinue", 1); // ✅ ULOŽIT možnost pokračovat
        PlayerPrefs.Save();                   // ✅ Uložit trvale

        timeLimit += 60f;
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

    public void SpustitProhru(TypProhry typ)
    {
        switch (typ)
        {
            case TypProhry.Klasická:
                Debug.Log("Klasická prohra – načítám lose screen.");
                SceneManager.LoadScene("Death");
                break;

            case TypProhry.FoxyJumpscare:
                Debug.Log("Jumpscare prohra – přehrávám animaci na místě.");
                SceneManager.LoadScene("DeathFoxy");
                break;

            case TypProhry.MyvalJumpscare:
                Debug.Log("Cinematic prohra – přepínám na scénu s robotem.");
                SceneManager.LoadScene("DeathMyval");
                break;
        }
    }
}
