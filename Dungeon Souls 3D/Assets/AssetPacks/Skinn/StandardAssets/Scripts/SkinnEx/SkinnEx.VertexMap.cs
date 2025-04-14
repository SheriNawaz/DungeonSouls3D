using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        internal static bool VertexMap(this SkinnedMeshRenderer source, Renderer target,
            out int[] mapping,
            out float[] mappingDistance,
            out Vector3[] targetVertices,
            out Vector3[] targetNormals,
            out Vector4[] targetTangents,
            out BoneWeight[] boneWeights,
            out Blendshape[] blendshapes,
            BakeOptions bakeSource = null,
            BakeOptions bakeTarget = null,
            SearchOptions searchOptions = null,
            WeightProjection weightOptions = null,
            ShapeProjection shapeOptions = null
            )
        {

            mapping = null;
            mappingDistance = null;
            targetVertices = null;
            targetNormals = null;
            targetTangents = null;
            boneWeights = null;
            blendshapes = new Blendshape[0];

            if (!HasMinimumRequirements(source, true) || !HasMinimumRequirements(target) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders())
            {
                { Debug.LogError("requirements! error"); }
                return false;
            }

            var targetMesh = target.GetSharedMesh();
            var sourceMesh = source.sharedMesh;

            var targetSkinnedMesh = target as SkinnedMeshRenderer;
            var sourceSkinnedMesh = source;

            targetVertices = targetMesh.vertices;
            targetNormals = targetMesh.GetNormalsRecalculateIfNeeded();
            targetTangents = targetMesh.GetTangentsRecalculateIfNeeded();

            var targetOptions = BakeOptions.FallbackToDefualtValue(bakeSource);

            if (targetSkinnedMesh && targetSkinnedMesh.HasMinimumRequirements())
            {
                if (!targetSkinnedMesh.GetSkinning(out targetVertices, out targetNormals, out targetTangents, targetOptions))
                { Debug.LogError("computer shader error!"); }
            }
            else
            {
                if (targetOptions.HasOverrides()) targetOptions.GetOverrides(ref targetVertices, ref targetNormals, ref targetTangents);
                if (targetOptions.world)
                {
                    if (targetOptions.vertex)
                        if (!targetVertices.Transformed(out targetVertices, target.transform.localToWorldMatrix, false))
                        { Debug.LogError("computer shader error!"); }

                    if (targetOptions.normal)
                        if (!targetNormals.Transformed(out targetNormals, target.transform.localToWorldMatrix, true))
                        { Debug.LogError("computer shader error!"); }

                    if (targetOptions.normal)
                        if (!targetTangents.Transformed(out targetTangents, target.transform.localToWorldMatrix, true))
                        { Debug.LogError("computer shader error!"); }
                }
            }

            Vector3[] sourceVertices;
            Vector3[] sourceNormals;
            Vector4[] sourceTangents;

            if (!sourceSkinnedMesh.GetSkinning(out sourceVertices, out sourceNormals, out sourceTangents, bakeSource)) { }

            var targetVertexCount = targetMesh.vertices.Length;
            boneWeights = IsNullOrEmpty(targetMesh.boneWeights, targetVertexCount) ? Weights(targetVertexCount, 0) : targetMesh.boneWeights;

            var searchOpt = SearchOptions.FallbackToDefualtValue(searchOptions);
            var weightOpt = WeightProjection.FallbackToDefualtValue(weightOptions);
            var shapeOpt = ShapeProjection.FallbackToDefualtValue(shapeOptions);

            VertexWrapper.SetData(
                targetVertices,
                sourceVertices,
                targetMesh.triangles,
                sourceMesh.triangles,

                IsNullOrEmpty(targetMesh.bindposes) ? 0 : targetMesh.bindposes.Length,
                IsNullOrEmpty(sourceMesh.bindposes) ? 0 : sourceMesh.bindposes.Length,

                targetNormals,
                sourceNormals,

                targetTangents,
                sourceTangents,

                targetMesh.GetSubMeshIndices(),
                sourceMesh.GetSubMeshIndices(),

                targetMesh.uv,
                sourceMesh.uv,

                boneWeights,
                sourceMesh.boneWeights,

                searchOptions,
                weightOpt,
                shapeOpt
                );


            var atMaxDistance = searchOpt.maxDistance <= 0.0f;
            float dis = atMaxDistance ? float.MaxValue : searchOpt.MaxDistance;
            VertexWrapper.MaxDistance = dis;

            VertexWrapper.PrePass();
            VertexWrapper.SearchMultiPass();
            VertexWrapper.FallbackPass();

            VertexWrapper.DistancePass();
            VertexWrapper.MappingDistances.GetData(VertexWrapper.mappingDistance);
            VertexWrapper.IndexRetargetingPass();
            VertexWrapper.MappingIndices.GetData(VertexWrapper.mapping);
            mapping = VertexWrapper.mapping;
            mappingDistance = VertexWrapper.mappingDistance;

            if (weightOptions.enabled)
            {
                var mushPasses = weightOpt.deltaPasses;

                VertexWrapper.SurfaceCurve = weightOpt.surfaceBlending;
                VertexWrapper.SurfaceWeight = weightOpt.surfaceWeight;

                if (weightOpt.startingPose != 0 || mushPasses > 0) { VertexWrapper.SmoothDeltaPass(); }

                if (weightOpt.startingPose != 0)
                {
                    if (!atMaxDistance) dis += Mathf.Abs(weightOpt.startingPose);

                    VertexWrapper.InflationPass(weightOpt.StartingPose);
                    VertexWrapper.PrePass();
                    VertexWrapper.SearchMultiPass();
                    VertexWrapper.FallbackPass();
                }

                if (mushPasses > 0)
                {
                    float inflate = weightOpt.DeltaPose;
                    var step = (inflate / (float)mushPasses);
                    float w = mushPasses;
                    for (int i = 0; i < mushPasses; i++)
                    {
                        var passWeight = weightOpt.deltaBlending.Evaluate((i + 1f) / (float)mushPasses);

                        w -= passWeight;
                        VertexWrapper.PassWeight = Mathf.Clamp(w, 0.0001f, float.MaxValue);
#if CWM_DEV
                        Debug.LogFormat("pass: {0}, weight: {1}, step: {2}", i, w, step);
#endif

                        VertexWrapper.InflationPass(step);
                        VertexWrapper.MaxDistance = atMaxDistance ? dis : dis + Mathf.Abs(step);
                        VertexWrapper.PrePass();

                        VertexWrapper.SearchSinglePass();
                        VertexWrapper.FallbackPass();
                        VertexWrapper.DistancePass();

                        if(i < 1000) VertexWrapper.MaxBones = 1;
                        else VertexWrapper.MaxBones = weightOptions.skinQuality;


                        VertexWrapper.AddWeightsPass();
                    }
                }
                else
                {
                    VertexWrapper.PassWeight = 1f;
                    VertexWrapper.AddWeightsPass();
                }

                VertexWrapper.MaxBones = weightOptions.skinQuality;
                VertexWrapper.WeightIndexRetargetingPass();
                VertexWrapper.TargetWeights.GetData(boneWeights);
            }

            if (mapping[0] < 0)
            {
                VertexWrapper.ReleaseBuffers();
                return mapping[0] > -1;
            }

            if (!shapeOpt.enabled)
            {
                VertexWrapper.ReleaseBuffers();
                return true;
            }

            var blendShapeCount = sourceMesh.blendShapeCount;
            if (blendShapeCount < 1) { VertexWrapper.ReleaseBuffers(); return true; }

            var sourceShapes = source.BindposedBlendShapes(bakeSource);


            var existingBlendshapes = targetMesh.GetBlendshapeNames();
            var targetShapes = new List<Blendshape>(blendShapeCount);
            var newShapeNames = new List<int>(blendShapeCount);

            ComputeShader computeShader = SkinnInternalAsset.Asset.deltaProjector;

            uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;
            int threadGroupsX;

            var TargetVertices = VertexWrapper.TargetVertices;
            var TargetNormals = VertexWrapper.TargetNormals;
            var TargetTangents = VertexWrapper.TargetTangents;
            var TargetDuplicates = VertexWrapper.TargetDuplicates;
            var Mapping = VertexWrapper.MappingIndices;
            Mapping.SetData(mapping);

            Vector3[] extraDeltas;

            if (shapeOpt.Inflate == 0 || !InflateDeltas(targetVertices, targetMesh.triangles, out extraDeltas, shapeOpt.Inflate, true))
            { extraDeltas = new Vector3[targetVertexCount]; }

            var TargetExtraDeltas = new ComputeBuffer(targetVertexCount, sizeof(float) * 3);
            TargetExtraDeltas.SetData(extraDeltas);

            int targetRadialCount = shapeOpt.radialCount;
            Vector2[] targetRadialWeights;
            if (!GetRadialWeights(targetVertices, out targetRadialWeights, targetRadialCount))
            { targetRadialWeights = new Vector2[targetVertices.Length * targetRadialCount]; }

            var TargetRadialAdj = new ComputeBuffer(targetRadialWeights.Length, sizeof(float) * 2);
            TargetRadialAdj.SetData(targetRadialWeights);

            var SourceVertices = VertexWrapper.SourceVertices;
            var SourceDeltas = new ComputeBuffer(sourceMesh.vertexCount, sizeof(float) * 3);
            SourceDeltas.SetData(new Vector3[sourceMesh.vertexCount]);

            var OutVertices = new ComputeBuffer(targetVertexCount, sizeof(float) * 3);
            OutVertices.SetData(new Vector3[targetVertexCount]);
            var OutNormals = new ComputeBuffer(targetVertexCount, sizeof(float) * 3);
            OutNormals.SetData(new Vector3[targetVertexCount]);
            var OutTangents = new ComputeBuffer(targetVertexCount, sizeof(float) * 3);
            OutTangents.SetData(new Vector3[targetVertexCount]);

            var OutMagnitude = new ComputeBuffer(1, sizeof(float) * 4);
            OutMagnitude.SetData(new Vector4[1]);

            computeShader.SetInt("TargetVertexCount", targetVertices.Length);
            computeShader.SetInt("SourceVertexCount", sourceVertices.Length);
            computeShader.SetInt("TargetRadialAdjCount", targetRadialCount);
            computeShader.SetFloat("NormalIntensity", shapeOpt.lighting);

            for (int i = 0; i < sourceShapes.Length; i++)
            {
                var shapeName = sourceShapes[i].name;
                if (existingBlendshapes.GetIndex(shapeName) > -1) continue;

                var shapeFilter = shapeOpt.ShapeMask;
                if (shapeFilter)
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

                var frameCount = sourceShapes[i].frames.Count;
                var blendshape = new Blendshape() { name = shapeName };

                for (int ii = 0; ii < frameCount; ii++)
                {
                    var frameWeight = sourceShapes[i].frames[ii].frameWeight;
                    var sourceDeltas = sourceShapes[i].frames[ii].GetDeltas(sourceMesh.vertexCount);
                    SourceDeltas.SetData(sourceDeltas);
                    OutMagnitude.SetData(new Vector4[1]);

                    {
                        int kernal = computeShader.FindKernel("MagnitudePass");
                        computeShader.SetBuffer(kernal, "SourceDeltas", SourceDeltas);
                        computeShader.SetBuffer(kernal, "OutMagnitude", OutMagnitude);

                        computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                        threadGroupsX = (sourceVertices.Length + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                        computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
                    }

                    {
                        int kernal = computeShader.FindKernel("ProjectDeltas");
                        computeShader.SetBuffer(kernal, "TargetVertices", TargetVertices);
                        computeShader.SetBuffer(kernal, "TargetNormals", TargetNormals);
                        computeShader.SetBuffer(kernal, "TargetTangents", TargetTangents);
                        computeShader.SetBuffer(kernal, "TargetDuplicates", TargetDuplicates);
                        computeShader.SetBuffer(kernal, "Mapping", Mapping);
                        computeShader.SetBuffer(kernal, "TargetExtraDeltas", TargetExtraDeltas);
                        computeShader.SetBuffer(kernal, "TargetRadialAdj", TargetRadialAdj);

                        computeShader.SetBuffer(kernal, "SourceVertices", SourceVertices);
                        computeShader.SetBuffer(kernal, "SourceDeltas", SourceDeltas);

                        computeShader.SetBuffer(kernal, "OutVertices", OutVertices);
                        computeShader.SetBuffer(kernal, "OutNormals", OutNormals);
                        computeShader.SetBuffer(kernal, "OutTangents", OutTangents);

                        computeShader.SetBuffer(kernal, "OutMagnitude", OutMagnitude);

                        computeShader.GetKernelThreadGroupSizes(kernal, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);
                        threadGroupsX = (targetVertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
                        computeShader.Dispatch(kernal, threadGroupsX, 1, 1);
                    }

                    Vector3[] deltaVertices = new Vector3[targetVertexCount];
                    Vector3[] deltaNormals = new Vector3[targetVertexCount];
                    Vector3[] deltaTangets = new Vector3[targetVertexCount];

                    OutVertices.GetData(deltaVertices);
                    OutNormals.GetData(deltaNormals);
                    OutTangents.GetData(deltaTangets);

                    blendshape.AddFrame(frameWeight, deltaVertices, deltaNormals, deltaTangets);
                }


                if (blendshape.frames.Count > 0) targetShapes.Add(blendshape);
            }

            blendshapes = targetShapes.ToArray();

            Release(TargetExtraDeltas); Release(TargetRadialAdj); Release(SourceDeltas);
            Release(OutVertices); Release(OutNormals); Release(OutTangents); Release(OutMagnitude);
            VertexWrapper.ReleaseBuffers();
            return true;
        }


    }
}
