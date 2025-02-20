using System.Collections.Generic;
using System.Linq.Extensions;
using UnityEngine;

public class SpatialHashMap<T,Y,U> where T:struct where Y:class,IGraph<T>,IGraphMapping<T> where U:ITransform
{
    private Y m_Query;
    private List<U> m_Elements = new List<U>();
    private Dictionary<T,  List<U>> m_Grids = new Dictionary<T, List<U>>();

#region Lifecycle
    public SpatialHashMap(Y _query)
    {
        m_Query = _query;
    }

    public void Register(U _element)=>m_Elements.Add(_element);
    public void SignOut(U _element)=> m_Elements.Remove(_element);
    
    public void Reset()
    {
        m_Elements.Clear();
        m_Grids.Clear();
    }

    public void Dispose()
    {
        m_Query = null;
        m_Elements = null;
        m_Grids = null;
    }
#endregion
    
    public IEnumerable<U> Query(Vector3 _position,float _radius)
    {
        var sqrRadius = _radius * _radius;
        m_Query.PositionToNode(_position,out var srcNode);
        if (!m_Grids.ContainsKey(srcNode))
            yield break;
        var elements = m_Grids[srcNode];
        var elementsCount = elements.Count;
        for (int j = 0; j < elementsCount; j++)
        {
            var element = elements[j];
            if((element.position-_position).sqrMagnitude <= sqrRadius)
                yield return element;
        }
    }
    public IEnumerable<U> QueryRange(Vector3 _position,float _radius)
    {
        var sqrRadius = _radius * _radius;
        m_Query.PositionToNode(_position,out var srcNode);
        foreach (var node in m_Query.GetAdjacentNodes(srcNode).Extend(srcNode))
        {
            if (!m_Grids.ContainsKey(node))
                continue;
            var elements = m_Grids[node];
            var elementsCount = elements.Count;
            for (int j = 0; j < elementsCount; j++)
            {
                var element = elements[j];
                if((element.position-_position).sqrMagnitude <= sqrRadius)
                    yield return element;
            }
        }
    }

    public void Tick(float _deltaTime)
    {
        m_Grids.Clear();
        foreach (var element in m_Elements)
        {
           m_Query.PositionToNode(element.position,out var node);
            if (!m_Grids.ContainsKey(node))
                m_Grids.Add(node,new List<U>());
            m_Grids[node].Add(element);
        }
    }
}