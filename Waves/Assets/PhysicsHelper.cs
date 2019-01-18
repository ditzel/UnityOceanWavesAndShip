
using UnityEngine;

namespace Ditzelgames
{
    public static class PhysicsHelper
    {

        public static void ApplyForceToReachVelocity(Rigidbody rigidbody, Vector3 velocity, float force, ForceMode mode = ForceMode.Force)
        {
            if (force == 0 || velocity.magnitude == 0)
                return;

            //force = 1 => need 1 s to reach velocity => force can be max 1 / Time.fixedDeltaTime
            force = Mathf.Clamp(force, - 1f / Time.fixedDeltaTime, 1f / Time.fixedDeltaTime);

            //dot product is a projection from rhs to lhs with a length of result / lhs.magnitude https://www.youtube.com/watch?v=h0NJK4mEIJU
            if (rigidbody.velocity.magnitude == 0)
            {
                rigidbody.AddForce(velocity * force, mode);
            }
            else
            {
                var velocityProjectedToTarget = (velocity.normalized * Vector3.Dot(velocity, rigidbody.velocity) / velocity.magnitude);
                rigidbody.AddForce((velocity - velocityProjectedToTarget) * force, mode);
            }
        }

        public static void ApplyForceToReachVelocity(Rigidbody rigidbody, Vector3 velocity)
        {
            ApplyForceToReachVelocity(rigidbody, velocity, (1 / (rigidbody.drag + 1)) * rigidbody.mass);
        }

        public static void ApplyTorqueToRotate(Rigidbody rigidbody, Quaternion rotation, float force, float maxSpeedDegPerSec)
        {
            float angleInDegrees;
            Vector3 rotationAxis;
            rotation.ToAngleAxis(out angleInDegrees, out rotationAxis);

            if (rotationAxis == Vector3.zero)
                return;

            var currentSpeed = Vector3.Project(rigidbody.angularVelocity * Mathf.Rad2Deg, rotationAxis).magnitude;

            var torque = rotationAxis * (maxSpeedDegPerSec - currentSpeed) * force * Mathf.Deg2Rad;

            rigidbody.AddTorque(torque - rigidbody.angularVelocity);
        }

        public static Vector3 QuaternionToAngularVelocity(Quaternion rotation)
        {
            float angleInDegrees;
            Vector3 rotationAxis;
            rotation.ToAngleAxis(out angleInDegrees, out rotationAxis);

            return rotationAxis * angleInDegrees * Mathf.Deg2Rad;
        }

        public static Quaternion AngularVelocityToQuaternion(Vector3 angularVelocity)
        {
            var rotationAxis = (angularVelocity * Mathf.Rad2Deg).normalized;
            float angleInDegrees = (angularVelocity * Mathf.Rad2Deg).magnitude;

            return Quaternion.AngleAxis(angleInDegrees, rotationAxis);
        }

        public static Vector3 GetNormal(Vector3[] points)
        {
            //https://www.ilikebigbits.com/2015_03_04_plane_from_points.html
            if (points.Length < 3)
                return Vector3.up;

            var center = GetCenter(points);

            float xx = 0f, xy = 0f, xz = 0f, yy = 0f, yz = 0f, zz = 0f;

            for (int i = 0; i < points.Length; i++)
            {
                var r = points[i] - center;
                xx += r.x * r.x;
                xy += r.x * r.y;
                xz += r.x * r.z;
                yy += r.y * r.y;
                yz += r.y * r.z;
                zz += r.z * r.z;
            }

            var det_x = yy * zz - yz * yz;
            var det_y = xx * zz - xz * xz;
            var det_z = xx * yy - xy * xy;

            if (det_x > det_y && det_x > det_z)
                return new Vector3(det_x, xz * yz - xy * zz, xy * yz - xz * yy).normalized;
            if (det_y > det_z)
                return new Vector3(xz * yz - xy * zz, det_y, xy * xz - yz * xx).normalized;
            else
                return new Vector3(xy * yz - xz * yy, xy * xz - yz * xx, det_z).normalized;

        }

        public static Vector3 GetCenter(Vector3[] points)
        {
            var center = Vector3.zero;
            for (int i = 0; i < points.Length; i++)
                center += points[i] / points.Length;
            return center;
        }
    }
}
