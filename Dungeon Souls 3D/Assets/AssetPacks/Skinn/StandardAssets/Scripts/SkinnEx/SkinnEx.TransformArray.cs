using UnityEngine;

namespace CWM.Skinn
{
    public static partial class SkinnEx
    {
        public static bool Mask(this Vector3[] vectors, BoneWeight[] boneWeights, int[] bones, out Vector3[] masked, float power = 1)
        {
            masked = null;
            if (IsNullOrEmpty(vectors)  || IsNullOrEmpty(boneWeights, vectors.Length) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders()) return false;

            var computeShader = SkinnInternalAsset.Asset.maskFilter;
            var vertexCount = vectors.Length;
            var OutVector3 = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            var InBoneWeight = new ComputeBuffer(vertexCount, 4 * sizeof(float) + 4 * sizeof(int));
            var InBones = new ComputeBuffer(bones.Length, sizeof(int));

            OutVector3.SetData(vectors);
            InBoneWeight.SetData(boneWeights);
            InBones.SetData(bones);

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetInt("BoneCount", bones.Length);

            computeShader.SetFloat("Power", power);

            var kernel = computeShader.FindKernel("Vector3BoneWeightMask");

            computeShader.SetBuffer(kernel, "InBoneWeight", InBoneWeight);
            computeShader.SetBuffer(kernel, "OutVector3", OutVector3);
            computeShader.SetBuffer(kernel, "InBones", InBones);

            uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;

            computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);

            var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
            computeShader.Dispatch(kernel, threadGroupsX, 1, 1);

            masked = new Vector3[vertexCount];
            OutVector3.GetData(masked);

            Release(OutVector3); Release(InBoneWeight); Release(InBones);
            return true;
        }

        public static bool Transformed(this Vector3[] vectors, out Vector3[] transfromed, Matrix4x4 transform, bool isDirection)
        {
            transfromed = null;
            if (IsNullOrEmpty(vectors) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders()) return false;
           
            var computeShader = SkinnInternalAsset.Asset.linearTransform;
            var vertexCount = vectors.Length;
            var InVector3 = new ComputeBuffer(vertexCount, 3 * sizeof(float));
            var OutVector3 = new ComputeBuffer(vertexCount, 3 * sizeof(float));

            InVector3.SetData(vectors);
            OutVector3.SetData(vectors);

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetMatrix("Transform", transform);
            computeShader.SetBool("IsDirection", isDirection);
         
            var kernel = computeShader.FindKernel("Vector3Pass");

            computeShader.SetBuffer(kernel, "InVector3", InVector3);
            computeShader.SetBuffer(kernel, "OutVector3", OutVector3);

            uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;

            computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);

            var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
            computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
            transfromed = new Vector3[vertexCount];
            OutVector3.GetData(transfromed);

            Release(InVector3); Release(OutVector3);
            return true;
        }

        public static bool Transformed(this Vector4[] vectors, out Vector4[] transfromed, Matrix4x4 transform, bool isDirection)
        {
            transfromed = null;
            if (IsNullOrEmpty(vectors) || !SkinnInternalAsset.Asset || !SkinnInternalAsset.Asset.HasShaders()) return false;

            var computeShader = SkinnInternalAsset.Asset.linearTransform;
            var vertexCount = vectors.Length;
            var InVector4 = new ComputeBuffer(vertexCount, 4 * sizeof(float));
            var OutVector4 = new ComputeBuffer(vertexCount, 4 * sizeof(float));

            InVector4.SetData(vectors);
            OutVector4.SetData(vectors);

            computeShader.SetInt("VertexCount", vertexCount);
            computeShader.SetMatrix("Transform", transform);
            computeShader.SetBool("IsDirection", isDirection);

            var kernel = computeShader.FindKernel("Vector4Pass");

            computeShader.SetBuffer(kernel, "InVector4", InVector4);
            computeShader.SetBuffer(kernel, "OutVector4", OutVector4);

            uint threadGroupSizeX, threadGroupSizeY, threadGroupSizeZ;

            computeShader.GetKernelThreadGroupSizes(kernel, out threadGroupSizeX, out threadGroupSizeY, out threadGroupSizeZ);

            var threadGroupsX = (vertexCount + (int)threadGroupSizeX - 1) / (int)threadGroupSizeX;
            computeShader.Dispatch(kernel, threadGroupsX, 1, 1);
            transfromed = new Vector4[vertexCount];
            OutVector4.GetData(transfromed);

            Release(InVector4); Release(OutVector4);
            return true;
        }

        public static Vector3[] ToVector3Array(this Transform[] transforms)
        {
            if (IsNullOrEmpty(transforms)) return null;
            var array = new Vector3[transforms.Length];
            for (int i = 0; i < transforms.Length; i++) if(transforms[i]) array[i] = transforms[i].position;
            return array;
        }

        public static void ConvertArray(this Vector3[] array, out Vector4[] converted, float w = 0)
        {
            converted = IsNullOrEmpty(array) ? new Vector4[0] : new Vector4[array.Length];
            for (int i = 0; i < converted.Length; i++)
            {
                converted[i].x = array[i].x; converted[i].y = array[i].y; converted[i].z = array[i].z;
                converted[i].w = w;
            }
        }

        public static void ConvertArray(this Vector4[] array, out Vector3[] converted)
        {
            converted = IsNullOrEmpty(array) ? new Vector3[0] : new Vector3[array.Length];
            for (int i = 0; i < converted.Length; i++)
            { converted[i].x = array[i].x; converted[i].y = array[i].y; converted[i].z = array[i].z; }
        }
    }
}
