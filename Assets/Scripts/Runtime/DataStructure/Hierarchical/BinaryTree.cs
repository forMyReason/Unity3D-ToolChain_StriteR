﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Extensions;
using UnityEngine;

namespace Runtime.DataStructure
{
    public interface ITreeNode<TreeNode,Element> where TreeNode : ITreeNode<TreeNode,Element>
    {
        public int iteration { get; set; }
        public IList<Element> elements { get; set; }
        public IList<TreeNode> children { get; set; }
        public bool IsParent => children != null;
        public void SetParent(IList<TreeNode> _childrens)
        {
            children = _childrens;
            elements = null;
        }
    }
    
    public class BinaryTree<Key, Element> : IEnumerable<Element> where Key : ITreeNode<Key,Element>
    {
        private Key m_Root;
        private List<Key> m_TreeNodes = new List<Key>();
        protected virtual bool Optimize => true;
        protected void Construct_Internal(Key _root, int _maxIteration, int _volumeCapacity,Func<Key,IEnumerable<Key>> _split)
        {
            m_TreeNodes.Clear();
            m_Root = _root;
            m_TreeNodes.Add(m_Root);

            var rootChildCount = _root.elements.Count;
            var finalChildCount = rootChildCount;
            var constructing = true;
            while (constructing)
            {
                var split = false;
                foreach (var treeNode in m_TreeNodes)
                {
                    if(treeNode.IsParent)
                        continue;
                
                    if (treeNode.iteration >= _maxIteration)
                        continue;

                    if (treeNode.elements.Count <= _volumeCapacity)
                        continue;

                    var childrenList = new List<Key>();
                    finalChildCount -= treeNode.elements.Count;
                    foreach (var childElements in _split(treeNode))
                    {
                        if(Optimize && childElements.elements.Count == 0)
                            continue;
                        
                        m_TreeNodes.Add(childElements);
                        childrenList.Add(childElements);
                        finalChildCount += childElements.elements.Count;
                    }
                
                    treeNode.SetParent(childrenList);
                    split = true;
                    break;
                }

                constructing = split;
            }
            
            if(finalChildCount != rootChildCount)
                Debug.LogError($"Tree construct failed, finalChildCount != childCount {finalChildCount} != {rootChildCount}");
        }
        
        public IEnumerable<Key> GetLeafs() => from node in m_TreeNodes where !node.IsParent select node;
        public IEnumerable<Key> GetParents() => from node in m_TreeNodes where node.IsParent select node;
        
        private static readonly Stack<Key> m_QueryNodes = new Stack<Key>();
        public IEnumerable<Element> Query(Predicate<Key> _queryFunction)
        {
            m_QueryNodes.Clear();
            m_QueryNodes.Push(m_Root);
            while (m_QueryNodes.Count > 0)
            {
                var currentTreeNode = m_QueryNodes.Pop();
                if(!_queryFunction(currentTreeNode))
                    continue;
                
                if (currentTreeNode.IsParent)
                    m_QueryNodes.PushRange(currentTreeNode.children);

                if (currentTreeNode.elements == null) continue;
                
                foreach (var element in currentTreeNode.elements)
                    yield return element;
            }
        }
        public IEnumerator<Element> GetEnumerator()
        {
            foreach (var volume in m_TreeNodes)
            {
                if(volume.IsParent)
                    continue;
                foreach (var element in volume.elements)
                    yield return element;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    
}