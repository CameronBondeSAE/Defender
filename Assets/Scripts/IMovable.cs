using System.Collections;
using UnityEngine;

public interface IMovable
{
    IEnumerator MoveToPosition(Vector3 targetPosition);
}
