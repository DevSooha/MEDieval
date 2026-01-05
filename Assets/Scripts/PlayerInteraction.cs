using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        FindUIAndLinkButtons();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isCampfire = false;
        canInteract = false;
        currentNPC = null;
        currentItem = null;

        FindUIAndLinkButtons();
    }

    void FindUIAndLinkButtons()
    {
        if (craftUIWindow != null)
        {
            craftUIWindow.SetActive(false);
        }

        if (craftUIWindow == null)
        {
            GameObject foundObj = GameObject.Find("CraftUIWindow");
            if (foundObj == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                {
                    Transform t = canvas.transform.Find("CraftUIWindow");
                    if (t != null) foundObj = t.gameObject;
                }
            }
            craftUIWindow = foundObj;
        }

        if (craftUIWindow != null)
        {
            Button btnCraft = craftUIWindow.transform.Find("Btn_Craft")?.GetComponent<Button>();
            Button btnPotion = craftUIWindow.transform.Find("Btn_Potion")?.GetComponent<Button>();

            if (btnCraft != null)
            {
                btnCraft.onClick.RemoveAllListeners();
                btnCraft.onClick.AddListener(GoCrafting);
            }

            if (btnPotion != null)
            {
                btnPotion.onClick.RemoveAllListeners();
                btnPotion.onClick.AddListener(GoPotion);
            }

            craftUIWindow.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.F))
        {
            if (!canInteract) return;

            if (Player.Instance != null) Player.Instance.CancelAttack();

            // 1순위: 아이템 줍기 (가장 먼저 체크)
            if (currentItem != null)
            {
                PickUpItem();
                return;
            }

            // 2순위: NPC 대화 (순서를 위로 올림!)
            if (currentNPC != null)
            {
                if (DialogueManager.Instance == null) return;

                // 대화 시작 혹은 진행
                if (!DialogueManager.Instance.IsDialogueActive())
                    StartDialogue();
                else
                    DialogueManager.Instance.AdvanceDialogue();

                return;
            }

            // 3순위: 모닥불 (NPC가 없을 때만 실행됨)
            if (isCampfire)
            {
                if (craftUIWindow != null)
                {
                    bool isActive = craftUIWindow.activeSelf;
                    craftUIWindow.SetActive(!isActive);
                }
                else
                {
                    FindUIAndLinkButtons();
                    if (craftUIWindow != null) craftUIWindow.SetActive(true);
                }
            }
        }
    }

    public void GoPotion() { StartCoroutine(LoadSceneWithFade("Potions")); }
    public void GoCrafting() { StartCoroutine(LoadSceneWithFade("Crafting")); }

    IEnumerator LoadSceneWithFade(string sceneName)
    {
        if (Player.Instance != null)
        {
            Player.Instance.SetCanMove(false);
            Player.Instance.SaveCurrentPosition();
        }
        if (UIManager.Instance != null) yield return StartCoroutine(UIManager.Instance.FadeOut(0.3f));
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
        Debug.Log($"충돌 감지됨: {other.name} / 태그: {other.tag}");

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