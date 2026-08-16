using UnityEngine;

[ExecuteInEditMode]
public class CCTVSpawnPointGizmo : MonoBehaviour
{
    public Color gizmoColor = Color.red;
    public string pointLabel = "CCTV Spawn Point";

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1.2f);
        Gizmos.DrawWireCube(transform.position + transform.forward * 1.2f, new Vector3(0.1f, 0.1f, 0.1f));

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.normal.textColor = gizmoColor;
        style.fontStyle = FontStyle.Bold;
        style.fontSize = 12;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, pointLabel, style);
#endif
    }
}
