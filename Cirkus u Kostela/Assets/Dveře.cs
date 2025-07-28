using UnityEngine;

public class Dvere : MonoBehaviour
{
    public float lockTime = 5f; // Doba, po kterou budou dveře zamčené
    public AudioClip lockSound; // Zvuk při zamčení
    public AudioClip unlockSound; // Zvuk při odemčení
    public AudioClip dvereObsazeneSound; // Zvuk, když jsou dveře obsazené
    public float robotTimeLimit = 10f; // Časový limit pro zamčení dveří, když je robot u dveří

    private bool zamceno = false; // Určuje, zda jsou dveře zamčené
    private float robotTimer = 0f; // Časovač pro robotův útok
    private AudioSource audioSource; // Komponenta pro přehrávání zvuků
    private bool hrajeZvukObsazeni = false; // Zabraňuje opakovanému přehrávání zvuku

    private void Start()
    {
        // Získání nebo přidání komponenty AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        // Kontrola, zda jsou dveře obsazené
        if (GameManager.Instance != null && GameManager.Instance.JsouDvereObsazene())
        {
            if (!hrajeZvukObsazeni)
            {
                // Přehrát zvuk obsazení dveří
                if (dvereObsazeneSound != null)
                {
                    audioSource.PlayOneShot(dvereObsazeneSound);
                    hrajeZvukObsazeni = true; // Zabrání opakovanému přehrávání
                }
                else
                {
                    Debug.LogWarning("Není nastaven zvuk obsazení dveří!");
                }
            }
        }
        else
        {
            hrajeZvukObsazeni = false; // Resetovat stav pro další přehrání
        }

        // Pokud je robot u dveří a dveře nejsou zamčené, spusťte časovač
        if (GameManager.Instance != null && GameManager.Instance.JsouDvereObsazene() && !zamceno)
        {
            robotTimer += Time.deltaTime;

            // Pokud čas vypršel, hráč prohrál
            if (robotTimer >= robotTimeLimit)
            {
                Debug.Log("💀 Robot tě dostal!");
                GameManager.Instance.LoseGame(); // Zavolá metodu pro prohru v GameManager
            }
        }
    }

    private void OnMouseDown()
    {
        if (!zamceno) // Pokud dveře nejsou zamčené, zamkni je
        {
            ZamknoutDvere();
        }
    }

    private void ZamknoutDvere()
    {
        zamceno = true;
        Debug.Log("Dveře byly zamčeny na " + lockTime + " sekund.");

        // Přehrát zvuk zamčení, pokud je nastaven
        if (lockSound != null)
        {
            audioSource.PlayOneShot(lockSound);
        }
        else
        {
            Debug.LogWarning("Není nastaven zvuk zamčení!");
        }

        // Informovat GameManager o stavu dveří
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AktualizovatStavDveri(zamceno);
        }
        else
        {
            Debug.LogError("GameManager není nalezen!");
        }

        // Spustit odemčení po uplynutí času
        Invoke("OdemknoutDvere", lockTime);
    }

    private void OdemknoutDvere()
    {
        zamceno = false;
        Debug.Log("Dveře byly odemčeny.");

        // Přehrát zvuk odemčení, pokud je nastaven
        if (unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
        else
        {
            Debug.LogWarning("Není nastaven zvuk odemčení!");
        }

        // Informovat GameManager o stavu dveří
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AktualizovatStavDveri(zamceno);
        }
        else
        {
            Debug.LogError("GameManager není nalezen!");
        }
    }
}