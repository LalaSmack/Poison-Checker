using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DishUI : MonoBehaviour
{
    [Header("UI References")]
    public Image foodImage;
    public TMPro.TextMeshProUGUI label;

    [Header("Optional visuals")]
    //public GameObject markedIcon;  
    //public GameObject revealedIcon;  // indicator for revealed state (to show result of tasting) 


    public FoodData data { get; private set; }
    public bool IsPoisoned { get; private set; }
    public bool IsMarked { get; private set; }
    public bool isRevealed { get; private set; }



    private RoundSpawner spawner;
    private Outline outline;
private Image backgroundImage;
private Coroutine flashRoutine;

[Header("Tint Colors")]
public Color defaultTint = Color.white;
public Color safeTint = new Color(0.6f, 1f, 0.6f);   // green
public Color poisonTint = new Color(1f, 0.3f, 0.3f); // brighter red for flash

    private void Awake()
    {
        
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

        backgroundImage = GetComponent<Image>();

        if (backgroundImage != null)
            defaultTint = backgroundImage.color;

    }
    public void Setup(FoodData foodData, RoundSpawner roundSpawner, bool isPoisoned)
    {
        spawner = roundSpawner;
        IsPoisoned = isPoisoned;
        data = foodData;

        foodImage.sprite = data.foodSprite;
        label.text = data.foodName;

        SetSelected(false);
        SetMarked(false);
        isRevealed = false;
    }
    
    public void OnDishClicked()
    {
        // Select this dish
        spawner.SelectDish(this);
        GameAudio.Instance.PlayClick();

    }

    public void SetSelected(bool selected)
    {
        Debug.Log($"Setting selected: {selected} for dish {data.foodName}");
        if (outline != null)
            outline.enabled = selected;
    }

    public void SetMarked(bool marked)
    {
        IsMarked = marked;
        if (IsMarked)
        {
            Debug.Log($"Applying poison tint to marked dish {data.foodName}");
            ApplyTint();
            }
        
    }


    public void Reveal()
    {
        isRevealed = true;
        if (IsPoisoned)
        {
            StartFlash(poisonTint);
        }
        else
        {
            StartFlash(safeTint);
        }
    }
    private void StartFlash(Color flashColor)
{
    if (flashRoutine != null)
        StopCoroutine(flashRoutine);

    flashRoutine = StartCoroutine(FlashRoutine(flashColor));
}

    private IEnumerator FlashRoutine(Color flashColor)
{
    if (backgroundImage == null) yield break;

    float flashDuration = 2f;
    int flashCount = 4;
    
    float flashInterval = flashDuration / (flashCount * 2); // on and off counts as 2

    for (int i = 0; i < flashCount; i++)
    {
        backgroundImage.color = flashColor;
        yield return new WaitForSeconds(flashInterval);
        ApplyTint(); // return to appropriate tint (marked/safe/default)
        yield return new WaitForSeconds(flashInterval);
    }
    
    ApplyTint();

    flashRoutine = null;
}
    private void ApplyTint()
    {
        if (backgroundImage == null) return;

            // Marked overrides everything
        if (IsMarked)
        {
            Debug.Log($"Applying poison tint to marked dish {data.foodName}");
            backgroundImage.color = poisonTint;
            return;
        }

        backgroundImage.color = defaultTint;
    }
    public void RevealPoisonResult()
    {
        // You can decide how you want to visually reveal:
        // show revealedIcon always, or only show if poisoned, etc.
        Reveal();
        Debug.Log($"Revealed {data.foodName}: IsPoisoned={IsPoisoned}");
        // Example: change label text (optional)
        // label.text = IsPoisoned ? $"{Data.foodName} ☠" : $"{Data.foodName} ✓";
    }
}
