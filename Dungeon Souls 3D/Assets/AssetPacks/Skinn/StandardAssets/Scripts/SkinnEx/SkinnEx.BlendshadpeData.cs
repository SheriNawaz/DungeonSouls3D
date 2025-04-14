using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{

    public static partial class SkinnEx
    {
        public static List<Blendshape> GetBlendshapes(this Mesh sourceMesh)
        {
            var blendShapeCount = sourceMesh.blendShapeCount;
            if (blendShapeCount < 1) new List<Blendshape>();
            var shapes = new List<Blendshape>();
            var vertexCount = sourceMesh.vertexCount;
            for (int i = 0; i < blendShapeCount; i++)
            {
                var shapeName = sourceMesh.GetBlendShapeName(i);
                var frameCount = sourceMesh.GetBlendShapeFrameCount(i);
                var blendshape = new Blendshape() { name = shapeName };

                for (int ii = 0; ii < frameCount; ii++)
                {
                    var frameWeight = sourceMesh.GetBlendShapeFrameWeight(i, ii);

                    var sourceDeltas = new Vector3[sourceMesh.vertexCount];
                    var sourceNormals = new Vector3[sourceMesh.vertexCount];
                    var sourceTangents = new Vector3[sourceMesh.vertexCount];

                    sourceMesh.GetBlendShapeFrameVertices(i, ii, sourceDeltas, sourceNormals, sourceTangents);

                    if (IsNullOrEmpty(sourceDeltas, vertexCount)) sourceDeltas = null;
                    if (IsNullOrEmpty(sourceNormals, vertexCount)) sourceNormals = null;
                    if (IsNullOrEmpty(sourceTangents, vertexCount)) sourceTangents = null;

                    blendshape.AddFrame(frameWeight, sourceDeltas, sourceNormals, sourceTangents);
                }
                if (blendshape.frames.Count > 0) shapes.Add(blendshape);
            }
            return shapes;
        }

    }
}