using System.Collections;
using UnityEngine;

namespace mothershipScripts
{

	public interface IMovable
	{
		IEnumerator MoveToPosition(Vector3 targetPosition);
	}
}