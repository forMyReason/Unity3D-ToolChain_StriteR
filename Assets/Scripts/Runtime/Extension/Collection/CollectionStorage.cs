﻿using System.Collections.Generic;

namespace System.Linq.Extensions
{
    public static class UList
    {
        static class Storage<T>
        {
            public static Dictionary<int,List<T>> m_Dictionary = new( ){ {-1,new List<T>()} };
        }
        public static List<T> Empty<T>(int _uniqueId = -1)
        {
            if (Storage<T>.m_Dictionary.TryGetValue(_uniqueId, out var existList))
            {
                existList.Clear();
                return existList; 
            }
            
            var list = new List<T>();
            Storage<T>.m_Dictionary.Add(_uniqueId, list);
            return list;
        }
        public static List<T> Traversal<T>(IEnumerable<T> _src)
        {
            var list = Empty<T>();
            list.Clear();
            list.AddRange(_src);
            return list;
        }
    }

    public static class UQueue
    {
        static class Storage<T> { public static Queue<T> m_Queue = new Queue<T>(); }
        
        public static Queue<T> Empty<T>()
        {
            var queue = Storage<T>.m_Queue;
            queue.Clear();
            return queue;
        }
        public static Queue<T> Traversal<T>(IEnumerable<T> _src)
        {
            var queue = Storage<T>.m_Queue;
            queue.Clear();
            queue.EnqueueRange(_src);
            return queue;
        }
    }

    public static class UStack
    {
        static class Storage<T> { public static Stack<T> m_Stack = new Stack<T>(); }

        public static Stack<T> Empty<T>()
        {
            var stack = Storage<T>.m_Stack;
            stack.Clear();
            return stack;
        }

        public static Stack<T> Traversal<T>(IEnumerable<T> _src)
        {
            var stack = Storage<T>.m_Stack;
            stack.Clear();
            stack.PushRange(_src);
            return stack;
        }
    }

    public static class UHashSet
    {
        static class Storage<T> { public static HashSet<T> m_HashSet = new HashSet<T>(); }

        public static HashSet<T> Empty<T>()
        {
            var hashSet = Storage<T>.m_HashSet;
            hashSet.Clear();
            return hashSet;
        }

        public static HashSet<T> Traversal<T>(IEnumerable<T> _src)
        {
            var hashSet = Storage<T>.m_HashSet;
            hashSet.Clear();
            hashSet.UnionWith(_src);
            return hashSet;
        }
    }
}