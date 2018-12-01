using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NPBehave;
using UnityEngine.Profiling;

public partial class AIController : MonoBehaviour
{

    private Root CreateBehaviourTree()
    {
        Debug.Log("m_Behaviour:" + m_Behaviour);
        m_Behaviour = 2;
        switch (m_Behaviour)
        {
            case 1:
                //敌人在地图上四处游走，似乎没有什么危险.
                //return LostBehaviour();
            case 2:
                return AttackOnSightBehaviour();

            default:
                return new Root(new Action(() => MoveTo()));
        }
    }

    private Node RandomFire()
    {
        //return new Action(() => Fire(UnityEngine.Random.Range(0.0f, 1.0f)));
        return null;
    }

    //使用6个传感器来创建对周围环境的感知.
    private void Awareness()
    {
        Vector3 localPos = this.transform.position;
        Vector3 obsticle;
        Vector3 collHit;
        Vector3 rightEye = Quaternion.Euler(0, 30, 0) * this.transform.forward;
        Vector3 leftEye = Quaternion.Euler(0, -30, 0) * this.transform.forward;
        RaycastHit hit;

        //Eyes forward
        Ray eyesFwd = new Ray(this.transform.position, this.transform.forward);
        //Eyes forward 45 degrees to the right
        Ray eyesFwdRight = new Ray(this.transform.position, rightEye);
        //Eyes forward 45 degrees to the left
        Ray eyesFwdLeft = new Ray(this.transform.position, leftEye);
        //Eyes to the back
        Ray eyesBck = new Ray(this.transform.position, -this.transform.forward);
        //Eyes to back 45 on the right
        Ray eyesBckRight = new Ray(this.transform.position, -leftEye);
        //Eyes to the back 45 on the left
        Ray eyesBckLeft = new Ray(this.transform.position, -rightEye);

        //Debug rays
        Debug.DrawRay(this.transform.position, this.transform.forward * sightDistance, Color.red);
        Debug.DrawRay(this.transform.position, rightEye * sightDistance, Color.green);
        Debug.DrawRay(this.transform.position, leftEye * sightDistance, Color.blue);
        Debug.DrawRay(this.transform.position, -this.transform.forward * sightDistance, Color.black);
        Debug.DrawRay(this.transform.position, -rightEye * sightDistance, Color.white);
        Debug.DrawRay(this.transform.position, -leftEye * sightDistance, Color.yellow);

        Profiler.BeginSample("Raycast Profiler");
        if (Physics.Raycast(eyesFwd, out hit, sightDistance))
        {
            collHit = hit.point;
            obsticle = hit.transform.position;
            Debug.Log("Too close in front!!" + hit.collider);
            blackboard["obsticleInFront"] = true;
        }
        else
        {
            blackboard["obsticleInFront"] = false;
        }

        if (Physics.Raycast(eyesFwdRight, out hit, sightDistance))
        {
            collHit = hit.point;
            obsticle = hit.transform.position;

            Debug.Log("Too close to the right!!" + hit.collider);
            blackboard["obsticleInFrontRight"] = true;
        }
        else
        {
            blackboard["obsticleInFrontRight"] = false;
        }

        if (Physics.Raycast(eyesFwdLeft, out hit, sightDistance))
        {
            collHit = hit.point;
            obsticle = hit.transform.position;
            Debug.Log("Too close to the left!!" + hit.collider);
            blackboard["obsticleInFrontLeft"] = true;
        }
        else
        {
            blackboard["obsticleInFrontLeft"] = false;
        }

        if (Physics.Raycast(eyesBck, out hit, sightDistance))
        {
            collHit = hit.point;
            obsticle = hit.transform.position;
            Debug.Log("Too close to the left!!" + hit.collider);
            blackboard["obsticleInBack"] = true;
        }
        else
        {
            blackboard["obsticleInBack"] = false;
        }

        if (Physics.Raycast(eyesBckRight, out hit, sightDistance))
        {
            collHit = hit.point;
            obsticle = hit.transform.position;

            Debug.Log("Too close to the left!!" + hit.collider);
            blackboard["obsticleInBackRight"] = true;
        }
        else
        {
            blackboard["obsticleInBackRight"] = false;
        }

        if (Physics.Raycast(eyesBckLeft, out hit, sightDistance))
        {
            collHit = hit.point;
            obsticle = hit.transform.position;
            Debug.Log("Too close to the left!!" + hit.collider);
            blackboard["obsticleInBackLeft"] = true;
        }
        else
        {
            blackboard["obsticleInBackLeft"] = false;
        }

        Profiler.EndSample();

    }

    private bool LookForTarget(Vector3 targetPos, Vector3 localPos)
    {
        Vector3 elevatedVector = this.transform.position;
        elevatedVector.y = 0.7f;

        RaycastHit hit;
        Ray eyes = new Ray(elevatedVector, targetPos - this.transform.position);

        if (Physics.Raycast(eyes, out hit, localPos.magnitude) && hit.collider.tag == "Player")
        {
            return true; 
        }
        //Debug.Log("Enemy FALSE: "  + hit.collider);

        return false;
    }

    //private Root LostBehaviour()
    //{
    //    return new Root(
    //        new Service(0.2f, UpdatePerception,
    //            //Steering tree
    //            new Selector(
    //                //到了死胡同
    //                new BlackboardCondition("deadEnd", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
    //                    new Sequence(
    //                        //Stop or reverse
    //                        new Selector(
    //                            new NPBehave.Random(0.5f, new Action(() => MoveTo())),
    //                            new NPBehave.Random(1f, new TimeMin(1f, new Action(() => MoveTo())))
    //                        )
    //                        //Turn left or right
    //                        //new Selector(
    //                        //    new NPBehave.Random(0.5f, new TimeMin(4f, new Action(() => Turn(0.5f)))),
    //                        //    new NPBehave.Random(1f, new TimeMin(4f, new Action(() => Turn(-0.5f))))
    //                        //)
    //                    )
    //                ),
    //                //Slows down and Perform sharp left turn
    //                new BlackboardCondition("turnLeft", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
    //                    new Sequence(
    //                        new Action(() => MoveTo(0.3f)),
    //                        new Action(() => Turn(-0.5f))
    //                    )
    //                ),
    //                //Slows down and perform sharp right turn
    //                new BlackboardCondition("turnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
    //                    new Sequence(
    //                        new Action(() => MoveTo(0.3f)),
    //                        new Action(() => Turn(0.5f))
    //                    )
    //                ),
    //                //Obsticle in front turn left or right
    //                new BlackboardCondition("obsticleInFront", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
    //                    new Selector(
    //                        new TimeMin(0.5f, new NPBehave.Random(0.5f, new Action(() => Turn(-0.5f)))),
    //                        new TimeMin(0.5f, new NPBehave.Random(1f, new Action(() => Turn(0.5f))))
    //                    )
    //                ),
    //                //Perform smooth left turn
    //                new BlackboardCondition("smoothTurnLeft", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
    //                    new Selector(
    //                        new NPBehave.Random(0.5f, new Action(() => Turn(-0.1f))), new NPBehave.Random(1f, new Action(() => Turn(-0.3f)))
    //                    )
    //                ),
    //                //Perform smooth right turn
    //                new BlackboardCondition("smoothTurnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
    //                    new Selector(
    //                        new NPBehave.Random(0.5f, new Action(() => Turn(0.1f))),
    //                        new NPBehave.Random(1f, new Action(() => Turn(0.3f)))
    //                    )
    //                ),
    //                //Path is clear Go straight ahead
    //                new Sequence(
    //                    new Action(() => Turn(0f)),
    //                    new Action(() => MoveTo())
    //                )
    //            )
    //        )
    //    );
    //}

    private Root AttackOnSightBehaviour()
    {
        return new Root(
            new Service(0.2f, UpdatePerception,
                new Selector(
                    new BlackboardCondition("clearPathToEnemy", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                        new Selector(
                            new Selector(
                                new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 10f, Stops.IMMEDIATE_RESTART,
                                    new Sequence(
                                        new Selector(
                                            new NPBehave.Random(0.5f, new Action(() => Turn(-0.5f))),
                                            new NPBehave.Random(1.0f, new Action(() => Turn(0.5f)))
                                        ),
                                        new TimeMin(2f, new Action(() => MoveTo())
                                        )
                                    )
                                ),
                                new BlackboardCondition("targetInFront", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                                    new Selector(
                                        new BlackboardCondition("targetOffCentre", Operator.IS_SMALLER_OR_EQUAL, 0.2f, Stops.IMMEDIATE_RESTART,
                                            // Stop turning and fire
                                            new Selector(

                                                new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 20f, Stops.IMMEDIATE_RESTART,
                                                    new Sequence(
                                                        new Action(() => Turn(0.0f)),
                                                        new Action(() => MoveTo()),
                                                        RandomFire()
                                                    )
                                                ),
                                                new Sequence(new Action(() => Turn(0)), new Action(() => MoveTo()))
                                            )
                                        ),
                                        new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                                            // Turn right toward target
                                            new Action(() => Turn(0.5f))
                                        ),
                                        // Turn left toward target
                                        new Action(() => Turn(-0.5f))
                                    )
                                ),
                                new Sequence(
                                    new Action(() => MoveTo()),
                                    new Selector(
                                        new NPBehave.Random(0.5f, new TimeMin(3f, new Action(() => Turn(0.5f)))),
                                        new NPBehave.Random(1.0f, new TimeMin(3f, new Action(() => Turn(-0.5f))))
                                    )
                                )
                            )
                        )
                    ),
                    //Steering tree
                    new Selector(
                        //Reached dead end
                        new BlackboardCondition("deadEnd", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                            new Sequence(
                                //Stop or reverse                         
                                new NPBehave.Random(0.5f, new Action(() => Idle())),                                                         
                                //Turn left or right
                                new Selector(
                                    new NPBehave.Random(0.5f, new TimeMin(4f, new Action(() => Turn(0.5f)))),
                                    new NPBehave.Random(1f, new TimeMin(4f, new Action(() => Turn(-0.5f))))
                                )
                            )
                        ),
 
                        new BlackboardCondition("turnLeft", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                            new Sequence(
                                new Action(() => MoveTo()),
                                new Action(() => Turn(-0.5f))
                            )
                        ),

                        new BlackboardCondition("turnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                            new Sequence(
                                new Action(() => MoveTo()),
                                new Action(() => Turn(0.5f))
                            )
                        ),
         
                        new BlackboardCondition("obsticleInFront", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                            new Selector(
                                new TimeMin(0.5f, new NPBehave.Random(0.5f, new Action(() => Turn(-0.5f)))),
                                new TimeMin(0.5f, new NPBehave.Random(1f, new Action(() => Turn(0.5f))))
                            )
                        ),
  
                        new BlackboardCondition("smoothTurnLeft", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                            new Selector(
                                new NPBehave.Random(0.5f, new Action(() => Turn(-0.1f))),
                                new NPBehave.Random(1f, new Action(() => Turn(-0.3f)))
                            )
                        ),
                  
                        new BlackboardCondition("smoothTurnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                            new Selector(
                                new NPBehave.Random(0.5f, new Action(() => Turn(0.1f))),
                                new NPBehave.Random(1f, new Action(() => Turn(0.3f)))
                            )
                        ),
                        //道路畅通，直走
                        new Sequence(
                            new Action(() => Turn(0f)),
                            new Action(() => MoveTo())
                        )
                    )
                )
            )
        );
    }

    private Root AttackOnSightBehaviour2()
    {
        return new Root(
            new Service(0.2f, UpdatePerception,
                new Selector(
                    new BlackboardCondition("clearPathToEnemy", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                        new Selector(
                            new Selector(
                                new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 10f, Stops.IMMEDIATE_RESTART,
                                    new Sequence(
                                        new Selector(
                                            new NPBehave.Random(0.5f, new Action(() => Turn(-0.5f))),
                                            new NPBehave.Random(1.0f, new Action(() => Turn(0.5f)))
                                        ),
                                        new TimeMin(2f, new Action(() => MoveTo())
                                        )
                                    )
                                ),
                                new BlackboardCondition("targetInFront", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                                    new Selector(
                                        new BlackboardCondition("targetOffCentre", Operator.IS_SMALLER_OR_EQUAL, 0.2f, Stops.IMMEDIATE_RESTART,
                                            // Stop turning and fire
                                            new Selector(

                                                new BlackboardCondition("targetDistance", Operator.IS_SMALLER_OR_EQUAL, 20f, Stops.IMMEDIATE_RESTART,
                                                    new Sequence(
                                                        new Action(() => Turn(0.0f)),
                                                        new Action(() => MoveTo()),
                                                        RandomFire()
                                                    )
                                                ),
                                                new Sequence(new Action(() => Turn(0)), new Action(() => MoveTo()))
                                            )
                                        ),
                                        new BlackboardCondition("targetOnRight", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART,
                                            // Turn right toward target
                                            new Action(() => Turn(0.5f))
                                        ),
                                        // Turn left toward target
                                        new Action(() => Turn(-0.5f))
                                    )
                                ),
                                new Sequence(
                                    new Action(() => MoveTo()),
                                    new Selector(
                                        new NPBehave.Random(0.5f, new TimeMin(3f, new Action(() => Turn(0.5f)))),
                                        new NPBehave.Random(1.0f, new TimeMin(3f, new Action(() => Turn(-0.5f))))
                                    )
                                )
                            )
                        )
                    ),
                   
                    //道路畅通，直走
                    new Sequence(
                        new Action(() => Turn(0f)),
                        new Action(() => MoveTo())
                    )            
                )
            )           
        );
    }

    private void UpdatePerception()
    {
        //Update sensors blackboards
        Awareness();

        Vector3 targetPos = attackTarget.transform.position;
        Vector3 localPos = this.transform.InverseTransformPoint(targetPos);
        Vector3 heading = localPos.normalized;
        blackboard["targetDistance"] = localPos.magnitude;
        blackboard["targetInFront"] = heading.z > 0;
        blackboard["targetOnRight"] = heading.x > 0;
        blackboard["targetOffCentre"] = Mathf.Abs(heading.x);

        //My additions
        //Path is clear
        if (!blackboard.Get<bool>("obsticleInFront") && !blackboard.Get<bool>("obsticleInFrontRight") && !blackboard.Get<bool>("obsticleInFrontLeft"))
        {
            blackboard["deadEnd"] = false;
            blackboard["turnLeft"] = false;
            blackboard["turnRight"] = false;
            blackboard["smoothTurnLeft"] = false;
            blackboard["smoothTurnRight"] = false;
            Debug.Log("Clear");
        }
        //Deadend reached
        if (blackboard.Get<bool>("obsticleInFront") && blackboard.Get<bool>("obsticleInFrontRight") && blackboard.Get<bool>("obsticleInFrontLeft"))
        {
            blackboard["deadEnd"] = true;

            blackboard["turnRight"] = false;
            blackboard["turnLeft"] = false;
            blackboard["smoothTurnLeft"] = false;
            blackboard["smoothTurnRight"] = false;
            Debug.Log("DeadEnd");
        }
        //Obstiles to the front and to the right
        if (blackboard.Get<bool>("obsticleInFront") && blackboard.Get<bool>("obsticleInFrontRight") && !blackboard.Get<bool>("obsticleInFrontLeft"))
        {
            blackboard["turnLeft"] = true;

            blackboard["deadEnd"] = false;
            blackboard["turnRight"] = false;
            blackboard["smoothTurnLeft"] = false;
            blackboard["smoothTurnRight"] = false;
            Debug.Log("Left");
        }
        //Obstiles to the front and to the left
        if (blackboard.Get<bool>("obsticleInFront") && !blackboard.Get<bool>("obsticleInFrontRight") && blackboard.Get<bool>("obsticleInFrontLeft"))
        {
            blackboard["turnRight"] = true;

            blackboard["deadEnd"] = false;
            blackboard["turnLeft"] = false;
            blackboard["smoothTurnLeft"] = false;
            blackboard["smoothTurnRight"] = false;
            Debug.Log("Right");
        }
        //Obsticles to the right
        if (!blackboard.Get<bool>("obsticleInFront") && blackboard.Get<bool>("obsticleInFrontRight") && !blackboard.Get<bool>("obsticleInFrontLeft"))
        {
            blackboard["smoothTurnLeft"] = true;

            blackboard["deadEnd"] = false;
            blackboard["turnLeft"] = false;
            blackboard["turnRight"] = false;
            blackboard["smoothTurnRight"] = false;
            Debug.Log("smooth left");
        }
        //Obstices to the left
        if (!blackboard.Get<bool>("obsticleInFront") && !blackboard.Get<bool>("obsticleInFrontRight") && blackboard.Get<bool>("obsticleInFrontLeft"))
        {
            blackboard["smoothTurnRight"] = true;

            blackboard["deadEnd"] = false;
            blackboard["turnLeft"] = false;
            blackboard["turnRight"] = false;
            blackboard["smoothTurnLeft"] = false;
            Debug.Log("smooth right");
        }

        //Obsticles to the left and right
        if (!blackboard.Get<bool>("obsticleInFront") && blackboard.Get<bool>("obsticleInFrontRight") && blackboard.Get<bool>("obsticleInFrontLeft"))
        {
            blackboard["deadEnd"] = false;
            blackboard["turnLeft"] = false;
            blackboard["turnRight"] = false;
            blackboard["smoothTurnLeft"] = false;
            blackboard["smoothTurnRight"] = false;
            Debug.Log("Tunnel");
        }

        //通向敌人之路
        blackboard["clearPathToEnemy"] = LookForTarget(targetPos, localPos);
    }

    protected static readonly Color GizmoColor = new Color(51 / 255f, 255 / 255f, 255 / 255f);
    protected static readonly Color GizmoBlockedColor = Color.red;


    public void OnDrawGizmosSelected()
    {
        if (!isActiveAndEnabled) return;

        Gizmos.color = GizmoColor;

        //弧长
        float fovRadius = fov * Mathf.PI / 180.0f;
        var leftRayPoint = transform.TransformPoint(new Vector3(sightDistance * Mathf.Sin(fovRadius), 0, sightDistance * Mathf.Cos(fovRadius)));
        var rightRayPoint = transform.TransformPoint(new Vector3(-sightDistance * Mathf.Sin(fovRadius), 0, sightDistance * Mathf.Cos(fovRadius)));

        Gizmos.color = new Color(250 / 255f, 0, 0);
        var drawPos = transform.position + Vector3.up * 1.8f;
        Gizmos.DrawLine(drawPos, drawPos + transform.forward * sightDistance);
        Gizmos.DrawLine(drawPos, leftRayPoint + Vector3.up * 1.8f);
        Gizmos.DrawLine(drawPos, rightRayPoint + Vector3.up * 1.8f);

    }
}

