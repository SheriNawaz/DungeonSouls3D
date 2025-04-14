using System;
using System.Collections.Generic;
using UnityEngine;

namespace CWM.Skinn
{
    [Serializable]
    public class BoneElement : TreeElement
    {
        [HideInInspector] public int orginalIndex;
        [HideInInspector] public string tag;
        [HideInInspector] public int layer;
        [HideInInspector] public bool enabled;

        public float weight;
        [HideInInspector] public float size;
        [HideInInspector] public float sizeStart;
        [HideInInspector] public float sizeEnd;
        [HideInInspector] public float radius;

        [HideInInspector] public Vector3 offest;
        [HideInInspector] public Vector3 direction;

        [HideInInspector] public Color startColor;
        [HideInInspector] public Color endColor;

        [HideInInspector] public Matrix4x4 bindpose;
        [HideInInspector] public Matrix4x4 bindpose2;

        [HideInInspector] public Matrix4x4 bonePose;
        [HideInInspector] public Matrix4x4 bonePose2;

        [HideInInspector] public UnityEngine.Object objectRefrenceValue;

        [HideInInspector] public List<int> indices;
        [HideInInspector] public List<Vector2Int> indices2;

        public void Reset()
        {
            orginalIndex = 0;
            tag = "";
            layer = 0;
            enabled = true;
            weight = 0f;
            size = 1f;
            sizeStart = 1f;
            sizeEnd = 1f;
            radius = 1f; ;
            offest = Vector3.zero;
            direction = Vector3.up;
            startColor = Color.white;
            endColor = Color.white;
            bindpose = Matrix4x4.identity;
            bindpose2 = Matrix4x4.identity;
            bonePose = Matrix4x4.identity;
            bonePose2 = Matrix4x4.identity;
            objectRefrenceValue = null;
            indices = null;
            indices2 = null;
        }

        public BoneElement(string name, int depth, int id) : base(name, depth, id)
        {
            Reset();
            orginalIndex = id;
        }

        public BoneElement(BoneElement boneTreeElement)
        {
            Name = boneTreeElement.Name;
            ID = boneTreeElement.ID;
            Depth = boneTreeElement.Depth;

            orginalIndex = boneTreeElement.orginalIndex;
            tag = boneTreeElement.tag;
            layer = boneTreeElement.layer;
            enabled = boneTreeElement.enabled;
            weight = boneTreeElement.weight;
            size = boneTreeElement.size;
            sizeStart = boneTreeElement.sizeStart;
            sizeEnd = boneTreeElement.sizeEnd;
            radius = boneTreeElement.radius;
            offest = boneTreeElement.offest;
            direction = boneTreeElement.direction;
            startColor = boneTreeElement.startColor;
            endColor = boneTreeElement.endColor;
            bindpose = boneTreeElement.bindpose;
            bindpose2 = boneTreeElement.bindpose2;
            objectRefrenceValue = boneTreeElement.objectRefrenceValue;
        }

        public bool CreatedFromBone { get { return orginalIndex != ID; } }

        public static implicit operator bool(BoneElement value) { return value != null; }
    }
}
