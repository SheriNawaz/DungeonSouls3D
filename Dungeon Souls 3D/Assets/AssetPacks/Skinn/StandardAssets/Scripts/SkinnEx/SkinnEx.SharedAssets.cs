using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static bool HasSharedMesh(this Renderer source)
        {
            Mesh mesh = null;
            if (!source) return mesh;
            var mr = source.GetComponent<MeshFilter>();
            if (mr) mesh = mr.sharedMesh;
            var smr = source.GetComponent<SkinnedMeshRenderer>();
            if (smr) mesh = smr.sharedMesh;
            return mesh;
        }

        public static Mesh GetSharedMesh(this Renderer source)
        {
            Mesh mesh = null;
            if (!source) return mesh;
            var mr = source.GetComponent<MeshFilter>();
            if (mr) mesh = mr.sharedMesh;
            var smr = source.GetComponent<SkinnedMeshRenderer>();
            if (smr) mesh = smr.sharedMesh;
            return mesh;
        }

        public static void SetSharedMesh(this Renderer source, Mesh mesh)
        {
            if (!source) return;
            var mr = source.GetComponent<MeshFilter>();
            if (mr) mr.sharedMesh = mesh;
            var smr = source.GetComponent<SkinnedMeshRenderer>();
            if (smr) smr.sharedMesh = mesh;
        }

        public static Material[] GetSharedMaterials(this Renderer source)
        {
            var materials = new Material[0];
            if (!source) return materials;
            var mr = source.GetComponent<MeshRenderer>();
            if (mr) materials = mr.sharedMaterials;
            var smr = source.GetComponent<SkinnedMeshRenderer>();
            if (smr) materials = smr.sharedMaterials;
            return materials;
        }

        public static Material[] GetSharedMaterialsFromSubMeshes(this Renderer source)
        {
            var materials = new Material[0];
            if (!source) return materials;
            var mesh  = GetSharedMesh(source);
            if (!mesh) return materials;
            int subMeshCount = mesh.subMeshCount;
            materials = GetSharedMaterials(source);
            if (materials.Length == subMeshCount) return materials;
            var newMaterials = new Material[subMeshCount];
            int maxMaterials = Mathf.Min(materials.Length, subMeshCount);
            for (int i = 0; i < maxMaterials; i++) newMaterials[i] = materials[i];
            return newMaterials;
        }

        public static Material GetSharedMaterial(this Renderer source)
        {
            Material material = null;
            if (!source) return material;
            var mr = source.GetComponent<MeshRenderer>();
            if (mr) material = mr.sharedMaterial;
            var smr = source.GetComponent<SkinnedMeshRenderer>();
            if (smr) material = smr.sharedMaterial;
            return material;
        }
    }
}