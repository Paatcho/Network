using System;
using UnityEngine;

public class Billboarding : MonoBehaviour
{
    private CameraController _cam;

    private void Start()
    {
        _cam = CameraController.Instance;
    }

    void Update()
    {
        transform.LookAt(_cam.transform);
    }
}
