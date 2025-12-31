using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for Button

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;

    [Header("UI Reference (Auto-found)")]
    public GameObject craftUIWindow;

    private CircleCollider2D interactionCollider;
    private NPC currentNPC;
    private WorldItem currentItem;

    private bool canInteract = false;
    private bool isCampfire = false;

    public bool IsInteractable => canInteract;

    // Register Scene Loaded Event
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        interactionCollider = GetComponent<CircleCollider2D>();
        interactionCollider.radius = interactionRadius;
        interactionCollider.isTrigger = true;

        FindUIAndLinkButtons(); // Initial find
    }

    // Called every time a scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Reset Interaction States
        isCampfire = false;
        canInteract = false;
        currentNPC = null;
        currentItem = null;

        // 2. Find UI and Re-link Buttons
        FindUIAndLinkButtons();
    }

    void FindUIAndLinkButtons()
    {
        // If already connected and active, just disable and return
        if (craftUIWindow != null)
        {
            craftUIWindow.SetActive(false);
            // We still need to check buttons just in case, but usually finding the window is enough if references held
            // However, safe to re-find if null
        }

        // Try to find the UI Window even if it's inactive
        if (craftUIWindow == null)
        {
            GameObject foundObj = GameObject.Find("CraftUIWindow");
            if (foundObj == null)
            {
                // Fallback: Search inside Canvas
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    Transform t = canvas.transform.Find("CraftUIWindow");
                    if (t != null) foundObj = t.gameObject;
                }
            }
            craftUIWindow = foundObj;
        }

        // Link Buttons if window is found
        if (craftUIWindow != null)
        {
            // Important: Names "Btn_Craft" and "Btn_Potion" must match your Hierarchy exactly!
            Button btnCraft = craftUIWindow.transform.Find("Btn_Craft")?.GetComponent<Button>();
            Button btnPotion = craftUIWindow.transform.Find("Btn_Potion")?.GetComponent<Button>();

            if (btnCraft != null)
            {
                btnCraft.onClick.RemoveAllListeners(); // Clear old links
                btnCraft.onClick.AddListener(GoCrafting); // Link new method
            }

            if (btnPotion != null)
            {
                btnPotion.onClick.RemoveAllListeners();
                btnPotion.onClick.AddListener(GoPotion);
            }

            craftUIWindow.SetActive(false); // Ensure it starts hidden
        }
    }

    // ... (Update, OnTriggerEnter/Exit, LoadSceneWithFade, etc. remain the same)

    // Update Method for Interaction
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.F))
        {
            if (!canInteract) return;

            if (Player.Instance != null) Player.Instance.CancelAttack();

            if (currentItem != null)
            {
                PickUpItem();
                return;
            }

            if (isCampfire)
            {
                if (craftUIWindow != null)
                {
                    bool isActive = craftUIWindow.activeSelf;
                    craftUIWindow.SetActive(!isActive);
                }
                else
                {
                    // Fail-safe re-find
                    FindUIAndLinkButtons();
                    if (craftUIWindow != null) craftUIWindow.SetActive(true);
                }
                return;
            }

            if (currentNPC != null)
            {
                // Dialogue logic...
                if (DialogueManager.Instance == null) return;
                if (!DialogueManager.Instance.IsDialogueActive())
                    StartDialogue();
                else
                    DialogueManager.Instance.AdvanceDialogue();
            }
        }
    }

    // ... (Rest of your script methods: PickUpItem, StartDialogue, OnTriggerEnter/Exit) ...

    public void GoPotion() { StartCoroutine(LoadSceneWithFade("Potions")); }
    public void GoCrafting() { StartCoroutine(LoadSceneWithFade("Crafting")); }

    IEnumerator LoadSceneWithFade(string sceneName)
    {
        if (Player.Instance != null) Player.Instance.SaveCurrentPosition();
        if (FadeManager.Instance != null) yield return StartCoroutine(FadeManager.Instance.FadeOut(0.5f));
        SceneManager.LoadScene(sceneName);
    }

    void PickUpItem()
    {
        if (currentItem != null && Inventory.Instance != null)
        {
            if (Inventory.Instance.AddItem(currentItem.itemData, currentItem.quantity))
            {
                Destroy(currentItem.gameObject);
                currentItem = null;
                canInteract = false;
            }
        }
    }

    void StartDialogue()
    {
        Vector2 dir = (transform.position - currentNPC.transform.position).normalized;
        currentNPC.FaceDirection(dir);
        DialogueManager.Instance.StartDialogue(currentNPC.dialogueData);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<NPC>()) { currentNPC = other.GetComponent<NPC>(); canInteract = true; }
        else if (other.CompareTag("Campfire")) { isCampfire = true; canInteract = true; }
        else if (other.GetComponent<WorldItem>()) { currentItem = other.GetComponent<WorldItem>(); canInteract = true; }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<NPC>() == currentNPC) { currentNPC = null; }
        else if (other.CompareTag("Campfire"))
        {
            isCampfire = false;
            if (craftUIWindow != null) craftUIWindow.SetActive(false);
        }
        else if (other.GetComponent<WorldItem>() == currentItem) { currentItem = null; }

        if (currentNPC == null && !isCampfire && currentItem == null) canInteract = false;
    }
}