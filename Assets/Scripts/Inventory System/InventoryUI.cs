using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10)]
public class InventoryUI : MonoBehaviour
{
    Inventory inventory;

    [Header("UI Elements")]
    [SerializeField] GameObject itemTileUI;
    [SerializeField] Transform itemTileParent;

    List<GameObject> itemTiles = new List<GameObject>();

    void Awake()
    {
        Refresh();
    }

    /**
    * Function to instantiate the UI elements once again incase of a miss
    */
    public void Refresh()
    {
        inventory = Inventory.instance;

        foreach (GameObject tile in itemTiles)
        {
            Destroy(tile);
        }

        itemTiles.Clear();

        for (int i = 0; i < inventory.currentItems.Count; i++)
        {
            GameObject tile = Instantiate(itemTileUI, itemTileParent);
            itemTiles.Add(tile);
            if (tile != null)
            {
                if (inventory.currentItems[i] != null)
                {
                    if (inventory.currentItems[i] is IInventoryItem so)
                    {
                        tile.GetComponent<InventoryItemTile>().setupTile(so, inventory.ItemSelected);

                        if (so.quantity <= 0)
                        {
                            inventory.currentItems.RemoveAt(i);
                            Destroy(tile);
                            itemTiles.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
