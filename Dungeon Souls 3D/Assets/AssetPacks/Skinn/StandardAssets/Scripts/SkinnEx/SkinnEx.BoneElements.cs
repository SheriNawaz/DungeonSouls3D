using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static BoneElement GetBoneTreeElement(this List<BoneElement> boneTreeElements, int boneIndex, bool fromID = false)
        {
            return boneTreeElements.Find(item => (fromID ? item.ID : item.orginalIndex) == boneIndex);
        }

        public static int GetBoneTreeElementIndex(this List<BoneElement> boneTreeElements, int boneIndex, bool fromID = false)
        {
            for (int i = 0; i < boneTreeElements.Count; i++)
            {
                if (fromID && boneTreeElements[i].ID == boneIndex) return i;
                else if (boneTreeElements[i].orginalIndex == boneIndex) return i;
            }
            return -1;
        }

        public static bool IsChild(this BoneElement treeElement, int index)
        {
            if (treeElement == null)
                return false;

            if (treeElement.ID == index)
                return false;

            int childCount = 0;
            ChildCountRecursive(treeElement, index, ref childCount);
            if (childCount > 0)
                return true;

            return false;
        }

        private static void ChildCountRecursive(BoneElement treeElement, int index, ref int count)
        {

            if (!treeElement.HasChildren || treeElement.Children == null)
                return;

            for (int i = 0; i < treeElement.Children.Count; ++i)
            {
                BoneElement currentTree = treeElement.Children[i] as BoneElement;
                if (currentTree.ID == index)
                    count++;
                ChildCountRecursive(currentTree, index, ref count);
            }
        }

        public static List<BoneElement> GenerateTransformTree(this SkinnedMeshRenderer source)
        {
            Transform boneTransform = source.bones.FindMinHierarchicalOrder();
            Transform[] bones = source.bones;
            Matrix4x4[] bindposes = source.sharedMesh.bindposes;

            List<BoneElement> treeElements = new List<BoneElement>();

            BoneElement root = new BoneElement("Null Root", -1, -1);
            treeElements.Add(root);

            Matrix4x4 bonePose = GetLocalMatrix(boneTransform);
            BoneElement child = new BoneElement(boneTransform.name, root.Depth + 1, boneTransform.GetInstanceID())
            {
                objectRefrenceValue = boneTransform,
                bonePose = bonePose,
            };

            int boneIndex = bones.GetIndex(boneTransform);
            if (boneIndex >= 0)
            {
                child.orginalIndex = boneIndex;
                child.bindpose = bindposes[boneIndex];
                treeElements.Add(child);
            }
            
            AddChildrenRecursive(child, boneTransform, source, ref treeElements);

            return treeElements;
        }

        private static void AddChildrenRecursive(TreeElement element, Transform transform, SkinnedMeshRenderer skinnedMeshRenderer, ref List<BoneElement> treeElements)
        {
            Transform[] bones = skinnedMeshRenderer.bones;
            Matrix4x4[] bindposes = skinnedMeshRenderer.sharedMesh.bindposes;
            Matrix4x4 worldMatrix = transform.root.worldToLocalMatrix;
            for (int i = 0; i < transform.childCount; ++i)
            {

                Transform boneTransform = transform.GetChild(i);
                Matrix4x4 bonePose = GetLocalMatrix(boneTransform);
                BoneElement child = new BoneElement(boneTransform.name, element.Depth + 1, boneTransform.GetInstanceID())
                {
                    objectRefrenceValue = boneTransform,
                    bonePose = bonePose,
                };

                int boneIndex = bones.GetIndex(boneTransform);
                if (boneIndex >= 0)
                {
                    child.orginalIndex = boneIndex;
                    child.bindpose = bindposes[boneIndex];
                }
                else
                {
                    child.orginalIndex = child.ID;
                    child.bindpose = boneTransform.localToWorldMatrix.inverse * worldMatrix;
                }

                treeElements.Add(child);

                AddChildrenRecursive(child, boneTransform, skinnedMeshRenderer, ref treeElements);
            }
        }

        public static Transform[] GenerateBones(this List<BoneElement> skeleton)
        {
            int transformCount = skeleton.Count;
            Transform[] transforms = new Transform[transformCount];
            for (int i = 0; i < transformCount; i++)
            {

                Transform trans = new GameObject(skeleton[i].Name).GetComponent<Transform>();
                trans.FromMatrix(skeleton[i].bindpose.inverse);
                trans.SetParent(null, true);
                transforms[i] = trans;
            }
            List<Transform> bones = new List<Transform>();
            for (int i = 1; i < transformCount; i++)
            {
                transforms[i].SetParent(transforms[GetBoneTreeElementIndex(skeleton, skeleton[i].Parent.ID, true)], true);
                bones.Add(transforms[i]);
            }
            return bones.ToArray();
        }

        public static List<BoneElement> GenerateTransformTree(Transform transform)
        {
            List<BoneElement> treeElements = new List<BoneElement>();
            BoneElement rootElement = new BoneElement("Null Root", -1, -1);
            treeElements.Add(rootElement);
            Matrix4x4 bonePose = SkinnEx.GetLocalMatrix(transform);
            int id = transform.GetInstanceID();
            BoneElement child = new BoneElement(transform.name, rootElement.Depth + 1, id)
            {
                orginalIndex = id,
                objectRefrenceValue = transform,
                bonePose = bonePose,
                bindpose = bonePose.inverse,
            };
            treeElements.Add(child);
            AddChildrenRecursive(child, transform, ref treeElements);
            return treeElements;
        }

        private static void AddChildrenRecursive(TreeElement element, Transform transform, ref List<BoneElement> treeElements)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform childTransform = transform.GetChild(i);
                Matrix4x4 bonePose = SkinnEx.GetLocalMatrix(childTransform);
                int id = childTransform.GetInstanceID();
                BoneElement child = new BoneElement(childTransform.name, element.Depth + 1, childTransform.GetInstanceID())
                {
                    orginalIndex = id,
                    objectRefrenceValue = childTransform,
                    bonePose = bonePose,
                    bindpose = bonePose.inverse,
                };
                treeElements.Add(child);
                AddChildrenRecursive(child, childTransform, ref treeElements);
            }
        }

        public static List<BoneElement> GetChildern(this BoneElement source, bool inculdeRoot = false)
        {
            List<BoneElement> treeElements = new List<BoneElement>();
            AddChildrenRecursive(source, ref treeElements, inculdeRoot);
            return treeElements;
        }

        private static void AddChildrenRecursive(BoneElement element, ref List<BoneElement> treeElements, bool inculdeRoot)
        {
            if (inculdeRoot) treeElements.Add(element);
            if (!element.HasChildren) return;
            var childern = element.Children;
            for (int i = 0; i < childern.Count; ++i) AddChildrenRecursive(childern[i] as BoneElement, ref treeElements, true);
        }
    }
}