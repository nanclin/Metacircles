using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public static GameController Instance;

	[Header("References")]
	[SerializeField] private Camera Camera;
	[SerializeField] private MapGenerator MapGenerator;
	[SerializeField] private Character Character;
	[SerializeField] private GameObject CameraProxy;

	[Header("World settings")]
	[SerializeField] private float CameraViewLeftEdge;
	[SerializeField] private float CameraViewRightEdge;
	[SerializeField] private float CameraViewBottomEdge;
	[SerializeField] private float CameraViewTopEdge;
	[SerializeField] private float WorldLeftEdge;
	[SerializeField] private float WorldRightEdge;
	[SerializeField] private float WorldBottomEdge;
	[SerializeField] private float WorldTopEdge;

	[Header("Character settings")]
	[SerializeField] public float CharacterWorldLeftEdge;
	[SerializeField] public float CharacterWorldRightEdge;
	[SerializeField] public float CharacterWorldBottomEdge;
	[SerializeField] public float CharacterWorldTopEdge;

	public Vector2 MousePos{ get; set; }

	private Vector3 InitCameraPos;
	private bool FollowMouse = true;

	void Awake(){
		Instance = this;
	}

	void Start(){
//		InitCameraPos = CameraProxy.transform.position;
		InitCameraPos = Camera.transform.position;
	}

	void Update(){
		if(Input.GetMouseButtonDown(0)){
			FollowMouse = !FollowMouse;
		}
	}

	void FixedUpdate(){
		MousePos = Camera.ScreenToWorldPoint(Input.mousePosition);

		if( FollowMouse)
			Character.TargetPos = MousePos;

		Vector2 pos = (Vector2)Character.transform.position;
		float valX = Anclin.MathUtils.RemapValue01(pos.x, CharacterWorldLeftEdge, CharacterWorldRightEdge);
		float valY = Anclin.MathUtils.RemapValue01(pos.y, CharacterWorldBottomEdge, CharacterWorldTopEdge);
		//		Anclin.Log("char X:{0}, remaped:{1}", pos.x, valX);
//		Anclin.Log("char Y:{0}, remaped:{1}", pos.y, valY);


		float minCameraPosX = WorldLeftEdge + CameraViewLeftEdge;
		float maxCameraPosX = WorldRightEdge + CameraViewRightEdge;
		float minCameraPosY = WorldBottomEdge + CameraViewBottomEdge;
		float maxCameraPosY = WorldTopEdge + CameraViewTopEdge;
		float cameraX = Mathf.Lerp(minCameraPosX, maxCameraPosX, valX);
		float cameraY = Mathf.Lerp(minCameraPosY, maxCameraPosY, valY);

		Camera.transform.position = InitCameraPos + new Vector3(cameraX, cameraY, 0);
//		CameraProxy.transform.position = InitCameraPos + new Vector3(cameraX, cameraY, 0);


		MapGenerator.Metacircles[0] = Character.Pos;

		MapGenerator.DisplayMap();
	}

}
