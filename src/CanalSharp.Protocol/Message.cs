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
using Com.Alibaba.Otter.Canal.Protocol;
using Google.Protobuf;

namespace CanalSharp.Protocol
{
    [Serializable]
    public class Message
    {
        public long Id { get; set; }

        public List<Entry> Entries { get; set; }

        public bool Raw { get; set; }

        public List<ByteString> RawEntries { get; set; } = new List<ByteString>();

        public Message(long id, List<Entry> entries)
        {
            Id = id;
            Entries = entries ?? new List<Entry>();
            Raw = false;
        }

        public Message(long id)
        {
            Id = id;
            Entries = new List<Entry>();
        }

        public Message(long id, bool raw, object entries)
        {
            Id = id;
            if (raw)
            {
                RawEntries = (List<ByteString>) (entries ?? new List<ByteString>());
            }
            else
            {
                Entries = (List<Entry>) (entries ?? new List<Entry>());
            }
        }
    }
}