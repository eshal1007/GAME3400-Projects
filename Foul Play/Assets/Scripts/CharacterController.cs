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

      private CharacterController _controller; // CharacterController reference
      private Vector3 _velocity; // current vertical velocity

      private InputAction _moveAction; // WASD/arrow input
      private InputAction _jumpAction; // space input

      private void Awake()
      {
          // get the controller
          _controller = GetComponent<CharacterController>();

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
      }

      private void OnEnable()
      {
          // enable inputs when the object becomes active
          _moveAction.Enable();
          _jumpAction.Enable();
      }

      private void OnDisable()
      {
          // disables inputs when the object becomes inactive
          _moveAction.Disable();
          _jumpAction.Disable();
      }

      private void Update()
      {
          // read 2d inputs and convert to world movement
          Vector2 input = _moveAction.ReadValue<Vector2>();
          Vector3 move = transform.right * input.x + transform.forward * input.y;
          _controller.Move(move * moveSpeed * Time.deltaTime);

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

          // apply gravity and move vertically
          _velocity.y += gravity * Time.deltaTime;
          _controller.Move(_velocity * Time.deltaTime);
      }
  }