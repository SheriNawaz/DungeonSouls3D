using UnityEngine;

namespace CWM.Skinn
{
    [System.Serializable]
    public class TextureRefrence
    {
        public string textureName;
        public string propertyName;
        public Texture texture;

        public static implicit operator bool(TextureRefrence refrence) { return refrence != null; }
    }
}