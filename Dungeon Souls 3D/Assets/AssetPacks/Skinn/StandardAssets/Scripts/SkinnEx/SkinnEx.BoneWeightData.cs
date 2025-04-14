using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public class BoneWeightData
        {
            public BoneWeightData() { Reset(); }

            public void Reset()
            {
                weights = new Vector2[4];
                weights[0].x = 0; weights[0].y = 1;
                weights[1].x = 0; weights[1].y = 0;
                weights[2].x = 0; weights[2].y = 0;
                weights[3].x = 0; weights[3].y = 0;
            }

            public Vector2[] weights;

            public int GetIndex(int index)
            {
                var i = Mathf.Abs(index);
                if (weights == null || i >= weights.Length) return 0;
                return (int)weights[i].x;
            }

            public float GetWeight(int index)
            {
                var i = Mathf.Abs(index);
                if (weights == null || i >= weights.Length) return 0;
                return weights[i].y;
            }

            public void SetIndex(int index, int value)
            {
                var i = Mathf.Abs(index);
                if (weights == null || i >= weights.Length) return;
                weights[i].x = value;
            }

            public void SetWeight(int index, float value)
            {
                var i = Mathf.Abs(index);
                if (weights == null || i >= weights.Length) return;
                weights[i].y = value;
            }

            public void Clamp(float minWeight = 0.00001f)
            {
                float totalWeight = 0f;
                for (int i = 0; i < weights.Length; i++)
                {
                    if (weights[i].y <= minWeight) weights[i] = Vector2.zero;
                    totalWeight += weights[i].y;
                }
                if(totalWeight <= 0f) { Reset(); }
                else {  for (int i = 0; i < weights.Length; i++) weights[i].y = Mathf.Clamp01((weights[i].y / totalWeight) * 1f); }


            }

            public int FindChannel(int boneIndex)
            {
                for (int i = 0; i < weights.Length; i++) if ((int)weights[i].x == boneIndex) return i;
                return -1;
            }

            //public void Order()
            //{
            //    weights = weights.OrderBy(x => x.y).ToArray();
            //    weights = weights.Reverse().ToArray();
            //}

            public static implicit operator BoneWeight(BoneWeightData value)
            {
                return new BoneWeight()
                {
                    boneIndex0 = value.GetIndex(0),
                    boneIndex1 = value.GetIndex(1),
                    boneIndex2 = value.GetIndex(2),
                    boneIndex3 = value.GetIndex(3),
                    weight0 = value.GetWeight(0),
                    weight1 = value.GetWeight(1),
                    weight2 = value.GetWeight(2),
                    weight3 = value.GetWeight(3),
                };
            }

            public static implicit operator BoneWeightData(BoneWeight value)
            {
                var weights = new Vector2[4];
                weights[0].x = value.boneIndex0;
                weights[0].y = value.weight0;
                weights[1].x = value.boneIndex1;
                weights[1].y = value.weight1;
                weights[2].x = value.boneIndex2;
                weights[2].y = value.weight2;
                weights[3].x = value.boneIndex3;
                weights[3].y = value.weight3;
                return new BoneWeightData() { weights = weights };
            }
        }

    }
}