﻿using System.Collections.Generic;
using Runtime.Geometry;

namespace Runtime.DataStructure
{
    public interface IKDTreeHelper<Boundary,Element> : IBoundaryTreeHelper<Boundary,Element> where Boundary : struct
    {
        public bool KDTreeValidate(int iteration,Boundary _boundary, Element _element);
    }
    
    public class KDTree<Boundary, Element ,Helper> : ABoundaryTree<Boundary, Element> 
        where Helper : class ,IKDTreeHelper<Boundary,Element> , new () 
        where Boundary : struct
    {
        private static readonly Helper kHelper = new ();
        public KDTree(int _nodeCapcity, int _maxIteration) : base(_nodeCapcity, _maxIteration) { }
        public void Construct(IList<Element> _elements) => Construct(kHelper.CalculateBoundary(_elements),_elements);
        private static List<Element> kElementHelper = new ();
        protected override void Split(Node _parent, IList<Element> _elements, List<Node> _nodeList)
        {
            var boundary = _parent.boundary;
            var newNode1 = Node.Spawn(_parent.iteration + 1);
            var newNode2 = Node.Spawn(_parent.iteration + 1);
            foreach (var elementIndex in _parent.elementsIndex)
            {
                if (kHelper.KDTreeValidate(_parent.iteration,boundary,_elements[elementIndex]))
                    newNode1.elementsIndex.Add(elementIndex);
                else
                    newNode2.elementsIndex.Add(elementIndex);
            }

            newNode1.boundary = kHelper.CalculateBoundary(newNode1.FillList(_elements,kElementHelper));
            newNode2.boundary = kHelper.CalculateBoundary(newNode2.FillList(_elements,kElementHelper));
            _nodeList.Add(newNode1);
            _nodeList.Add(newNode2);
        }

    }
}