using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class Hořeni : MonoBehaviour
{
    public GameObject badfilm;
    public GameObject replacementPartObjekt;
    public Transform replacementSpawnLocation;
    public Transform Ruka; 
    public VisualEffect fireEffect; 
    public GameObject img;

    private bool isHoldingPart = false;
    private GameObject heldPart;
    private bool isBrokenPartHidden = false;
    private Stack<GameObject> replacementPartsStack = new Stack<GameObject>();
    

    private bool isInitialSetupDone = false;

    void Start()
    {
        
        if (!isInitialSetupDone)
        {
            InitialSetup();
        }
    }

    void OnEnable()
    {
        if (isInitialSetupDone)
        {
            fireEffect.enabled = true;
            AttachFireEffect();
        }
    }

    void Update()
    {
        if (!isInitialSetupDone) return;

        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                
                if (hit.collider.gameObject == badfilm && !isHoldingPart && !isBrokenPartHidden)
                {
                    HideBrokenPart();
                }
                
                else if (hit.collider.gameObject.CompareTag("ReplacementPart") && isBrokenPartHidden && !isHoldingPart)
                {
                    PickUpReplacementPart(hit.collider.gameObject);
                }
                
                else if (hit.collider.gameObject == badfilm && isHoldingPart)
                {
                    opravit();
                }
            }
        }

        
        if (isHoldingPart && heldPart != null)
        {
            UpdateHeldPartPosition();
        }
    }

    void InitialSetup()
    {
       
        SpawnInitialReplacementParts(5); 
        isInitialSetupDone = true;
        Debug.Log("Initial setup completed.");
    }

    void AttachFireEffect()
    {
        if (fireEffect != null && badfilm != null)
        {
            
            img.SetActive(true);
            fireEffect.transform.SetParent(badfilm.transform);
            fireEffect.transform.localPosition = Vector3.zero; 
            fireEffect.Play();
        }
    }

    void HideBrokenPart()
    {
        MeshRenderer renderer = badfilm.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
            isBrokenPartHidden = true;
        }

        // Disable the fire effect
        if (fireEffect != null)
        {
            fireEffect.Stop();
        }
    }

    void SpawnInitialReplacementParts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float randomOffsetX = Random.Range(-0.2f, 0.2f);
            Vector3 spawnPosition = replacementSpawnLocation.position + new Vector3(randomOffsetX, i * 0.1f, 0); 
            GameObject spawnedPart = Instantiate(replacementPartObjekt, spawnPosition, Quaternion.identity);
            spawnedPart.tag = "ReplacementPart";
            spawnedPart.transform.Rotate(90f, 0f, 0f); 
            replacementPartsStack.Push(spawnedPart); 
        }
    }

    void PickUpReplacementPart(GameObject part)
    {
        // Only pick the top replacement part
        if (replacementPartsStack.Count > 0 && part == replacementPartsStack.Peek())
        {
            isHoldingPart = true;
            heldPart = replacementPartsStack.Pop(); 
            heldPart.SetActive(true); 
        }
    }

    void UpdateHeldPartPosition ()
    {
        
        heldPart.transform.position = Ruka.position;
        heldPart.transform.rotation = Ruka.rotation;
    }

    void opravit()
    {
        
        int casDoPoruchy = Random.Range(1, 7);
        if (casDoPoruchy != 0)
        {
            casDoPoruchy--;
            MeshRenderer renderer = badfilm.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }
            fireEffect.enabled = false;
            Destroy(heldPart);
            isHoldingPart = false;
            heldPart = null;
            isBrokenPartHidden = false;
            img.SetActive(false);
            this.enabled = false;
        }
    }
}
