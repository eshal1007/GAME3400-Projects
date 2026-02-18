  using UnityEngine;
  using UnityEngine.InputSystem;
  
  /*
  Character controller written by Jan Heinz and implemented in previous projects
  */

  // basic character controller for movement and jumping
  // uses Unity new input system
  [RequireComponent(typeof(CharacterController))]
  public class SimpleCharacterController : MonoBehaviour
  {
      public enum PigType {Straw, Sticky, Bricky}

      [Header("Pig Switching")]
      public PigType activePig = PigType.Straw;
      public float strawMoveSpeed = 7.5f;
      public float strawJumpHeight = 2.2f;
      public float stickyMoveSpeed = 5f;
      public float stickyJumpHeight = 1.5f;
      public float brickyMoveSpeed = 3.5f;
      public bool brickyCanJump = false;

      [Header("Pig UI")]
      public bool showPigTextBox = true;
      public Vector2 pigTextBoxOffset = new Vector2(15f, 15f);
      public Vector2 pigTextBoxSize = new Vector2(180f, 28f);
      public int pigTextFontSize = 14;

      public float gravity = -9.81f; // downward acceleration
      
      [Header("Player Scale")]
      public float playerHeight = 2.0f;
      public float playerRadius = 0.35f;
      public float cameraHeightRatio = 0.9f; // ex 0.9 = 90% of height
      public Transform cameraTransform;
      public float crouchHeightMultiplier = 0.5f; // ex 0.5f = 50% height when crouching

      private CharacterController _controller; // CharacterController reference
      private Vector3 _velocity; // current vertical velocity
      private bool _isCrouching;
      private float _currentHeight;
      private float _activeMoveSpeed;
      private float _activeJumpHeight;
      private bool _activeCanJump;
      private GUIStyle _pigTextStyle;

      private InputAction _moveAction; // WASD/arrow input
      private InputAction _jumpAction; // space input
      private InputAction _crouchAction; // shift input
      
      // strength of push applied to rigidbodies 
      public float pushPower = 2.0f;
      // when true, ignore pushes while moving downward
      // this is to prevent objects getting pushed into the ground
      public bool dontPushDown = true;

      private void Awake()
      {
          // get the controller
          _controller = GetComponent<CharacterController>();
          
          // apply player scale
          ApplyPlayerScale(playerHeight);

          // build input bindings using Unity's new input system (my first time using it)
          _moveAction = new InputAction("Move", InputActionType.Value);
          _moveAction.AddCompositeBinding("2DVector")
              .With("Up", "<Keyboard>/w")
              .With("Down", "<Keyboard>/s")
              .With("Left", "<Keyboard>/a")
              .With("Right", "<Keyboard>/d")
              .With("Up", "<Keyboard>/upArrow")
              .With("Down", "<Keyboard>/downArrow")
              .With("Left", "<Keyboard>/leftArrow")
              .With("Right", "<Keyboard>/rightArrow");

          _jumpAction = new InputAction("Jump", InputActionType.Button);
          _jumpAction.AddBinding("<Keyboard>/space");

          _crouchAction = new InputAction("Crouch", InputActionType.Button);
          _crouchAction.AddBinding("<Keyboard>/leftShift");

          // set movement values for current pig
          ApplyPigStats();
      }
      
      private void OnValidate()
      {
          ApplyPlayerScale(playerHeight);
          ApplyPigStats();

          // keep inspector font value valid
          if (pigTextFontSize < 1) pigTextFontSize = 1;
      }

      private void OnEnable()
      {
          // enable inputs when the object becomes active
          _moveAction.Enable();
          _jumpAction.Enable();
          _crouchAction.Enable();
      }

      private void OnDisable()
      {
          // disables inputs when the object becomes inactive
          _moveAction.Disable();
          _jumpAction.Disable();
          _crouchAction.Disable();
      }

      private void Update()
      {
          // switch pigs with number keys
          if (Keyboard.current != null)
          {
              // 1 = straw
              if (Keyboard.current.digit1Key.wasPressedThisFrame)
              {
                  activePig = PigType.Straw;
                  ApplyPigStats();
              }
              // 2 = sticky
              else if (Keyboard.current.digit2Key.wasPressedThisFrame)
              {
                  activePig = PigType.Sticky;
                  ApplyPigStats();
              }
              // 3 = bricky
              else if (Keyboard.current.digit3Key.wasPressedThisFrame)
              {
                  activePig = PigType.Bricky;
                  ApplyPigStats();
              }
          }

          // read 2d inputs and convert to world movement
          Vector2 input = _moveAction.ReadValue<Vector2>();
          Vector3 move = transform.right * input.x + transform.forward * input.y;

          // keep controller grounded by applying small downward force
          if (_controller.isGrounded && _velocity.y < 0f)
          {
              _velocity.y = -2f;
          }

          // only jump if on the ground when space is pressed
          if (_activeCanJump && _jumpAction.WasPressedThisFrame() && _controller.isGrounded)
          {
              _velocity.y = Mathf.Sqrt(_activeJumpHeight * -2f * gravity);
          }

          // hold shift to shrink height
          bool wantsCrouch = _crouchAction.IsPressed();
          if (wantsCrouch != _isCrouching)
          {
              _isCrouching = wantsCrouch;
              float targetHeight = _isCrouching ? playerHeight * crouchHeightMultiplier : playerHeight;
              ApplyPlayerScale(targetHeight);
          }

          // apply gravity
          _velocity.y += gravity * Time.deltaTime;

          // move once per frame 
          Vector3 totalMove = move * _activeMoveSpeed + Vector3.up * _velocity.y;
          _controller.Move(totalMove * Time.deltaTime);
      }

      // applies movement values based on active pig
      private void ApplyPigStats()
      {
          if (activePig == PigType.Straw)
          {
              _activeMoveSpeed = strawMoveSpeed;
              _activeJumpHeight = strawJumpHeight;
              _activeCanJump = true;
          }
          else if (activePig == PigType.Sticky)
          {
              _activeMoveSpeed = stickyMoveSpeed;
              _activeJumpHeight = stickyJumpHeight;
              _activeCanJump = true;
          }
          else
          {
              _activeMoveSpeed = brickyMoveSpeed;
              _activeJumpHeight = 0f;
              _activeCanJump = brickyCanJump;
          }
      }

      // simple ui text box to show current pig
      private void OnGUI()
      {
          if (!showPigTextBox) return;

          Rect pigBox = new Rect(pigTextBoxOffset.x, pigTextBoxOffset.y, pigTextBoxSize.x, pigTextBoxSize.y);
          GUI.Box(pigBox, "");

          // lazy init in case gui skin reloads
          if (_pigTextStyle == null)
          {
              _pigTextStyle = new GUIStyle(GUI.skin.label);
              _pigTextStyle.alignment = TextAnchor.MiddleLeft;
          }

          _pigTextStyle.fontSize = pigTextFontSize;
          Rect textRect = new Rect(pigBox.x + 8f, pigBox.y, pigBox.width - 16f, pigBox.height);
          GUI.Label(textRect, "Current Pig: " + activePig, _pigTextStyle);
      }
      
      // applies the scale values assigned in the inspector to the player object 
      private void ApplyPlayerScale(float height)
      // make sure there is a character controller
      {
          if (_controller == null) _controller = GetComponent<CharacterController>();
          _currentHeight = height;

          // resize the controller capsule
          _controller.height = _currentHeight;
          // radius must be smaller than half the height
          _controller.radius = Mathf.Min(playerRadius, _currentHeight * 0.5f - 0.01f);
          // center the capsule so the base stays near y=0
          _controller.center = new Vector3(0f, _currentHeight * 0.5f, 0f);

          // move camera up/down proportionally with player height
          if (cameraTransform != null)
          {
              // keep the camera inside the capsule
              float headroom = 0.05f;
              float maxCameraY = _currentHeight * 0.5f + (_controller.height * 0.5f - _controller.radius - headroom);
              float desiredCameraY = _currentHeight * cameraHeightRatio;

              Vector3 p = cameraTransform.localPosition;
              p.y = Mathf.Min(desiredCameraY, maxCameraY);
              cameraTransform.localPosition = p;
          }
      }

    
    // called when CharacterController collides during movement
    // this was used to push balls around in project 3 primarily
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
    // get rigidbody from the object hit
    Rigidbody rb = hit.collider.attachedRigidbody;
    
    // static colliders 
    if (rb == null) return;
    if (rb.isKinematic) return;

    // dont apply push force when falling
    if (dontPushDown && hit.moveDirection.y < -0.3f) return;

    // push only in the x plane 
    Vector3 pushDir = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);

    // ignore tiny movement vectors
    if (pushDir.sqrMagnitude < 0.0001f) return;
    
     // apply an instant velocity change 
     rb.AddForce(pushDir.normalized * pushPower, ForceMode.VelocityChange);
    }
}
