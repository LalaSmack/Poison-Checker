using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoundSpawner : MonoBehaviour
{
    [Header("Food Pool")]
    public List<FoodData> allFoods = new List<FoodData>();

    [Header("UI References")]
    public GameObject dishPrefab;
    public Transform dishContainer;

    [Header("Round Settings")]
    public int startDishes = 2;
    public int addPerRound = 1;

    private int round = 0;

    public void StartGame()
    {
        round = 0;
        NextRound();
    }

    public void NextRound()
    {
        round++;

        ClearContainer();

        int dishCount = startDishes + (round - 1) * addPerRound;

        for (int i = 0; i < dishCount; i++)
        {
            FoodData chosen = allFoods[Random.Range(0, allFoods.Count)];

            GameObject dish = Instantiate(dishPrefab, dishContainer);

            // Assign UI (FoodImage + Label)
            dish.transform.Find("FoodImage").GetComponent<Image>().sprite = chosen.foodSprite;
            dish.transform.Find("FoodLabel").GetComponent<TextMeshProUGUI>().text = chosen.foodName;

            int index = i;
            dish.GetComponent<Button>().onClick.AddListener(() => OnDishClicked(index, chosen));
        }
    }

    private void OnDishClicked(int index, FoodData data)
    {
        Debug.Log($"Clicked dish {index}: {data.foodName}");
        // Later: open Taste/Mark panel, toggle state, etc.
    }

    private void ClearContainer()
    {
        for (int i = dishContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(dishContainer.GetChild(i).gameObject);
        }
    }
}
