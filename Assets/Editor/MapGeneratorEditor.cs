using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

	public override void OnInspectorGUI ()
	{
		MapGenerator mapGen = (MapGenerator)target;

//		DrawDefaultInspector();

		if(DrawDefaultInspector()){
			if(mapGen.AutoUpdate){
				mapGen.DisplayMap();
			}
		}

//		if( GUILayout.Button("Generate")){
//			mapGen.DisplayMap();
//		}

//		if( GUILayout.Button("Erase")){
//			mapGen.EraseMap();
//		}
	}
}
