using Unity.Netcode;
using UnityEngine;

namespace Defender
{
	public class CharacterBase : NetworkBehaviour
	{
		[SerializeField] private float moveSpeed = 1f;
		[SerializeField] private float defaultSpeed = 1f;
		public float MoveSpeed
		{
			get => moveSpeed;
			set => moveSpeed = value;
		}

		public float DefaultSpeed
		{
			get => defaultSpeed;
			set => defaultSpeed = value;
		}
	}
}