using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class BlendableElements
    {
        private const int MaxAssets = 300;

        public List<BoneElement> elements;

        public SkinnedMeshRenderer[] SkinnedMeshes { get; private set; }
        public int[] Hashes { get; private set; }

        private int maxIndex;

        public int MaxIndex { get { return maxIndex; }  }

        public bool UpdateAssets(GameObject source)
        {
            if (source == null) return false;
            if (SkinnEx.IsNullOrEmpty(Hashes, MaxAssets)) Hashes = new int[MaxAssets];
            if (SkinnEx.IsNullOrEmpty(SkinnedMeshes, MaxAssets)) SkinnedMeshes = new SkinnedMeshRenderer[MaxAssets];

            maxIndex = 0;
            bool dirty = false;

            var item = source.GetComponent<SkinnedMeshRenderer>();
            if (item && item.sharedMesh && item.sharedMesh.blendShapeCount > 0)
            {
                var id = item.sharedMesh.GetInstanceID();
                if (id != Hashes[maxIndex]) { dirty = true; }
                Hashes[maxIndex] = id;
                SkinnedMeshes[maxIndex] = item;
                maxIndex++;
            }

            UpdateHashes(source.transform, ref dirty);
           
            if (dirty)
            {
                Debug.Log("UpdateElements");
                UpdateElements();
            }
            else
            {
                if (maxIndex == 0 && elements.Count > 1)
                {
                    elements = new List<BoneElement>() { new BoneElement("Null Root", -1, -1) };
                    Debug.Log("Root");

                    return true;
                }
            }

            return dirty;
        }

        public void UpdateAllBlendshapes()
        {
            if (SkinnEx.IsNullOrEmpty(Hashes, MaxAssets) || SkinnEx.IsNullOrEmpty(SkinnedMeshes, MaxAssets) || SkinnEx.IsNullOrEmpty(elements)) return;
            for (int i = 0; i < elements.Count; i++) { UpdateBlendshape(elements[i]); }
        }

        private void UpdateBlendshape(BoneElement shape)
        {
            if (SkinnEx.IsNullOrEmpty(Hashes, MaxAssets)) return;
            if (SkinnEx.IsNullOrEmpty(SkinnedMeshes, MaxAssets)) return;
            if (shape == null || SkinnEx.IsNullOrEmpty(shape.indices2)) return;
            var indices2 = shape.indices2;
            for (int i = 0; i < indices2.Count; i++)
            {
                var smr = SkinnedMeshes[indices2[i].x]; if (!smr) continue;
                smr.SetBlendShapeWeight(indices2[i].y, shape.weight);
            }
        }

        private void UpdateHashes(Transform transform, ref bool dirty)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                var item = child.GetComponent<SkinnedMeshRenderer>();
                if (item && item.sharedMesh && item.sharedMesh.blendShapeCount > 0)
                {
                    var id = item.sharedMesh.GetInstanceID();
                    if (id != Hashes[maxIndex]) { dirty = true; }
                    Hashes[maxIndex] = id; SkinnedMeshes[maxIndex] = item; maxIndex++;
                }
                UpdateHashes(child, ref dirty);
            }
        }

        private void UpdateElements()
        {
            var shapes = new List<BoneElement>();
            var root = new BoneElement("Null Root", -1, -1);
            shapes.Add(new BoneElement(root));
            var weightIndex = 0;
            for (int i = 0; i < maxIndex; i++)
            {
                var smr = SkinnedMeshes[i]; if (!smr) continue;
                var shapeNames = smr.GetBlendshapeNames();
                var shapesHashes = shapeNames.GetOrderedHashes();
                for (int ii = 0; ii < shapeNames.Length; ii++)
                {
                    var shape = shapes.GetBoneTreeElement(shapesHashes[ii], true);
                    var shapeName = shapeNames[ii];
                    var shapeHash = shapesHashes[ii];
                    if (shape)
                    {
                        shape.indices2.Add(new Vector2Int(i, ii));
                    }
                    else
                    {
                        shape = new BoneElement(shapeName, root.Depth + 1, shapeHash)
                        {
                            orginalIndex = weightIndex,
                            indices2 = new List<Vector2Int>() { new Vector2Int(i, ii) },
                            weight = smr.GetBlendShapeWeight(ii)
                        };
                        shapes.Add(shape);
                        weightIndex++;
                    }
                }
            }

            Debug.Log("UpdateElements 2");

            this.elements = shapes;
        }

        public static implicit operator bool(BlendableElements value) { return value != null; }
    }
}
