using UnityEngine;

[CreateAssetMenu(fileName = "NewFood", menuName = "PoisonChecker/Food")]
public class FoodData : ScriptableObject
{
    public string foodName;
    public Sprite foodSprite;
}
