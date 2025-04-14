using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class MeshBinding
    {
        public int otherVertexCount;
        public int[] indices;
        public float[] distance;

        public MeshBinding() { otherVertexCount = -1; indices = new int[0]; distance = new float[0]; }

        public static implicit operator bool (MeshBinding binding) { return binding != null; }
    }
}