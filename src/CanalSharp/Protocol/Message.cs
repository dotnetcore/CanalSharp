using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;

namespace CanalSharp.Protocol
{
    [Serializable]
    public class Message
    {
        public Message(long id)
        {
            Id = id;
            Entries = new List<Entry>();
            Raw = false;
            RawEntries = new List<ByteString>();
        }

        public Message(long id, List<Entry> entries)
        {
            Id = id;
            Entries = entries ?? new List<Entry>();
            Raw = false;

            RawEntries=new List<ByteString>();
        }

        public Message(long id,bool raw, IList entries)
        {
            Id = id;
            Raw = raw;

            if (raw)
            {
                RawEntries = entries switch
                {
                    null => new List<ByteString>(),
                    List<ByteString> obj => obj,
                    _ => throw new ArgumentException($"Cannot convert to type {typeof(List<ByteString>)}", nameof(entries))
                };

                Entries=new List<Entry>();
            }
            else
            {
                Entries = entries switch
                {
                    null => new List<Entry>(),
                    List<Entry> obj => obj,
                    _ => throw new ArgumentException($"Cannot convert to type {typeof(List<Entry>)}", nameof(entries))
                };
                RawEntries = new List<ByteString>();
            }
        }
        public long Id { get; }

        public List<Entry> Entries { get; }

        public List<ByteString> RawEntries { get; }

        public bool Raw { get; }

        public void AddEntry(Entry entry)
        {
            Entries.Add(entry);
        }

        public void AddRasEntry(ByteString rawEntry)
        {
            RawEntries.Add(rawEntry);
        }
    }
}