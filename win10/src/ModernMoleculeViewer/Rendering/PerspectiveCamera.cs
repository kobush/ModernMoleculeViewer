using System;
using SharpDX;

namespace ModernMoleculeViewer.Rendering
{
    public class PerspectiveCamera 
    {
        private Vector3 _lookAt;
        private Vector3 _eyePosition;
        private Vector3 _upDirection;

        private float _nearPlane;
        private float _farPlane;
        private float _fieldOfView;
        private bool _isRightHanded;

        public PerspectiveCamera()
        {
            LookAt = new Vector3(0, 0, 0);
            EyePosition = new Vector3(0, 0, -5);
            UpDirection = Vector3.UnitY;

            NearPlane = 0.1f;
            FarPlane = 100f;
            FieldOfView = MathUtil.PiOverFour; //45 deg
        }

        public bool IsRightHanded
        {
            get { return _isRightHanded; }
            set { _isRightHanded = value; }
        }

        public Vector3 LookDirection
        {
            get { return LookAt - EyePosition; }
        }

        public Vector3 LookAt
        {
            get { return _lookAt; }
            set
            {
                if (Equals(_lookAt, value)) return;
                _lookAt = value;
            }
        }

        public Vector3 EyePosition
        {
            get { return _eyePosition; }
            set
            {
                if (Equals(_eyePosition, value)) return;
                _eyePosition = value;
            }
        }

        public Vector3 UpDirection
        {
            get { return _upDirection; }
            set
            {
                if (Equals(_upDirection, value)) return;
                _upDirection = value;
            }
        }

        public float NearPlane
        {
            get { return _nearPlane; }
            set
            {
                if (_nearPlane.Equals(value)) return;
                _nearPlane = value;
            }
        }

        public float FarPlane
        {
            get { return _farPlane; }
            set
            {
                if (_farPlane.Equals(value)) return;
                _farPlane = value;
            }
        }

        public float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                if (_fieldOfView.Equals(value)) return;
                _fieldOfView = value;
            }
        }

        public Matrix GetViewMatrix()
        {
            Matrix view;
            if (IsRightHanded)
                Matrix.LookAtRH(ref _eyePosition, ref _lookAt, ref _upDirection, out view);
            else 
                Matrix.LookAtLH(ref _eyePosition, ref _lookAt, ref _upDirection, out view);
            return view;
        }


        public Matrix GetProjectionMatrix(ViewportF viewport)
        {
            Matrix projection;
            float aspect = viewport.Width / viewport.Height;
            if (IsRightHanded)
                Matrix.PerspectiveFovRH(_fieldOfView, aspect, _nearPlane, _farPlane, out projection);
            else
                Matrix.PerspectiveFovLH(_fieldOfView, aspect, _nearPlane, _farPlane, out projection);
            return projection;
        }

        public Vector3 GetRay(Vector2 mousePos, ViewportF viewport)
        {
            // To normalised device coords [-1:1, -1:1, -1:1]
            float x = (2.0f * mousePos.X) / viewport.Width - 1.0f;
            float y = 1.0f - (2.0f * mousePos.Y) / viewport.Height;
            float z = 1.0f;
            Vector3 ray_nds = new Vector3(x, y, z);

            // Homogeneous Clip Coordinates [-1:1, -1:1, -1:1, -1:1]
            Vector4 ray_clip = new Vector4(ray_nds.X, ray_nds.Y, -1f, 1f);

            // 4d Eye (Camera) Coordinates [-x:x, -y:y, -z:z, -w:w]
            var proj_inv = GetProjectionMatrix(viewport);
            proj_inv.Invert();
            Vector4 ray_eye = Vector4.Transform(ray_clip, proj_inv);
            ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1f, 0f);

            // 4d World Coordinates  [-x:x, -y:y, -z:z, -w:w]
            var view_inv = GetViewMatrix();
            view_inv.Invert();
            Vector4 ray_wor = Vector4.Transform(ray_eye, view_inv);

            var ray3d = new Vector3(ray_wor.X, ray_wor.Y, ray_wor.Z);
            // don't forget to normalise the vector at some point
            ray3d.Normalize();

            return ray3d;
        }
    }
}