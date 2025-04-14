using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class ModelRefrence
    {
        public GameObject root;
        public Renderer[] renderers;
        public int selected;
        public string meshInfo;
        public int instanceID;
        public string assetID;

        public ModelRefrence()
        {
            root = null; renderers = new Renderer[0]; selected = 0; meshInfo = "";
            instanceID = -1; assetID = "none";
        }

        public Renderer GetRenderer
        {
            get
            {
                if (SkinnEx.IsNullOrEmpty(renderers)) { selected = 0; return null; }
                selected = Mathf.Clamp(selected, 0, Mathf.Abs(renderers.Length - 1));
                return renderers[selected];
            }
        }

        public SkinnedMeshRenderer GetSkinnedMeshRenderer
        {
            get
            {
                if (SkinnEx.IsNullOrEmpty(renderers)) { selected = 0; return null; }
                selected = Mathf.Clamp(selected, 0, Mathf.Abs(renderers.Length - 1));
                var renderer = renderers[selected];
                if (renderer) return renderer as SkinnedMeshRenderer;
                return null;
            }
        }

        public static implicit operator bool (ModelRefrence x) { return x != null; }
    }

}