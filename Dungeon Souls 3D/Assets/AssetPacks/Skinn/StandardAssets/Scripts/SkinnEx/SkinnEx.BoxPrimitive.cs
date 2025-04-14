using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static string BoxPrimativeID(Color color, Vector3 scale, Vector3 parentScale, float size)
        {
            const string name = "BoxPrimative";
            var id = name + color.ToString() + parentScale.ToString() + size.ToString();
            return id;
        }

        public static Mesh BoxPrimative() { return BoxPrimative(Color.white, Vector3.one, Vector3.one, 1f); }

        public static Mesh BoxPrimative(float size) { return BoxPrimative(Color.white, Vector3.one, Vector3.one, size); }

        public static Mesh BoxPrimative(Color color) { return BoxPrimative(color, Vector3.one, Vector3.one, 1f); }

        public static Mesh BoxPrimative(Color color, Vector3 scale, Vector3 parentScale,  float size = 1, Mesh mesh = null, bool markDynamic = true){

			float scaleX = (size * scale.x) / (Mathf.Abs(parentScale.x) + 0.000001f);
			float scaleY = (size * scale.y) / (Mathf.Abs(parentScale.y) + 0.000001f);
            float scaleZ = (size * scale.z) / (Mathf.Abs(parentScale.z) + 0.000001f);

            Vector3[] verts = new Vector3[24] {
				new Vector3(0.5f * scaleX, -0.5f * scaleY, 0.5f * scaleZ), new Vector3(-0.5f * scaleX, -0.5f * scaleY, 0.5f * scaleZ), new Vector3(0.5f * scaleX, 0.5f * scaleY, 0.5f * scaleZ), new Vector3(-0.5f * scaleX, 0.5f * scaleY, 0.5f * scaleZ), new Vector3(0.5f * scaleX, 0.5f * scaleY, -0.5f * scaleZ), new Vector3(-0.5f * scaleX, 0.5f * scaleY, -0.5f * scaleZ), new Vector3(0.5f * scaleX, -0.5f * scaleY, -0.5f * scaleZ), new Vector3(-0.5f * scaleX, -0.5f * scaleY, -0.5f * scaleZ), new Vector3(0.5f * scaleX, 0.5f * scaleY, 0.5f * scaleZ), new Vector3(-0.5f * scaleX, 0.5f * scaleY, 0.5f * scaleZ), new Vector3(0.5f * scaleX, 0.5f * scaleY, -0.5f * scaleZ), new Vector3(-0.5f * scaleX, 0.5f * scaleY, -0.5f * scaleZ), new Vector3(0.5f * scaleX, -0.5f * scaleY, -0.5f * scaleZ), new Vector3(0.5f * scaleX, -0.5f * scaleY, 0.5f * scaleZ), new Vector3(-0.5f * scaleX, -0.5f * scaleY, 0.5f * scaleZ), new Vector3(-0.5f * scaleX, -0.5f * scaleY, -0.5f * scaleZ), new Vector3(-0.5f * scaleX, -0.5f * scaleY, 0.5f * scaleZ), new Vector3(-0.5f * scaleX, 0.5f * scaleY, 0.5f * scaleZ), new Vector3(-0.5f * scaleX, 0.5f * scaleY, -0.5f * scaleZ), new Vector3(-0.5f * scaleX, -0.5f * scaleY, -0.5f * scaleZ), new Vector3(0.5f * scaleX, -0.5f * scaleY, -0.5f * scaleZ), new Vector3(0.5f * scaleX, 0.5f * scaleY, -0.5f * scaleZ), new Vector3(0.5f * scaleX, 0.5f * scaleY, 0.5f * scaleZ), new Vector3(0.5f * scaleX, -0.5f * scaleY, 0.5f * scaleZ)		};
			Vector2[] uvs = new Vector2[24] {
				new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)
			};
			int[] tris = new int[36] {
				0, 2, 3, 0, 3, 1, 8, 4, 5, 8, 5, 9, 10, 6, 7, 10, 7, 11, 12, 13, 14, 12, 14, 15, 16, 17, 18, 16, 18, 19, 20, 21, 22, 20, 22, 23
			};
			Vector3[] normals = new Vector3[24] {
				new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f)
			};

			if(mesh == null)
            {
                mesh = new Mesh();
               if(markDynamic) mesh.MarkDynamic();
            }

            mesh.name = BoxPrimativeID(color, scale, parentScale, size);

            mesh.vertices = verts;
			mesh.normals = normals;
			mesh.uv = uvs;
			mesh.triangles = tris;
			Color[] colors = new Color[mesh.vertices.Length];
			for (int i = 0; i < mesh.vertices.Length; i++) colors [i] = color;
			mesh.colors = colors;
			mesh.RecalculateBounds ();
			return mesh;
		}
	}
}