using UnityEngine;
using UnityEngine.InputSystem;

// simple mouselook 
// pitch on the camera
// yaw on the player body
public class MouseLookInputSystem : MonoBehaviour
{
    public Transform body; // the player the camera is attached to
    public float sensitivity = 2f; // mouse sensitivity

    private float _xRotation; // current pitch rotation (up/down)
    private InputAction _lookAction; // mouse input

    private void Awake()
    {
        // create a mouse action
        _lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
    }

    private void OnEnable()
    {
        // enable input and lock the cursor
        _lookAction.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        // unlock the curse when disabled
        _lookAction.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        // read raw mouse inputs and scale by sensitivity
        Vector2 delta = _lookAction.ReadValue<Vector2>() * sensitivity;

        // pitch
        // rotate the camera up/down and clamp to prevent weird edge flipping cases
        _xRotation -= delta.y;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // yaw
        // rotate the body left/right
        body.Rotate(Vector3.up * delta.x);
    }
}