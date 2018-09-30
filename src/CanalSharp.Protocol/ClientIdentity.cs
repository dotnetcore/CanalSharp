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

namespace CanalSharp.Protocol
{
    [Serializable]
    public class ClientIdentity
    {
        public string Destination { get; set; }

        public short ClientId { get; set; }

        public string Filter { get; set; }

        public ClientIdentity()
        {
        }

        public ClientIdentity(string destination, short clientId)
        {
            Destination = destination;
            ClientId = clientId;
        }

        public ClientIdentity(string destination, short clientId, string filter)
        {
            Destination = destination;
            ClientId = clientId;
            Filter = filter;
        }

        public bool HasFilter()
        {
            return Filter != null && string.IsNullOrEmpty(Filter);
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            result = prime * result + ClientId;
            result = prime * result + ((Destination == null) ? 0 : Destination.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            if (!(obj is ClientIdentity))
            {
                return false;
            }

            var other = (ClientIdentity) obj;
            if (ClientId != other.ClientId)
            {
                return false;
            }

            if (Destination == null)
            {
                if (other.Destination != null)
                {
                    return false;
                }
            }
            else if (!Destination.Equals(other.Destination))
            {
                return false;
            }

            return true;
        }
    }
}