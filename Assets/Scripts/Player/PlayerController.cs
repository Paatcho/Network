using System;
using UnityEngine;
using Math = System.Math;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float runMoveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float staminaRechargeRate = 0.1f;
    [SerializeField] private float staminaDischargeRate = 0.2f;
    [SerializeField] private float exhaustionThreshold = 0.1f;
    [SerializeField] private float exhaustionEndThreshold = 0.5f;

    private Transform _cam;
    private bool _exhausted;
    private IPlayerDataListener _network;
    private Vector3 _spawnPosition;
    private float _stamina = 1f;

    private float Stamina
    {
        get => _stamina;
        set
        {
            _stamina = value;
            CameraController.instance.UpdateStaminaDisplay(value, _exhausted);
        }
    }

    private static bool Run => Input.GetKey(KeyCode.LeftShift);

    public void Init(IPlayerDataListener playerDataListener)
    {
        _cam = CameraController.instance.transform;
        CameraController.instance.LookAt(transform);
        _spawnPosition = transform.position;
        _network = playerDataListener;
    }

    public void Move()
    {
        if (!_cam) return;

        bool isRunning;

        if (Stamina >= exhaustionThreshold)
        {
            if (Stamina >= exhaustionEndThreshold)
            {
                _exhausted = false;
            }

            isRunning = Run && !_exhausted;
        }
        else
        {
            _exhausted = true;
            isRunning = false;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveInput = verticalInput * _cam.forward + horizontalInput * _cam.right;
        moveInput.y = 0;
        moveInput = Vector3.ClampMagnitude(moveInput, 1);

        float speed = isRunning ? runMoveSpeed : moveSpeed;

        Vector3 velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (velocity.magnitude < speed * 0.9f)
        {
            rb.AddForce(moveInput * (speed * 2f), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(moveInput * speed, ForceMode.Acceleration);
        }

        if (moveInput != Vector3.zero)
        {
            rb.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveInput),
                Time.deltaTime * rotationSpeed);
        }

        Stamina = isRunning && moveInput != Vector3.zero
            ? Math.Max(Stamina - staminaDischargeRate * Time.deltaTime, 0f)
            : Math.Min(Stamina + staminaRechargeRate * Time.deltaTime, 1f);

        _network.UpdatePlayerData(new PlayerData
        {
            Exhausted = _exhausted,
            Direction = moveInput,
            Velocity = rb.linearVelocity.magnitude
        });
    }

    public void Die()
    {
        Respawn();
    }

    private void Respawn()
    {
        transform.position = _spawnPosition;
    }
}