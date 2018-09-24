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

        public List<ByteString> RawEntries = new List<ByteString>();

        public Message(long id, List<Entry> entries)
        {
            Id = id;
            Entries = entries ?? new List<Entry>();
            Raw = false;
        }

        public Message(long id)
        {
            Id = id;
            Entries = new List<Entry>();;
        }
        public Message(long id, bool raw, object entries)
        {
            Id = id;
            if (raw)
            {
                RawEntries = (List<ByteString>)(entries ?? new List<ByteString>());
            }
            else
            {
                Entries = (List<Entry>)(entries ?? new List<Entry>());
            }
        }
    }
}
