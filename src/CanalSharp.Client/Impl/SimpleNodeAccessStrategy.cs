// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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