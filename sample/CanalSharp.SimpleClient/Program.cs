using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CanalSharp.Client;
using CanalSharp.Client.Impl;
using Com.Alibaba.Otter.Canal.Protocol;

namespace CanalSharp.SimpleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var destination = "example";
            var connector = CanalConnectors.NewSingleConnector("127.0.0.1", 11111, destination, "", "");
            connector.Connect();
            connector.Subscribe();
            while (true)
            {
                var message = connector.Get(100);
                var batchId = message.Id;
                if (batchId == -1 || message.Entries.Count<=0)
                {
                    Thread.Sleep(300);
                }
                PrintEntry(message.Entries);
            }
        }

        private static void PrintEntry(List<Entry> entrys)
        {
            foreach (var entry in entrys)
            {
                if (entry.EntryType == EntryType.Transactionbegin || entry.EntryType == EntryType.Transactionend)
                {
                    continue;
                }

                RowChange rowChange = null;

                try
                {
                    rowChange = RowChange.Parser.ParseFrom(entry.StoreValue);
                }
                catch (Exception e)
                {

                }

                EventType eventType = rowChange.EventType;
                Console.WriteLine(
                    $"================> binlog[{entry.Header.LogfileName}:{entry.Header.LogfileOffset}] , name[{entry.Header.SchemaName},{entry.Header.TableName}] , eventType :{eventType}");

                foreach (var rowData in rowChange.RowDatas)
                {
                    if (eventType == EventType.Delete)
                    {
                        PrintColumn(rowData.BeforeColumns.ToList());
                    }
                    else if (eventType == EventType.Insert)
                    {
                        PrintColumn(rowData.BeforeColumns.ToList());
                    }
                    else
                    {
                        Console.WriteLine("-------> before");
                        PrintColumn(rowData.BeforeColumns.ToList());
                        Console.WriteLine("-------> after");
                        PrintColumn(rowData.AfterColumns.ToList());
                    }
                }
            }
        }

        private static void PrintColumn(List<Column> columns)
        {
            foreach (var column in columns)
            {
                Console.WriteLine($"{column.Name} ： {column.Value}  update=  {column.Updated}");
            }
        }
    }
}
