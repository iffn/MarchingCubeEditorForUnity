using UnityEngine;

public abstract class EditShape : MonoBehaviour
{
    public abstract float Distance(Vector3 point);

    public Vector3 Position => transform.position;
    public Vector3 Scale => transform.localScale;
}
