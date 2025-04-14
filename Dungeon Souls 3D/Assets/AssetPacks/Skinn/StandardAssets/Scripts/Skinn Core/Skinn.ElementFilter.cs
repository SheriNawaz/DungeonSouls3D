using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class ElementFilter
    {
        public string name = "";
        public string assetID = "";

        public TextAsset textFilter = null;

        [HideInInspector] public List<BoneElement> elements = new List<BoneElement>();

        public int enabledCount = 0;

        public static implicit operator bool (ElementFilter value) { return value != null; }

        public override string ToString()
        {
            if (SkinnEx.IsNullOrEmpty(elements)) return "Filter: 0 out of 0 items";
            return string.Format("Filter: {0} out of {1} items"
                , enabledCount, elements.Count - 1);
        }

        public void Update()
        {
            if (SkinnEx.IsNullOrEmpty(elements)) return;
            int enabledCount = 0;
            for (int i = 1; i < elements.Count; i++) if (elements[i].ID != -1 && elements[i].enabled) enabledCount++;
            this.enabledCount = enabledCount;
        }

        public void GenerateAsShapes(SkinnedMeshRenderer source)
        {
            this.elements = new List<BoneElement>();
            if (!SkinnEx.HasMinimumRequirements(source)) return;

            var shapes = (source.GetSharedMesh()).GetBlendshapeNames();
            this.elements.Capacity = shapes.Length + 1;
            this.name = SkinnEx.EnforceObjectName(source);
            BoneElement root = new BoneElement("Null Root", -1, -1) { orginalIndex = -1 };
            this.elements.Add(root);
            for (int i = 0; i < shapes.Length; i++)
            {
                BoneElement child = new BoneElement(shapes[i], root.Depth + 1, Animator.StringToHash(shapes[i]));
                child.orginalIndex = i;
                this.elements.Add(child);
            }
            Update();
        }

        public void GenerateAsShapes(string[] shapes)
        {
            this.elements = new List<BoneElement>();
            this.elements.Capacity = shapes.Length + 1;
            this.name = "Shapes";
            BoneElement root = new BoneElement("Null Root", -1, -1) { orginalIndex = -1 };
            this.elements.Add(root);
            for (int i = 0; i < shapes.Length; i++)
            {
                BoneElement child = new BoneElement(shapes[i], root.Depth + 1, Animator.StringToHash(shapes[i]));
                child.orginalIndex = i;
                this.elements.Add(child);
            }
            Update();
        }

        public void GenerateAsBones(SkinnedMeshRenderer source)
        {
            this.elements = new List<BoneElement>();
            if (!SkinnEx.HasMinimumRequirements(source)) return;

            this.name = SkinnEx.EnforceObjectName(source);

            Transform rootBone = source.bones.FindMinHierarchicalOrder();
            BoneElement root = new BoneElement("Null Root", -1, -1) { orginalIndex = -1};
            this.elements.Add(root);
            BoneElement child = new BoneElement(rootBone.name, root.Depth + 1, rootBone.GetInstanceID());
            int boneIndex = source.bones.GetIndex(rootBone);
            if (boneIndex >= 0)
            {
                child.orginalIndex = boneIndex;
            }
            else
            {
                child.orginalIndex = child.ID;
            }
            this.elements.Add(child);
            AddChildrenRecursive(child, rootBone, Matrix4x4.identity, source, ref this.elements);

            Update();
        }


        private static void AddChildrenRecursive(TreeElement element, Transform transform, Matrix4x4 transformPoint, SkinnedMeshRenderer skinnedMeshRenderer, ref List<BoneElement> treeElements)
        {
            Transform[] bones = skinnedMeshRenderer.bones;
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform childTransform = transform.GetChild(i);
                BoneElement child = new BoneElement(childTransform.name, element.Depth + 1, childTransform.GetInstanceID());
                int boneIndex = bones.GetIndex(childTransform);
                if (boneIndex >= 0)
                {
                    child.orginalIndex = boneIndex;
                }
                else
                {
                    child.orginalIndex = child.ID;
                }

                treeElements.Add(child);
                AddChildrenRecursive(child, childTransform, transformPoint, skinnedMeshRenderer, ref treeElements);
            }
        }

        public int[] GetBoneFilter(Transform[] transforms)
        {
            if (SkinnEx.IsNullOrEmpty(transforms)) return null;
            if (!IsValidTransforms(transforms)) return null;

            int[] boneMask = new int[transforms.Length];
            for (int i = 0; i < transforms.Length; i++)
            {
                var bone = elements[i]; if (!bone) continue;
                var boneElement = elements.GetBoneTreeElement(i, false); if (!boneElement) continue;
                boneMask[i] = boneElement.enabled ? 0 : 1;
            }
            return boneMask;
        }

        public int[] GetShapeFilter(Renderer renderer)
        {
            if (SkinnEx.IsNullOrNotVailid(renderer)) return null;
            if (!IsValidShapes(renderer)) return null;

            var blendShapeNames = (renderer.GetSharedMesh()).GetBlendshapeNames();
            int[] boneMask = new int[blendShapeNames.Length];
            for (int i = 0; i < blendShapeNames.Length; i++)
            {
                var bone = elements[i]; if (!bone) continue;
                var boneElement = elements.GetBoneTreeElement(Animator.StringToHash(blendShapeNames[i]), true); if (!boneElement) continue;
                boneMask[i] = boneElement.enabled ? 0 : 1;
            }
            return boneMask;
        }

        public bool IsValidTransforms(Transform[] transforms)
        {
            if (SkinnEx.IsNullOrEmpty(transforms)) return false;
            for (int i = 0; i < transforms.Length; i++)
                if (transforms[i] && elements.GetBoneTreeElementIndex(transforms[i].GetInstanceID(), true) < 0) return false;
            return true;
        }

        public bool IsValidShapes(Renderer renderer)
        {
            if (SkinnEx.IsNullOrNotVailid(renderer)) return false;
            var blendShapeCount = (renderer.GetSharedMesh()).blendShapeCount;
            if (blendShapeCount + 1 != elements.Count) return false;
            var blendShapeNames = (renderer.GetSharedMesh()).GetBlendshapeNames();
            for (int i = 0; i < blendShapeCount; i++)
                if (elements.GetBoneTreeElementIndex(Animator.StringToHash(blendShapeNames[i]), true) < 0) return false;
            return true;
        }

        public bool IsValidShapes(string[] shapes)
        {
            var blendShapeCount = shapes.Length;
            if (blendShapeCount + 1 != elements.Count) return false;
            var blendShapeNames = shapes;
            for (int i = 0; i < blendShapeCount; i++)
                if (elements.GetBoneTreeElementIndex(Animator.StringToHash(blendShapeNames[i]), true) < 0) return false;
            return true;
        }
    }
}
