#define ANNOTATE_NAVMESH

using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Profiling;

public class SteeringForNavmesh : MonoBehaviour
{

    [SerializeField] private float _avoidanceForceFactor = 0.75f;

    [SerializeField] private float _minTimeToCollision = 2;

    [SerializeField] private bool _offMeshCheckingEnabled = true;

    [SerializeField] private Vector3 _probePositionOffset = new Vector3(0, 0.2f, 0);

    [SerializeField] private float _probeRadius = 0.1f;
    public float maxForce = 10;

    private Rigidbody rb;
    private AIController ai;
    private Transform myTransform;

    private int _navMeshLayerMask;
    public bool IsPostProcess
    {
        get { return true; }
    }

    private void Start()
    {
        myTransform = transform;
        _navMeshLayerMask = 1 << NavMesh.GetAreaFromName("Default");
        rb = GetComponent<Rigidbody>();
        ai = GetComponent<AIController>();
    }

    private  Vector3 CalculateForce()
    {
        NavMeshHit hit;

        var futurePosition = myTransform.position + (rb.velocity * _minTimeToCollision);
        var movement = futurePosition - myTransform.position;

#if ANNOTATE_NAVMESH
            Debug.DrawRay(myTransform.position, movement, Color.cyan);
#endif

        if (_offMeshCheckingEnabled)
        {
            var probePosition = myTransform.position + _probePositionOffset;

            Profiler.BeginSample("Off-mesh checking");
            NavMesh.SamplePosition(probePosition, out hit, _probeRadius, _navMeshLayerMask);
            Profiler.EndSample();

            if (!hit.hit)
            {
                // we're not on the navmesh
                Profiler.BeginSample("Find closest edge");
                NavMesh.FindClosestEdge(probePosition, out hit, _navMeshLayerMask);
                Profiler.EndSample();

                if (hit.hit)
                {
                    // closest edge found
#if ANNOTATE_NAVMESH
                    Debug.DrawLine(probePosition, hit.position, Color.red);
#endif
                    return (hit.position - probePosition).normalized * maxForce;
                } // no closest edge - too far off the mesh
#if ANNOTATE_NAVMESH
                Debug.DrawLine(probePosition, probePosition + Vector3.up * 3, Color.red);
#endif
                return Vector3.zero;
            }
        }

        Profiler.BeginSample("NavMesh raycast");
        NavMesh.Raycast(myTransform.position, futurePosition, out hit, _navMeshLayerMask);
        Profiler.EndSample();

        if (!hit.hit)
            return Vector3.zero;

        Profiler.BeginSample("Calculate NavMesh avoidance");
        var moveDirection = rb.velocity.normalized;
        var avoidance = PerpendicularComponent(hit.normal, moveDirection);

        avoidance.Normalize(); 

#if ANNOTATE_NAVMESH
        Debug.DrawLine(myTransform.position, myTransform.position + avoidance, Color.white);
#endif

        avoidance += moveDirection * maxForce * _avoidanceForceFactor;

#if ANNOTATE_NAVMESH
        Debug.DrawLine(myTransform.position, myTransform.position + avoidance, Color.yellow);
#endif

        Profiler.EndSample();

        return avoidance;
    }
    
    //Perpendicular:垂直
    public static Vector3 PerpendicularComponent(Vector3 source, Vector3 unitBasis)
    {
        var projection = Vector3.Dot(source, unitBasis);
        var tmp = unitBasis * projection;
        return source - tmp;
    }
}
