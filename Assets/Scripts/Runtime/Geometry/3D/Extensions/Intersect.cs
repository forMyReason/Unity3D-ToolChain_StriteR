using Unity.Mathematics;

namespace Runtime.Geometry.Extension
{
    using static math;

    public static partial class UGeometry
    {
        #region AABB

        public static bool Intersect(GPlane _plane, GBox _box)
        {
            var c = _box.center;
            var e = abs(_box.extent);

            var n = _plane.normal;
            var d = _plane.distance;
            var r = dot(e, abs(n));
            var s = dot(n, c) - d;
            return s <= r;
        }

        public static bool Intersect(GBox _src, GBox _dst)
        {
            return _src.min.x <= _dst.max.x && _src.max.x >= _dst.min.x &&
                   _src.min.y <= _dst.max.y && _src.max.y >= _dst.min.y &&
                   _src.min.z <= _dst.max.z && _src.max.z >= _dst.min.z;
        }

        public static bool Intersect(GSphere _sphere, GBox _box) => Distance(_sphere, _box) <= 0;
        
        public static bool Intersect(this GLine _line,GQuad _quad,out float _distance,bool _directed = false)
        {
            _quad.GetTriangles(out var _triangle1,out var _triangle2);
            var ray = _line.ToRay();
            if (ray.Intersect(_triangle1,out _distance))
                return true;

            return ray.Intersect(_triangle2,out _distance);
        }
        
        public static bool Intersect(GFrustumPlanes _frustumPlanes, GBox _bounding)
        {
            for (var i = 0; i < _frustumPlanes.Length; i++)
                if (!Intersect(_frustumPlanes[i], _bounding))
                    return false;

            return true;
        }

        public static bool Intersect(GFrustumPlanes _frustumPlanes, GBox _bounding, GFrustumPoints _frustumPoints)
        {
            if (!Intersect(_frustumPlanes, _bounding))
                return false;

            if (!Intersect(_frustumPoints.bounding, _bounding))
            // if (!Intersect(_frustumPoints,_bounding)) //More expensive
                return false;

            return true;
        }

        public static bool Intersect(GFrustumPoints _frustumPoints, GBox _box)
        {
            var outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].x > _box.max.x;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].x < _box.min.x;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].y > _box.max.y;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].y < _box.min.y;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].z > _box.max.z;
            if (outside) return false;
            outside = true;
            for (var i = 0; i < 8; i++) outside &= _frustumPoints[i].z < _box.min.z;
            if (outside) return false;
            return true;
        }

        #endregion
    }
}