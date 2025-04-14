using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        /// <summary>
        /// meets the minimum requirements for visibility and or deformation.
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        public static bool HasMinimumRequirements(this Renderer renderer)
        {
            if(renderer as SkinnedMeshRenderer && (renderer as SkinnedMeshRenderer).HasMinimumRequirements(true)) return true;
            else if (renderer as MeshRenderer && renderer.GetComponent<MeshFilter>() && HasMinimumRequirements(renderer.GetComponent<MeshFilter>().sharedMesh)) return true;
            return false;
        }

        /// <summary>
        /// alternative for HasMinimumRequirements(Renderer).
        /// </summary>
        public static bool IsNullOrNotVailid(Renderer renderer){ return (!HasMinimumRequirements(renderer));}

        /// <summary>
        /// meets the minimum requirements for skinning deformation.
        /// optionally log errors.
        /// </summary>
        /// <param name="smr"></param>
        /// <returns></returns>
        public static bool HasMinimumRequirements(this SkinnedMeshRenderer smr, bool log = false)
        {
            if (smr == null) { if(log) Debug.LogError("smr == null"); return false; }
            if (smr.sharedMesh == null) { if (log) Debug.LogError("smr.sharedMesh == null"); return false; }
            if (!HasMinimumRequirements(smr.sharedMesh, log)) { return false; }
            if (smr.sharedMesh.boneWeights == null || smr.sharedMesh.boneWeights.Length != smr.sharedMesh.vertexCount) { if (log) Debug.LogError("smr.sharedMesh.boneWeights == null"); return false; }
            if (IsNullOrEmpty(smr.bones)) { if (log) Debug.LogError("is null or empty : bones"); return false; }
            if (smr.sharedMesh.bindposes == null) { if (log) Debug.LogError("smr.sharedMesh.bindposes == null"); return false; }
            if (IsNullOrEmpty(smr.sharedMesh.triangles)) { if (log) Debug.LogError("smr.sharedMesh.triangles == null"); return false; }
            if (smr.sharedMesh.bindposes.Length != smr.bones.Length)
            {
                if (log) Debug.LogErrorFormat("smr bind-pose is out of bounds. pose count:{0}, bone count{1}",
                        smr.sharedMesh.bindposes.Length, smr.bones.Length); return false;
            }
            if (log && smr.bones.HasAnyNulls())
            { Debug.LogError(string.Format("skinned mesh render {0} is missing bones.", EnforceString(smr.name))); return false; }
            return true;
        }

        /// <summary>
        /// alternative for HasMinimumRequirements(SkinnedMeshRenderer).
        /// </summary>
        public static bool IsNullOrNotVailid(SkinnedMeshRenderer renderer, bool log = false) { return (!HasMinimumRequirements(renderer, log)); }

       
        /// <summary>
        /// meets the minimum requirements for visibility.
        ///optionally log errors.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static bool HasMinimumRequirements(this Mesh mesh, bool log = false)
        {
            if (mesh == null) { if (log) Debug.LogError("mesh is null"); return false; }
            if (IsNullOrLessThan(mesh.vertices, 3)) { if (log) Debug.LogError("mesh.vertices == null"); return false; }
            if (IsNullOrLessThan(mesh.triangles, 3)) { if (log) Debug.LogError("mesh.trinagles == null"); return false; }
            return true;
        }

        /// <summary>
        /// alternative for HasMinimumRequirements(Mesh).
        /// </summary>
        public static bool IsNullOrNotVailid(Mesh mesh) { return (!HasMinimumRequirements(mesh)); }

        
        /// <summary>
        /// return true if vertex and triangles counts of two meshes match.
        /// </summary>
        /// <param name="meshA"></param>
        /// <param name="meshB"></param>
        /// <returns></returns>
        public static bool CanApplyVertices(this Mesh meshA, Mesh meshB)
        {
            if (!meshA || !meshB) return false;
            if (IsNullOrEmpty(meshA.vertices) || IsNullOrEmpty(meshA.triangles)) return false;
            if (IsNullOrEmpty(meshB.vertices, meshA.vertices.Length) || IsNullOrEmpty(meshB.triangles, meshA.triangles.Length)) return false;
            return true;
        }

        public static List<SkinnedMeshRenderer> GetValidSkinnedMeshes(this GameObject source, bool includeSource = true, bool hasRequirements = true)
        {
            if (!source) return new List<SkinnedMeshRenderer>();
            var items = new List<SkinnedMeshRenderer>();
            if (includeSource)
            {
                var item = source.GetComponent<SkinnedMeshRenderer>();
                if (hasRequirements) { if (HasMinimumRequirements(item)) items.Add(item); }
                else { if ((item)) items.Add(item); }
            }
            AddSkinnedMeshsRecursive(source.transform, ref items, hasRequirements);
            return items;
        }


        private static void AddSkinnedMeshsRecursive(Transform transform, ref List<SkinnedMeshRenderer> items, bool hasRequirements)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                var item = child.GetComponent<SkinnedMeshRenderer>();
                if (hasRequirements) { if (HasMinimumRequirements(item)) items.Add(item); }
                else { if ((item)) items.Add(item); }
                AddSkinnedMeshsRecursive(child, ref items, hasRequirements);
            }
        }

        public static List<Renderer> GetRenderers(this GameObject source, bool includeSource = true, bool allowMeshRenderers = true)
        {
            if (!source) return new List<Renderer>();
            var items = new List<Renderer>();
            if (includeSource)
            {
                Renderer item = null;
                if (allowMeshRenderers)
                {
                    item = source.GetComponent<MeshRenderer>() as Renderer;
                    if (item && !item.GetComponent<MeshFilter>()) item = null;
                }
                if (!item) item = source.GetComponent<SkinnedMeshRenderer>() as Renderer;
                if (item) items.Add(item);

            }
            AddRenderersRecursive(source.transform, ref items);
            return items;
        }

        private static void AddRenderersRecursive(Transform transform, ref List<Renderer> items)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                var item = child.GetComponent<MeshRenderer>() as Renderer;
                if (item && !item.GetComponent<MeshFilter>()) item = null;
                if (!item) item = child.GetComponent<SkinnedMeshRenderer>() as Renderer;
                if (item) items.Add(item);
                AddRenderersRecursive(child, ref items);
            }
        }

        public static string GetRendererInstanceID(this GameObject source)
        {
            if (!source) return "";
            var items = source.GetMeshInstanceID();
            GetMeshInstanceIDRecursive(source.transform, ref items);
            return items;
        }

        private static void GetMeshInstanceIDRecursive(Transform transform, ref string items)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                items += child.gameObject.GetMeshInstanceID();
                GetMeshInstanceIDRecursive(child, ref items);
            }
        }

        private static string GetMeshInstanceID(this GameObject source)
        {
            var id = "";
            var renderer = source.GetComponent<Renderer>(); if(!renderer) return id;
            var mesh = renderer.GetSharedMesh(); if (!mesh) return id;
            id = mesh.GetInstanceID().ToString();
            return id;
        }
    }
}
