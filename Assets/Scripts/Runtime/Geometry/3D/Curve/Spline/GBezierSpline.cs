using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry.Curves.Spline
{

    public enum EBSplineMode
    {
        OpenUniformClamped,
        OpenUniform,
        Uniform,
        NonUniform,
    }
    
    [Serializable]
    public struct GBezierSpline:ISpline,ISerializationCallbackReceiver
    {
        public float3[] coordinates;
        [Clamp(ISpline.kMinDegree)] public int k;

        public EBSplineMode mode;
        public float[] knotVectors;

        public GBezierSpline(IEnumerable<float3> _positions, int _k = 3, EBSplineMode _mode = EBSplineMode.OpenUniformClamped)
        {
            coordinates = _positions.ToArray();
            k = _k;
            mode = _mode;
            knotVectors = null;
            Ctor();
        }
        
        public static readonly GBezierSpline kDefault = new GBezierSpline() {
            coordinates = new float3[]{new float3(-1,0,-1),new float3(0,0,1),new float3(1,0,-1)},
            k = 3,
            mode = EBSplineMode.OpenUniformClamped,
        }.Ctor();

        public IEnumerable<float3> Coordinates => coordinates;
        public float3 Evaluate(float _value)
        {
            var n = coordinates.Length -1;
            var t = math.lerp(knotVectors[k-1] ,knotVectors[n+1] ,_value);
            float3 result = 0;
            for (int i = 1; i <= n + 1; i++)
            {
                var nik = Basis(i - 1 ,k,t,knotVectors);
                result += nik * coordinates[i - 1];
            }
            return result;
        }

        static float Divide(float _numerator,float _denominator)
        {
            if (math.abs(_denominator) < 0.01f)
                return 0;
            return _numerator / _denominator;
        }
        
        static float Basis(int _i,int _k,float _t,float[] _knots)
        {
            if (_k == 1)
                return (_knots[_i] <= _t && _t < _knots[_i + 1])?1:0;

            float coefficient1 = Divide(
                _t - _knots[_i],
                _knots[_i+_k - 1] - _knots[_i]);
            float coefficient2 = 
                Divide(_knots[_i+_k] - _t,
                _knots[_i+_k] - _knots[_i + 1]);
            
            var nextK = _k - 1;
            return coefficient1 * Basis(_i,nextK, _t,_knots) + coefficient2 * Basis(_i + 1,nextK, _t,_knots) ;
        }

        public GBezierSpline Ctor()
        {
            var n = coordinates.Length - 1;
            k = math.min(n + 1, k);
            int knotVectorLength = coordinates.Length + k + 1;
            
            switch (mode)
            {
                default:
                {
                    if (knotVectors == null || knotVectors.Length != knotVectorLength)
                        knotVectors = new float[knotVectorLength];
                }
                    break;
                case EBSplineMode.Uniform:
                {
                    knotVectors = new float[knotVectorLength];
                    for (int i = 1; i <= knotVectors.Length; i++)
                        knotVectors[i - 1] = i;
                }
                    break;
                case EBSplineMode.OpenUniform:
                {
                    float constant = 0;
                    knotVectors = new float[knotVectorLength];
                    
                    for (int i = 1; i <= knotVectors.Length; i++)
                    {
                        float value = 0;
                        if (i < k)
                            value = constant;
                        else if (i <= n+2 )
                            value = constant++;
                        else
                            value = constant;
                        knotVectors[i - 1] = value;
                    }
                }
                    break;
                case EBSplineMode.OpenUniformClamped:
                {
                    float constant = 0;
                    knotVectors = new float[knotVectorLength];
                    
                    for (int i = 1; i <= knotVectors.Length; i++)
                    {
                        float value = 0;
                        if (i < k)
                            value = constant;
                        else if (i <= n + 1)
                            value = constant++;
                        else if (i <= n + k)
                            value = constant;
                        else
                            value = ++constant;
                        knotVectors[i - 1] = value;
                    }
                }
                    break;
            }
            return this;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() => Ctor();
        public float3 Origin => coordinates[0];
        public void DrawGizmos() => this.DrawGizmos(64);
    }
}