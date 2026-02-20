using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RoundSpawner : MonoBehaviour
{
    [Header("Food Pool")]
    public List<FoodData> allFoods = new List<FoodData>();

    [Header("UI References")]
    public GameObject dishPrefab;
    public Transform dishContainer;
    [SerializeField] private GameOver gameOverManager;

    [Header("Round Settings")]
    public int startDishes = 3;
    public int addPerRound = 1;

     [Header("Poison Settings")]
    public int startPoisons = 1;
    public int addPoisonEveryXDishes = 4; // +1 poison every X dishes
    public int maxPoisons = 4;

    [Header("Taste Settings")]
    public int startTastes = 2;
    public int addTasteEveryXRounds = 2;  // +1 taste every X rounds
    public int maxTastes = 4;

    [Header("UI")]
    public Button serveButton;
    public Button tasteButton;
    public Button markButton;
    [SerializeField]  private TextMeshProUGUI tastesLeftText;
    [SerializeField]  private TextMeshProUGUI poisonCountText;
    [SerializeField]  private TextMeshProUGUI roundText;
    [SerializeField]  private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI dayStartText;
    [SerializeField] private float popupDuration = 0.3f;

    private int round = 0;
    
    // Tracking Days
    [Header("Day Settings")]
    public int totalDays = 5;
    public int minRoundsPerDay = 4;
    public int maxRoundsPerDay = 6;
    private int day = 1;
    private int roundInDay = 0;
    private int roundsThisDay = 0;

    private int poisonCount = 1;
    private int tastesLeft = 0;
    private int carriedOverTastes = 0;

    private int markedCount = 0;
    private DishUI selectedDish;
    private List<DishUI> spawnedDishes = new List<DishUI>();
    private HashSet<DishUI> poisonedDishes = new HashSet<DishUI>();
    
    public void StartGame()
    {
        round = 0;
        day = 1;
        StartNewDay();
        //NextRound();
    }

    private void StartNewDay()
    {
        roundsThisDay = UnityEngine.Random.Range(minRoundsPerDay, maxRoundsPerDay + 1);
        roundInDay = 0;
        round = 0;
        //no carried over tastes between days 
        carriedOverTastes = 0; // Reset carried over tastes at the start of a new day

        ShowDayStartPopup();

        NextRound();
    }
    public void NextRound()
    {
        round++;
        roundInDay++;

        ClearContainer();
        spawnedDishes.Clear();
        poisonedDishes.Clear();
        selectedDish = null;
        markedCount = 0;

        int dishCount = startDishes + (round - 1) * addPerRound + (day - 1); // Increase dishes by round and day

        if (dishCount > 10) 
        {
            dishCount = 10; // Cap at 10 dishes for performance and playability
            Debug.Log("Max dish count reached!");
        }

        poisonCount = CalculatePoisonCount(dishCount);
        tastesLeft = CalculateTastesLeft(round) + carriedOverTastes + (day - 1); // Add carried over tastes and bonus taste per day
        carriedOverTastes = 0; // Reset carried over tastes, will be updated if player doesn't use all tastes this round

        // Decide which indices are poisoned (unique)
        List<int> indices = new List<int>();
        for (int i = 0; i < dishCount; i++) indices.Add(i);
        Shuffle(indices);

        // Spawn dishes
        for (int i = 0; i < dishCount; i++)
        {
            FoodData chosen = allFoods[UnityEngine.Random.Range(0, allFoods.Count)];
            bool isPoisoned = (i < poisonCount) ? false : false; // For testing: no poisons yet, can set to (i < poisonCount) later
        }


         // We want poisonCount unique poisoned dishes using shuffled indices
        HashSet<int> poisonedIndices = new HashSet<int>();
        for (int p = 0; p < poisonCount; p++)
            poisonedIndices.Add(indices[p]);

        for (int i = 0; i < dishCount; i++)
        {
            FoodData chosenFood = allFoods[UnityEngine.Random.Range(0, allFoods.Count)];
            bool isPoisoned = poisonedIndices.Contains(i);

            GameObject dishObj = Instantiate(dishPrefab, dishContainer);
            DishUI dishUI = dishObj.GetComponent<DishUI>();

            dishUI.Setup(chosenFood, this, isPoisoned);
            spawnedDishes.Add(dishUI);

            // Wire click
            Button btn = dishObj.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(dishUI.OnDishClicked);

            if (isPoisoned) poisonedDishes.Add(dishUI);
        }

        UpdateTopUI();
        UpdateButtons();

    }

    public void OnMarkPressed()
{
    if (selectedDish == null) return;

    if (selectedDish.IsMarked) return; // Already marked, do nothing. Unmarking is not allowed in current design.

    selectedDish.SetMarked(true);
    GameAudio.Instance.PlayClick();
    Debug.Log($"Dish {selectedDish.data.foodName} marked: {selectedDish.IsMarked}");
    // Update markedCount
    markedCount++;

    UpdateButtons();
}
    public void SelectDish(DishUI dish)
    {
        if (selectedDish != null) selectedDish.SetSelected(false);

        selectedDish = dish;
        selectedDish.SetSelected(true);
        Debug.Log($"Selected dish: {selectedDish.data.foodName}");
        UpdateButtons();
    }

    // Called by DishUI whenever it toggles mark
    public void OnMarkedChanged(bool nowMarked)
    {
        markedCount += nowMarked ? 1 : -1;
        UpdateButtons();
    }

    public void OnTastePressed()
    {
        if (selectedDish == null) return;
        if (tastesLeft <= 0) return;

        tastesLeft--;
        GameAudio.Instance.PlayClick();
        // Reveal just shows UI info (you decide what “reveal” looks like)
        selectedDish.RevealPoisonResult();
        
        UpdateTopUI();
        UpdateButtons();
    }

    public void OnServePressed()
    {
        GameAudio.Instance.PlayClick();
        // Only allow serve when you have marked exactly poisonCount dishes
        if (markedCount != poisonCount) return;

        // Correct if and only if marked set == poisoned set
        foreach (DishUI dish in spawnedDishes)
        {
            bool shouldBePoisoned = poisonedDishes.Contains(dish);
            bool playerMarked = dish.IsMarked;

            if (playerMarked != shouldBePoisoned)
            {
                Debug.Log("LOSE: incorrect poison selection");
                gameOverManager.TriggerGameOver("You served the wrong dishes!");
                return;
            }
        }
        Debug.Log("WIN: correct poisons! Next round.");
        carriedOverTastes = tastesLeft; // Save unused tastes for next round
        
    // If finished today's rounds, advance day or win
    if (roundInDay >= roundsThisDay)
        {       
            if (day >= totalDays)
            {
                TriggerWin();
                return;
            }

            day++;
            StartNewDay();
            return;
        }

        NextRound();
    }
    private void UpdateButtons()
    {
        bool requiredMarksMade = (markedCount == poisonCount);
        if (serveButton != null)
            serveButton.interactable = (markedCount == poisonCount);

        if (tasteButton != null)
            tasteButton.interactable = (selectedDish != null && tastesLeft > 0 && !requiredMarksMade) ;

        if (markButton != null)
        markButton.interactable = (selectedDish != null && !selectedDish.IsMarked && !requiredMarksMade); // Can only mark if a dish is selected and not already marked

    }

    private void UpdateTopUI()
    {
        if (dayText != null) dayText.text = $"Day: {day}";
        if (tastesLeftText != null) tastesLeftText.text = $"Tastes: {tastesLeft}";
        if (poisonCountText != null) poisonCountText.text = $"Poisons: {poisonCount}";
        if (roundText != null) roundText.text = $"Round: {round}";
    }

    private int CalculatePoisonCount(int dishCount)
    {
        int extra = dishCount / addPoisonEveryXDishes;
        int count = startPoisons + extra;
        return Mathf.Clamp(count, 1, maxPoisons);
    }

    private int CalculateTastesLeft(int currentRound)
    {
        int extra = (currentRound - 1) / addTasteEveryXRounds;
        int count = startTastes + extra;
        return Mathf.Clamp(count, 0, maxTastes);
    }

    private void ClearContainer()
    {
        for (int i = dishContainer.childCount - 1; i >= 0; i--)
            Destroy(dishContainer.GetChild(i).gameObject);
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    private void TriggerWin()
    {
        Debug.Log("YOU WIN: Completed all rounds on Day 5!");
        gameOverManager.TriggerGameOver("You survived all 5 days!\n Your King is safe!", "You Win!");

        if (serveButton != null) serveButton.interactable = false;
        if (tasteButton != null) tasteButton.interactable = false;
        if (markButton != null) markButton.interactable = false;
    }

    private void ShowDayStartPopup()
{
    StartCoroutine(DayPopupRoutine());
}

private System.Collections.IEnumerator DayPopupRoutine()
{
    dayStartText.gameObject.SetActive(true);
    dayStartText.text = $"Day {day}";

    CanvasGroup canvasGroup = dayStartText.GetComponent<CanvasGroup>();

    if (canvasGroup == null)
        canvasGroup = dayStartText.gameObject.AddComponent<CanvasGroup>();

    canvasGroup.alpha = 1f;

    yield return new WaitForSeconds(popupDuration);

    float fadeTime = 0.4f;
    float timer = 0f;

    while (timer < fadeTime)
    {
        timer += Time.deltaTime;
        canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
        yield return null;
    }

    dayStartText.gameObject.SetActive(false);
}
}
