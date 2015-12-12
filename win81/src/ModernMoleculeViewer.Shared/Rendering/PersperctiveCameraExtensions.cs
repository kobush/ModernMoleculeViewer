using System;
using Windows.Foundation;
using SharpDX;

namespace ModernMoleculeViewer.Rendering
{
    public static class PersperctiveCameraExtensions
    {
        //http://www.codeproject.com/Articles/161464/Custom-Gestures-for-3D-Manipulation-Using-Windows

        public static void Pan(this PerspectiveCamera camera, Vector2 vPan, float zPan, Size rClient)
        {
            Vector3 vTPan, vRadius;

            float fpWidth = (float)(rClient.Width);
            float fpHeight = (float)(rClient.Height);
            vRadius = camera.EyePosition - camera.LookAt;
            float fpRadius = vRadius.Length();

            //our field of view determines how far it is from one side of the screen to the other
            //in world coordinates
            //determine this distance, and scale our normalized xy pan vector to it
            float fpYPanCoef = 2 * fpRadius / (float)Math.Tan(((MathUtil.Pi - camera.FieldOfView) / 2.0f));
            float fpXPanCoef = fpYPanCoef * (fpWidth / fpHeight);
            vTPan.X = vPan.X * fpXPanCoef;
            vTPan.Y = vPan.Y * fpYPanCoef;
            vTPan.Z = zPan;

            vTPan = ScreenVecToCameraVec(camera, vTPan);
            camera.EyePosition += vTPan;
            camera.LookAt += vTPan;
        }

        public static void SphericalPan(this PerspectiveCamera camera, Vector2 pan)
        {

            Vector3 vRadius = (camera.EyePosition - camera.LookAt);
            float radius = vRadius.Length();

            //Translate the relative pan vector to the absolute distance we want to travel 
            //the radius of the oribit the camera makes with a screen x-axis input 
            float cameraHeight = Vector3.Dot(vRadius, camera.UpDirection);
            float xOrbitRadius = (float)Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(cameraHeight, 2));

            //panning across the entire screen will rotate the view 180 degrees 
            float yShperePanCoef = 2f * (float)Math.Sqrt(2 * Math.Pow(radius, 2));
            float xSpherePanCoef = 2f * (float)Math.Sqrt(2 * Math.Pow(xOrbitRadius, 2));

            pan.X *= xSpherePanCoef;
            pan.Y *= yShperePanCoef;

            Vector3 vTPan = new Vector3(pan.X, pan.Y, 0);

            //the angle of the arc of the path around the sphere we want to take 
            float theta = pan.Length() / radius;

            //the other angle (the triangle is icoseles) of the triangle interior to the arc 
            float gamma = (MathUtil.Pi - theta) / 2f;

            //the length of the chord beneath the arc we traveling 
            //ultimately we will set vTPos to be this chord, 
            //therefore m_vPos+vTPan will be the new position 
            float chordLen = (float)((radius * Math.Sin(theta)) / Math.Sin(gamma));

            //translate pan to the camera's frame of reference 
            vTPan = ScreenVecToCameraVec(camera, vTPan);

            //then set pan to the length of the chord
            vTPan.Normalize();
            vTPan *= chordLen;

            //rotate the chord into the sphere by pi/2 - gamma 
            Vector3 rotAxis = Vector3.Cross(vTPan, camera.EyePosition);
            float rotAngle = -(MathUtil.PiOverTwo - gamma);
            Quaternion q = Quaternion.RotationAxis(rotAxis, rotAngle);
            //Matrix rotMatrix = Matrix.RotationQuaternion(q);
            vTPan = Vector3.Transform(vTPan, q);

            //vTPan is now equal to the chord beneath
            //the arc we wanted to travel along 
            //our view sphere 
            Vector3 vNewPos = camera.EyePosition + vTPan;

            //watch to see if the cross product flipped directions 
            //this happens if we go over the top/bottom of our sphere 
            Vector3 vXBefore, vXAfter;
            vRadius = camera.EyePosition - camera.LookAt;
            vXBefore = Vector3.Cross(vRadius, camera.UpDirection);
            vXBefore.Normalize();

            vRadius = vNewPos - camera.LookAt;
            vXAfter = Vector3.Cross(vRadius, camera.UpDirection);
            vXAfter.Normalize();
            Vector3 vXPlus = vXBefore + vXAfter;

            //if we went straight over the top the vXPlus would be zero 
            //the < 0.5 lets it go almost straight over the top too 
            if (vXPlus.Length() < 0.5f)
            {
                //go upside down 
                camera.UpDirection = -camera.UpDirection;
            }

            //update our camera position 
            camera.EyePosition = vNewPos;
        }

        public static Vector3 ScreenVecToCameraVec(PerspectiveCamera camera, Vector3 vScreen)
        {
            Matrix view, iview;
            Vector3 vTranslatedPos, vTranslatedAt;

            //move the camera to the origin - we don't care about translation
            vTranslatedPos = camera.EyePosition - camera.EyePosition;
            vTranslatedAt = camera.LookAt - camera.EyePosition;

            //calculate the view, and inverse view matricies
            view = Matrix.LookAtRH(vTranslatedPos, vTranslatedAt, camera.UpDirection);
            iview = Matrix.Invert(view);

            //transform the vScreen coordinates to pvCamera coordinates
            return Vector3.TransformCoordinate(vScreen, iview);
        }

        public static float CalcZPan(this PerspectiveCamera camera, float scaleDelta)
        {
            //calc the camera position as if we were scaling, the distance the 
            //camera would have traveled is the distance we want to pan
            Vector3 posBefore = camera.EyePosition - camera.LookAt;
            float fpLengthBefore = posBefore.Length();

            Vector3 posAfter = camera.EyePosition - camera.LookAt;
            posAfter /= scaleDelta;

            float fpLengthAfter = posAfter.Length();

            return fpLengthBefore - fpLengthAfter;
        }

        public static void Scale(this PerspectiveCamera camera, float scale)
        {
            //position direction and magnitude (we want to scale around 0,0,0)
            var pos = camera.EyePosition - camera.LookAt;
            //scale the position
            pos /= scale;
            //move back to the original location
            camera.EyePosition = pos + camera.LookAt;
        }

        public static void Zoom(this PerspectiveCamera camera, float delta)
        {
            //position direction and magnitude (we want to scale around 0,0,0)
            var pos = camera.EyePosition - camera.LookAt;
            
            //scale the position
            pos *= (1+delta);

            //move back to the original location
            camera.EyePosition = pos + camera.LookAt;
        }

        public static void Rotate(this PerspectiveCamera camera, float rotation)
        {
            //rotate along the camera axis, as it faces the origin
            Vector3 axis = -(camera.EyePosition - camera.LookAt);

            Quaternion q = Quaternion.RotationAxis(axis, rotation);
            Matrix mRot = Matrix.RotationQuaternion(q);
            camera.UpDirection = Vector3.TransformCoordinate(camera.UpDirection, mRot);
        }
    }
}