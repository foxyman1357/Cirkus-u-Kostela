using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Laboři : MonoBehaviour
{
    public enum TypLaboře
    {
        Foxy,
        Myval,
        Cenda,
        Zabak
    }

    [System.Serializable]
    public class WaypointData
    {
        public Transform transform;
        public string animace;
        public bool jeKonecHry = false; // ✅ Přidáno
    }

    [Header("Typ robota")]
    public TypLaboře typ = TypLaboře.Foxy;

    [Header("Cesty pro Standardní robota")]
    public List<WaypointData> cestaPredResetem = new List<WaypointData>();
    public List<WaypointData> cestaPoResetu = new List<WaypointData>();

    [Header("Waypoints pro jiné typy")]
    public List<WaypointData> waypointy = new List<WaypointData>();

    [Header("Pro Strašidelného robota")]
    public List<WaypointData> loopWaypoints = new List<WaypointData>();

    [Header("Zvuky")]
    public AudioClip knockSound;
    private AudioSource audioSource;

    private int currentWaypointIndex = 0;
    private int direction = 1;
    private bool resetovany = false;

    [Header("Pohyb")]
    public float moveDelay = 10f;
    private float moveTimer = 0f;
    private bool isResetting = false;

    private Animator animator;

    [Header("Dveřní chování")]
    public float watchTimeToJumpscare = 1f;

    private bool isWatching = false;
    private float watchTimer = 0f;

    private bool doorEffectsPlayed = false;
  
    [Header("Typ prohry při jumpscare")]
    public TypProhry typProhry = TypProhry.Klasická;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        var cesta = GetAktualniCesta();
        if (cesta.Count > 0 && cesta[0].transform != null)
        {
            transform.position = cesta[0].transform.position;
            PlayWaypointAnimation(cesta[0].animace);
        }
    }

    void Update()
    {
        // ✅ Ovládání aktivace robotů podle noci
        int noc = GameManager.Instance != null ? GameManager.Instance.sunday : 1;

        // Foxy se hýbe vždy (noc 1+), Cenda od noci 2, ostatní od noci 3
        if ((typ == TypLaboře.Foxy && noc < 1) ||
            (typ == TypLaboře.Cenda && noc < 2) ||
            ((typ == TypLaboře.Myval || typ == TypLaboře.Zabak) && noc < 3))
        {
            return; // Ještě není čas tohoto robota
        }

        if (isResetting) return;

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveDelay)
        {
            MoveToNextWaypoint();
            moveTimer = 0f;
        }

        // TEST: simulace dveří (pro testování, nahradíš reálnou detekcí)
        if (isWatching)
        {
            bool isDoorClosed = false; // Změň podle potřeby
            UpdateDoorWatch(isDoorClosed, Time.deltaTime, null);
        }
    }

    private void MoveToNextWaypoint()
    {
        var cesta = GetAktualniCesta();
        if (cesta.Count == 0) return;

        if (typ == TypLaboře.Cenda && !resetovany)
        {
            currentWaypointIndex += direction;

            if (currentWaypointIndex >= cesta.Count)
            {
                direction = -1;
                currentWaypointIndex = cesta.Count - 2;
            }
            else if (currentWaypointIndex < 0)
            {
                direction = 1;
                currentWaypointIndex = 1;
            }

            var waypoint = cesta[currentWaypointIndex];
            transform.position = waypoint.transform.position;
            PlayWaypointAnimation(waypoint.animace);
            TriggerScaryEffect();
            return;
        }

        currentWaypointIndex++;

        if (currentWaypointIndex < cesta.Count)
        {
            var waypoint = cesta[currentWaypointIndex];
            transform.position = waypoint.transform.position;
            PlayWaypointAnimation(waypoint.animace);

            // ✅ Pokud waypoint má být konec hry:
            if (waypoint.jeKonecHry)
            {
                Debug.Log($"{name} dosáhl koncového bodu – spouštím konec hry (prohra).");
                TriggerGameOver();
                return;
            }

            if (IsAtLastWaypoint())
            {
                StartWatchingAtDoor();
            }

            if (typ == TypLaboře.Myval)
            {
                moveTimer = +moveDelay * 0.5f;
            }

            Debug.Log($"{name} ({typ}) se přesunul na waypoint {currentWaypointIndex}");
        }
        else
        {
            Debug.Log($"{name} dosáhl cílového bodu.");
        }
    }

    private void TriggerGameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseGame();
        }
        else
        {
            Debug.LogWarning("GameManager nebyl nalezen – nemohu spustit prohru.");
        }
    }

    private void PlayWaypointAnimation(string animace)
    {
        if (animator != null && !string.IsNullOrEmpty(animace))
        {
            animator.speed = 1f;
            animator.Play(animace);
            Debug.Log($"{name} spustil animaci '{animace}'");
        }
    }

    private List<WaypointData> GetAktualniCesta()
    {
        if (typ == TypLaboře.Foxy)
            return resetovany ? cestaPoResetu : cestaPredResetem;
        else if (typ == TypLaboře.Cenda && !resetovany)
            return loopWaypoints;
        else
            return waypointy;
    }

    public bool IsAtFirstWaypoint()
    {
        return currentWaypointIndex == 0;
    }

    public bool IsAtLastWaypoint()
    {
        var cesta = GetAktualniCesta();
        return currentWaypointIndex == cesta.Count - 1;
    }

    public void ResetToNextAction()
    {
        resetovany = true;
        currentWaypointIndex = 0;
        direction = 1;

        if (typ == TypLaboře.Cenda)
        {
            typ = TypLaboře.Foxy;
        }

        var cesta = GetAktualniCesta();
        if (cesta.Count > 0 && cesta[0].transform != null)
        {
            transform.position = cesta[0].transform.position;
            PlayWaypointAnimation(cesta[0].animace);
        }

        moveTimer = 0f;
        isResetting = false;

        Debug.Log($"{name} byl resetován na začátek a nyní používá {(resetovany ? "cestuPoResetu" : "loopWaypoints")}.");
    }

    public Transform GetCurrentWaypoint()
    {
        var cesta = GetAktualniCesta();
        if (currentWaypointIndex >= 0 && currentWaypointIndex < cesta.Count)
            return cesta[currentWaypointIndex].transform;

        return null;
    }

    private void TriggerScaryEffect()
    {
        Debug.Log($"{name} spouští strašidelný efekt na waypointu {currentWaypointIndex}");
    }

    public void StartWatchingAtDoor()
    {
        if (!isWatching)
        {
            isWatching = true;
            watchTimer = 0f;

            doorEffectsPlayed = false;

            var cesta = GetAktualniCesta();
            if (currentWaypointIndex < cesta.Count)
            {
                PlayWaypointAnimation(cesta[currentWaypointIndex].animace);
            }

            if (!doorEffectsPlayed && audioSource != null && knockSound != null)
            {
                audioSource.PlayOneShot(knockSound);
                doorEffectsPlayed = true;
            }

            Debug.Log($"{name} sleduje dveře...");
        }
    }

    public void UpdateDoorWatch(bool isDoorClosed, float deltaTime, System.Action onJumpscare)
    {
        if (!isWatching || animator == null) return;

        if (isDoorClosed)
        {
            watchTimer += deltaTime;

            if (watchTimer >= 1f) // <- hráč udržel dveře zavřené 1 sekundu
            {
                Debug.Log($"{name} byl zablokován dveřmi. Resetuji robota.");
                StopWatchingAtDoor();
                ResetToNextAction();
            }
        }
        else
        {
            // dveře nejsou zavřené, tak čas běží k jumpscare
            watchTimer += deltaTime;

            if (watchTimer >= watchTimeToJumpscare)
            {
                Debug.Log($"{name} provedl jumpscare (hráč nereagoval).");
                isWatching = false;
                doorEffectsPlayed = false;

                TriggerSceneJumpscare();

                // volitelný callback (zvenku)
                onJumpscare?.Invoke();
            }
        }
    }

    public void StopWatchingAtDoor()
    {
        isWatching = false;
        watchTimer = 0f;

        doorEffectsPlayed = false;

        if (animator != null)
            animator.speed = 1f;

        currentWaypointIndex = 0;
        resetovany = false;
        transform.position = GetAktualniCesta()[0].transform.position;
        PlayWaypointAnimation(GetAktualniCesta()[0].animace);

        Debug.Log($"{name} byl odražen – návrat na začátek.");
    }

    public void TriggerSceneJumpscare()
    {
        Debug.Log($"{name} spouští scénický jumpscare!");

        GameManager.Instance.SpustitProhru(typProhry);
    }
}
