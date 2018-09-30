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

namespace CanalSharp.Protocol.Position
{
    public class LogPosition : Position
    {
        public LogIdentity Identity { get; }

        private EntryPosition Postion { get; }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            result = prime * result + ((Identity == null) ? 0 : Identity.GetHashCode());
            result = prime * result + ((Postion == null) ? 0 : Postion.GetHashCode());
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

            if (!(obj is LogPosition))
            {
                return false;
            }

            var other = (LogPosition) obj;
            if (Identity == null)
            {
                if (other.Identity != null)
                {
                    return false;
                }
            }
            else if (!Identity.Equals(other.Identity))
            {
                return false;
            }

            if (Postion == null)
            {
                if (other.Postion != null)
                {
                    return false;
                }
            }
            else if (!Postion.Equals(other.Postion))
            {
                return false;
            }

            return true;
        }
    }
}