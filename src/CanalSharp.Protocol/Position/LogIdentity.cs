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

using System.Net;

namespace CanalSharp.Protocol.Position
{
    public class LogIdentity : Position
    {
        // 链接服务器的地址
        public SocketAddress SourceAddress { get; }

        // 对应的slaveId
        public long? SlaveId { get; }

        public LogIdentity()
        {
        }

        public LogIdentity(SocketAddress sourceAddress, long slaveId)
        {
            SourceAddress = sourceAddress;
            SlaveId = slaveId;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            result = prime * result + ((SlaveId == null) ? 0 : SlaveId.GetHashCode());
            result = prime * result + ((SourceAddress == null) ? 0 : SourceAddress.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (this != obj) return false;
            var other = (LogIdentity) obj;
            if (SlaveId == null)
            {
                if (other.SlaveId != null) return false;
            }
            else if (SlaveId != (other.SlaveId)) return false;

            if (SourceAddress == null)
            {
                if (other.SourceAddress != null) return false;
            }
            else if (!SourceAddress.Equals(other.SourceAddress)) return false;

            return true;
        }
    }
}