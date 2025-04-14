using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        internal static Blendshape[] BindposedBlendShapes(this SkinnedMeshRenderer source, BakeOptions bakeOptions = null)
        {
            if (IsNullOrNotVailid(source) || !SkinnInternalAsset.CanCompute())
            {
                return new Blendshape[0];
            }

            var sharedMesh = source.sharedMesh;
            var transformCount = sharedMesh.bindposes.Length;
            var sourceMesh = source.GetSharedMesh();
            var blendShapeCount = sourceMesh.blendShapeCount;
            if (blendShapeCount < 1) return new Blendshape[0];
            var vertexCount = sourceMesh.vertexCount;

            var shapes = new Blendshape[blendShapeCount];
            var tempDeltas = new Vector3[sourceMesh.vertexCount];

            ComputeShader computeShader = SkinnInternalAsset.Asset.linearSkinning;
            ComputeBuffer Vertices, Normals, Tangents3, BoneWeights, BoneMatrices, OutVertices, OutNormals, OutTangents3;

            uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
            int kernel, threadGroupsX;
            kernel = computeShader.FindKernel("LinearShapePass");
            computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
            threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
            Vertices = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            Normals = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            Tangents3 = new ComputeBuffer(vertexCount, 3* sizeof(float));
            BoneWeights = new ComputeBuffer(vertexCount, 4 * sizeof(float) + 4 * sizeof(int));
            BoneMatrices = new ComputeBuffer(transformCount, 16 * sizeof(float));
            OutVertices = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            OutNormals = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            OutTangents3 = new ComputeBuffer(vertexCount, 3 * sizeof(float));

            OutVertices.SetData(tempDeltas);
            OutNormals.SetData(tempDeltas);
            OutTangents3.SetData(tempDeltas);

            BoneWeights.SetData(sharedMesh.boneWeights);
            BoneMatrices.SetData(BakeOptions.FallbackToDefualtValue(bakeOptions).GetMatrices(sharedMesh.bindposes, source.bones));

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetFloat("MinDelta", 0.00000001f);

            for (int i = 0; i < blendShapeCount; i++)
            {
                var shapeName = sourceMesh.GetBlendShapeName(i);
                var frameCount = sourceMesh.GetBlendShapeFrameCount(i);
                var blendshape = new Blendshape() { name = shapeName };

                for (int ii = 0; ii < frameCount; ii++)
                {
                    var frameWeight = sourceMesh.GetBlendShapeFrameWeight(i, ii);

                    var deltaVertices = new Vector3[sourceMesh.vertexCount];
                    var deltaNormals = new Vector3[sourceMesh.vertexCount];
                    var deltaTangents = new Vector3[sourceMesh.vertexCount];

                    sourceMesh.GetBlendShapeFrameVertices(i, ii, deltaVertices, deltaNormals, deltaTangents);

                    var hasDeltas = !IsNullOrEmpty(deltaVertices, vertexCount);
                    var hasNormals = !IsNullOrEmpty(deltaNormals, vertexCount);
                    var hasTangents = !IsNullOrEmpty(deltaTangents, vertexCount);

                    if (!hasDeltas) deltaVertices = tempDeltas;
                    if (!hasNormals) deltaNormals = tempDeltas;
                    if (!hasTangents) deltaTangents = tempDeltas;

                    Vertices.SetData(deltaVertices);
                    Normals.SetData(deltaNormals);
                    Tangents3.SetData(deltaTangents);

                    {
                        computeShader.SetBuffer(kernel, "Vertices", Vertices);
                        computeShader.SetBuffer(kernel, "Normals", Normals);
                        computeShader.SetBuffer(kernel, "Tangents3", Tangents3);
                        computeShader.SetBuffer(kernel, "Weights", BoneWeights);
                        computeShader.SetBuffer(kernel, "Bones", BoneMatrices);

                        computeShader.SetBuffer(kernel, "OutVertices", OutVertices);
                        computeShader.SetBuffer(kernel, "OutNormals", OutNormals);
                        computeShader.SetBuffer(kernel, "OutTangents3", OutTangents3);
                        computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
                    }

                    OutVertices.GetData(deltaVertices);
                    OutNormals.GetData(deltaNormals);
                    OutTangents3.GetData(deltaTangents);

                    blendshape.AddFrame(frameWeight, hasDeltas ? deltaVertices : null, hasNormals ? deltaNormals : null, hasTangents ? deltaTangents : null);
                }
                shapes[i] = blendshape;
            }

            Release(Vertices); Release(Normals); Release(Tangents3); Release(BoneWeights);
            Release(BoneMatrices); Release(OutVertices); Release(OutNormals); Release(OutTangents3);

            return shapes;
        }
    }
}