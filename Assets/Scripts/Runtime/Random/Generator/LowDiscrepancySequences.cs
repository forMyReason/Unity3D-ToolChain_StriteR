using System;
using System.Collections.Generic;
using System.Linq;
using Procedural.Tile;
using Runtime.DataStructure;
using Runtime.Random;
using Unity.Mathematics;
using static UBitwise;
using static kmath;
using static Unity.Mathematics.math;

public static class ULowDiscrepancySequences
{
    static float RadicalInverseOptimized(uint _n,uint _dimension) =>_dimension == 0 ? RadicalInverse2(_n) : RadicalInverse(_n, kPrimes128[_dimension]);
    public static float Halton(uint _index, uint _dimension) => RadicalInverseOptimized(_index,_dimension);
    public static float Hammersley(uint _index,uint _dimension,uint _numSamples)=>_dimension==0?(_index/(float)_numSamples):RadicalInverseOptimized(_index,_dimension-1);
    public static float2 Hammersley2D(uint _index, uint _size)=>new float2(Hammersley(_index,0,_size),Hammersley(_index,1,_size));
    public static float2 Halton2D(uint _index) => new float2( Halton(_index,0),Halton(_index,kPrimes128[1]));

    public static float2[] Grid2D(int _width,int _height)
    {
        float2[] grid = new float2[_width * _height];
        float2 uvOffset = new float2(1f/(_width ),1f / (_height));

        float2 start = -.5f + uvOffset * .5f;
        for(int y = 0; y < _height; y++)
        for(int x = 0; x < _width; x++)
            grid[y * _width + x] = start +  new float2(x, y) * uvOffset;
        return grid;
    }

    public static float2[] Stratified2D(int _width, int _height, bool _jitter = false,float _offset = -.5f, IRandomGenerator _random = null)
    {
        float2 uvOffset = 1f / new float2(_width,_height);
        float2[] grid = new float2[_width*_height];
        for(int x = 0; x < _width; x++)
        for(int y = 0; y < _height; y++)
        {
            var jx = _jitter ? URandom.Random01(_random) : .5f;
            var jy = _jitter ? URandom.Random01(_random) : .5f;
            grid[y * _width + x] = new float2(x+jx,y+jy)*uvOffset + _offset;
        } 
        UShuffle.LatinHypercube(grid,grid.Length,_width,_random);
        return grid;
    }

    
    struct SobelMatrix
    {
        public uint a;
        public uint[] m;

        public SobelMatrix(uint _a, uint[] _m)
        {
            a = _a;
            m = _m;
        }
    }
    static readonly SobelMatrix[] kSobelMatrices = {
        new (0,new uint[]{0,0}),new (0,new uint[]{0,1}),          new (1,new uint[]{0,1,3}),       new (1,new uint[]{0,1,3,1}),  
        new (2,new uint[]{0,1,1,1}),new (1,new uint[]{0,1,1,3,3}),     new (4,new uint[]{0,1,3,5,13}),  new (2,new uint[]{0,1,1,5,5,17}),
        new (4,new uint[]{0,1,1,5,5,5}),  new (4,new uint[]{0,1,1,7,11,19}), new (7,new uint[]{0,1,1,5,1,1}), new (11,new uint[]{0,1,1,1,3,11}),
        new (13,new uint[]{0,1,3,5,5,31}), new (14,new uint[]{0,1,3,3,9,7,49}), new (1,new uint[]{0,1,1,1,15,21,21}), new (13,new uint[]{0,1,3,1,13,27,49}),
    };

    private static readonly float kSobolMaxValue = math.pow(2, 32);

    public static float[] Sobel(uint _size,float _offset)
    {
        var N = _size;
        var points = new float[N];
        var C = new uint[N];
        for (int i = 0; i < N; i++)
        {
            C[i] = 1;
            var value = i;
            while ((value & 1) > 0)
            {
                value >>= 1;
                C[i]++;
            }
        }

        var L = (uint) math.ceil(math.log(N) / math.log(2.0f));
        
        var V = new uint[L + 1];
        for (int i = 1; i <= L; i++) V[i] = 1u << (32 - i);
        
        var X = new uint[N];
        X[0] = 0;
        for (uint i = 1u; i < N; i++)
        {
            X[i] = X[i - 1] ^ V[C[i - 1]];
            points[i] = X[i] / kSobolMaxValue + _offset;
        }
        
        return points;
    }
    
    public static float2[] Sobol2D(uint _size,float _offset = -.5f)
    {
        var N = _size;
        float2[] points = new float2[N];
        var C = new uint[N];
        for (int i = 0; i < N; i++)
        {
            C[i] = 1;
            var value = i;
            while ((value & 1) > 0)
            {
                value >>= 1;
                C[i]++;
            }
        }

        var L = (uint) math.ceil(math.log(N) / math.log(2.0f));
        var V = new uint[L + 1];
        for (int i = 1; i <= L; i++) V[i] = 1u << (32 - i);

        var X = new uint[N];
        X[0] = 0;
        for (uint i = 1u; i < N; i++)
        {
            X[i] = X[i - 1] ^ V[C[i - 1]];
            points[i].x = X[i] / kSobolMaxValue + _offset;
        }

        var matrix = kSobelMatrices[1];
        var a = matrix.a;
        var m = matrix.m;
        var s = m.Length - 1;
        if (L <= s) {
            for (int i=1;i<=L;i++) V[i] = m[i] << (32-i); 
        }
        else {
            for (int i=1;i<=s;i++) V[i] = m[i] << (32-i); 
            for (int i = s+1; i <= L; i++)
            {
                V[i] = V[i-s] ^ (V[i-s] >> s); 
                for (int k=1;k<=s-1;k++) 
                    V[i] ^= (((a >> (s-1-k)) & 1) * V[i-k]); 
            }
        }

        for (uint i = 1; i < N; i++)
        {
            X[i] = X[i-1] ^ V[C[i-1]];
            points[i].y = X[i] /kSobolMaxValue + _offset;
        }
        
        return points;
    }

    public static float2[] PoissonDisk2D(int _maxCount,int _k = 30,IRandomGenerator _seed = null,Func<float2,float> _getRadiusNormalized = null)
    {
        var count = sqrt(_maxCount) + .5f;
        
        var gridSize = new float2(count,count);
        var r = 1;
        
        var k = _k;

        var checkList = new List<float2>();
        var samplePoints = new MultiHashMap<int2,float2>();
        
        var initialPoint = new float2(URandom.Random01(_seed) , URandom.Random01(_seed) ) * gridSize;
        
        checkList.Add(initialPoint);
        samplePoints.Add((int2)floor(initialPoint), initialPoint);
        
        while (checkList.Count > 0)     //Optimize with spatial hashmap
        {
            var activeIndex = URandom.RandomInt(checkList.Count - 1,_seed);
            var activePoint = checkList[activeIndex];

            var found = false;
            for (var i = 0; i < k; i++)
            {
                var angle = URandom.Random01(_seed)* PI * 2;
                var direction = new float2(cos(angle), sin(angle));
                var radius = _getRadiusNormalized?.Invoke(activePoint/gridSize) ?? r;
                var distance = URandom.Random01(_seed) * (2 * radius - radius) + radius;
                var newPoint = activePoint + direction * distance;

                if (newPoint.x < 0 || newPoint.x >= gridSize.x || newPoint.y < 0 || newPoint.y >= gridSize.y)
                    continue;

                var gridPosition = (int2)floor(newPoint);
                if (!samplePoints.GetValues(UTile.GetAxisRange(gridPosition, 2)
                                 .Select(p => new int2(p.x, p.y)))
                                 .All(p => (newPoint - p).sqrmagnitude() > radius * radius))
                    continue;
                found = true;
                checkList.Add(newPoint);
                samplePoints.Add(gridPosition, newPoint);
                break;
            }

            if (!found)
                checkList.RemoveAt(activeIndex);
        }
        
        return samplePoints.Values.Select(p=>p/gridSize - .5f).ToArray();
    }
}