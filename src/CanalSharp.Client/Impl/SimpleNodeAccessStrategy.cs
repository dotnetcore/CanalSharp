using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CanalSharp.Client.Impl
{
    /// <summary>
    /// 简单版本的 node 访问实现
    /// </summary>
    public class SimpleNodeAccessStrategy : ICanalNodeAccessStrategy
    {
        private readonly List<SocketAddress> _nodes = new List<SocketAddress>();
        private int _index;

        public SimpleNodeAccessStrategy(List<SocketAddress> nodes)
        {
            if (nodes == null || !nodes.Any())
            {
                throw new ArgumentException("at least 1 node required.", nameof(nodes));
            }

            _nodes.AddRange(nodes);
        }

        public SocketAddress NextNode()
        {
            try
            {
                return _nodes[_index];
            }
            finally
            {
                _index = (_index + 1) % _nodes.Count();
            }
        }

        public SocketAddress CurrentNode()
        {
            return _nodes[_index];
        }
    }
}