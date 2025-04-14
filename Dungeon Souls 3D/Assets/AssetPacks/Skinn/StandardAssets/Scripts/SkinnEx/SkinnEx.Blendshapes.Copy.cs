using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        
        public static bool CopyBlendshapes(this Mesh targetMesh, Mesh sourceMesh,
            int[] mapping = null,
            Blendshape.CopySettings copySetting = null,
            ShapeFilter  shapeFilter = null
            )
        {
            if(mapping == null && targetMesh.vertexCount != sourceMesh.vertexCount) { return false ; }
            var blendShapeCount = sourceMesh.blendShapeCount;
            if (blendShapeCount < 1) return false;
            var existingBlendshapes = targetMesh.GetBlendshapeNames();
            var vertexCount = targetMesh.vertexCount;
            var bscs = copySetting == null ? Blendshape.CopySettings.DefaultValue : copySetting;
            var shapes = new List<Blendshape>(blendShapeCount);
            var newShapeNames = new List<int>();

            for (int i = 0; i < blendShapeCount; i++)
            {
                var shapeName = sourceMesh.GetBlendShapeName(i);
                if (existingBlendshapes.GetIndex(shapeName) > -1) continue;
                if (bscs.IsExcluded(shapeName)) continue;


                if(shapeFilter != null)
                {
                    string newName;
                    if (shapeFilter.Evaluate(shapeName, out newName))
                    {
                        if (existingBlendshapes.GetIndex(newName) > -1) continue;
                        shapeName = newName;
                    }
                    else continue;
                }

                var shapeHash = Animator.StringToHash(shapeName);
                if (newShapeNames.Contains(shapeHash)) continue;
                else newShapeNames.Add(shapeHash);

                var frameCount = sourceMesh.GetBlendShapeFrameCount(i);
                var blendshape = new Blendshape() { name = shapeName };

                for (int ii = 0; ii < frameCount; ii++)
                {
                    var frameWeight = sourceMesh.GetBlendShapeFrameWeight(i, ii);

                    var sourceDeltas = new Vector3[sourceMesh.vertexCount];
                    var sourceNormals = new Vector3[sourceMesh.vertexCount];
                    var sourceTangents = new Vector3[sourceMesh.vertexCount];

                    sourceMesh.GetBlendShapeFrameVertices(i, ii, sourceDeltas, sourceNormals, sourceTangents);

                    var newDeltas = GetFrameDeltas(mapping, sourceDeltas);
                    var newNormals = GetFrameDeltas(mapping, sourceNormals);
                    var newTangents = GetFrameDeltas(mapping, sourceTangents);
                    
                    blendshape.AddFrame(frameWeight, newDeltas , bscs.normals ?  newNormals : null, bscs.normals ? newTangents : null );
                }
                 
                if (blendshape.frames.Count > 0) shapes.Add(blendshape);
            }

            if (bscs.alphabitize) shapes.Sort();

            for (int i = 0; i < shapes.Count; i++)
            {
                var shape = shapes[i];
                for (int ii = 0; ii < shape.frames.Count; ii++)
                {
                    var keyframe = shape.frames[ii];
                    targetMesh.AddBlendShapeFrame(shape.name, keyframe.frameWeight, keyframe.GetDeltas(vertexCount), keyframe.GetNormals(vertexCount), keyframe.GetTangents(vertexCount));
                }
            }
            return true;
        }

        internal static void CopyBlendshapes(this Mesh mesh, Dictionary<Renderer, Vector2Int[]> mapping)
        {
            if (!mesh) return;
            int vertexCount = mesh.vertexCount;
            var blendshapes = new List<Blendshape>();
            foreach (var item in mapping)
            {
                var sourceMesh = item.Key.GetSharedMesh();
                var sourceVertexCount = sourceMesh.vertexCount;
                var sourceShapes = sourceMesh.GetBlendshapes();

                for (int i = 0; i < sourceShapes.Count; i++)
                {
                    var sourceShape = sourceShapes[i];
                    var shapeIndex = blendshapes.FindIndex((a) => { return sourceShape.name == a.name; });
                    Blendshape newShape;
                    List<Blendshape.DeltaKeyFrame> newFrames;
                    if (shapeIndex < 0)
                    {
                        newFrames = new List<Blendshape.DeltaKeyFrame>(1) { new Blendshape.DeltaKeyFrame(vertexCount) { frameWeight = 0 } };
                        newShape = new Blendshape() { name = sourceShape.name, frames = newFrames };
                        newShape.frames = newFrames;
                    }
                    else
                    {
                        newShape = blendshapes[shapeIndex];
                        newFrames = blendshapes[shapeIndex].frames;
                    }
                    var frames = sourceShapes[i].frames;
                    for (int ii = 0; ii < frames.Count; ii++)
                    {
                        var sourceFrame = frames[ii];
                        int frameIndex = newFrames.FindIndex((a) => { return sourceFrame.frameWeight == a.frameWeight; });
                        Blendshape.DeltaKeyFrame targetFrame;

                        if (frameIndex < 0)
                        {
                            targetFrame = new Blendshape.DeltaKeyFrame(vertexCount) { frameWeight = sourceFrame.frameWeight };
                            newFrames.Add(targetFrame);
                        }
                        else targetFrame = newFrames[frameIndex];

                        var vertexMap = item.Value;

                        if(i == 0)
                        {
                            Debug.LogFormat("Source vertexCount:  {0}", item.Key.GetSharedMesh().vertexCount);
                            Debug.LogFormat("Source  sourceFrame.shapeDeltas.Length:  {0}", sourceFrame.shapeDeltas.Length);


                            Debug.LogFormat("vertexMap L:  {0}", vertexMap.Length);
                            Debug.LogFormat("vertexCount  {0}", vertexCount);
                        }

                        for (int iii = 0; iii < sourceVertexCount; iii++)
                        {
                            if (!IsNullOrEmpty(sourceFrame.shapeDeltas))
                            {
                                newShape.DeltaVerticesSet = true;
                                targetFrame.shapeDeltas[vertexMap[iii].x] = sourceFrame.shapeDeltas[vertexMap[iii].y];
                            }

                            if (!IsNullOrEmpty(sourceFrame.shapeNormals))
                            {
                                newShape.DeltaNormalSet = true;
                                targetFrame.shapeNormals[vertexMap[iii].x] = targetFrame.shapeNormals[vertexMap[iii].y];
                            }
                            if (!IsNullOrEmpty(sourceFrame.shapeTangents))
                            {
                                newShape.DeltaTangentSet = true;
                                targetFrame.shapeTangents[vertexMap[iii].x] = targetFrame.shapeTangents[vertexMap[iii].y];
                            }
                        }
                    }
                    if (shapeIndex < 0) blendshapes.Add(newShape);
                    else blendshapes[shapeIndex] = newShape;
                }
            }

            for (int i = 0; i < blendshapes.Count; i++)
            {
                var shape = blendshapes[i];
                shape.SortFrames();
                var frameCount = blendshapes[i].frames.Count;
                for (int ii = 0; ii < frameCount; ii++)
                {
                    var frame = blendshapes[i].frames[ii];
                    mesh.AddBlendShapeFrame(
                        shape.name,
                        frame.frameWeight,
                        shape.DeltaVerticesSet ? frame.shapeDeltas : null,
                        shape.DeltaNormalSet ? frame.shapeNormals : null,
                        shape.DeltaTangentSet ? frame.shapeTangents : null);

                    Debug.Log(frame.shapeDeltas);
                }
            }
        }

        private static Vector3[] GetFrameDeltas(int[] mapping, Vector3[] deltas)
        {
            if (mapping == null) return deltas;
            if (IsNullOrEmpty(deltas)) return null;
            var newDeltas = new Vector3[mapping.Length];
            for (int i = 0; i < mapping.Length; i++) newDeltas[i] = deltas[mapping[i]];
            return newDeltas;
        }
        
        public static bool BakeBlendshapes(this SkinnedMeshRenderer source, Mesh target, bool clearBlendshapes = true)
        {
            if (!SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders()) return false;
            if (!source.HasSharedMesh() || !target) return false;
            if (source.sharedMesh.vertexCount != target.vertexCount) return false;

            int blendShapeCount = target.blendShapeCount;
            if (blendShapeCount < 1) return false;
            int vertexCount = target.vertexCount;

            var vertices = target.vertices;
            var normals = target.GetNormalsRecalculateIfNeeded();
            var tangents = target.GetTangentsRecalculateIfNeeded();

            var tempDeltas = new Vector3[vertexCount];
            var tempNormals = new Vector3[vertexCount];
            var tempTangents = new Vector3[vertexCount];

            var computeShader = SkinnInternalAsset.Asset.combineDeltas;

            ComputeBuffer Vertices, Normals, Tangents, OutVertices, OutNormals, OutTangents;

            Vertices = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            Vertices.SetData(tempDeltas);
            OutVertices = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            OutVertices.SetData(vertices);
            Normals = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            Normals.SetData(tempNormals);
            OutNormals = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            OutNormals.SetData(normals);
            Tangents = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            Tangents.SetData(tempTangents);
            OutTangents = new ComputeBuffer(vertexCount, 4 * sizeof(float));
            OutTangents.SetData(tangents);

            computeShader.SetInt("VertexCount", vertexCount);

            var kernel = computeShader.FindKernel("BakeDeltas");

            computeShader.SetBuffer(kernel, "Vertices", Vertices);
            computeShader.SetBuffer(kernel, "OutVertices", OutVertices);

            computeShader.SetBuffer(kernel, "Normals", Normals);
            computeShader.SetBuffer(kernel, "OutNormals", OutNormals);

            computeShader.SetBuffer(kernel, "Tangents", Tangents);
            computeShader.SetBuffer(kernel, "OutTangents", OutTangents);

            uint threadGroupSizeX, threadGroupSizeY,threadGroupSizeZ;

            computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
            var sourceMesh = source.sharedMesh;

            for (int i = 0; i < blendShapeCount; i++)
            {
                var frameCount = sourceMesh.GetBlendShapeFrameCount(i);
                var weight = source.GetBlendShapeWeight(i);
                if (frameCount < 1 || Mathf.Approximately(0.0f, weight)) continue;

                for (int ii = 0; ii < frameCount; ii++)
                {
                    var frameWeight = sourceMesh.GetBlendShapeFrameWeight(i, ii);
                    if (frameWeight * weight <= Mathf.Epsilon) continue;
                    var sourceDeltas = new Vector3[sourceMesh.vertexCount];
                    var sourceNormals = new Vector3[sourceMesh.vertexCount];
                    var sourceTangents= new Vector3[sourceMesh.vertexCount];

                    sourceMesh.GetBlendShapeFrameVertices(i, ii, sourceDeltas, sourceNormals, sourceTangents);

                    if (!IsNullOrEmpty(sourceDeltas)) Vertices.SetData(sourceDeltas);
                    else Vertices.SetData(tempDeltas);

                    if (!IsNullOrEmpty(sourceNormals)) Normals.SetData(sourceNormals);
                    else Normals.SetData(tempNormals);

                    if (!IsNullOrEmpty(sourceTangents)) Normals.SetData(sourceTangents);
                    else Normals.SetData(tempTangents);

                    computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
                }
            }

            OutVertices.GetData(vertices);
            OutNormals.GetData(normals);
            OutTangents.GetData(tangents);

            Release(Vertices); Release(OutVertices); Release(Normals);
            Release(OutNormals); Release(Tangents); Release(OutTangents);

            target.vertices = vertices;
            if (!IsNullOrEmpty(target.normals, vertexCount)) target.normals = normals;
            if (!IsNullOrEmpty(target.tangents, vertexCount)) target.tangents = tangents;

            if (clearBlendshapes) target.ClearBlendShapes();
            target.RecalculateBounds();

            return true;
        }

    }
}