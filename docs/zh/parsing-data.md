# 解析数据

## Entry

`conn.GetAsync()` 返回的是一个 Entry 集合，Entry 对应 binlog 记录，它可能是事务标记也有可能是行数据变化，通过 `Entry.EntryType ` 来区分，一般事务的标记在业务消费处理时不需要处理。

示例：

````csharp
var entries = await conn.GetAsync(1024);
foreach (var entry in entries)
{
    //不处理事务标记
    if (entry.EntryType == EntryType.Transactionbegin || entry.EntryType == EntryType.Transactionend)
    {
        continue;
    }
}
````

Entry.Header 包含了一些binlog以及数据库信息

| 属性                       | 说明              |
| -------------------------- | ----------------- |
| Entry.Header.LogfileName   | binlog 文件名     |
| Entry.Header.LogfileOffset | binlog 偏移       |
| Entry.Header.SchemaName    | mysql schema 名称 |
| Entry.Header.TableName     | 表名              |

## RowChange

一般在业务处理中，都会需要行数据的变更，将 Entry 转换为 `RowChange `对象

示例：

````csharp
RowChange rowChange = null;
try
{
    rowChange = RowChange.Parser.ParseFrom(entry.StoreValue);
}
catch (Exception e)
{
    _logger.LogError(e);
}
````

通过 `RowChange.EventType ` 来Row是什么变化，Update、Delete和 Insert 对应 sql 中的 update、delete 和 insert 语句

通过 `RowChange.RowDatas` 属性，来访问 RowChange 对象中包含的行变化数据集合。

示例，遍历 RowChange 中的行数据：

````csharp
foreach (var rowData in rowChange.RowDatas)
{
    //删除的数据
    if (eventType == EventType.Delete)
    {
        PrintColumn(rowData.BeforeColumns.ToList());
    }
    //插入的数据
    else if (eventType == EventType.Insert)
    {
        PrintColumn(rowData.AfterColumns.ToList());
    }
    //更新的数据
    else
    {
        _logger.LogInformation("-------> before");
        PrintColumn(rowData.BeforeColumns.ToList());
        _logger.LogInformation("-------> after");
        PrintColumn(rowData.AfterColumns.ToList());
    }
}

private static void PrintColumn(List<Column> columns)
{
    foreach (var column in columns)
    {
        Console.WriteLine($"{column.Name} ： {column.Value}  update=  {column.Updated}");
    }
}
````

## Column

Column 如其名，代表数据库中表的每一列的信息。

| 属性名         | 说明         |
| -------------- | ------------ |
| Column.Name    | 列名         |
| Column.Value   | 列的值       |
| Column.Updated | 列是否被更新 |

如执行 sql `update user set Name='Allen'`，那么获取到的数据变更则有

````csharp
Column.Name = 'Name';
Column.Value = 'Allen';
Column.Value = True
````





