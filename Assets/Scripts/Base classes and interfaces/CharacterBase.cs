using Unity.Netcode;
using UnityEngine;

namespace Defender
{
	public class CharacterBase : NetworkBehaviour
	{
		[SerializeField] private float moveSpeed = 5f;
		[SerializeField] private float defaultSpeed = 5f;
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