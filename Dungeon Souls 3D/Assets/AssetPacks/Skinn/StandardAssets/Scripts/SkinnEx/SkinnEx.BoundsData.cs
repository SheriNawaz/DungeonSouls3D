using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        private class BoundsData
        {
            public Vector3 a1;
            public Vector3 a2;
            public Vector3 a3;
            public Vector3 a4;
            public Vector3 b1;
            public Vector3 b2;
            public Vector3 b3;
            public Vector3 b4;

            public BoundsData()
            {
                this.a1 = Vector3.zero;
                this.a2 = Vector3.zero;
                this.a3 = Vector3.zero;
                this.a4 = Vector3.zero;
                this.b1 = Vector3.zero;
                this.b2 = Vector3.zero;
                this.b3 = Vector3.zero;
                this.b4 = Vector3.zero;
            }

            public bool HasValues()
            {
                var points = ToList();
                foreach (var point in points) if (point.sqrMagnitude > 0.0f) return true;
                return false;
            }

            public BoundsData(Bounds bounds)
            {
                Vector3 center = bounds.center;
                Vector3 extents = bounds.extents;
                this.a1 = center + (extents);
                this.a4 = extents;
                this.a4.x = (-this.a4.x);
                this.a4 = center + this.a4;
                this.a3 = extents;
                this.a3.x = -this.a3.x;
                this.a3.z = -this.a3.z;
                this.a3 = center + this.a3;
                this.a2 = extents;
                this.a2.z = -this.a2.z;
                this.a2 = center + this.a2;
                this.b1 = center + (-extents);
                this.b4 = -extents;
                this.b4.x = (-this.b4.x);
                this.b4 = center + this.b4;
                this.b3 = -extents;
                this.b3.x = -this.b3.x;
                this.b3.z = -this.b3.z;
                this.b3 = center + this.b3;
                this.b2 = -extents;
                this.b2.z = -this.b2.z;
                this.b2 = center + this.b2;
            }

            public Bounds GetBounds() { return GetBounds(Matrix4x4.identity); }

            public Bounds GetBounds(Matrix4x4 transform)
            {
                var points = this.ToArray();

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

            public Bounds Encapsulated(Bounds bounds)
            {
               return Encapsulated(new BoundsData(bounds).ToArray());
            }

            public Bounds Encapsulated(Vector3[] points)
            {
                var minX = Mathf.Infinity;
                var maxX = -Mathf.Infinity;
                var minY = Mathf.Infinity;
                var maxY = -Mathf.Infinity;
                var minZ = Mathf.Infinity;
                var maxZ = -Mathf.Infinity;

                if (HasValues())
                {
                    var positons = this.ToArray();
                    for (int i = 0; i < positons.Length; i++)
                    {
                        var pos = positons[i];
                        if (pos.x < minX) minX = pos.x;
                        if (pos.x > maxX) maxX = pos.x;
                        if (pos.y < minY) minY = pos.y;
                        if (pos.y > maxY) maxY = pos.y;
                        if (pos.z < minZ) minZ = pos.z;
                        if (pos.z > maxZ) maxZ = pos.z;
                    }
                }

                for (int i = 0; i < points.Length; i++)
                {
                    var pos = points[i];
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


            public List<Vector3> ToList()
            {
                return new List<Vector3>(8) { a1, a2, a3, a4, b1, b2, b3, b4 };
            }

            public Vector3[] ToArray()
            {
                return new Vector3[8] { a1, a2, a3, a4, b1, b2, b3, b4 };
            }

            public static implicit operator Bounds(BoundsData value) { return value.GetBounds(); }

            public static implicit operator BoundsData(Bounds value){return new BoundsData(value); }
        }

    }
}