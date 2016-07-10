using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {
	
	[SerializeField] private float Speed;
	[SerializeField] private float TopSpeed;

	public Vector2 Pos{ get; private set; }
	public Vector2 TargetPos;

	void Start(){
		Pos = (Vector2)transform.position;
	}

	void FixedUpdate(){
		Vector2 moveVector = (TargetPos - Pos) * Time.fixedDeltaTime * Speed;
		moveVector = Vector2.ClampMagnitude( moveVector, TopSpeed );
		Pos += moveVector;
			
//		Pos = ClampPosToMapSpace( BrushMap, Pos );

		transform.position = Pos;
	}

}
