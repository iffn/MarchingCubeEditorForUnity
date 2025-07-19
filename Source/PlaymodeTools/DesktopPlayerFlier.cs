using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopPlayerFlier : MonoBehaviour
{
    public float speed = 10f;
    public float mouseSensitivity = 2f;

    float pitch = 0f;
    float heading = 0f;

    bool cursorLocked = false;
    bool CursorLocked
    {
        set
        {
            if (value)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        get
        {
            return cursorLocked;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CursorLocked = true;
    }

    // Update is called once per frame
    void Update()
    {
        float positiveFrameIndependentSpeed = speed * Time.deltaTime;
        float negativeFrameIndependentSpeed = -positiveFrameIndependentSpeed;

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E))
            transform.position += positiveFrameIndependentSpeed * transform.up;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.Q))
            transform.position += negativeFrameIndependentSpeed * transform.up;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            transform.position += positiveFrameIndependentSpeed * transform.right;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            transform.position += negativeFrameIndependentSpeed * transform.right;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            transform.position += positiveFrameIndependentSpeed * transform.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            transform.position += negativeFrameIndependentSpeed * transform.forward;

        heading += Input.GetAxis("Mouse X") * Time.deltaTime * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * Time.deltaTime * mouseSensitivity;

        transform.rotation = Quaternion.Euler(pitch, heading, 0f);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CursorLocked = !CursorLocked;
        }
    }
}
