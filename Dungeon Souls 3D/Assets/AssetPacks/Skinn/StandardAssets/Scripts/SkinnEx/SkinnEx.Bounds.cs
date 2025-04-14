using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        private static bool GetSkinnedBounds(this SkinnedMeshRenderer source, out Bounds bounds)
        {
            bounds = new Bounds();
            if (!HasMinimumRequirements(source)) { return false; }
            var skinner = new LinearSkinner();
            Vector3[] bakedVertices, bakedNormals; Vector4[] bakedTangents;
            var options = new BakeOptions() { world = false };
            if (!skinner.GetSkinning(source, out bakedVertices, out bakedNormals, out bakedTangents, options, true)) { return false; }
            bounds = bakedVertices.CalculateBounds();
            return true;
        }

        public static void SetBounds(this SkinnedMeshRenderer source , bool skinnedBounds = true)
        {
            if (!HasSharedMesh(source) || IsNullOrEmpty(source.bones)) return;
            Bounds bounds;

            if (skinnedBounds && source.GetSkinnedBounds(out bounds)) { }
            else bounds = source.sharedMesh.GetBoundsRecalculated();

            source.transform.ResetLocal();
            var data = new BoundsData(bounds);
            var transform = source.rootBone ? source.rootBone : source.transform;
            bounds = data.GetBounds(GetLocalMatrix(transform).inverse * source.transform.root.localToWorldMatrix);
            source.localBounds = bounds;
        }


        public static void SetBounds(this SkinnedMeshRenderer[] sources, bool skinnedBounds = true)
        {
            var wholeModel = new BoundsData();
            foreach (var source in sources)
            {
                if (!HasSharedMesh(source) || IsNullOrEmpty(source.bones)) return;
                Bounds bounds;

                if (skinnedBounds && source.GetSkinnedBounds(out bounds)) { }
                else bounds = source.sharedMesh.vertices.CalculateBounds();

                wholeModel = wholeModel.Encapsulated(bounds);
            }

            foreach (var source in sources)
            {
                if (!HasSharedMesh(source) || IsNullOrEmpty(source.bones)) return;
                source.transform.ResetLocal();
                var transform = source.rootBone ? source.rootBone : source.transform;
                var newBounds = wholeModel.GetBounds(GetLocalMatrix(transform).inverse * source.transform.root.localToWorldMatrix);
                source.localBounds = newBounds;
            }
            
        }

        public static Bounds GetBoundsRecalculated(this Mesh mesh)
        {
            if (mesh == null) return new Bounds();
            Bounds bounds = mesh.bounds;
            mesh.RecalculateBounds();
            Bounds newBounds = mesh.bounds;
            mesh.bounds = bounds;
            return newBounds;
        }

        public static Bounds CalculateBounds(this Vector3[] points)
        {
            return CalculateBounds(points, Matrix4x4.identity);
        }

        public static Bounds CalculateBounds(this Vector3[] points, Matrix4x4 transform)
        {
            var minX = Mathf.Infinity;
            var maxX = -Mathf.Infinity;
            var minY = Mathf.Infinity;
            var maxY = -Mathf.Infinity;
            var minZ = Mathf.Infinity;
            var maxZ = -Mathf.Infinity;

            for (int i = 0; i < points.Length; i++)
            {
                var pos = transform.MultiplyPoint(points[i]);
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
                if (pos.z < minZ) minZ = pos.z;
                if (pos.z > maxZ) maxZ = pos.z;
            }

            var sizeX = maxX - minX;
            var sizeY = maxY - minY;
            var sizeZ = maxZ - minZ;
            var center = new Vector3(minX + sizeX / 2.0f, minY + sizeY / 2.0f, minZ + sizeZ / 2.0f);
            return new Bounds(center, new Vector3(sizeX, sizeY, sizeZ));
        }

        public static List<Vector3> ToPointList(this Bounds bounds)
        {
            BoundsData data = new BoundsData(bounds);
            return data.ToList();
        }

        public static Vector3[] ToPointArray(this Bounds bounds)
        {
            BoundsData data = new BoundsData(bounds);
            return data.ToArray();
        }

        public static Bounds Transformed(this Bounds bounds, Transform transform)
        {
            BoundsData data = new BoundsData(bounds);
            return data.GetBounds(transform.localToWorldMatrix);
        }
    }
}