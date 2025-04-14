using UnityEngine;

#if UNITY_EDITOR
#endif

namespace CWM.Skinn
{
    [System.Serializable]
    public class LocalTransform
    {
        public string name;
        public int hashID, parentHashID, rootHashID;
        public Vector3 localPosition, localScale;
        public Quaternion localRotation;

        public LocalTransform() { Reset(); }
        public LocalTransform(Transform transform) { Set(transform); }

        public void Reset()
        {
            name = "null";
            hashID = -1; parentHashID = -1; rootHashID = -1;
            localPosition = Vector3.one; localRotation = Quaternion.identity; localScale = Vector3.one;
        }

        public void Set(Transform transform)
        {
            Reset();
            if (!transform) return;
            name = transform.name;
            localPosition = transform.localPosition; localRotation = transform.localRotation; localScale = transform.localScale;
            hashID = transform.gameObject.GetInstanceID();
            parentHashID = transform.parent ? transform.parent.gameObject.GetInstanceID() : -1;
            rootHashID = transform.root ? transform.root.gameObject.GetInstanceID() : -1;
        }

        public static implicit operator bool(LocalTransform t) { return t != null; }
    }
}