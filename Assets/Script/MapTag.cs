using UnityEngine;
using System.Collections;

public class MapTag : MonoBehaviour {

    public GameObject target;

    public PathFinder.MapDistance distance;

    public static bool showINF;

    public static bool showAllGizmo;

    void OnDrawGizmos()
    {
        if (!showAllGizmo)
            return;
        DrawTagGizmo();
    }

    void OnDrawGizmosSelected()
    {
        if (showAllGizmo)
            return;
        DrawTagGizmo();      
    }

    void DrawTagGizmo()
    {
        if (target == null)
            return;

        Color r;

        switch (distance)
        {
            case PathFinder.MapDistance.Self:
                Gizmos.DrawSphere(this.transform.position, 0.5f);
                return;
            case PathFinder.MapDistance.Jump:
                r = Color.blue;
                break;
            case PathFinder.MapDistance.Walk:
                r = Color.green;
                break;
            default://没有连通的不绘制
                r = Color.red;
                if (!showINF)
                    return;
                else
                    break;
        }

        Debug.DrawLine(this.transform.position, target.transform.position, r);
    }
}
