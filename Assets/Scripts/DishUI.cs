using UnityEngine;
using UnityEngine.UI;

public class DishUI : MonoBehaviour
{
    public Image foodImage;
    public TMPro.TextMeshProUGUI label;
    private FoodData data;

    public void Setup(FoodData foodData)
    {
        data = foodData;
        foodImage.sprite = data.foodSprite;
        label.text = data.foodName;
    }
    
}
