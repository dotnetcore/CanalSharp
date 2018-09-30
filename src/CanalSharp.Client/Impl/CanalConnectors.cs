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

namespace CanalSharp.Client.Impl
{
    public class CanalConnectors
    {
        /// <summary>
        /// 创建单链接的客户端链接
        /// </summary>
        /// <param name="address"></param>
        /// <param name="destination"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static ICanalConnector NewSingleConnector(string address, int port, string destination, string username,
            string password)
        {
            var canalConnector = new SimpleCanalConnector(address, port, username, password, destination)
            {
                SoTimeout = 60 * 1000,
                IdleTimeout = 60 * 60 * 1000
            };
            return canalConnector;
        }
    }
}