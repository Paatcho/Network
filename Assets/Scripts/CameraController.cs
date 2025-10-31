using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    [SerializeField] private Slider staminaDisplay;
    [SerializeField] private Image staminaDisplayFill;

    private Transform _target;

    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float height = 0.8f;
    [SerializeField] private float distance = 3f;
    [SerializeField] private float minAngle = -20f;
    [SerializeField] private float maxAngle = 20f;
    [SerializeField] private LayerMask obstructionMask;
    [SerializeField] private float wallOffset = 0.1f;
    [SerializeField] private Color staminaBaseColor;
    [SerializeField] private Color staminaCooldownColor;

    private float _rotationX;
    private float _rotationY;

    private void Update()
    {
        if (!_target) return;
        UpdateRotation();
        UpdatePosition();
    }

    private void UpdateRotation()
    {
        _rotationY += Input.GetAxis("Mouse X") * rotationSpeed;
        _rotationX -= Input.GetAxis("Mouse Y") * rotationSpeed;
        _rotationX = Mathf.Clamp(_rotationX, minAngle, maxAngle);

        transform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
    }

    private void UpdatePosition()
    {
        Vector3 offset = transform.rotation * new Vector3(0f, 0f, -distance) + new Vector3(0f, height, 0f);
        Vector3 position = _target.position + offset;

        Vector3 rayOrigin = _target.position + Vector3.up * height;
        Vector3 rayDir = (position - rayOrigin).normalized;
        float rayDist = Vector3.Distance(position, rayOrigin);

        if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, rayDist, obstructionMask))
        {
            transform.position = hit.point - rayDir * wallOffset;
        }
        else
        {
            transform.position = position;
        }
    }

    public void LookAt(Transform target)
    {
        _target = target;
    }

    public void UpdateStaminaDisplay(float value, bool cooldown)
    {
        staminaDisplay.value = value;
        staminaDisplayFill.color = cooldown ? staminaCooldownColor : staminaBaseColor;
    }
}
