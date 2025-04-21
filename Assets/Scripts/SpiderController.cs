using UnityEngine;
using UnityEngine.InputSystem;

public class SpiderController : MonoBehaviour
{
    [Header("GameObject Assignment")]
    public Rigidbody rigidbody;
    public Transform camera;

    [Header("Spider Properties")]
    public float speed = 10;

    private Vector2 moveInput;
    private PlayerInputActions inputActions;

    /**
     * Initializes input action bindings.
     */
    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    /**
     * Subscribes to input callbacks and enables input handling.
     */
    void OnEnable()
    {
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Enable();
    }

    /**
     * Unsubscribes from input callbacks and disables input handling.
     */
    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Disable();
    }

    /**
     * Updates movement input state when movement is performed.
     * 
     * @param context Input context containing directional input.
     */
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /**
     * Resets movement input when movement is canceled.
     * 
     * @param context Input context (unused).
     */
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    
    /**
     * Applies movement force in the direction of input, relative to camera rotation.
     * Limits force application based on current velocity.
     */
    void Update()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (direction.magnitude >= 0.1f && rigidbody.linearVelocity.magnitude < 4)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg 
                                + camera.eulerAngles.y;

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            rigidbody.AddForce(moveDir.normalized * speed * Time.deltaTime);
        }
    }
}