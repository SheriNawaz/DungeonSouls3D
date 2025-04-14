using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CWM.Skinn
{
    public class SmoothingOption
    {
        public BasicCurve2D mappingCurve = new BasicCurve2D(CurveType.LinearIn);
        public float[] Multiplier { get; set; }

        private float power = 1f;
        public float Power
        {
            get { return Mathf.Clamp01(power); }
            set { power = Mathf.Clamp01(value); }
        }

        private float minWieght = 0f;
        public float MinWieght
        {
            get { return Mathf.Clamp01(minWieght); }
            set { minWieght = Mathf.Clamp01(value); }
        }

        private float maxWieght = 1f;
        public float MaxWieght
        {
            get { return Mathf.Clamp01(maxWieght); }
            set { maxWieght = Mathf.Clamp01(value); }
        }

        public List<int> SmoothableBones { get; set; }

        public static readonly SmoothingOption Defualt = new SmoothingOption();
        public static implicit operator bool(SmoothingOption x) { return x != null; }
    }

    public static partial class SkinnEx
    {
        public static bool SmoothWeights(this SkinnedMeshRenderer source, out BoneWeight[] smoothedWeights,
            SmoothingOption smoothData = null,
            int adjacencyCount = 16)
        {
            smoothedWeights = null;
            if (IsNullOrNotVailid(source)) return false;

            var mesh = source.GetSharedMesh();
            BoneWeight[] weights;

            if (mesh.boneWeights.SmoothWeights(mesh.vertices, mesh.triangles, mesh.bindposes.Length, out weights, smoothData, adjacencyCount))
            {
                smoothedWeights = weights;
                return true;
            }
            return false;
        }

        public static bool SmoothWeights(
            this BoneWeight[] boneWeights,
            Vector3[] vertices,
            int[] triangles,
            int bindPoseCount,
            out BoneWeight[] smoothedWeights,
            SmoothingOption smoothData = null,
            int adjacencyCount = 16
            )
        {
            smoothedWeights = null;
            if (IsNullOrEmpty(boneWeights) || IsNullOrEmpty(boneWeights, vertices.Length)) return false;
            if (!smoothData) smoothData = SmoothingOption.Defualt;
            smoothedWeights = new BoneWeight[vertices.Length];

            var useMultiplier = !IsNullOrEmpty(smoothData.Multiplier, vertices.Length);
            var useCurveMultipler = useMultiplier && smoothData.mappingCurve != null;
            var useBoneMask = !IsNullOrEmpty(smoothData.SmoothableBones);

            adjacencyCount = Mathf.Clamp(adjacencyCount, 2, 32);

            int[,] adjacency;
            if (!GetAdjacentIndices(triangles, vertices, out adjacency, adjacencyCount)) return false;
            var duplicates = vertices.GetUniqueIndices();

            for (int i = 0; i < vertices.Length; i++)
            {
                if (duplicates[i] != i) { smoothedWeights[i] = smoothedWeights[duplicates[i]]; continue; }
                var distances = new List<Vector3>(new Vector3[bindPoseCount]);
                for (int ii = 0; ii < adjacencyCount; ii++)
                {
                    var index = adjacency[i, ii]; if (index < 0) continue;
                    var channelWeights = ((BoneWeightData)boneWeights[index]).weights;
                    if (channelWeights[0].y > smoothData.MaxWieght || channelWeights[0].y < smoothData.MinWieght) continue;
                    for (int iii = 0; iii < channelWeights.Length; iii++)
                    {
                        var weightPower = channelWeights[iii].y * smoothData.Power;

                        if (weightPower <= 0f) continue;
                        var channelIndex = (int)channelWeights[iii].x;

                        if (useBoneMask && !smoothData.SmoothableBones.Contains(channelIndex)) continue;
                        if (useMultiplier) weightPower *= Mathf.Clamp01(smoothData.Multiplier[index]);
                        if (useCurveMultipler) weightPower = smoothData.mappingCurve.Evaluate(weightPower);

                        var w = distances[channelIndex];
                        w.x = channelIndex;
                        w.y += weightPower;
                        w.z++;
                        distances[channelIndex] = w;
                    }
                }

                for (int ii = 0; ii < distances.Count; ii++)
                {
                    if (distances[ii].z < 1) continue;
                    var w = distances[ii];
                    w.y /= w.z;
                    distances[ii] = w;
                }

                {
                    var channelWeights = ((BoneWeightData)boneWeights[i]).weights;
                    for (int iii = 0; iii < channelWeights.Length; iii++)
                    {
                        if (channelWeights[iii].y <= Mathf.Epsilon) continue;
                        int channelIndex = (int)channelWeights[iii].x;
                        var w = distances[channelIndex];
                        w.x = channelIndex;
                        w.y += channelWeights[iii].y;
                        w.z++;
                        distances[channelIndex] = w;
                    }
                }

                distances = distances.OrderBy(x => x.y).ToList();
                distances.Reverse();

                var totalWeight = 0f;
                var weights = new Vector2[4];
                for (int ii = 0; ii < weights.Length; ii++) { weights[ii] = distances[ii]; totalWeight += distances[ii].y; };
                for (int ii = 0; ii < weights.Length; ii++) weights[ii].y /= totalWeight;
                smoothedWeights[i] = new BoneWeightData() { weights = weights };
            }

            return true;
        }
    }
}
