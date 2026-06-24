using UnityEngine;

/**
* A default inventory object template
*/
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject, IInventoryItem
{
    [SerializeField] int _index;
    [SerializeField] string _displayName;
    [SerializeField] Sprite _image;
    [SerializeField] int _quantity;

    public string displayName => _displayName;
    public Sprite image => _image;
    public int quantity => _quantity;
    public int index => _index;

    public GameObject scrollPrefab;

    public void EquipItem()
    {
        _quantity--;
    }

    public void AddItem()
    {
        _quantity++;
    }
}
