using System.Collections.Generic;
using authapi.Models;
using authapi.Services;

namespace authapi
{
    class MemCache : IMemCache
    {
        internal class Node
        {
            public string username;
            public Jwt token;
        }
        private LinkedList<Node> _deque;
        private readonly uint _cacheSize;

        IDictionary<string, LinkedListNode<Node>> _cache;

        public MemCache(uint cacheSize)
        {
            this._cacheSize = cacheSize;
            _deque = new LinkedList<Node>();
            _cache = new Dictionary<string, LinkedListNode<Node>>();
        }

        public void Put(string userName, Jwt token)
        {
            LinkedListNode<Node> node;
            if (this._cache.ContainsKey(userName))
            {
                node = this._cache[userName];
                this._deque.Remove(node);
                node.Value.token = token;
                node = this._deque.AddFirst(node.Value);
                this._cache[userName] = node;
            }
            else
            {
                if ((uint)this._deque.Count >= this._cacheSize)
                {
                    // remove the last node
                    var lastNode = this._deque.Last;
                    this._cache.Remove(lastNode.Value.username);
                    this._deque.RemoveLast();
                }
                node = this._deque.AddFirst(new Node() { username = userName, token = token });
                this._cache.Add(userName, node);
            }
        }

        public Jwt Get(string userName)
        {
            if (this._cache.ContainsKey(userName))
            {
                var node = this._cache[userName];
                this._deque.Remove(node);
                node = this._deque.AddFirst(node.Value);
                this._cache[userName] = node;
                return node.Value.token;
            }
            return null;
        }
    }
}