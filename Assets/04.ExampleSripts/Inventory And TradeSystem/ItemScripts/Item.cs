using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Inventory
{
    [CreateAssetMenu(fileName = "GameItem", menuName = "SAO/GameItemSAO", order = 1)]
    public class Item : ScriptableObject
    {
        public int Key;
        public string Name; 
        public string Description;
        public Sprite Icon;
        public int Price;
        public string Tag;
    }
}