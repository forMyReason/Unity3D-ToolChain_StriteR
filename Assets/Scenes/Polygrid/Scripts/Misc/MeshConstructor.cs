using System;
using System.Collections.Generic;
using LinqExtension;
using Procedural;
using Procedural.Hexagon;
using TPool;
using TPoolStatic;
using UnityEngine;

namespace PolyGrid
{
    public class MeshConstructor :MonoBehaviour, IPolyGridControl
    {
        [ColorUsage(false,false)]public Color m_SelectionValid;
        [ColorUsage(false,false)]public Color m_SelectionInvalid;
        public AnimationCurve m_SelectionFade;
        private Timer m_SelectionFadeTimer;
        private Transform m_Selection;
        private Mesh m_SelectionMesh;
        private MeshRenderer m_SelectionRenderer;
        private MaterialPropertyBlock m_SelectionRendererBlock;
        private TObjectPoolClass<AreaRenderer> m_AreaMeshes;
        
        public void Tick(float _deltaTime)
        {
            if (!m_SelectionFadeTimer.m_Timing)
                return;
            
            m_SelectionFadeTimer.Tick(_deltaTime);
            m_SelectionRendererBlock.SetFloat(URender.kIDAlpha,m_SelectionFade.Evaluate( m_SelectionFadeTimer.m_TimeElapsed));
            m_SelectionRenderer.SetPropertyBlock(m_SelectionRendererBlock);
        }
        public void Init(Transform _transform)
        {
            m_Selection = _transform.Find("Selection");
            m_SelectionMesh = new Mesh {name="Selection",hideFlags = HideFlags.HideAndDontSave};
            m_SelectionMesh.MarkDynamic();
            m_Selection.GetComponent<MeshFilter>().sharedMesh = m_SelectionMesh;
            m_SelectionRenderer = m_Selection.GetComponent<MeshRenderer>();
            m_SelectionRendererBlock = new MaterialPropertyBlock();
            m_AreaMeshes = new TObjectPoolClass<AreaRenderer>(_transform.Find("AreaContainer/AreaMesh"));
      
            OnValidate();
        }
        private void OnValidate()
        {
            m_SelectionFadeTimer = new Timer(m_SelectionFade.length);
            m_SelectionFadeTimer.Replay();
        }
        public void ConstructCornerMarkup(PolyVertex _vertex,byte _height)
        {
            var color = _vertex.m_Invalid ? m_SelectionInvalid : m_SelectionValid;
            if (_height == 0)
            {
                _vertex.ConstructLocalMesh(m_SelectionMesh,EQuadGeometry.Half,EVoxelGeometry.Plane,true,false,true,color);
                m_Selection.position = _vertex.m_Coord.ToPosition();
            }
            else
            {
                _vertex.ConstructLocalMesh(m_SelectionMesh,EQuadGeometry.Half,EVoxelGeometry.VoxelFull,true,false,true,color);
                m_Selection.position = DPolyGrid.GetCornerHeight(_height)+ _vertex.m_Coord.ToPosition();
            }
            m_SelectionFadeTimer.Replay();
        }
        public void ConstructArea(PolyArea _area)
        {
            Debug.LogWarning($"Area Mesh Populate {_area.m_Identity.coord}");
            m_AreaMeshes.Spawn().GenerateMesh (_area);
        }

        public void Clear()
        {
            m_Selection.SetActive(false);
            m_SelectionMesh.Clear();
            m_AreaMeshes.Clear();
        }

        private class AreaRenderer : ITransform,IPoolCallback<int>
        {
            public Transform iTransform { get; }
            public MeshFilter m_MeshFilter { get; }

            public AreaRenderer(Transform _transform)
            {
                iTransform = _transform;
                m_MeshFilter = _transform.GetComponent<MeshFilter>();
            }
            public void OnPoolInit(Action<int> _DoRecycle) { }

            public void GenerateMesh(PolyArea _area)
            {
                int quadCount = _area.m_Quads.Count;
                List<Vector3> vertices = TSPoolList<Vector3>.Spawn();
                List<Vector3> normals = TSPoolList<Vector3>.Spawn();
                List<Vector2> uvs = TSPoolList<Vector2>.Spawn(); 
                List<int> indices = TSPoolList<int>.Spawn();
                var center = _area.m_Identity.centerCS.ToCoord();
                foreach (var quad in _area.m_Quads)
                {
                    int startIndex = vertices.Count;
                    foreach (var tuple in quad.m_CoordQuad.LoopIndex())
                    {
                        var coord = tuple.value;
                        var index = tuple.index;
                        var positionOS = (coord-center).ToPosition();
                        vertices.Add(positionOS);
                        normals.Add(Vector3.up);
                        uvs.Add(URender.IndexToQuadUV(index));
                    }

                    indices.Add(startIndex);
                    indices.Add(startIndex+1);
                    indices.Add(startIndex+2);
                    indices.Add(startIndex+3);
                }
            
                var areaMesh = new Mesh {hideFlags = HideFlags.HideAndDontSave, name = _area.m_Identity.coord.ToString()};
                areaMesh.SetVertices(vertices);
                areaMesh.SetNormals(normals);
                areaMesh.SetIndices(indices,MeshTopology.Quads,0);
                areaMesh.SetUVs(0,uvs);
                areaMesh.Optimize();
                areaMesh.UploadMeshData(true);
                m_MeshFilter.sharedMesh = areaMesh;
                iTransform.SetPositionAndRotation(_area.m_Identity.centerCS.ToPosition(),Quaternion.identity);
                iTransform.localScale=Vector3.one;
                
                TSPoolList<Vector3>.Recycle(vertices);
                TSPoolList<Vector3>.Recycle(normals);
                TSPoolList<Vector2>.Recycle(uvs);
                TSPoolList<int>.Recycle(indices);
            }
            public void OnPoolSpawn(int identity)
            {
                
            }

            public void OnPoolRecycle()
            {
                if (m_MeshFilter.sharedMesh == null)
                    return;
                GameObject.DestroyImmediate(m_MeshFilter.mesh);
                m_MeshFilter.sharedMesh = null;
            }
        }
    }
}