using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; 

public class DoubleDoorOpen : MonoBehaviour
{
    [Header("Door Leafs")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Open Settings")]
    public float openAngle = 90f;
    public float openSpeed = 3f;

    [Header("Inward Direction")]
    public bool flipInward = false;

    private bool isOpen;
    private Quaternion leftClosedRot, rightClosedRot;
    private Quaternion leftOpenRot, rightOpenRot;
    private Coroutine currentCoroutine;

    void Start()
    {
        leftClosedRot = leftDoor.rotation;
        rightClosedRot = rightDoor.rotation;

        float leftSign = flipInward ? -1f : 1f;
        float rightSign = flipInward ? 1f : -1f;

        leftOpenRot  = Quaternion.Euler(leftDoor.eulerAngles  + new Vector3(0f, openAngle * leftSign, 0f));
        rightOpenRot = Quaternion.Euler(rightDoor.eulerAngles + new Vector3(0f, openAngle * rightSign, 0f));
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentCoroutine != null) StopCoroutine(currentCoroutine);
            currentCoroutine = StartCoroutine(ToggleDoors());
        }
    }

    private IEnumerator ToggleDoors()
    {
        Quaternion leftTarget = isOpen ? leftClosedRot : leftOpenRot;
        Quaternion rightTarget = isOpen ? rightClosedRot : rightOpenRot;

        isOpen = !isOpen;

        while (Quaternion.Angle(leftDoor.rotation, leftTarget) > 0.01f ||
               Quaternion.Angle(rightDoor.rotation, rightTarget) > 0.01f)
        {
            leftDoor.rotation = Quaternion.Lerp(leftDoor.rotation, leftTarget, Time.deltaTime * openSpeed);
            rightDoor.rotation = Quaternion.Lerp(rightDoor.rotation, rightTarget, Time.deltaTime * openSpeed);
            yield return null;
        }

        leftDoor.rotation = leftTarget;
        rightDoor.rotation = rightTarget;

        currentCoroutine = null;
    }
}