using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foxy : MonoBehaviour
{
    public enum RobotActionType { GoToGuard, Escape, Wait, CustomAction }

    [System.Serializable]
    public class PathPoint
    {
        public Transform position;
        public AnimationClip animationClip;
        public float baseWaitTime = 1f;
        public bool loopAnimation = false;
        public bool isLastWaypoint = false;
    }

    [System.Serializable]
    public class RobotAction
    {
        public RobotActionType actionType;
        public List<PathPoint> path;
    }

    public List<RobotAction> actionsQueue = new List<RobotAction>();
    public Animation animationComponent;
    public AudioClip doorAudioClip;
    public AudioClip jumpscareSound;
    public float doorHoldTime = 3f;
    public float robotStayTime = 2f;
    public float jumpscareDuration = 2f;
    public Transform jumpscareWaypoint;
    public AnimationClip jumpscareAnimation;
    public AnimationClip defaultAnimation; // ✅ Výchozí animace, nastav přes Unity Inspector!

    private int currentActionIndex = 0;
    private int currentPathIndex = 0;
    private Coroutine currentCoroutine;
    private bool isAtDoor = false;
    private Dictionary<Transform, Vector3> defaultPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Quaternion> defaultRotations = new Dictionary<Transform, Quaternion>();

    void Start()
    {
        foreach (Transform child in transform)
        {
            defaultPositions[child] = child.localPosition;
            defaultRotations[child] = child.localRotation;
        }
        if (animationComponent == null)
        {
            animationComponent = GetComponent<Animation>();
            if (animationComponent == null)
            {
                Debug.LogError("Animation component není nalezen!");
            }
        }

        StartNextAction();
    }

    private void StartNextAction()
    {
        if (currentActionIndex >= actionsQueue.Count)
        {
            currentActionIndex = 0;
        }

        RobotAction currentAction = actionsQueue[currentActionIndex];
        currentPathIndex = 0;

        switch (currentAction.actionType)
        {
            case RobotActionType.GoToGuard:
            case RobotActionType.Escape:
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                currentCoroutine = StartCoroutine(FollowPath(currentAction));
                break;

            case RobotActionType.Wait:
                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }
                currentCoroutine = StartCoroutine(WaitCoroutine(1f));
                break;

            case RobotActionType.CustomAction:
                Debug.Log(gameObject.name + " provádí vlastní akci.");
                break;
        }
    }

    private IEnumerator FollowPath(RobotAction action)
    {
        if (action.path.Count == 0)
        {
            Debug.LogWarning(gameObject.name + ": Cesta je prázdná!");
            NextAction();
            yield break;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager není inicializován!");
            yield break;
        }

        while (currentPathIndex < action.path.Count)
        {
            PathPoint point = action.path[currentPathIndex];

            if (point.position == null)
            {
                Debug.LogWarning("PathPoint.position není nastaven!");
                currentPathIndex++;
                continue;
            }

            // Resetování transformací robota před změnou pozice
            ResetRobotTransform();

            // Nastavení nové pozice
            transform.position = point.position.position;

            if (animationComponent != null && point.animationClip != null)
            {
                PlayAnimation(point.animationClip, point.loopAnimation);
            }

            float adjustedWaitTime = point.baseWaitTime / GameManager.Instance.GetMoveSpeed();
            yield return new WaitForSeconds(adjustedWaitTime);

            if (point.isLastWaypoint)
            {
                if (action.actionType == RobotActionType.Escape)
                {
                    GameManager.Instance.LoseGame();
                    yield break;
                }
                else if (action.actionType == RobotActionType.GoToGuard)
                {
                    while (GameManager.Instance.JsouDvereObsazene())
                    {
                        yield return new WaitForSeconds(0.1f);
                    }

                    GameManager.Instance.ObsaditDvere();
                    isAtDoor = true;

                    yield return StartCoroutine(DoorMiniGame());

                    GameManager.Instance.UvolnitDvere();
                    isAtDoor = false;
                }
            }

            currentPathIndex++;
        }

        NextAction();
    }

    private IEnumerator DoorMiniGame()
    {
        float timer = 0f;
        float requiredTime = 2f; // Požadovaná doba, po kterou musí být dveře zamčené

        // Po dobu 2 sekund kontrolujeme stav dveří
        while (timer < requiredTime)
        {
            if (!GameManager.Instance.dvereZamceny)
            {
                Debug.Log("Dveře nejsou zamčené! Jumpscare!");
                yield return StartCoroutine(Jumpscare());
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Dveře zůstaly zamčené. Robot odchází.");
    }

    private IEnumerator Jumpscare()
    {
        if (jumpscareWaypoint != null)
        {
            transform.position = jumpscareWaypoint.position;
            transform.rotation = jumpscareWaypoint.rotation;
        }

        if (jumpscareSound != null)
        {
            AudioSource.PlayClipAtPoint(jumpscareSound, transform.position);
        }

        PohybHrace pohybHrace = FindFirstObjectByType<PohybHrace>(); // ✅ Opraveno
        if (pohybHrace != null)
        {
            pohybHrace.ZaseknoutHrace(true);
            pohybHrace.OtočitHráčeKRobotovi(transform);
        }

        if (animationComponent != null && jumpscareAnimation != null)
        {
            animationComponent.Play(jumpscareAnimation.name);
        }

        yield return new WaitForSeconds(jumpscareDuration);

        if (pohybHrace != null)
        {
            pohybHrace.ZaseknoutHrace(false);
        }

        GameManager.Instance.LoseGame();
    }

    public bool IsAtDoor()
    {
        return isAtDoor;
    }

    private IEnumerator WaitCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        NextAction();
    }

    private void PlayAnimation(AnimationClip clip, bool loop)
    {
        if (animationComponent != null)
        {
            // Resetování transformací robota před přehráním animace
            ResetRobotTransform();

            // Nejprve přehrát výchozí animaci, pokud je nastavena
            if (defaultAnimation != null)
            {
                animationComponent.Stop();
                animationComponent.clip = defaultAnimation;
                animationComponent[defaultAnimation.name].wrapMode = WrapMode.Loop;
                animationComponent.Play();
            }

            // Pokud je definována nová animace, přehrajeme ji po výchozí animaci
            if (clip != null)
            {
                animationComponent.Stop();
                animationComponent.clip = clip;
                animationComponent[clip.name].time = 0f;
                animationComponent[clip.name].wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
                animationComponent.Play();
            }
        }
    }

    private void ResetRobotTransform()
    {
        // Zastavení všech animací
        if (animationComponent != null)
        {
            animationComponent.Stop();
        }

        // Resetování všech částí těla na jejich výchozí pozice a rotace
        foreach (Transform child in transform)
        {
            if (defaultPositions.ContainsKey(child))
            {
                child.localPosition = defaultPositions[child];
                child.localRotation = defaultRotations[child];
            }
        }
    }

    private void NextAction()
    {
        currentActionIndex++;

        if (currentActionIndex >= actionsQueue.Count)
        {
            currentActionIndex = 0;
        }

        StartNextAction();
    }

    public bool IsAtFirstWaypoint()
    {
        return currentActionIndex == 0 && currentPathIndex == 0;
    }

    // Přidána chybějící metoda ResetToNextAction
    public void ResetToNextAction()
    {
        currentActionIndex = 0;
        currentPathIndex = 0;
        StartNextAction();
    }
}
