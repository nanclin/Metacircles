using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	[SerializeField] private Camera Camera;
	[SerializeField] private Renderer TextureRenderer;
	[SerializeField] private GameObject Dummy;
	[SerializeField] public bool AutoUpdate;
	[SerializeField] public int Width = 100;
	[SerializeField] public int Height = 100;
	[SerializeField] public int Seed = 0;
	[SerializeField] private float Threshold = 0.5f;
	[SerializeField] private float Erase = 0.01f;
	[SerializeField] private float ValueScale = 1;
	[SerializeField] private bool IsBitMap = true;
	[SerializeField] private Vector2[] Metacircles = null;
	[SerializeField] private float[] Radiuses = null;
	[SerializeField] private float[] Powers = null;

	private float[,] Map;

	void OnValidate(){
		if( Width < 1 )
			Width = 1;

		if( Height < 1 )
			Height = 1;
	}

	void Start(){
		Map = new float[Width,Height];
		ResetMap(Map);
	}

	public void EraseMap(){
		ResetMap(Map);
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

	[Range(-5,5)] public float[] Pos = new float[2];
	[Range(0,9)] public int[] Coord = new int[2];
	void Update(){
		Vector2 mousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
		mousePos = ClampPosToMapSpace(Map, mousePos);
		Dummy.transform.position = new Vector3(mousePos.x, mousePos.y, Camera.nearClipPlane);

		int[] coord = PosToCoord(Map,mousePos.x, mousePos.y);
		print("pos to coord: " + coord[0] + ", " + coord[1] );

		DrawMap();
	}

	public void DrawMap(){
		Map = new float[Width,Height];
		if( IsBitMap ){
			DrawMap( ConvertHeightToBitMap( ApplyMetacircles( Map, Metacircles, Radiuses ), Threshold ) );
		}else{
			DrawMap(ApplyMetacircles( Map, Metacircles, Radiuses ) );
		}
	}

	// TODO: add plane offset
	private int[] PosToCoord(float[,] map, float xPos, float yPos){
		float width = map.GetLength(0);
		float height = map.GetLength(1);
		float halfWidth = width / 2f;
		float halfHeight = height / 2f;

		if( xPos < -halfWidth || xPos > halfWidth )
			throw new System.ArgumentException("Position off plane: ", xPos.ToString());

		if( yPos < -halfWidth || yPos > halfHeight )
			throw new System.ArgumentException("Position off plane: ", yPos.ToString());
		
		int xCoord = Mathf.FloorToInt(xPos) + Mathf.FloorToInt(width / 2f);
		int yCoord = Mathf.FloorToInt(yPos) + Mathf.FloorToInt(height / 2f);

		return new int[]{xCoord, yCoord};
	}

	// TODO: add plane offset
	private float[] CoordToPos(float[,] map, int xCoord, int yCoord){
		float width = map.GetLength(0);
		float height = map.GetLength(1);

		if( xCoord < 0 || xCoord >= width )
			throw new System.ArgumentException("Coordinate out of range: ", xCoord.ToString());

		if( yCoord < 0 || yCoord >= height )
			throw new System.ArgumentException("Coordinate out of range: ", yCoord.ToString());

		float xPos = (float)xCoord + 0.5f - width / 2f;
		float yPos = (float)yCoord + 0.5f - height / 2f;

		return new float[]{xPos, yPos};
	}

	private float[,] ApplyMetacircles(float[,] map, Vector2[] circles, float[] radiuses){
		int width = map.GetLength(0);
		int height = map.GetLength(1);
		for( int y = 0; y < height; y++){
			for( int x = 0; x < width; x++){
				float xPos = (float)x + 0.5f - (float)width / 2f;
				float yPos = (float)y + 0.5f - (float)height / 2f;
//				print("coors: " + x + ", " + y);
//				print("pos: " + xPos + ", " + yPos);
				float value = 0;
				for(int i = 0; i < circles.Length; i++){
					float dx = xPos - circles[i].x;
					float dy = yPos - circles[i].y;
					float sqrDis = dx * dx + dy * dy;
					value += (Mathf.Pow( radiuses[i], 2) / sqrDis);
				}
				map[x,y] += value / circles.Length;

//				map[x,y] = ((float)x / (float)width + (float)y / (float)height) / 2f;
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
					value += (Mathf.Pow( Radiuses[i], 2) / sqrDis) * Powers[i];
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
				colorMap[y * width + x] = Color.Lerp(Color.white, Color.red, heightMap[x,y] / ValueScale);
			}
		}

		Texture2D texture = new Texture2D(width, height);
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Point;
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}
}
