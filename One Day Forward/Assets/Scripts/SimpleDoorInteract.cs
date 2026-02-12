using UnityEngine;
using UnityEngine.InputSystem;

// basic door interaction for level prototype
// player presses e near the door to open it
// just disables the door object
public class SimpleDoorInteract : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float interactDistance = 10.0f; // how close player needs to be to interact
    [SerializeField] private GameObject doorToDisable;
    [SerializeField] private GameObject promptUI; // optional ui prompt when in interact distance

    private bool isOpen; // is door "open" already

    private void Start()
    {
        // fallback if player not assigned in inspector
        if (playerTransform == null)
        {
            SimpleCharacterController player = FindFirstObjectByType<SimpleCharacterController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // fallback if no door object was assigned
        if (doorToDisable == null)
        {
            doorToDisable = gameObject;
        }

        // prompt should be hidden until player is in range
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void Update()
    {
        // don't do anything if door already open or no player found
        if (isOpen || playerTransform == null)
        {
            return;
        }

        // calculate range between player and door 
        float distanceToDoor = Vector3.Distance(playerTransform.position, transform.position);
        bool inRange = distanceToDoor <= interactDistance;

        // show prompt only when player is close enough
        if (promptUI != null)
        {
            promptUI.SetActive(inRange);
        }

        // open door when player is in range and presses e
        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        isOpen = true;

        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        // disable the door object
        if (doorToDisable != null)
        {
            doorToDisable.SetActive(false);
        }
    }
}
