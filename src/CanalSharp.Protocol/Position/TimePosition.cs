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
    public class TimePosition : Position
    {
        protected long? Timestamp { get; }

        public TimePosition(long? timestamp)
        {
            Timestamp = timestamp;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
            result = prime * result + ((Timestamp == null) ? 0 : Timestamp.GetHashCode());
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

            if (!(obj is TimePosition))
            {
                return false;
            }

            var other = (TimePosition) obj;
            if (Timestamp == null)
            {
                if (other.Timestamp != null)
                {
                    return false;
                }
            }
            else if (!Timestamp.Equals(other.Timestamp))
            {
                return false;
            }

            return true;
        }
    }
}