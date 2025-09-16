using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ArtifactManager : MonoBehaviour
{
    [Header("Artifact Config")]
    [SerializeField] private List<ArtifactSO> temporaryPool; // run-only buffs
    [SerializeField] private Image[] artifactSlotImages;
    [SerializeField] private GameObject artifactUIPanel;

    // New: permanent reward definitions
    [Header("Permanent Pool Config")]
    [SerializeField] private Sprite flowerIcon;
    [SerializeField] private Sprite leafIcon;
    [SerializeField] private Sprite waterIcon;
    [SerializeField] private Sprite fruitIcon;
    [SerializeField] private Sprite rootIcon;
    [SerializeField] private Sprite keyIcon;

    [Header("Audio")]
    [SerializeField] private AudioClip navigateClip;
    [SerializeField] private AudioClip confirmClip;

    private AudioSource audioSource;
    private ArtifactChoice[] currentChoices = new ArtifactChoice[2];
    private int currentSelectionIndex = -1;
    private bool selectionActive = false;
    private RoomManager roomManagerRef;

    public float selectScale = 1.2f;
    public float normalScale = 1f;

    // Permanent pool tracking (cap-based)
    private Dictionary<PermanentItemType, int> permanentCollected = new Dictionary<PermanentItemType, int>()
    {
        { PermanentItemType.Flower, 0 },
        { PermanentItemType.Leaf, 0 },
        { PermanentItemType.Water, 0 },
        { PermanentItemType.Fruit, 0 },
        { PermanentItemType.Root, 0 },
        { PermanentItemType.WeaponKey, 0 }
    };

    private const int MAX_PER_STAT = 5;
    private const int MAX_KEYS = 2;

    public void ShowArtifactChoices(RoomManager roomManager)
    {
        roomManagerRef = roomManager;
        artifactUIPanel.SetActive(true);
        PlayerMovement.Instance.canMove = false;

        // Fill 2 choices
        FillChoices();

        StartCoroutine(EnableSelectionWithDelay(0.5f));
    }

    private void FillChoices()
    {
        // --- Choice 1 = Permanent if available, otherwise fallback to temporary ---
        ArtifactChoice permanentChoice = GetPermanentChoice();
        if (permanentChoice == null)
            permanentChoice = new ArtifactChoice { isPermanent = false, temporaryArtifact = GetRandomFromPool(temporaryPool) };

        currentChoices[0] = permanentChoice;
        artifactSlotImages[0].sprite = permanentChoice.GetIcon(flowerIcon, leafIcon, waterIcon, fruitIcon, rootIcon, keyIcon);
        artifactSlotImages[0].transform.localScale = Vector3.one * normalScale;

        // --- Choice 2 = Temporary ---
        ArtifactSO tempChoice = GetRandomFromPool(temporaryPool);
        currentChoices[1] = new ArtifactChoice { isPermanent = false, temporaryArtifact = tempChoice };
        artifactSlotImages[1].sprite = tempChoice.artifactIcon;
        artifactSlotImages[1].transform.localScale = Vector3.one * normalScale;
    }

    private ArtifactChoice GetPermanentChoice()
    {
        List<PermanentItemType> eligible = new List<PermanentItemType>();

        foreach (PermanentItemType type in permanentCollected.Keys)
        {
            int collected = permanentCollected[type];
            int maxCap = (type == PermanentItemType.WeaponKey) ? MAX_KEYS : MAX_PER_STAT;

            if (collected < maxCap)
                eligible.Add(type);
        }

        if (eligible.Count == 0) return null;

        PermanentItemType selected = eligible[Random.Range(0, eligible.Count)];
        return new ArtifactChoice { isPermanent = true, permanentType = selected };
    }

    private ArtifactSO GetRandomFromPool(List<ArtifactSO> pool)
    {
        if (pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    private IEnumerator EnableSelectionWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        selectionActive = true;
        currentSelectionIndex = 0;
        UpdateUISelection();
    }

    private void Update()
    {
        if (!selectionActive) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            currentSelectionIndex = Mathf.Max(0, currentSelectionIndex - 1);
            UpdateUISelection();
            PlayNavigateSound();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            currentSelectionIndex = Mathf.Min(artifactSlotImages.Length - 1, currentSelectionIndex + 1);
            UpdateUISelection();
            PlayNavigateSound();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
            PlayConfirmSound();
        }
    }

    private void UpdateUISelection()
    {
        for (int i = 0; i < artifactSlotImages.Length; i++)
        {
            artifactSlotImages[i].transform.localScale = (i == currentSelectionIndex)
                ? Vector3.one * selectScale
                : Vector3.one * normalScale;
        }
    }

    private void ConfirmSelection()
    {
        ArtifactChoice selected = currentChoices[currentSelectionIndex];
        Debug.Log("Artifact Selected: " + selected.GetName());

        if (selected.isPermanent)
        {
            permanentCollected[selected.permanentType]++;
            PlayerInventory.Instance.AddPermanentItem(selected.permanentType);
            Debug.Log($"[ArtifactManager] Collected permanent item {selected.permanentType}, total = {permanentCollected[selected.permanentType]}");
        }
        else
        {
            selected.temporaryArtifact.ApplyEffect();
        }

        selectionActive = false;
        PlayerMovement.Instance.canMove = true;
        artifactUIPanel.SetActive(false);

        roomManagerRef.OnArtifactChosen();
    }
        private void PlayNavigateSound()
    {
        if (navigateClip != null)
            audioSource.PlayOneShot(navigateClip, 0.6f);
    }

    private void PlayConfirmSound()
    {
        if (confirmClip != null)
            audioSource.PlayOneShot(confirmClip, 0.8f);
    }
}
