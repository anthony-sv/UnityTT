#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
#define USE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering
{
    /// <summary>
    /// Utility Free Camera component.
    /// </summary>
    [CoreRPHelpURLAttribute("Free-Camera")]
    public class FreeCameraController : MonoBehaviour
    {
        const float k_MouseSensitivityMultiplier = 0.01f;

        /// <summary>
        /// Rotation speed when using a controller.
        /// </summary>
        public float m_LookSpeedController = 120f;
        /// <summary>
        /// Rotation speed when using the mouse.
        /// </summary>
        public float m_LookSpeedMouse = 4.0f;
        /// <summary>
        /// Movement speed.
        /// </summary>
        public float m_MoveSpeed = 10.0f;
        /// <summary>
        /// Value added to the speed when incrementing.
        /// </summary>
        public float m_MoveSpeedIncrement = 2.5f;
        /// <summary>
        /// Scale factor of the turbo mode.
        /// </summary>
        public float m_Turbo = 10.0f;

        private static string kMouseX = "Mouse X";
        private static string kMouseY = "Mouse Y";
        private static string kVertical = "Vertical";
        private static string kHorizontal = "Horizontal";


        private static string kYAxis = "YAxis";
        
        void OnEnable()
        {
            RegisterInputs();
        }

        void RegisterInputs()
        {
        }

        float inputRotateAxisX, inputRotateAxisY;
        float inputChangeSpeed;
        float inputVertical, inputHorizontal, inputYAxis;
        bool leftShiftBoost, leftShift, fire1;

        void UpdateInputs()
        {
            inputRotateAxisX = 0.0f;
            inputRotateAxisY = 0.0f;
            leftShiftBoost = false;
            fire1 = false;

            if (Input.GetMouseButton(1))
            {
                // leftShiftBoost = true;
                inputRotateAxisX = Input.GetAxis(kMouseX) * m_LookSpeedMouse;
                inputRotateAxisY = Input.GetAxis(kMouseY) * m_LookSpeedMouse;
            }

            fire1 = Input.GetAxis("Fire1") > 0.0f;

            inputVertical = Input.GetAxis(kVertical);
            inputHorizontal = Input.GetAxis(kHorizontal);
            inputYAxis = Input.GetAxis(kYAxis);
        }

        void Update()
        {
            // If the debug menu is running, we don't want to conflict with its inputs.
            if (DebugManager.instance.displayRuntimeUI)
                return;

            UpdateInputs();

            bool moved = inputRotateAxisX != 0.0f || inputRotateAxisY != 0.0f || inputVertical != 0.0f || inputHorizontal != 0.0f || inputYAxis != 0.0f;
            if (moved)
            {
                float rotationX = transform.localEulerAngles.x;
                float newRotationY = transform.localEulerAngles.y + inputRotateAxisX;

                // Weird clamping code due to weird Euler angle mapping...
                float newRotationX = (rotationX - inputRotateAxisY);
                if (rotationX <= 90.0f && newRotationX >= 0.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);
                if (rotationX >= 270.0f)
                    newRotationX = Mathf.Clamp(newRotationX, 270.0f, 360.0f);

                transform.localRotation = Quaternion.Euler(newRotationX, newRotationY, transform.localEulerAngles.z);

                float moveSpeed = Time.deltaTime * m_MoveSpeed;
                if (fire1 || leftShiftBoost)
                    moveSpeed *= m_Turbo;
                transform.position += transform.forward * moveSpeed * inputVertical;
                transform.position += transform.right * moveSpeed * inputHorizontal;
                transform.position += Vector3.up * moveSpeed * inputYAxis;
            }
        }
    }
}
