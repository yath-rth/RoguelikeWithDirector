using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;
    [SerializeField] List<ScriptableObject> allItems;
    List<ScriptableObject> copyItems = new List<ScriptableObject>();
    public List<ScriptableObject> currentItems = new List<ScriptableObject>();
    [SerializeField] InventoryUI inventoryUI;

    void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;

        for (int i = 0; i < allItems.Count; i++)
        {
            ScriptableObject obj = Instantiate(allItems[i]);
            if (allItems[i] != null) copyItems.Add(obj);
        }
    }

    public bool HasItem(ScriptableObject item)
    {
        foreach (ScriptableObject o in currentItems)
        {
            if (item is IInventoryItem a && o is IInventoryItem b)
            {
                if (b.displayName == a.displayName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void ItemSelected(IInventoryItem item)
    {
        item.EquipItem();

        if (item.quantity <= 0) currentItems.Remove(item as ScriptableObject);
        if (inventoryUI != null) inventoryUI.Refresh();
    }

    public void AddItem(ScriptableObject item)
    {
        if (currentItems.Contains(item))
        {
            //TODO 
            //Add functionality for what happens when you pick up more than 1 of the same item

            if (item is IInventoryItem so)
            {
                so.AddItem();
            }
        }
        else
        {
            if (item is IInventoryItem so)
            {
                so.AddItem();
                currentItems.Add(item);
            }
        }

        if (inventoryUI != null)
        {
            inventoryUI.Refresh();
        }
    }
}
