using UnityEngine;
using System.Collections.Generic;

public class Dveře : MonoBehaviour
{
    [Header("Kamery")]
    public Camera defaultCamera;
    public Camera closeupCamera;

    [Header("Dveře (otočné)")]
    public Transform doorTransform;
    public Vector3 openRotationEuler;
    public Vector3 closedRotationEuler;
    public float rotationSpeed = 5f;
    public KeyCode holdKey = KeyCode.LeftShift;

    [Header("Nepřátelé")]
    public List<Laboři> nepratele;
    public Transform doorWaypoint;

    [Header("Nastavení")]
    public float autoReturnTime = 10f;
    public float timeToHoldDoor = 1f;

    private float doorHoldTimer = 0f;
    private bool isCloseupActive = false;
    private float inactivityTimer = 0f;

    private Quaternion openRotation;
    private Quaternion closedRotation;

    // Nové proměnné pro obranu dveří
    private bool robotJeUtechDveri = false;
    private float timeSinceRobotArrived = 0f;
    private bool podminkySplneny = false;

    void Start()
    {
        if (doorTransform == null) doorTransform = this.transform;

        openRotation = Quaternion.Euler(openRotationEuler);
        closedRotation = Quaternion.Euler(closedRotationEuler);

        doorTransform.localRotation = openRotation;

        SetToDefaultCamera();
    }

    void Update()
    {
        bool isHolding = Input.GetKey(holdKey);
        Quaternion targetRot = isHolding ? closedRotation : openRotation;
        doorTransform.localRotation = Quaternion.Lerp(doorTransform.localRotation, targetRot, Time.deltaTime * rotationSpeed);

        Laboři nepritelZaDvermi = GetEnemyAtDoor();

        if (nepritelZaDvermi != null)
        {
            nepritelZaDvermi.UpdateDoorWatch(isHolding && isCloseupActive, Time.deltaTime, () =>
            {
                Debug.Log("Nepřítel provedl jumpscare přes dveře!");
                TriggerJumpscare(nepritelZaDvermi);
            });// Robot právě dorazil k dveřím
            if (!robotJeUtechDveri)
            {
                robotJeUtechDveri = true;
                timeSinceRobotArrived = 0f;
                podminkySplneny = false;
                doorHoldTimer = 0f;

                Debug.Log("Nepřítel dorazil k dveřím. Máš 5 sekund na jejich uzavření!");
            }

            timeSinceRobotArrived += Time.deltaTime;

            // Sleduj, jestli hráč drží dveře
            if (isHolding && isCloseupActive)
            {
                doorHoldTimer += Time.deltaTime;

                if (doorHoldTimer >= timeToHoldDoor && !podminkySplneny)
                {
                    podminkySplneny = true;
                    Debug.Log("Dveře byly udrženy zavřené dostatečně dlouho. Hráč přežil útok.");
                    nepritelZaDvermi.StopWatchingAtDoor();
                    robotJeUtechDveri = false;
                }
            }
            else
            {
                doorHoldTimer = 0f;
            }

            // Pokud uplynulo 5 sekund a podmínky nebyly splněny, spustit jumpscare
            if (timeSinceRobotArrived >= 5f && !podminkySplneny)
            {
                Debug.Log("Hráč nezavřel dveře včas. Spouštím jumpscare.");
                TriggerJumpscare(nepritelZaDvermi);
                robotJeUtechDveri = false;
            }
        }
        else
        {
            // Reset, pokud u dveří není nepřítel
            robotJeUtechDveri = false;
            timeSinceRobotArrived = 0f;
            podminkySplneny = false;
            doorHoldTimer = 0f;
        }

        // Kamera: návrat po neaktivitě
        inactivityTimer = isHolding ? 0f : inactivityTimer + Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape) || inactivityTimer >= autoReturnTime)
        {
            SetToDefaultCamera();
        }
    }

    void OnMouseDown()
    {
        if (!isCloseupActive)
        {
            SetToCloseupCamera();
        }
    }

    void SetToCloseupCamera()
    {
        if (defaultCamera != null) defaultCamera.enabled = false;
        if (closeupCamera != null) closeupCamera.enabled = true;

        isCloseupActive = true;
        inactivityTimer = 0f;
    }

    void SetToDefaultCamera()
    {
        if (defaultCamera != null) defaultCamera.enabled = true;
        if (closeupCamera != null) closeupCamera.enabled = false;

        isCloseupActive = false;
        inactivityTimer = 0f;
        doorHoldTimer = 0f;

        if (doorTransform != null)
        {
            doorTransform.localRotation = openRotation;
        }
    }

    Laboři GetEnemyAtDoor()
    {
        foreach (var nepritel in nepratele)
        {
            if (nepritel != null && nepritel.GetCurrentWaypoint() == doorWaypoint)
            {
                return nepritel;
            }
        }
        return null;
    }

    void TriggerJumpscare(Laboři nepritel)
    {
        Debug.Log($"Jumpscare! {nepritel.name} tě dostal za dveřmi!");
        nepritel.TriggerSceneJumpscare();
    }
}
