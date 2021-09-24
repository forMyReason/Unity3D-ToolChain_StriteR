using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural
{
    public static class UProcedural
    {
        static Matrix4x4 TransformMatrix = Matrix4x4.identity;
        static Matrix4x4 InvTransformMatrix = Matrix4x4.identity;

        public static void InitMatrix(Matrix4x4 _transformMatrix, float _scale)
        {
            TransformMatrix = _transformMatrix * Matrix4x4.Scale(_scale * Vector3.one);
            InvTransformMatrix = _transformMatrix * Matrix4x4.Scale(Vector3.one / _scale);
        }
        
        public static Vector3 ToPosition(this Coord _pixel)
        {
            return TransformMatrix * new Vector3(_pixel.x, 0, _pixel.y);
        }

        public static Coord ToCoord(this Vector3 _world)
        {
            return new Coord(_world.x,  _world.z);
        }
        
        public static Coord Lerp(this Coord _src, Coord _dst, float _value)
        {
            return _src + (_dst-_src) * _value;
        }
    }
}