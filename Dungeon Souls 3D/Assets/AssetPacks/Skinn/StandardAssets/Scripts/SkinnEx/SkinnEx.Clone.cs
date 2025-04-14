using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
	public static partial class SkinnEx
    {
        /// <summary>
        /// creates a cloned mesh with the original name bypassing hideFlags.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>can return null if the object does not met the minimum requirements.</returns>
        public static Object Clone(this Mesh source)
        {
            if (!source) return null;
            HideFlags hideFlags = source.hideFlags;
            source.hideFlags = HideFlags.None;
            var clone = Mesh.Instantiate(source);
            source.hideFlags = hideFlags;
            clone.name = EnforceString(source.name);
            clone.name.Replace("(Clone)", "");
            return clone;
        }

        public static Object Clone(this Mesh source, HideFlags hideFlags)
        {
            if (!source) return null;
            var clone = source.Clone();
            clone.hideFlags = hideFlags;
            return clone;
        }

        /// <summary>
        /// creates a cloned transform with the original name, tag and layer.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>can return null if the object does not met the minimum requirements.</returns>
        public static Object Clone(this Transform source)
        {
            if (!source) return null;
            return (source.gameObject.Clone() as GameObject).transform;
        }

        public static Object Clone(this Transform source, bool setPosition)
        {
            if (!source) return null;
            return (source.gameObject.Clone(setPosition) as GameObject).transform;
        }

        /// <summary>
        /// creates a cloned game-object with the original name, tag and layer.
        /// </summary>
        /// <param name="source"></param>
        /// <returns>can return null if the object does not met the minimum requirements.</returns>
        public static Object Clone(this GameObject source, bool setTransformation = true, bool setTagAndLayer = true)
        {
            if (!source) return null;
            GameObject clone = new GameObject(source.name){ };
            if (setTagAndLayer)
            {
                clone.layer = source.layer;
                clone.tag = source.tag;
            }
            if (setTransformation)
            {
                clone.transform.position = source.transform.position;
                clone.transform.rotation = source.transform.rotation;
                clone.transform.localScale = source.transform.lossyScale;
            }
            return clone;
        }

        /// <summary>
        /// creates cloned component and new mesh keeping the original names.
        /// with 'leaveSkinningAssetsNull' the mesh, root bone, and bone array are left null.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="leaveBonesNul"></param>
        /// <returns>can return null if the object does not met the minimum requirements.</returns>
        public static Object Clone(this SkinnedMeshRenderer source, bool leaveMeshNull = false, bool leaveBonesNul = false)
        {
            if (source == null || !source.HasMinimumRequirements()) return null;

            var gameObject = source.gameObject.Clone(false) as GameObject;
            SkinnedMeshRenderer clone = gameObject.AddComponent<SkinnedMeshRenderer>();

            //clone.renderingLayerMask = source.renderingLayerMask;
            //clone.rendererPriority = source.rendererPriority;
            clone.quality = source.quality;
            clone.updateWhenOffscreen = source.updateWhenOffscreen;
            //clone.skinnedMotionVectors = source.skinnedMotionVectors;

            if (!leaveMeshNull)
            {
                clone.sharedMesh = source.sharedMesh.Clone() as Mesh;
            }
            if (!leaveBonesNul)
            {
                clone.bones = source.bones;
                clone.rootBone = source.rootBone;
            }

            //clone.lightProbeUsage = source.lightProbeUsage;
            //clone.reflectionProbeUsage = source.reflectionProbeUsage;
            clone.probeAnchor = clone.probeAnchor;
            clone.shadowCastingMode = clone.shadowCastingMode;
            clone.receiveShadows = clone.receiveShadows;
            //clone.motionVectorGenerationMode = clone.motionVectorGenerationMode;
            clone.materials = source.sharedMaterials;
            //clone.allowOcclusionWhenDynamic = source.allowOcclusionWhenDynamic;

            clone.SetBounds();

            return clone;
        }

        public static Mesh CloneSharedMesh(this Renderer source)
        {
            Mesh mesh = GetSharedMesh(source);
            if (mesh) mesh = mesh.Clone() as Mesh;
            return mesh;
        }

        public static Object CloneAsSkinnedMeshRenderer(this Renderer source, bool leaveMeshNull = false)
        {
            if (!source || !source.HasMinimumRequirements()) return null;
            SkinnedMeshRenderer smr = null;
            if (source as SkinnedMeshRenderer)
            {
                var skinnedMeshRenderer = source as SkinnedMeshRenderer;
                smr = skinnedMeshRenderer.Clone(leaveMeshNull) as SkinnedMeshRenderer;

                smr.bones = skinnedMeshRenderer.bones; smr.rootBone = skinnedMeshRenderer.rootBone;
            }
            else
            {
                smr = (source.gameObject.Clone() as GameObject).AddComponent<SkinnedMeshRenderer>();
                smr.materials = source.sharedMaterials;
                if (!leaveMeshNull)
                {
                    smr.sharedMesh = source.GetComponent<MeshFilter>().sharedMesh.Clone() as Mesh;
                    smr.SetBounds();
                }
            }
            return smr;
        }

        public static Object CloneAsMeshRenderer(this Renderer source, bool leaveMeshNull = false)
        {
            if (!HasMinimumRequirements(source)) return null;
            var gameObject = (source.gameObject.Clone() as GameObject);
            var meshFilter = gameObject.AddComponent<MeshFilter>(); var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.materials = source.sharedMaterials;
            if (!leaveMeshNull) meshFilter.sharedMesh = source.CloneSharedMesh() as Mesh;
            return meshRenderer;
        }

        public static Object Clone(this Renderer source, bool leaveMeshNull = false)
        {
            if (!source || !source.HasMinimumRequirements()) return null;
            Object clone;
            if (source as SkinnedMeshRenderer) clone = CloneAsSkinnedMeshRenderer(source, leaveMeshNull);
            else clone = CloneAsMeshRenderer(source, leaveMeshNull);
            return clone;
        }

        public static Transform[] CloneTransformStack(this Transform transform)
        {
            if (transform == null) return new Transform[0];
            var transforms = CloneTransformStack(transform.GetAllChildern(true).ToArray(), transform);
            return transforms;
        }

        public static Transform[] CloneTransformStack(Transform[] bones, Transform rootBone, Transform newRoot = null)
        {
            var newBones = new Transform[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                newBones[i] = new GameObject(bones[i].name).transform;
                newBones[i].SetParent(bones[i], false);
            }
            for (int i = 0; i < bones.Length; i++)
            {
                if (newBones[i].name == rootBone.name) newBones[i].SetParent(newRoot, true);
                else
                {
                    var childTransform = FindTransformByName(newBones, bones[i].parent.name, true);
                    if (childTransform)
                    {
                        newBones[i].SetParent(childTransform, true);
                        continue;
                    }
                    Transform parent = null;
                    var parents = bones[i].GetComponentsInParent<Transform>();
                    for (int ii = 0; ii < parents.Length; ii++)
                    {
                        Transform nextParent = FindTransformByName(newBones, parents[ii].name, true);
                        if (parent) continue;
                        if (nextParent != null)
                        {
                            if (nextParent.name != newBones[i].name)
                            {
                                parent = nextParent;
                                newBones[i].SetParent(parent, true);
                            }
                        }
                    }
                }
            }
            return newBones;
        }

    }
}