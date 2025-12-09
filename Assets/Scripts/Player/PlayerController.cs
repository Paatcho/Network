using System;
using System.Collections;
using UnityEngine;
using Math = System.Math;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider coll;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float runMoveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float staminaRechargeRate = 0.1f;
    [SerializeField] private float staminaDischargeRate = 0.2f;
    [SerializeField] private float exhaustionThreshold = 0.1f;
    [SerializeField] private float exhaustionEndThreshold = 0.5f;
    [SerializeField] private int deathCooldownTime = 5;
    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private float jumpStrength = 100;
    [SerializeField] private float horizontalDampingFactor = 5f;

    private CameraController _cam;
    private bool _exhausted;
    private PlayerNetwork _network;
    private float _stamina = 1f;
    private bool _deathCooldown;
    
    public MouseHole CurrentHole { get; set; }

    private float Stamina
    {
        get => _stamina;
        set
        {
            _stamina = value;
            _cam.UpdateStaminaDisplay(value, _exhausted);
        }
    }

    private static bool Run => Input.GetKey(KeyCode.LeftShift);

    public void Init(PlayerNetwork playerNetwork)
    {
        _cam = CameraController.instance;
        CameraController.instance.LookAt(transform);
        transform.position = spawnPosition;
        _network = playerNetwork;
    }

    public void Move()
    {
        if (!_cam || rb.isKinematic) return;

        Vector3 moveInput = Vector3.zero;
        
        if (_deathCooldown)
        {
            _exhausted = false;
        }
        else
        {
            #region Horizontal Movement
            
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

            moveInput = verticalInput * _cam.transform.forward + horizontalInput * _cam.transform.right;
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
            
            #endregion
            
            #region Vertical Movement

            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
            }
            
            #endregion
            
            Vector3 v = rb.linearVelocity;

            float y = v.y;

            Vector3 horizontal = new Vector3(v.x, 0, v.z);
            horizontal *= horizontalDampingFactor;

            rb.linearVelocity = horizontal + Vector3.up * y;
        }

        _network.UpdatePlayerData(new PlayerData
        {
            Exhausted = _exhausted,
            Direction = moveInput,
            Velocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude,
            Dead = _deathCooldown
        });
    }

    public void Die()
    {
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        _deathCooldown = true;
        EnableController(false);
        
        for (int cooldown = deathCooldownTime; cooldown > 0; cooldown -= 1)
        {
            _cam.SetTitle("You died.");
            _cam.SetSubtitle("Respawning in: " + cooldown);
            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(Respawn());
    }

    public void Teleport(Vector3 position)
    {
        _network.TeleportRpc(position);
    }

    private IEnumerator Respawn()
    {
        _deathCooldown = false;
        EnableController(false);
        Teleport(spawnPosition);
        yield return null;
        EnableController(true);
        _network.OnRespawnRpc();
    }

    public void EnableController(bool value)
    {
        rb.isKinematic = !value;
        coll.enabled = value;
    }

    public void PickUpCollectible(Collectible.CollectibleType collectibleType)
    {
        print(_network);
        _network.PickUpCollectibleRpc(collectibleType);
    }
}