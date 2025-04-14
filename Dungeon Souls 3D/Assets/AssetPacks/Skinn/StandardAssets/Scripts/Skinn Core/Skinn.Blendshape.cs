using System;
using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public class Blendshape : IComparable<Blendshape>
    {
        public string name;
        public List<DeltaKeyFrame> frames;

        public Blendshape() { name = "Null"; frames = new List<DeltaKeyFrame>(); }

        public bool DeltaVerticesSet { get; set; }
        public bool DeltaNormalSet { get; set; }
        public bool DeltaTangentSet { get; set; }

        public void SortFrames()
        {
            if (SkinnEx.IsNullOrEmpty(frames)) return;
            frames.Sort((x, y) => x.frameWeight.CompareTo(y.frameWeight));
        }

        public class DeltaKeyFrame
        {
            public float frameWeight;

            public Vector3[] shapeDeltas;
            public Vector3[] shapeNormals;
            public Vector3[] shapeTangents;

            public DeltaKeyFrame()
            {
                shapeDeltas = null;
                shapeNormals = null;
                shapeTangents = null;
            }

            public DeltaKeyFrame(int vertexCount)
            {
                frameWeight = 0;
                shapeDeltas = new Vector3[vertexCount];
                shapeNormals = new Vector3[vertexCount];
                shapeTangents = new Vector3[vertexCount];
            }


            public Vector3[] GetDeltas(int vertexCount, bool allowNull = true)
            {
                if (SkinnEx.IsNullOrEmpty(shapeDeltas, vertexCount))
                {
                    if (allowNull) return null;
                    else return new Vector3[vertexCount];
                }
                return shapeDeltas;
            }

            public Vector3[] GetNormals(int vertexCount, bool allowNull = true)
            {
                if (SkinnEx.IsNullOrEmpty(shapeNormals, vertexCount))
                {
                    if (allowNull) return null;
                    else return new Vector3[vertexCount];
                }
                return shapeDeltas;
            }

            public Vector3[] GetTangents(int vertexCount, bool allowNull = true)
            {
                if (SkinnEx.IsNullOrEmpty(shapeTangents, vertexCount))
                {
                    if (allowNull) return null;
                    else return new Vector3[vertexCount];
                }
                return shapeDeltas;
            }
        }

        public void AddFrame(float frameWeight, Vector3[] vertices, Vector3[] normals, Vector3[] tangents)
        {
            if (frames == null) frames = new List<DeltaKeyFrame>();
            frames.Add(new DeltaKeyFrame()
            {
                frameWeight = frameWeight,
                shapeDeltas = vertices,
                shapeNormals = normals,
                shapeTangents = tangents
            });
        }

        public int CompareTo(Blendshape obj)
        {
            return string.Compare(this.name, obj.name);
        }

        public class CopySettings
        {
            public bool alphabitize;

            public bool vertices;
            public bool normals;
            public bool tangents;

            public List<string> exculded;
            public bool flipExculded;

            public CopySettings()
            {
                alphabitize = false;
                vertices = true;
                normals = true;
                tangents = false;
                exculded = null;
                flipExculded = false;
            }

            public bool IsExcluded(string blendshapeName)
            {
                if (SkinnEx.IsNullOrEmpty(exculded)) return false;

                if (flipExculded)
                {
                    if (exculded.Contains(blendshapeName)) return false;
                    else return true;
                }
                else
                {
                    if (!exculded.Contains(blendshapeName)) return false;
                    else return true;
                }
            }

            public static readonly CopySettings DefaultValue = new CopySettings();
        }
    }
}