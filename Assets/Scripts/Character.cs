using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {
	
    [SerializeField] private float Speed;
    [SerializeField] private float TopSpeed;

    public Vector2 Pos;
    public Vector2 TargetPos;

    private Vector3 InitPos;

    void Start() {
//		InitPos = transform.position;	
        Pos = (Vector2) transform.position;
    }

    void FixedUpdate() {
        Vector2 moveVector = (TargetPos - Pos) * Time.fixedDeltaTime * Speed;
        moveVector = Vector2.ClampMagnitude(moveVector, TopSpeed);
        Pos += moveVector;
			
//		Pos = ClampPosToMapSpace( BrushMap, Pos );
        Pos.x = Mathf.Clamp(Pos.x, GameController.Instance.CharacterWorldLeftEdge, GameController.Instance.CharacterWorldRightEdge);
        Pos.y = Mathf.Clamp(Pos.y, GameController.Instance.CharacterWorldBottomEdge, GameController.Instance.CharacterWorldTopEdge);

        transform.position = InitPos + new Vector3(Pos.x, Pos.y, -10);
    }

}
