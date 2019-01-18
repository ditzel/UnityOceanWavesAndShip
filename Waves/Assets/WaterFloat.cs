using Ditzelgames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterFloat : MonoBehaviour
{
    protected Rigidbody Rigidbody;
    protected Waves Waves;
    public float GravMultiplier = 5f;
    public float RotSpeed = 20f;
    public float RotForce = 20f;
    public bool AffectDirection = true;
    public bool NoDrown = false;
    public bool SurfaceAttached = false;
    protected Vector3 SurfaceAttachedVector;
    public Transform[] FloatPoints;

    protected float WaterLine;
    protected Vector3[] WaterLinePoints;

    protected Vector3 TargetUp;
    protected Vector3 centerOffset;

    public Vector3 Center { get { return transform.position + centerOffset; } }

    // Start is called before the first frame update
    void Awake()
    {
        Waves = FindObjectOfType<Waves>();
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.useGravity = false;

        WaterLinePoints = new Vector3[FloatPoints.Length];
        for (int i = 0; i < FloatPoints.Length; i++)
            WaterLinePoints[i] = FloatPoints[i].position;
        centerOffset = PhysicsHelper.GetCenter(WaterLinePoints) - transform.position;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        WaterLine = 0;
        var pointUnderWater = false;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            //height
            WaterLinePoints[i] = FloatPoints[i].position;
            WaterLinePoints[i].y = Waves.GetHeight(FloatPoints[i].position);
            WaterLine += WaterLinePoints[i].y / FloatPoints.Length;
            if (WaterLinePoints[i].y > FloatPoints[i].position.y)
                pointUnderWater = true;
        }

        TargetUp = PhysicsHelper.GetNormal(WaterLinePoints);

        //gravity
        var gravity = Vector3.zero;
        if (WaterLine > Center.y)
        {
            //under water
            if (NoDrown)
            {
                Rigidbody.position = new Vector3(Rigidbody.position.x, WaterLine - centerOffset.y, Rigidbody.position.z);
            }
            else
            {
                gravity = AffectDirection ? TargetUp * -Physics.gravity.y : -Physics.gravity;
                if (Rigidbody.velocity.y < 0) //Dampen
                    Rigidbody.AddForce(-Physics.gravity * GravMultiplier);
            }
        }
        else
        {
            //above water
            gravity = Physics.gravity;
        }
        PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, GravMultiplier * gravity);


        //rotation
        if (pointUnderWater)
            if (!SurfaceAttached)
            {
                PhysicsHelper.ApplyTorqueToRotate(Rigidbody, Quaternion.FromToRotation(transform.up, TargetUp), RotForce, RotSpeed);
            }
            else
            {
                TargetUp = Vector3.SmoothDamp(transform.up, TargetUp, ref SurfaceAttachedVector, 0.2f);
                Rigidbody.rotation = Quaternion.FromToRotation(transform.up, TargetUp) * Rigidbody.rotation;
            }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (FloatPoints == null)
            return;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            if (FloatPoints[i] == null)
                continue;


            if (Waves != null)
            {

                //draw cube
                Gizmos.color = Color.red;
                Gizmos.DrawCube(WaterLinePoints[i], Vector3.one * 1f);
            }

            //draw sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(FloatPoints[i].position, 0.3f);

        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, WaterLine, Center.z), Vector3.one * 3f);
            Gizmos.DrawRay(new Vector3(Center.x, WaterLine, Center.z), TargetUp * 10f);
        }
    }
}