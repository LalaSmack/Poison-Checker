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
    private int round = 0;

    private int poisonCount = 1;
    private int tastesLeft = 0;
    private int markedCount = 0;
    private DishUI selectedDish;
    private List<DishUI> spawnedDishes = new List<DishUI>();
    private HashSet<DishUI> poisonedDishes = new HashSet<DishUI>();
    
    public void StartGame()
    {
        round = 0;
        NextRound();
    }

    public void NextRound()
    {
        round++;

        ClearContainer();
        spawnedDishes.Clear();
        poisonedDishes.Clear();
        selectedDish = null;
        markedCount = 0;

        int dishCount = startDishes + (round - 1) * addPerRound;

        poisonCount = CalculatePoisonCount(dishCount);
        tastesLeft = CalculateTastesLeft(round);

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

        // Reveal just shows UI info (you decide what “reveal” looks like)
        selectedDish.RevealPoisonResult();

        UpdateTopUI();
        UpdateButtons();
    }

    public void OnServePressed()
    {
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
}
