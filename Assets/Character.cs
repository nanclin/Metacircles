using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {
	
	[SerializeField] private float Speed = 20;

	private Vector2 Pos;
	public Vector2 TargetPos;

	void Start(){
		Pos = (Vector2)transform.position;
	}

	void FixedUpdate(){
		
		Vector2 dir = (TargetPos - Pos).normalized;
		Pos += dir * Time.fixedDeltaTime * Speed;
//		Pos = ClampPosToMapSpace( BrushMap, Pos );

		transform.position = Pos;
	}

}
