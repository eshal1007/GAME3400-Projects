using UnityEngine;
using UnityEngine.InputSystem;

// simple loop for prototype
// press e to pick up and deposit
public class PickupDeposit : MonoBehaviour
{
    [Header("Object Interaction")]
    public float interactRange = 3f;
    public string interactableTag = "InteractableItem";
    public string depositTag = "DepositBox";

    [Header("Holding")]
    public Transform holdPosition;
    public Vector3 holdOffset = new Vector3(0f, -0.1f, 1.2f);

    private Camera cam;
    private Transform itemBeingHeld;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        if (holdPosition == null)
        {
            // determines the position for the held item in world space based on the player's position
            GameObject p = new GameObject("HoldPoint");
            holdPosition = p.transform;
            holdPosition.SetParent(transform);
            holdPosition.localPosition = holdOffset;
            holdPosition.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        // was e pressed?
        if (!Keyboard.current.eKey.wasPressedThisFrame) return;
        if (cam == null) return;

        // raycast to check if looking at correct object
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange)) return;

        // can only pick up if nothing is being held currently
        if (itemBeingHeld == null)
        {
            Transform pickup = FindObjectWithSpecificTag(hit.transform, interactableTag);
            if (pickup == null) return;

            itemBeingHeld = pickup;
            itemBeingHeld.SetParent(holdPosition);
            itemBeingHeld.localPosition = Vector3.zero;
            itemBeingHeld.localRotation = Quaternion.identity;
            return;
        }

        // item is being held, only allow deposit
        Transform depositRoot = FindObjectWithSpecificTag(hit.transform, depositTag);
        if (depositRoot == null) return;

        itemBeingHeld.gameObject.SetActive(false);
        itemBeingHeld = null;
    }

    // trys to find the given tag on an object
    private Transform FindObjectWithSpecificTag(Transform start, string tagToSearch)
    {
        Transform current = start;
        while (current != null)
        {
            if (current.CompareTag(tagToSearch)) return current;
            current = current.parent;
        }

        return null;
    }
}
