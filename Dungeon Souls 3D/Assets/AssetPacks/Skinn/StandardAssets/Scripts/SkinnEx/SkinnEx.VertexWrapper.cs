using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        internal static class VertexWrapper
        {
            private static ComputeShader shader;
            public static ComputeShader GetShader
            {
                get
                {
                    if (!shader) shader = SkinnInternalAsset.Asset.weightMapping;
                    return shader;
                }
            }

            private static int targetVertexCount, sourceVertexCount;

            public static bool CompareSubMesh { set { if (GetShader) GetShader.SetBool("CompareSubMesh", value); } }
            public static bool CompareUV { set { if (GetShader) GetShader.SetBool("CompareUV", value); } }
            public static bool CompareNormals { set { if (GetShader) GetShader.SetBool("CompareNormals", value); } }
            public static bool CompareWeights { set { if (GetShader) GetShader.SetBool("CompareWeights", value); } }
            public static bool CompareTangents { set { if (GetShader) GetShader.SetBool("CompareTangents", value); } }

            public static float MaxDistance { set { if (GetShader) GetShader.SetFloat("MaxDistance", value); } }
            private static bool UseMaxDistance { set { if (GetShader) GetShader.SetBool("UseMaxDistance", value); } }

            private static bool UseSourceBoneFilter { set { if (GetShader) GetShader.SetBool("UseSourceBoneFilter", value); } }

            private static int SourceVertexCount { set { if (GetShader) GetShader.SetInt("SourceVertexCount", value); } }
            private static int SourceBoneCount { set { if (GetShader) GetShader.SetInt("SourceBoneCount", value); } }

            private static bool UseTargetBoneFilter { set { if (GetShader) GetShader.SetBool("UseTargetBoneFilter", value); } }

            private static int TargetVertexCount { set { if (GetShader) GetShader.SetInt("TargetVertexCount", value); } }
            private static int TargetBoneCount { set { if (GetShader) GetShader.SetInt("TargetBoneCount", value); } }

            public static int WeightMatrixCount { set { if (GetShader) GetShader.SetInt("WeightMatrixCount", value); } }

            private static int threadCount = 1;
            private static int SubThreadCount
            {
                set
                {
                    if (GetShader) GetShader.SetInt("SubThreadCount", value);
                    threadCount = value;
                }
                get
                {
                    return threadCount;
                }
            }

            private static int TargetAdjacentCount { set { if (GetShader) GetShader.SetInt("TargetAdjacentCount", value); } }

            private static int TargetRadialAdjCount { set { if (GetShader) GetShader.SetInt("TargetRadialAdjCount", value); } }

            public static int TargetRadialMaxDistance{ set { if (GetShader) GetShader.SetFloat("TargetRadialMaxDistance", value); } }

            private static float Inflation { set { if (GetShader) GetShader.SetFloat("Inflation", value); } }

            public static float PassWeight { set { if (GetShader) GetShader.SetFloat("PassWeight", value); } }

            public static float SurfaceWeight { set { if (GetShader) GetShader.SetFloat("SurfaceWeight", value); } }

            public static int MaxBones { set { if (GetShader) GetShader.SetInt("MaxBones", Mathf.Clamp(value, 1, 4)); } }

            public static BasicCurve2D SurfaceCurve
            {
                set
                {
                    if (!GetShader || !value) return;
                    GetShader.SetVector("SurfaceCurve", new Vector2(value.start, value.end));
                    GetShader.SetVector("SurfaceCurveTangents", value.tangents);
                }
            }


            internal static ComputeBuffer TargetVertices, TargetNormals, TargetTangents, TargetInvalidBones,
                TargetSubMesh, TargetUV, TargetDuplicates, TargetAdjacency, TargetRadialAdj, SourceVertices, SourceNormals, SourceTangents, SourceInvalidBones, SourceWeights, SourceSubMesh, SourceUV, MappingData, MappingVertices, MappingDeltas, WeightMatrix;

            public static ComputeBuffer IndexData, MappingIndices, MappingDistances, TargetWeights;

            public static float[] indexData = new float[4];
            public static int[] mapping = new int[0];
            public static float[] mappingDistance = new float[0];

            public static void ReleaseBuffers()
            {
                indexData = new float[4]; mapping = new int[0]; mappingDistance = new float[0];

                Release(TargetVertices); Release(TargetNormals); Release(TargetTangents); Release(TargetInvalidBones);
                Release(TargetWeights); Release(TargetSubMesh); Release(TargetUV); Release(TargetDuplicates);
                Release(TargetAdjacency); Release(TargetRadialAdj);
                Release(SourceVertices); Release(SourceNormals); Release(SourceTangents); Release(SourceInvalidBones);
                Release(SourceWeights); Release(SourceSubMesh); Release(SourceUV);
                Release(IndexData); Release(MappingIndices); Release(MappingData); Release(MappingDistances);
                Release(MappingVertices); Release(MappingDeltas); Release(WeightMatrix); //Release(SubThreads);
            }

            private static void ResetValues()
            {
                CompareSubMesh = false;
                CompareUV = false;
                CompareNormals = false;
                CompareTangents = false;
                CompareWeights = false;
                MaxDistance = float.MaxValue;
                UseMaxDistance = true;
                MaxBones = 4;
                WeightMatrixCount = 4;
                TargetRadialMaxDistance = 0;
                PassWeight = 1;
            }

            public static void SetData
                (
                Vector3[] targetVertices,
                Vector3[] sourceVertices,

                int[] targetTriangles,
                int[] sourceTriangles,

                int targetBindPoseCount,
                int sourceBindPoseCount,

                Vector3[] targetNormals = null,
                Vector3[] sourceNormals = null,

                Vector4[] targetTangents = null,
                Vector4[] sourceTangents = null,

                int[] targetSubmeshIndex = null,
                int[] sourceSubmeshIndex = null,

                Vector2[] targetUVs = null,
                Vector2[] sourceUVs = null,

                BoneWeight[] targetBoneWeights = null,
                BoneWeight[] sourceBoneWeights = null,

                SearchOptions search = null,
                WeightProjection weightProjection = null,
                ShapeProjection shapeProjection = null
                )
            {
                ResetValues();

                CompareSubMesh = SearchOptions.FallbackToDefualtValue(search).compareSubmesh;
                CompareUV = SearchOptions.FallbackToDefualtValue(search).compareUV;
                CompareNormals = SearchOptions.FallbackToDefualtValue(search).compareNormals;
                CompareWeights = SearchOptions.FallbackToDefualtValue(search).compareWeights;
                CompareTangents = SearchOptions.FallbackToDefualtValue(search).compareUV;

                int[] targetInvalidBones = SearchOptions.FallbackToDefualtValue(search).targetBoneMask;
                int[] sourceInvalidBones = SearchOptions.FallbackToDefualtValue(search).sourceBoneMask;
                int maxBlendChannels = WeightProjection.FallbackToDefualtValue(weightProjection).blendQuality;
                int targetAdjacentCount = WeightProjection.FallbackToDefualtValue(weightProjection).adjacentCount;
                int targetRadialCount = WeightProjection.FallbackToDefualtValue(weightProjection).radialCount;


                targetVertexCount = targetVertices.Length;
                sourceVertexCount = sourceVertices.Length;

                UseTargetBoneFilter = !IsNullOrEmpty(sourceInvalidBones, targetBindPoseCount);
                TargetVertexCount = targetVertexCount;
                TargetBoneCount = targetBindPoseCount;

                UseSourceBoneFilter = !IsNullOrEmpty(sourceInvalidBones, sourceBindPoseCount);
                SourceVertexCount = sourceVertexCount;
                SourceBoneCount = sourceBindPoseCount;

                TargetVertices = new ComputeBuffer(targetVertexCount, sizeof(float) * 3);
                TargetVertices.SetData(targetVertices);
                TargetNormals = new ComputeBuffer(targetVertexCount, sizeof(float) * 3);
                TargetNormals.SetData(IsNullOrEmpty(targetNormals, targetVertexCount) ? new Vector3[targetVertexCount] : targetNormals);
                TargetTangents = new ComputeBuffer(targetVertexCount, sizeof(float) * 4);
                TargetTangents.SetData(IsNullOrEmpty(targetTangents, targetVertexCount) ? new Vector4[targetVertexCount] : targetTangents);

                var tbones = IsNullOrEmpty(targetInvalidBones) ? new int[1] : targetInvalidBones;
                TargetInvalidBones = new ComputeBuffer(tbones.Length, sizeof(int));
                TargetInvalidBones.SetData(tbones);

                TargetWeights = new ComputeBuffer(targetVertexCount, 4 * sizeof(float) + 4 * sizeof(int));
                TargetWeights.SetData(IsNullOrEmpty(targetBoneWeights, targetVertexCount) ? Weights(targetVertexCount, 0) : targetBoneWeights);
                TargetSubMesh = new ComputeBuffer(targetVertexCount, sizeof(int));
                var tSubMeshIndex = IsNullOrEmpty(targetSubmeshIndex) ? new int[targetVertexCount] : targetSubmeshIndex;
                TargetSubMesh.SetData(tSubMeshIndex);
                TargetUV = new ComputeBuffer(targetVertexCount, 2 * sizeof(float));
                TargetUV.SetData(IsNullOrEmpty(targetUVs, targetVertexCount) ? new Vector2[targetVertexCount] : targetUVs);

                TargetDuplicates = new ComputeBuffer(targetVertexCount, sizeof(int));
                TargetDuplicates.SetData(targetVertices.GetUniqueIndices());
               
                targetAdjacentCount = Mathf.Clamp(targetAdjacentCount, 1, 1024);
                TargetAdjacentCount = targetAdjacentCount;
                int[] targetAdjacentIndices;
                if (!GetAdjacentIndices(targetTriangles, targetVertices, out targetAdjacentIndices, targetAdjacentCount)) { }
                TargetAdjacency = new ComputeBuffer(targetAdjacentIndices.Length, sizeof(int));
                TargetAdjacency.SetData(targetAdjacentIndices);

                var targetRadialAdjCount = Mathf.Clamp(targetRadialCount, 1, 1024);
                Vector2[] targetRadialAdj = new Vector2[targetVertexCount * targetRadialAdjCount];
                TargetRadialAdjCount = targetRadialAdjCount;
                TargetRadialAdj = new ComputeBuffer(targetRadialAdj.Length, sizeof(float) * 2);
                TargetRadialAdj.SetData(targetRadialAdj);

                {
                    ComputeShader computeShader = SkinnInternalAsset.Asset.radialIndices;
                    uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                    int threadGroupsX;
                    computeShader.SetInt("VertexCount", targetVertexCount);
                    computeShader.SetInt("RadialCount", targetRadialAdjCount);

                    {
                        int kernal = computeShader.FindKernel("RadialPass");

                        computeShader.SetBuffer(kernal, "Vertices", TargetVertices);
                        computeShader.SetBuffer(kernal, "Duplicates", TargetDuplicates);
                        computeShader.SetBuffer(kernal, "RadialIndices", TargetRadialAdj);

                        computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                        threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                        computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
                    }

                    {

                        int kernal = computeShader.FindKernel("RadialWeightPass");

                        computeShader.SetBuffer(kernal, "Vertices", TargetVertices);
                        computeShader.SetBuffer(kernal, "RadialIndices", TargetRadialAdj);

                        computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                        threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                        computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
                    }

                    //TargetRadialAdj.GetData(targetRadialAdj);
                    //for (int i = 0; i < targetRadialAdjCount; i++) Debug.LogFormat("'TargetRadialAdj' index: {0}, weight: {1}", i, targetRadialAdj[i].y.ToString("0.00"));
                }

                SourceVertices = new ComputeBuffer(sourceVertexCount, 3 * sizeof(float));
                SourceVertices.SetData(sourceVertices);
                SourceNormals = new ComputeBuffer(sourceVertexCount, 3 * sizeof(float));
                SourceNormals.SetData(IsNullOrEmpty(sourceNormals, sourceVertexCount) ? new Vector3[sourceVertexCount] : sourceNormals);
                SourceTangents = new ComputeBuffer(sourceVertexCount, sizeof(float) * 4);
                SourceTangents.SetData(IsNullOrEmpty(sourceTangents, sourceVertexCount) ? new Vector4[sourceVertexCount] : sourceTangents);

                var sbones = IsNullOrEmpty(sourceInvalidBones) ? new int[1] : sourceInvalidBones;
                SourceInvalidBones = new ComputeBuffer(sbones.Length, sizeof(int));
                SourceInvalidBones.SetData(sbones);

                SourceWeights = new ComputeBuffer(sourceVertexCount, 4 * sizeof(float) + 4 * sizeof(int));
                SourceWeights.SetData(IsNullOrEmpty(sourceBoneWeights, sourceVertexCount) ? Weights(sourceVertexCount, 0) : sourceBoneWeights);
                SourceSubMesh = new ComputeBuffer(sourceVertexCount, sizeof(int));
                var sSubMeshIndex = IsNullOrEmpty(sourceSubmeshIndex) ? new int[sourceVertexCount] : sourceSubmeshIndex;
                SourceSubMesh.SetData(sSubMeshIndex);
                SourceUV = new ComputeBuffer(sourceVertexCount, 2 * sizeof(float));
                SourceUV.SetData(IsNullOrEmpty(sourceUVs, sourceVertexCount) ? new Vector2[sourceVertexCount] : sourceUVs);

                IndexData = new ComputeBuffer(4, sizeof(float));
                indexData = new float[4];
                IndexData.SetData(indexData);

                MappingIndices = new ComputeBuffer(targetVertexCount, sizeof(int));
                mapping = new int[targetVertexCount];
                MappingIndices.SetData(mapping);


                var subThreadCount = 32;
                SubThreadCount = subThreadCount;
                MappingData = new ComputeBuffer(targetVertexCount * subThreadCount, sizeof(int) * 6);
                MappingData.SetData(new int[targetVertexCount * subThreadCount * 6]);

                MappingDistances = new ComputeBuffer(targetVertexCount, sizeof(float));
                mappingDistance = new float[targetVertexCount];
                MappingDistances.SetData(mappingDistance);

                MappingVertices = new ComputeBuffer(targetVertexCount, sizeof(float) * 3);
                MappingVertices.SetData(targetVertices);

                MappingDeltas = new ComputeBuffer(targetVertexCount, sizeof(float) * 3);
                MappingDeltas.SetData(targetVertices);

                WeightMatrixCount = maxBlendChannels;
                WeightMatrix = new ComputeBuffer(targetVertexCount * maxBlendChannels, sizeof(float) * 2);
                WeightMatrix.SetData(new Vector2[targetVertexCount * maxBlendChannels]);
 

                PrePass();
            }

            public static void PrePass()
            {
                var kernel = GetShader.FindKernel("PrePass");

                GetShader.SetBuffer(kernel, "MappingData", MappingData);
                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            public static void SearchSinglePass()
            {
                var kernel = GetShader.FindKernel("SearchSinglePass");

                GetShader.SetBuffer(kernel, "MappingVertices", MappingVertices);
                GetShader.SetBuffer(kernel, "TargetNormals", TargetNormals);
                GetShader.SetBuffer(kernel, "TargetTangents", TargetTangents);
                GetShader.SetBuffer(kernel, "TargetInvalidBones", TargetInvalidBones);
                GetShader.SetBuffer(kernel, "TargetWeights", TargetWeights);
                GetShader.SetBuffer(kernel, "TargetSubMesh", TargetSubMesh);
                GetShader.SetBuffer(kernel, "TargetUV", TargetUV);

                GetShader.SetBuffer(kernel, "SourceVertices", SourceVertices);
                GetShader.SetBuffer(kernel, "SourceNormals", SourceNormals);
                GetShader.SetBuffer(kernel, "SourceTangents", SourceTangents);
                GetShader.SetBuffer(kernel, "SourceInvalidBones", SourceInvalidBones);
                GetShader.SetBuffer(kernel, "SourceWeights", SourceWeights);
                GetShader.SetBuffer(kernel, "SourceSubMesh", SourceSubMesh);
                GetShader.SetBuffer(kernel, "SourceUV", SourceUV);


                GetShader.SetBuffer(kernel, "MappingData", MappingData);

                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            public static void SearchMultiPass()
            {
                {
                    var kernel = GetShader.FindKernel("SearchMultiPass");

                    GetShader.SetBuffer(kernel, "MappingVertices", MappingVertices);
                    GetShader.SetBuffer(kernel, "TargetNormals", TargetNormals);
                    GetShader.SetBuffer(kernel, "TargetTangents", TargetTangents);
                    GetShader.SetBuffer(kernel, "TargetInvalidBones", TargetInvalidBones);
                    GetShader.SetBuffer(kernel, "TargetWeights", TargetWeights);
                    GetShader.SetBuffer(kernel, "TargetSubMesh", TargetSubMesh);
                    GetShader.SetBuffer(kernel, "TargetUV", TargetUV);

                    GetShader.SetBuffer(kernel, "SourceVertices", SourceVertices);
                    GetShader.SetBuffer(kernel, "SourceNormals", SourceNormals);
                    GetShader.SetBuffer(kernel, "SourceTangents", SourceTangents);
                    GetShader.SetBuffer(kernel, "SourceInvalidBones", SourceInvalidBones);
                    GetShader.SetBuffer(kernel, "SourceWeights", SourceWeights);
                    GetShader.SetBuffer(kernel, "SourceSubMesh", SourceSubMesh);
                    GetShader.SetBuffer(kernel, "SourceUV", SourceUV);

                    GetShader.SetBuffer(kernel, "MappingData", MappingData);

                    uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                    GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                    var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                    for (int i = 0; i < SubThreadCount; i++)
                    {
                        GetShader.SetInt("CurrentSubThread", i);
                        GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
                    }
                }

                {
                    var kernel = GetShader.FindKernel("SearchMultiPostPass");

                    GetShader.SetBuffer(kernel, "MappingVertices", MappingVertices);
                    GetShader.SetBuffer(kernel, "SourceVertices", SourceVertices);
                    GetShader.SetBuffer(kernel, "MappingData", MappingData);

                    uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                    GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                    var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                    GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
                }
            }

            public static void FallbackPass()
            {
                var kernel = GetShader.FindKernel("FallbackSinglePass");
                GetShader.SetBuffer(kernel, "MappingVertices", MappingVertices);
                GetShader.SetBuffer(kernel, "MappingData", MappingData);

                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            public static void SmoothDeltaPass()
            {
                var kernel = GetShader.FindKernel("SmoothDeltaPass");

                GetShader.SetBuffer(kernel, "MappingDeltas", MappingDeltas);
                GetShader.SetBuffer(kernel, "TargetAdjacency", TargetAdjacency);

                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                for (int i = 0; i < 300; i++)
                    GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            public static void InflationPass(float value)
            {
                var kernel = GetShader.FindKernel("InflationPass");

                GetShader.SetBuffer(kernel, "TargetVertices", TargetVertices);
                GetShader.SetBuffer(kernel, "MappingDeltas", MappingDeltas);
                GetShader.SetBuffer(kernel, "MappingVertices", MappingVertices);
                GetShader.SetBuffer(kernel, "TargetDuplicates", TargetDuplicates);

                Inflation = value;

                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            public static void DistancePass()
            {
                var kernel = GetShader.FindKernel("WeightedDistancePass");
                GetShader.SetBuffer(kernel, "IndexData", IndexData);
                GetShader.SetBuffer(kernel, "MappingData", MappingData);
                GetShader.SetBuffer(kernel, "SourceVertices", SourceVertices);
                GetShader.SetBuffer(kernel, "MappingVertices", MappingVertices);
                GetShader.SetBuffer(kernel, "MappingDistances", MappingDistances);

                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            public static void IndexRetargetingPass()
            {
                var kernel = GetShader.FindKernel("IndexRetargetingPass");
                GetShader.SetBuffer(kernel, "MappingIndices", MappingIndices);
                GetShader.SetBuffer(kernel, "MappingData", MappingData);

                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }


            public static void AddWeightsPass()
            {
                var kernel = GetShader.FindKernel("AddWeightsPass");

                GetShader.SetBuffer(kernel, "MappingDistances", MappingDistances);
                GetShader.SetBuffer(kernel, "WeightMatrix", WeightMatrix);
                GetShader.SetBuffer(kernel, "MappingData", MappingData);
                GetShader.SetBuffer(kernel, "SourceWeights", SourceWeights);
                GetShader.SetBuffer(kernel, "TargetRadialAdj", TargetRadialAdj);
                GetShader.SetBuffer(kernel, "TargetAdjacency", TargetAdjacency);
                GetShader.SetBuffer(kernel, "TargetVertices", TargetVertices);
                GetShader.SetBuffer(kernel, "MappingVertices", MappingVertices);

                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

            public static void WeightIndexRetargetingPass()
            {
                var kernel = GetShader.FindKernel("WeightIndexRetargetingPass");
                GetShader.SetBuffer(kernel, "WeightMatrix", WeightMatrix);
                GetShader.SetBuffer(kernel, "TargetWeights", TargetWeights);
                GetShader.SetBuffer(kernel, "TargetDuplicates", TargetDuplicates);

                uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
                GetShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                var threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;

                GetShader.Dispatch(kernel, threadGroupsX, 1, 1);
            }

        }
    }
}
