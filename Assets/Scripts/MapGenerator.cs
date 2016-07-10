using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	[SerializeField] private Camera Camera;
	[SerializeField] private Renderer TextureRenderer;
	[SerializeField] private GameObject Dummy;
	[SerializeField] private Color ShadowColor = Color.black;
	[SerializeField] private Color LightColor = Color.white;
	[SerializeField] private Color DebugThresholdColor = Color.red;
	[SerializeField] public bool AutoUpdate;
	[SerializeField] public int Width = 100;
	[SerializeField] public int Height = 100;
	[SerializeField] public int Seed = 0;
	[Range(1,1000)][SerializeField] private float ValueScale = 1;
	[Range(0.01f,1f)][SerializeField] private float ThresholdZero = 0.01f;
	[Range(0.01f,1f)][SerializeField] private float Threshold = 0.5f;
	[Range(1f,10f)][SerializeField] private float FalloffPower = 1f;
	[SerializeField] private float CharacterSpeed = 1f;
	[SerializeField] private float Erase = 0.01f;
	[SerializeField] private bool IsBitMap;
	[SerializeField] private bool DebugThreshold;
	[SerializeField] private Vector2[] Metacircles = null;
	[SerializeField] private float[] Radiuses = null;

	private float[,] Map;
	private float[,] BrushMap;
	private bool FollowMouse = true;
	private Vector2 MousePos;
	private Vector2 CharacterPos;

	void OnValidate(){
		if( Width < 1 )
			Width = 1;

		if( Height < 1 )
			Height = 1;
	}

	void Start(){
		Map = new float[Width,Height];
		BrushMap = new float[Width,Height];
		ResetMap(Map);
		ResetMap(BrushMap);

		CharacterPos = Metacircles[0];
	}

	public void EraseMap(){
		ResetMap(Map);
		ResetMap(BrushMap);
	}

	private void ResetMap(float[,] map){
		int width = map.GetLength(0);
		int height = map.GetLength(1);
		for( int y = 0; y < height; y++){
			for( int x = 0; x < width; x++){
				map[x,y] = 0;
			}
		}
	}

	private Vector2 ClampPosToMapSpace(float[,] map, Vector2 pos){
		float width = map.GetLength(0);
		float height = map.GetLength(1);
		float halfWidth = width / 2f;
		float halfHeight = height / 2f;
		pos.x = Mathf.Clamp(pos.x, -halfWidth, halfWidth );
		pos.y = Mathf.Clamp(pos.y, -halfHeight, halfHeight );
		return pos;
	}

	private bool IsInMapSpace(float[,] map, Vector2 pos){
		float width = map.GetLength(0);
		float height = map.GetLength(1);
		float halfWidth = width / 2f;
		float halfHeight = height / 2f;
		return (pos.x >= -halfWidth && pos.x <= halfWidth && pos.y >= -halfHeight && pos.y <= halfHeight);
	}

	private bool IsInMapSpace(float[,] map, int[] coord){
		float width = map.GetLength(0);
		float height = map.GetLength(1);
		return (coord[0] >= 0 && coord[0] < width && coord[1] >= 0 && coord[1] < height );
	}

//	void FixedUpdate(){
//		if( FollowMouse ){
//			Vector2 dir = (MousePos - CharacterPos).normalized;
//			CharacterPos += dir * Time.fixedDeltaTime * CharacterSpeed;
//			CharacterPos = ClampPosToMapSpace( BrushMap, CharacterPos );
//		}
//	}
//
//	void Update(){
//		if( Input.GetMouseButtonDown(0) ){
//			FollowMouse = !FollowMouse;
//		}
//
//		if( FollowMouse ){
//			MousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
//			MousePos = ClampPosToMapSpace(Map, MousePos);
//
//			ApplyBrush(BrushMap, Radiuses[0], CharacterPos);
//			DrawMap( BrushMap );
//		}
//	}

	public void DrawMap(){
		Map = new float[Width,Height];
		if( IsBitMap ){
			DrawMap( ConvertHeightToBitMap( ApplyMetacircles( Map, Metacircles, Radiuses ), Threshold ) );
		}else{
			float[,] metacirclesMap = ApplyMetacircles( Map, Metacircles, Radiuses );
//			float[,] gradientMap = GenerateGradientMap(Width, Height);
			List<float[,]> maps = new List<float[,]>{ metacirclesMap };
			DrawMap( CombineMaps(maps) );
		}
	}

	// TODO: add plane offset
	private int[] PosToCoord(float[,] map, Vector2 pos){
		float width = map.GetLength(0);
		float height = map.GetLength(1);
		float halfWidth = width / 2f;
		float halfHeight = height / 2f;

		if( pos.x < -halfWidth || pos.x > halfWidth )
			throw new System.ArgumentException("Position off plane: ", pos.x.ToString());

		if( pos.y < -halfWidth || pos.y > halfHeight )
			throw new System.ArgumentException("Position off plane: ", pos.x.ToString());
		
		int xCoord = Mathf.FloorToInt(pos.x) + Mathf.FloorToInt(width / 2f);
		int yCoord = Mathf.FloorToInt(pos.y) + Mathf.FloorToInt(height / 2f);

		return new int[]{xCoord, yCoord};
	}

	// TODO: add plane offset
	private Vector2 CoordToPos(float[,] map, int xCoord, int yCoord){
		float width = map.GetLength(0);
		float height = map.GetLength(1);

		if( xCoord < 0 || xCoord >= width )
			throw new System.ArgumentException("Coordinate out of range: ", xCoord.ToString());

		if( yCoord < 0 || yCoord >= height )
			throw new System.ArgumentException("Coordinate out of range: ", yCoord.ToString());

		float xPos = (float)xCoord + 0.5f - width / 2f;
		float yPos = (float)yCoord + 0.5f - height / 2f;

		return new Vector2( xPos, yPos );
	}

	private float[,] ApplyMetacircles(float[,] map, Vector2[] circles, float[] radiuses){
		int width = map.GetLength(0);
		int height = map.GetLength(1);
		for( int y = 0; y < height; y++){
			for( int x = 0; x < width; x++){
				map[x,y] += GetMetacirclesValue(map, circles, radiuses, x, y) / circles.Length;
			}
		}
		return map;
	}

	private float GetMetacirclesValue( float[,] map, Vector2[] circles, float[] radiuses, int x, int y ){
		Vector2 pos = CoordToPos(map, x, y );
		float sum = 0;
		for(int i = 0; i < circles.Length; i++){
			float dx = pos.x - circles[i].x;
			float dy = pos.y - circles[i].y;
			float sqrDis = dx * dx + dy * dy;
			float value = Mathf.Pow( radiuses[i], 2) / sqrDis;
			value = Mathf.Pow(value, FalloffPower);
			sum += value;
		}
		return sum;
	}

	private float[,] GenerateGradientMap(int width, int height){
		float[,] map = new float[width,height];
		for( int y = 0; y < height; y++){
			for( int x = 0; x < width; x++){
				map[x,y] += (float)x / (float)width;
			}
		}
		return map;
	}

	private void ApplyBrush( float[,] map, float radius, Vector2 pos ){
		for( int y = (int)-radius; y <= radius; y++ ){
			for( int x = (int)-radius; x <= radius; x++ ){
				int[] centerCoord = PosToCoord(map, pos);
				int px = centerCoord[0] + x;
				int py = centerCoord[1] + y;
				if( !IsInMapSpace(map, new int[2]{px,py} )){
					continue;
				}
				float sqrDis = x*x + y*y;
				map[px,py] += Mathf.Pow(radius,2) / sqrDis > 1 ? 1 : 0;
			}
		}
	}

	private float[,] CombineMaps(List<float[,]> maps){
//		Debug.LogWarning("Maps might not be the same size!");

		int width = maps[0].GetLength(0);
		int height = maps[0].GetLength(1);
		float[,] map = new float[width,height];

		for(int i = 0; i < maps.Count; i++){
			for(int y = 0; y < height; y++){
				for( int x = 0; x < width; x++){
					map[x,y] += maps[i][x,y] / maps.Count;
				}
			}
		}
		return map;
	}

	private float[,] ModifyMap(float[,] map){
		int width = map.GetLength(0);
		int height = map.GetLength(1);
		for( int y = 0; y < height; y++){
			for( int x = 0; x < width; x++){
				float xCoor = -((float)x - width / 2);
				float yCoor = -((float)y - height / 2);
				float value = 0;
				float numOfCircles = Metacircles.Length;
				for(int i = 0; i < numOfCircles; i++){
					float dx = xCoor - Metacircles[i].x;
					float dy = yCoor - Metacircles[i].y;
					float sqrDis = dx * dx + dy * dy;
					value += (Mathf.Pow( Radiuses[i], 2) / sqrDis);
				}

//				if( x == width/2 && y == height/2 ){
//					print(value);
//				}
//
				value /= numOfCircles;

				float newMapValue = map[x,y];
				newMapValue += value;
				if( newMapValue < 3 )
					newMapValue -= Erase;
				map[x,y] = Mathf.Clamp(newMapValue, 0, 10);
			}
		}
		return map;
	}

	public void DrawMap(float[,] heightMap, int seed = 0){
		Texture2D texture = TextureFromHeightMap(heightMap);
		TextureRenderer.sharedMaterial.mainTexture = texture;
		TextureRenderer.transform.localScale = new Vector3(texture.width/10f, 1, texture.height/10f);
	}

	private float[,] ConvertHeightToBitMap(float[,] heightMap, float threshold){
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);
		float[,] bitMap = new float[width,height];

		for( int y = 0; y < height; y++){
			for( int x = 0; x < width; x++){
				bitMap[x,y] = heightMap[x,y] / ValueScale > threshold ? 1 : 0;
//				bitMap[x,y] = heightMap[x,y];
			}
		}
		return bitMap;
	}

	public Texture2D TextureFromHeightMap(float[,] heightMap){
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);

		Color[] colorMap = new Color[width * height];
		for( int y = 0; y < height; y++){
			for( int x = 0; x < width; x++){
				float value = heightMap[x,y];
				Color color = Color.Lerp(ShadowColor, LightColor, value / ValueScale);

				if( DebugThreshold ){
					if( value <= ThresholdZero || value >= Threshold ){
						color = Color.Lerp( color, DebugThresholdColor, DebugThresholdColor.a );
					}
				}

				colorMap[y * width + x] = color;
			}
		}

		Texture2D texture = new Texture2D(width, height);
		texture.wrapMode = TextureWrapMode.Repeat;
		texture.filterMode = FilterMode.Point;
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}
}
