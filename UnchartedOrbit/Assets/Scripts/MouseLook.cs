using UnityEngine;


//This code is reused from a previous class, Game Programming. 
public class MouseLook : MonoBehaviour
{

    public float mouseSens = 100f;
    Transform playerBody;
    float pitch;
    public float pitchMin = -90f;
    public float pitchMax = 90f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerBody = transform.parent.transform;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //THE ONLY NEW PART ADDED TO THE REUSED SCRIPT: making it so that when the escape key is pressed, the cursor unlocks and becomes visible.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        
        float moveX = Input.GetAxis("Mouse X") * mouseSens * Time.deltaTime;
        float moveY = Input.GetAxis("Mouse Y") * mouseSens * Time.deltaTime;

        //yaw - player

        if(playerBody)
        {
            playerBody.Rotate(Vector3.up * moveX);
        }

        //pitch - camera

        pitch -= moveY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);
        transform.localRotation = Quaternion.Euler(pitch, 0, 0);

    }
}
