using System;
using System.Diagnostics;

namespace Q3Movement
{
    /// <summary>
    /// Custom script based on the version from the Standard Assets.
    /// </summary>
    [Serializable]
    public class MouseLook
    {
		private const float radToDeg = 360 / ((float)Math.PI * 2);

		[Property] private float m_XSensitivity = 2f; // test
        [Property] private float m_YSensitivity = 2f;
        [Property] private bool m_ClampVerticalRotation = true;
        [Property] private float m_MinimumX = -90F;
        [Property] private float m_MaximumX = 90F;
        [Property] private bool m_Smooth = false;
        [Property] private float m_SmoothTime = 5f;
        [Property] private bool m_LockCursor = true;

        private Rotation m_CharacterTargetRot;
        private Rotation m_CameraTargetRot;
        private bool m_cursorIsLocked = true;

        public void Init(Transform transformCharacter, Transform transformCamera)
        {
            m_CharacterTargetRot = transformCharacter.Rotation;
            m_CameraTargetRot = transformCamera.Rotation;
        }

        public void LookRotation(Transform transformCharacter, Transform transformCamera)
        {
            float yRot = Input.MouseDelta.x * m_XSensitivity;
            float xRot = Input.MouseDelta.y * m_YSensitivity;

            m_CharacterTargetRot *= Rotation.From(0f, yRot, 0f);
            m_CameraTargetRot *= Rotation.From(-xRot, 0f, 0f);

            if (m_ClampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }

            if (m_Smooth)
            {
				transformCharacter.WithRotation(Rotation.Slerp(transformCharacter.Rotation, m_CharacterTargetRot, m_SmoothTime * Time.Delta));
				transformCamera.WithRotation(Rotation.Slerp(transformCamera.Rotation, m_CameraTargetRot, m_SmoothTime * Time.Delta));
			}
            else
            {
				transformCharacter.WithRotation(m_CharacterTargetRot);
				transformCamera.WithRotation(m_CameraTargetRot);

				Log.Info( transformCamera );
			}

			UpdateCursorLock();
        }

        public void SetCursorLock(bool value)
        {
   //         m_LockCursor = value;
   //         if (!m_LockCursor)
   //         {//we force unlock the cursor if the user disable the cursor locking helper
			//	Mouse.Visible = true;
			//}
        }

        public void UpdateCursorLock()
        {
            ////if the user set "lockCursor" we check & properly lock the cursos
            //if (m_LockCursor)
            //{
            //    InternalLockUpdate();
            //}
        }

        private void InternalLockUpdate()
        {
   //         if (Input.EscapePressed)
   //         {
   //             m_cursorIsLocked = false;
   //         }
   //         else if (Input.Pressed("stop"))
   //         {
   //             m_cursorIsLocked = true;
   //         }

   //         if (m_cursorIsLocked)
   //         {
   //             Mouse.Visible = false;
   //         }
   //         else if (!m_cursorIsLocked)
   //         {
			//	Mouse.Visible = true;
			//}
        }

        private Rotation ClampRotationAroundXAxis( Rotation q )
        {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * radToDeg * MathF.Atan(q.x);

            angleX = (float) Math.Clamp(angleX, m_MinimumX, m_MaximumX);

            q.x = MathF.Tan(0.5f * radToDeg * angleX);

            return q;
        }
    }
}
