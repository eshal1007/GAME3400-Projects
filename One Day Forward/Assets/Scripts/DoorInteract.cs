using UnityEngine;
using UnityEngine.InputSystem;

// basic door interaction for prototype
// player presses e near the door to open it
// just disables the door object
public class DoorInteract : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactDistance = 10.0f; // how close player needs to be to interact
    [SerializeField] private GameObject door;

    private bool isOpen; // if true, door already opened once

    private void Update()
    {
        // door is already open, do nothing
        if (isOpen) return;

        // check the range between the player and the door
        // player can only open door once in range
        float distanceToDoor = Vector3.Distance(playerTransform.position, transform.position);
        bool inRange = distanceToDoor <= interactDistance;

        if (!inRange) return;

        // open door with e
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (!kb.fKey.wasPressedThisFrame) return;

        OpenDoor();
    }

    private void OpenDoor()
    {
        isOpen = true;

        // disable door
        door.SetActive(false);
    }
}
