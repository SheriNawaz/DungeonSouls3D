using UnityEngine;

namespace CWM.Skinn
{
	public static partial class SkinnEx
    {
        public static string MeshDisplayInfo(this Mesh mesh)
        {
			if(mesh == null) return "null";
			string meshInfo = mesh.vertices.Length.ToString () + " verts, " + mesh.triangles.Length.ToString () + " tris ";
			if (mesh.subMeshCount > 1) meshInfo = meshInfo + ", " + mesh.subMeshCount.ToString () + " submeshes ";
			if (mesh.blendShapeCount > 0) meshInfo = meshInfo + ", " + mesh.blendShapeCount.ToString () + " blendShapes ";
			meshInfo = meshInfo + " ";
			if (mesh.uv != null && mesh.uv.Length > 0) meshInfo = meshInfo + "uv";
			if (mesh.uv2 != null && mesh.uv2.Length > 0) meshInfo = meshInfo + ",uv2";
			if (mesh.uv3 != null && mesh.uv3.Length > 0) meshInfo = meshInfo + ",uv3";
			if (mesh.uv4 != null && mesh.uv4.Length > 0) meshInfo = meshInfo + ",uv4";

#if UNITY_2018_2_OR_NEWER
            if (mesh.uv5 != null && mesh.uv5.Length > 0) meshInfo = meshInfo + "uv5";
            if (mesh.uv6 != null && mesh.uv6.Length > 0) meshInfo = meshInfo + ",uv6";
            if (mesh.uv7 != null && mesh.uv7.Length > 0) meshInfo = meshInfo + ",uv7";
            if (mesh.uv8 != null && mesh.uv8.Length > 0) meshInfo = meshInfo + ",uv8";
#endif

            if (mesh.colors != null && mesh.colors.Length > 0) meshInfo = meshInfo + ",colors";
			if (mesh.boneWeights != null && mesh.boneWeights.Length > 0) meshInfo = meshInfo + ",skin";
			return meshInfo;
		}

        public static string MeshDisplayInfo(this MeshFilter context)
        {
            if (context == null) return "null";
            Mesh mesh = context.sharedMesh;
            return MeshDisplayInfo(mesh);
        }

        public static string MeshDisplayInfo(this MeshRenderer context)
        {
            if (context == null) return "null";
            return MeshDisplayInfo(context.GetComponent<MeshFilter>());
        }

        public static string MeshDisplayInfo(this SkinnedMeshRenderer context)
        {
            if (context == null) return "null";
            Mesh mesh = context.sharedMesh;
            return MeshDisplayInfo(mesh);
        }

        public static string MeshDisplayInfo(this Renderer context)
        {
            if (context == null) return "null";

            SkinnedMeshRenderer skinnedMesh = context as SkinnedMeshRenderer;
            if (skinnedMesh) return skinnedMesh.MeshDisplayInfo();
            return MeshDisplayInfo(context as MeshRenderer);
        }
    }
}