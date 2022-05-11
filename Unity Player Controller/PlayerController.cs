using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform playerCamera = null;
    [SerializeField] float mouseSensitivity = 3.5f;
    [SerializeField] float walkSpeed = 6.0f;
    [SerializeField] float sprintMod = 1.8f;
    [SerializeField] float gravity = -13.0f;
    [SerializeField] float jump = 8.0f;
    [SerializeField][Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;
    [SerializeField][Range(0.0f, 0.5f)] float mouseSmoothTime = 0.03f;
    [SerializeField] float floorDistance = 1;

    [SerializeField] bool lockCursor = true;
    [SerializeField] Vector3 velocity;

    float realSpeed, cameraPitch = 0.0f, velocityY = 0.0f;
    CharacterController controller = null;

    RaycastHit hit, groundInfo;
    bool mainHit;
    Vector3 RayDir, Pos;
    Vector2 currentDir = Vector2.zero, currentDirVelocity = Vector2.zero, currentMouseDelta = Vector2.zero, currentMouseDeltaVelocity = Vector2.zero, targetDir;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        RayDir = transform.TransformDirection(Vector3.down);
        if (lockCursor){ Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
    }

    void Update()
    { UpdateMouseLook(); UpdateMovement(); }

    void UpdateMouseLook()
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
        cameraPitch -= currentMouseDelta.y * mouseSensitivity;
        playerCamera.localEulerAngles = Vector3.right * cameraPitch;
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }

    void UpdateMovement()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){ realSpeed = walkSpeed * sprintMod; }
        else { realSpeed = walkSpeed; }
        targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); targetDir.Normalize();
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);
        velocityY += (gravity * 3) * Time.deltaTime;
        if (controller.isGrounded){ velocityY = 0.0f; }
        Pos = transform.position; var yes = -(((transform.localScale.y / 2) * controller.height) - 0.1f);
        mainHit = Physics.SphereCast(Pos, controller.radius, RayDir, out hit, floorDistance);
        //fc = Physics.Raycast(Pos, RayDir, out groundInfo, floorDistance);

        if (Input.GetKeyDown(KeyCode.Space))
        { if (mainHit) { velocityY += jump * 2; } }
        if (velocityY > 10){ velocityY = 10; }
        velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * realSpeed + Vector3.up * velocityY;
        controller.Move(velocity * Time.deltaTime);
    }
}
