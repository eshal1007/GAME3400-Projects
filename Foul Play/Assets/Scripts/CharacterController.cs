  using UnityEngine;
  using UnityEngine.InputSystem;

  // basic character controller for movement and jumping
  // uses Unity new input system
  [RequireComponent(typeof(CharacterController))]
  public class SimpleCharacterController : MonoBehaviour
  {
      public float moveSpeed = 5f; // horizontal move speed
      public float jumpHeight = 1.5f; // jump height
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

      private InputAction _moveAction; // WASD/arrow input
      private InputAction _jumpAction; // space input
      private InputAction _crouchAction; // shift input

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
      }
      
      private void OnValidate()
      {
          ApplyPlayerScale(playerHeight);
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
          // read 2d inputs and convert to world movement
          Vector2 input = _moveAction.ReadValue<Vector2>();
          Vector3 move = transform.right * input.x + transform.forward * input.y;

          // keep controller grounded by applying small downward force
          if (_controller.isGrounded && _velocity.y < 0f)
          {
              _velocity.y = -2f;
          }

          // only jump if on the ground when space is pressed
          if (_jumpAction.WasPressedThisFrame() && _controller.isGrounded)
          {
              _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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
          Vector3 totalMove = move * moveSpeed + Vector3.up * _velocity.y;
          _controller.Move(totalMove * Time.deltaTime);
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
  }
