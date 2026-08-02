using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    private bool isOpened;

    public void Interact(GameObject player)
    {
        if (isOpened) return;
        isOpened = true;
        Debug.Log("Chest opened!");
    }
}