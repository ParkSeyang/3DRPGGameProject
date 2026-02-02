using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Examples.Inventory
{
    public class SlotSystem : MonoBehaviour
    {
        public Item Item;
        public Image Image;
        private RectTransform rectTransform;

        public bool IsEmptySlot
        {
            get { return Item == null; }
        }

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            SetItem(null);
        }

        public void SetPosition(Vector2 inputPosition)
        {
            rectTransform.anchoredPosition = inputPosition - new Vector2(rectTransform.sizeDelta.x / 2, rectTransform.sizeDelta.y / 2);
        }
        
        public void SetItem(Item item)
        {
            this.Item = item;
            if (item == null)
            {
                Image.sprite = null;
                Image.enabled = false;
            }
            else
            {
                Image.sprite = this.Item.Icon;
                Image.enabled = true;
            }
            
        }

        
    }
}