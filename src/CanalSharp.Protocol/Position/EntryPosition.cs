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

namespace CanalSharp.Protocol.Position
{
    public class EntryPosition : TimePosition
    {
        public const int EVENTIDENTITY_SEGMENT = 3;
        public const char EVENTIDENTITY_SPLIT = (char)5;

        public bool Included => false;

        public string JournalName { get; set; }

        public long? Position { get; set; }

        /// <summary>
        /// To recode servce id of the entry position
        /// </summary>
        public long? ServerId { get; set; }

        public string Gtid = null;


        public EntryPosition() : base(null)
        {
        }

        public EntryPosition(long? timestamp) : this(null, null, timestamp)
        {
        }

        public EntryPosition(string journalName, long? position) : this(journalName, position, null)
        {
        }

        public EntryPosition(string journalName, long? position, long? timestamp) : base(timestamp)
        {
            JournalName = journalName;
            Position = position;
        }

        public EntryPosition(string journalName, long position, long timestamp, long? serverId) : this(journalName,
            position, timestamp)
        {
            ServerId = serverId;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = base.GetHashCode();
            result = prime * result + ((JournalName == null) ? 0 : JournalName.GetHashCode());
            result = prime * result + ((Position == null) ? 0 : Position.GetHashCode());
            // 手写 equals，自动生成时需注意
            result = prime * result + ((Timestamp == null) ? 0 : Timestamp.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!base.Equals(obj))
            {
                return false;
            }

            if (!(obj is EntryPosition))
            {
                return false;
            }

            var other = (EntryPosition)obj;
            if (JournalName == null)
            {
                if (other.JournalName != null)
                {
                    return false;
                }
            }
            else if (!JournalName.Equals(other.JournalName))
            {
                return false;
            }

            if (Position == null)
            {
                if (other.Position != null)
                {
                    return false;
                }
            }
            else if (!Position.Equals(other.Position))
            {
                return false;
            }

            // 手写equals，自动生成时需注意
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


        public int CompareTo(EntryPosition o)
        {
            var val = string.Compare(JournalName, o.JournalName, StringComparison.Ordinal);

            if (val != 0) return val;
            if (Position == null) return val;
            if (o.Position != null) return (int)(Position - o.Position);
            return val;
        }
    }
}