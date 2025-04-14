using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DungeonGenerator dungeonGenerator = (DungeonGenerator)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Generate Dungeon"))
        {
            dungeonGenerator.RegenerateDungeon();
        }
    }
}
#endif