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
      public float stickyJumpHeight = .6f; // can jump but not enough to clear obstcles
      public float brickyMoveSpeed = 3.5f;


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
      private Vector3 _horizontalVelocity;// smoothed horizontal motion per pig
      private bool _wantsGlide;// used by Straw pig glide

    [Header("Feel Tuning")]    
        public float strawAcceleration = 25f;
        public float strawAirControl = 0.9f;
        public float strawGlideGravityMultiplier = 0.35f; 

        public float stickyAcceleration = 15f;
        public float stickyAirControl = 0.6f;

        public float brickyAcceleration = 8f;
        public float brickyTurnSmoothing = 12f; 
      
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
                _horizontalVelocity = Vector3.zero; // reset feel on swap
            }
            // 2 = sticky
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                activePig = PigType.Sticky;
                ApplyPigStats();
                _horizontalVelocity = Vector3.zero;
            }
            // 3 = bricky
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                activePig = PigType.Bricky;
                ApplyPigStats();
             _horizontalVelocity = Vector3.zero;
            }
        }

        Vector2 input = _moveAction.ReadValue<Vector2>();

        // keep controller grounded by applying small downward force
        if (_controller.isGrounded && _velocity.y < 0f)
        {
            _velocity.y = -2f;
        }

        // jump only if allowed + grounded
        if (_activeCanJump && _jumpAction.WasPressedThisFrame() && _controller.isGrounded)
        {
            _velocity.y = Mathf.Sqrt(_activeJumpHeight * -2f * gravity);
        }

        // crouch toggle
        bool wantsCrouch = _crouchAction.IsPressed();
        if (wantsCrouch != _isCrouching)
        {
            _isCrouching = wantsCrouch;
            float targetHeight = _isCrouching ? playerHeight * crouchHeightMultiplier : playerHeight;
            ApplyPlayerScale(targetHeight);
        }

        // pig-specific movement 
        switch (activePig)
        {   
            case PigType.Straw:
                MoveStraw(input);
                break;

            case PigType.Sticky:
                MoveSticky(input);
                break;

            case PigType.Bricky:
                MoveBricky(input);
                break;
        }
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
              _activeCanJump = false;
          }
      }
      
      private void MoveStraw(Vector2 input)
      {
        // Fast + high air control + glide while holding jump and falling
        Vector3 desiredMove = (transform.right * input.x + transform.forward * input.y);
        desiredMove = Vector3.ClampMagnitude(desiredMove, 1f);

        bool grounded = _controller.isGrounded;

        float control = grounded ? 1f : strawAirControl;
        float accel = strawAcceleration;

        Vector3 targetHoriz = desiredMove * _activeMoveSpeed;
        _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetHoriz, accel * control * Time.deltaTime);

        // Glide: holding jump while falling reduces gravity
        _wantsGlide = _jumpAction.IsPressed() && !grounded && _velocity.y < 0f;
        float g = gravity;
        if (_wantsGlide) g *= strawGlideGravityMultiplier;

        _velocity.y += g * Time.deltaTime;

        Vector3 totalMove = _horizontalVelocity + Vector3.up * _velocity.y;
        _controller.Move(totalMove * Time.deltaTime);
     }
     
     private void MoveSticky(Vector2 input)
     {
        Vector3 desiredMove = transform.right * input.x + transform.forward * input.y;
        desiredMove = Vector3.ClampMagnitude(desiredMove, 1f);

        bool grounded = _controller.isGrounded;

        float accel = stickyAcceleration;
        float control = grounded ? 1f : stickyAirControl;
        
        // Smooths the acceleration 
        Vector3 targetHoriz = desiredMove * _activeMoveSpeed;
        _horizontalVelocity = Vector3.MoveTowards(
            _horizontalVelocity,
            targetHoriz,
            accel * control * Time.deltaTime
        );
        // Apllys the gravity since he cant jump
        _velocity.y += gravity * Time.deltaTime;

        Vector3 totalMove = _horizontalVelocity + Vector3.up * _velocity.y;
        _controller.Move(totalMove * Time.deltaTime);
     }
     
     private void MoveBricky(Vector2 input)
     {
        Vector3 desiredMove = transform.right * input.x + transform.forward * input.y;
        desiredMove = Vector3.ClampMagnitude(desiredMove, 1f);

        Vector3 targetHoriz = desiredMove * _activeMoveSpeed;

        // Slow turn smoothing 
        _horizontalVelocity = Vector3.Lerp(
        _horizontalVelocity,
        targetHoriz,
        brickyTurnSmoothing * Time.deltaTime
        );

        // Slow acceleration 
        _horizontalVelocity = Vector3.MoveTowards(
        _horizontalVelocity,
        targetHoriz,
        brickyAcceleration * Time.deltaTime
        );

        // Apply gravity he cnat jump
        _velocity.y += gravity * Time.deltaTime;

        Vector3 totalMove = _horizontalVelocity + Vector3.up * _velocity.y;
        _controller.Move(totalMove * Time.deltaTime);
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
