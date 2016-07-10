using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	[SerializeField] private Camera Camera;
	[SerializeField] private Character Character;
	[SerializeField] private MapGenerator MapGenerator;

	public Vector2 MousePos{ get; set; }

	void FixedUpdate(){
		MousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
		Character.TargetPos = MousePos;
	}
}
