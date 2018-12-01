using UnityEngine;
using System.Collections;
using DrawAttackAreaTool;

public class DrawAttackAreaTest : MonoBehaviour
{

    void Start()
    {
        //DrawTools.DrawSector(transform, transform.localPosition, 60, 3);

        //DrawTools.DrawCircle(transform, transform.localPosition, 3);

        DrawTools.DrawRectangle(transform, transform.localPosition, 5, 2);

        //DrawTools.DrawSectorSolid(transform, transform.localPosition, 60, 3);

        //DrawTools.DrawCircleSolid(transform, transform.localPosition, 3);

        //DrawTools.DrawRectangleSolid(transform, transform.localPosition, 5, 2);
    }

}


