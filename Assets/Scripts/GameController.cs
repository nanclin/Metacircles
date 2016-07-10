using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	[Header("References")]
	[SerializeField] private Camera Camera;
	[SerializeField] private MapGenerator MapGenerator;
	[SerializeField] private Character Character;

	public Vector2 MousePos{ get; set; }

	void FixedUpdate(){
		MousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
		Character.TargetPos = MousePos;

		MapGenerator.Metacircles[0] = Character.Pos;

		MapGenerator.DisplayMap();
	}
}
