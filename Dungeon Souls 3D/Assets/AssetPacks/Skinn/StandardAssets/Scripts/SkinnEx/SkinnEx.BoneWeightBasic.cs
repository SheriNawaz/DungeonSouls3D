using System;
using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static BoneWeight[] Weights(int length, int bone = 1)
        {
            BoneWeight[] bws = new BoneWeight[length];
            for (int i = 0; i < length; i++) bws[i] =  NewBoneWeight(bone);
            return bws;
        }

        public static BoneWeight NewBoneWeight(int bone)
        {
            BoneWeight bw = new BoneWeight
            {
                boneIndex0 = bone,
                boneIndex1 = 0,
                boneIndex2 = 0,
                boneIndex3 = 0,
                weight0 = 1f,
                weight1 = 0f,
                weight2 = 0f,
                weight3 = 0f
            };
            return bw;
        }

        public static BoneWeight Copy(this BoneWeight boneWeight)
        {
            BoneWeight bw = new BoneWeight
            {
                boneIndex0 = boneWeight.boneIndex0,
                boneIndex1 = boneWeight.boneIndex1,
                boneIndex2 = boneWeight.boneIndex2,
                boneIndex3 = boneWeight.boneIndex3,
                weight0 = boneWeight.weight0,
                weight1 = boneWeight.weight1,
                weight2 = boneWeight.weight2,
                weight3 = boneWeight.weight3
            };
            return bw;
        }

        public static bool HasWeight(this BoneWeight boneWeightA, BoneWeight boneWeightB, int skinQuality = 0)
        {
            if (boneWeightA.boneIndex0 == boneWeightB.boneIndex0) return true;
            if (boneWeightA.boneIndex0 == boneWeightB.boneIndex1) return true;
            if (boneWeightA.boneIndex0 == boneWeightB.boneIndex2) return true;
            if (boneWeightA.boneIndex0 == boneWeightB.boneIndex3) return true;
            if (skinQuality < 1)
                return false;
            if (boneWeightA.boneIndex1 == boneWeightB.boneIndex0) return true;
            if (boneWeightA.boneIndex1 == boneWeightB.boneIndex1) return true;
            if (boneWeightA.boneIndex1 == boneWeightB.boneIndex2) return true;
            if (boneWeightA.boneIndex1 == boneWeightB.boneIndex3) return true;
            if (skinQuality < 2)
                return false;
            if (boneWeightA.boneIndex2 == boneWeightB.boneIndex0) return true;
            if (boneWeightA.boneIndex2 == boneWeightB.boneIndex1) return true;
            if (boneWeightA.boneIndex2 == boneWeightB.boneIndex2) return true;
            if (boneWeightA.boneIndex2 == boneWeightB.boneIndex3) return true;
            if (skinQuality < 3)
                return false;
            if (boneWeightA.boneIndex3 == boneWeightB.boneIndex0) return true;
            if (boneWeightA.boneIndex3 == boneWeightB.boneIndex1) return true;
            if (boneWeightA.boneIndex3 == boneWeightB.boneIndex2) return true;
            if (boneWeightA.boneIndex3 == boneWeightB.boneIndex3) return true;
            return false;
        }

        public static float RelativeBoneWeight(this BoneWeight boneWeightA, BoneWeight boneWeightB, int skinQuality = 0)
        {
            if (boneWeightA.boneIndex0 == boneWeightB.boneIndex0) return boneWeightB.boneIndex0;
            if (boneWeightA.boneIndex0 == boneWeightB.boneIndex1) return boneWeightB.boneIndex1;
            if (boneWeightA.boneIndex0 == boneWeightB.boneIndex2) return boneWeightB.boneIndex2;
            if (boneWeightA.boneIndex0 == boneWeightB.boneIndex3) return boneWeightB.boneIndex3;
            if (skinQuality < 1)
                return -1f;
            if (boneWeightA.boneIndex1 == boneWeightB.boneIndex0) return boneWeightB.boneIndex0;
            if (boneWeightA.boneIndex1 == boneWeightB.boneIndex1) return boneWeightB.boneIndex1;
            if (boneWeightA.boneIndex1 == boneWeightB.boneIndex2) return boneWeightB.boneIndex2;
            if (boneWeightA.boneIndex1 == boneWeightB.boneIndex3) return boneWeightB.boneIndex3;
            if (skinQuality < 2)
                return -1f;
            if (boneWeightA.boneIndex2 == boneWeightB.boneIndex0) return boneWeightB.boneIndex0;
            if (boneWeightA.boneIndex2 == boneWeightB.boneIndex1) return boneWeightB.boneIndex1;
            if (boneWeightA.boneIndex2 == boneWeightB.boneIndex2) return boneWeightB.boneIndex2;
            if (boneWeightA.boneIndex2 == boneWeightB.boneIndex3) return boneWeightB.boneIndex3;
            if (skinQuality < 3)
                return -1f;
            if (boneWeightA.boneIndex3 == boneWeightB.boneIndex0) return boneWeightB.boneIndex0;
            if (boneWeightA.boneIndex3 == boneWeightB.boneIndex1) return boneWeightB.boneIndex1;
            if (boneWeightA.boneIndex3 == boneWeightB.boneIndex2) return boneWeightB.boneIndex2;
            if (boneWeightA.boneIndex3 == boneWeightB.boneIndex3) return boneWeightB.boneIndex3;
            return -1f;
        }

        public static float RelativeMaxBoneWeight(this BoneWeight boneWeightA, BoneWeight boneWeightB, float minWeight = 1f)
        {
            int boneIndex = 0;
            float boneWeight = 0;
            boneWeightA.MaxWeightIndex(ref boneIndex, ref boneWeight);
            if (boneWeightB.boneIndex0 == boneIndex & boneWeightB.weight0 >= Mathf.Min(minWeight, boneWeight))
                return boneWeightB.weight0;
            if (boneWeightB.boneIndex1 == boneIndex & boneWeightB.weight1 >= Mathf.Min(minWeight, boneWeight))
                return boneWeightB.weight1;
            if (boneWeightB.boneIndex2 == boneIndex & boneWeightB.weight2 >= Mathf.Min(minWeight, boneWeight))
                return boneWeightB.weight2;
            if (boneWeightB.boneIndex3 == boneIndex & boneWeightB.weight3 >= Mathf.Min(minWeight, boneWeight))
                return boneWeightB.weight3;
            return -1f;
        }

        public static void MaxWeightIndex(this BoneWeight boneWeight, ref int index, ref float weight)
        {
            if (boneWeight.weight0 > weight)
            {
                index = boneWeight.boneIndex0;
                weight = boneWeight.weight0;
            }
            if (boneWeight.weight1 > weight)
            {
                index = boneWeight.boneIndex1;
                weight = boneWeight.weight1;
            }
            if (boneWeight.weight2 > weight)
            {
                index = boneWeight.boneIndex2;
                weight = boneWeight.weight2;
            }
            if (boneWeight.weight3 > weight)
            {
                index = boneWeight.boneIndex3;
                weight = boneWeight.weight3;
            }
        }

        public static void MaxWeightIndex(this BoneWeight boneWeight, ref int index)
        {
            float weight = 0f;
            if (boneWeight.weight0 > weight)
            {
                index = boneWeight.boneIndex0;
                weight = boneWeight.weight0;
            }
            if (boneWeight.weight1 > weight)
            {
                index = boneWeight.boneIndex1;
                weight = boneWeight.weight1;
            }
            if (boneWeight.weight2 > weight)
            {
                index = boneWeight.boneIndex2;
                weight = boneWeight.weight2;
            }
            if (boneWeight.weight3 > weight)
            {
                index = boneWeight.boneIndex3;
            }
        }

        public static bool IsWeighted(this BoneWeight boneWeight, int boneIndex)
        {

            if (boneWeight.boneIndex0 == boneIndex & boneWeight.weight0 > 0) return true;
            if (boneWeight.boneIndex1 == boneIndex & boneWeight.weight1 > 0) return true;
            if (boneWeight.boneIndex2 == boneIndex & boneWeight.weight2 > 0) return true;
            if (boneWeight.boneIndex3 == boneIndex & boneWeight.weight3 > 0) return true;
            return false;
        }

        public static bool IsWeighted(this BoneWeight[] boneWeights, int boneIndex)
        {
            for (int i = 0; i < boneWeights.Length; i++) if (boneWeights[i].IsWeighted(boneIndex)) return true;
            return false;
        }

        internal static BoneWeight[] GetBoneWeights(this Mesh sourceMesh, int[] mapping)
        {
            BoneWeight[] boneWeights = new BoneWeight[mapping.Length];
            for (int i = 0; i < boneWeights.Length; i++)
            {
                BoneWeightData boneWeightData = sourceMesh.boneWeights[mapping[i]];
                boneWeights[i] = boneWeightData;
            }
            return boneWeights;
        }

        public static BoneWeight[] GetBoneWeights(this BoneWeight[] source, int[] mapping)
        {
            int count = Mathf.Min(source.Length, mapping.Length);
            BoneWeight[] boneWeights = new BoneWeight[count];
            for (int i = 0; i < count; i++)
            {
                BoneWeightData boneWeightData = boneWeights[mapping[i]];
                boneWeights[i] = boneWeightData;
            }
            return boneWeights;
        }

        public static float GetBoneWeight(this BoneWeight boneWeight, int channel, bool weight)
        {
            if (weight)
                switch (channel)
                {
                    case 0: return boneWeight.weight0;
                    case 1: return boneWeight.weight1;
                    case 2: return boneWeight.weight2;
                    case 3: return boneWeight.weight3;
                    default: return 0f;
                }
            else
                switch (channel)
                {
                    case 0: return boneWeight.boneIndex0;
                    case 1: return boneWeight.boneIndex1;
                    case 2: return boneWeight.boneIndex2;
                    case 3: return boneWeight.boneIndex3;
                    default: return 0;
                }
        }

        public static void SetBoneWeight(this BoneWeight boneWeight, float value, int channel, bool weight)
        {
            if (weight)
                switch (channel)
                {
                    case 0: boneWeight.weight0 = value; break;
                    case 1: boneWeight.weight1 = value; break;
                    case 2: boneWeight.weight2 = value; break;
                    case 3: boneWeight.weight3 = value; break;
                }
            else
                switch (channel)
                {
                    case 0: boneWeight.boneIndex0 = (int)value; break;
                    case 1: boneWeight.boneIndex1 = (int)value; break;
                    case 2: boneWeight.boneIndex2 = (int)value; break;
                    case 3: boneWeight.boneIndex3 = (int)value; break;
                }
        }

        public static float GetBoneWeight(this BoneWeight boneWeight, int channel)
        {
            switch (channel)
            {
                case 0: return boneWeight.weight0;
                case 1: return boneWeight.weight1;
                case 2: return boneWeight.weight2;
                case 3: return boneWeight.weight3;
                default: return 0f;
            }
        }

        public static int GetBoneIndex(this BoneWeight boneWeight, int channel)
        {
            switch (channel)
            {
                case 0: return boneWeight.boneIndex0;
                case 1: return boneWeight.boneIndex1;
                case 2: return boneWeight.boneIndex2;
                case 3: return boneWeight.boneIndex3;
                default: return 0;
            }
        }

        public static void SetBoneWeight(this BoneWeight boneWeight, float value, int channel)
        {
            switch (channel)
            {
                case 0: boneWeight.weight0 = value; break;
                case 1: boneWeight.weight1 = value; break;
                case 2: boneWeight.weight2 = value; break;
                case 3: boneWeight.weight3 = value; break;
            }
        }

        public static void SetBoneIndex(this BoneWeight boneWeight, int value, int channel)
        {
            switch (channel)
            {
                case 0: boneWeight.boneIndex0 = value; break;
                case 1: boneWeight.boneIndex1 = value; break;
                case 2: boneWeight.boneIndex2 = value; break;
                case 3: boneWeight.boneIndex3 = value; break;
            }
        }
    }
}