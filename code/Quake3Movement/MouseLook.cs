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
        [Property] private bool m_Smooth = true;
        [Property] private float m_SmoothTime = 5f;
        //[Property] private bool m_LockCursor = true;

        //private Rotation m_CharacterTargetRot;
        private Rotation m_CameraTargetRot;
        //private bool m_cursorIsLocked = true;

		private GameObject _charObj;
		private GameObject _cameraObj;

        public void Init(GameObject charObj, GameObject cameraObj)
        {
			_charObj = charObj;
			_cameraObj = cameraObj;

			Log.Info( "mouse init" );
		}

        public void LookRotation()
        {
            float yRot = Input.MouseDelta.x * m_XSensitivity;
            float xRot = Input.MouseDelta.y * m_YSensitivity;

			_charObj.WorldRotation *= Rotation.From(0f, -yRot, 0f);
			_cameraObj.WorldRotation *= Rotation.From(xRot, 0f, 0f);

            if (m_ClampVerticalRotation)
            {
                m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);
            }

            if (m_Smooth)
            {
				_charObj.WorldTransform.WithRotation(Rotation.Slerp( _charObj.WorldTransform.Rotation, _charObj.WorldRotation, m_SmoothTime * Time.Delta));
				_cameraObj.WorldTransform.WithRotation(Rotation.Slerp( _cameraObj.WorldTransform.Rotation, _cameraObj.WorldRotation, m_SmoothTime * Time.Delta));
			}
            else
            {
				_charObj.WorldTransform.WithRotation( _charObj.WorldRotation );
				_cameraObj.WorldTransform.WithRotation( _cameraObj.WorldRotation );

				Log.Info( "change look" );
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
