using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Geometry;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Examples.Rendering.Misc
{
    public enum ESDFShape
    {
        Capsule = 0,
        Sphere = 1,
        Box = 2,
        Plane = 3,
    }

    public enum ESDFMode
    {
        Union,
        SMin,
    }

    [Serializable]
    public struct SDFElement
    {
        public ESDFShape shape;
        public ESDFMode mode;
        public float3 position;
        [ColorUsage(false)]public Color albedo;

        [MFoldout(nameof(shape), ESDFShape.Capsule)] public float2 capsuleShape;
        [MFoldout(nameof(shape),ESDFShape.Sphere)] public float radius;
        [MFoldout(nameof(shape),ESDFShape.Box)] public float3 extents;
        [MFoldout(nameof(shape), ESDFShape.Plane)] public float3 normal;
        public IShapeGizmos FormatCPU(Matrix4x4 _localToWorldMatrix)
        {
            var origin = (float3)_localToWorldMatrix.GetPosition() + position;
            switch (shape)
            {
                case ESDFShape.Box: return new GBox(origin, extents);
                case ESDFShape.Sphere: return new GSphere(origin, radius);
                case ESDFShape.Capsule: return new GCapsule(origin, capsuleShape.x, kfloat3.up, capsuleShape.y);
                case ESDFShape.Plane: return new GPlane(normal, origin);
            }
            throw new InvalidEnumArgumentException();
        }

        public void FormatGPU(Matrix4x4 _localToWorldMatrix, out Vector4 _parameter1,out Vector4 _parameter2,out Vector4 _color)
        {
            var origin = _localToWorldMatrix.GetPosition();
            _parameter1= position.to4((int)shape);
            switch (shape)
            {
                default: throw new InvalidEnumArgumentException();
                case ESDFShape.Box: _parameter2 = extents.to4(); break;
                case ESDFShape.Sphere: _parameter2 = (float4)radius; break;
                case ESDFShape.Capsule: _parameter2 = capsuleShape.to4(); break;
                case ESDFShape.Plane: _parameter2 = normal.to4(); break;
            }
            _parameter1 += origin.ToVector4();
            _color = albedo.ToVector();
        }
    }
    
    [RequireComponent(typeof(MeshRenderer))]
    public class SDF_Combination : MonoBehaviour
    {
        public SDFElement[] m_Elements;

        private List<Vector4> parameters1 = new List<Vector4>();
        private List<Vector4> parameters2 = new List<Vector4>();
        private List<Vector4> colors = new List<Vector4>();
        private void OnValidate()
        {
            MaterialPropertyBlock kBlock = new MaterialPropertyBlock();

            parameters1.Clear();
            parameters2.Clear();
            colors.Clear();
            foreach (var element in m_Elements)
            {
                element.FormatGPU(transform.localToWorldMatrix,out var param1,out var param2,out var color);
                parameters1.Add(param1);
                parameters2.Add(param2);
                colors.Add(color);
            }
            
            kBlock.SetInt("_SDFCount",parameters1.Count);
            kBlock.SetVectorArray("_Parameters1",parameters1);
            kBlock.SetVectorArray("_Parameters2",parameters2);
            kBlock.SetVectorArray("_Colors",colors);
            GetComponent<MeshRenderer>().SetPropertyBlock(kBlock);
        }

        public bool m_DrawGizmos;
        private void OnDrawGizmos()
        {
            OnValidate();
            if (!m_DrawGizmos) return;
            foreach (var element in m_Elements)
                element.FormatCPU(transform.localToWorldMatrix).DrawGizmos();
        }
    }
}