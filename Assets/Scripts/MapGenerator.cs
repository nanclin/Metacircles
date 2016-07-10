using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	[SerializeField] private Renderer TextureRenderer;
	[SerializeField] private Color ShadowColor = Color.black;
	[SerializeField] private Color LightColor = Color.white;
	[SerializeField] private Color DebugThresholdColor = Color.red;
	[SerializeField] public bool AutoUpdate;
	[SerializeField] public int Width = 100;
	[SerializeField] public int Height = 100;
	[Range(1,1000)][SerializeField] private float ValueScale = 1;
	[Range(0.01f,1f)][SerializeField] private float ThresholdZero = 0.01f;
	[Range(0.01f,1f)][SerializeField] private float Threshold = 0.5f;
	[Range(1f,10f)][SerializeField] private float FalloffPower = 1f;
	[SerializeField] private bool DebugThreshold;
	[SerializeField] public Vector2[] Metacircles = null;
	[SerializeField] private float[] Radiuses = null;

	private float[,] Map;

	void OnValidate(){
		if( Width < 1 )
			Width = 1;

		if( Height < 1 )
			Height = 1;
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

	public void DisplayMap(){
		Map = new float[Width, Height];
		ResetMap(Map);
//		DrawMap( ApplyMetacircles( Map, Metacircles, Radiuses ) );
//		DrawMap( ConvertHeightToBitMap( ApplyMetacircles( Map, Metacircles, Radiuses ), Threshold ) );
//		DrawMap( GenerateGradientMap(Width, Height) );
//		DrawMap( ApplyBrush( Map, 10, Vector2.zero ) );
		DrawMap( CombineMaps( new List<float[,]>{ ApplyMetacircles( Map, Metacircles, Radiuses ), ApplyBrush( Map, 10, Vector2.zero ) } ) );
	}

	// Generate methods ////////////////////////////////////////////////////

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
//			value = Mathf.Clamp(value, 0, circles.Length);
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

	private float[,] ApplyBrush( float[,] map, float radius, Vector2 pos ){
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
		return map;
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

	private float[,] ConvertHeightToBitMap(float[,] heightMap, float threshold){
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);
		float[,] bitMap = new float[width,height];

		for( int y = 0; y < height; y++){
			for( int x = 0; x < width; x++){
				bitMap[x,y] = heightMap[x,y] / ValueScale > threshold ? 1 : 0;
			}
		}
		return bitMap;
	}

	// Texture methods ////////////////////////////////////////////////////

	public void DrawMap(float[,] heightMap){
		Texture2D texture = TextureFromHeightMap(heightMap);
		TextureRenderer.sharedMaterial.mainTexture = texture;
		TextureRenderer.transform.localScale = new Vector3(texture.width/10f, 1, texture.height/10f);
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

	// Helper methods ////////////////////////////////////////////////////

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

}
