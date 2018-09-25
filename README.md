
# CanalSharp

## 一.CanalSharp是什么?

CanalSharp 是阿里巴巴开源项目 Canal 的 .NET 客户端。为 .NET 开发者提供一个更友好的使用 Canal 的方式。Canal 是mysql数据库binlog的增量订阅&消费组件。

基于日志增量订阅&消费支持的业务：

1. 数据库镜像
2. 数据库实时备份
3. 多级索引 (卖家和买家各自分库索引)
4. search build
5. 业务cache刷新
6. 价格变化等重要业务消息

关于 Canal 的更多信息请访问 https://github.com/alibaba/canal/wiki

## 二.如何使用

1.安装Canal

Canal的安装以及配置使用请查看 https://github.com/alibaba/canal/wiki/QuickStart

2.建立一个.NET Core App项目

3.为该项目从 Nuget 安装 CanalSharp

````shell
Install-Package CanalSharp.Client
````

4.建立与Canal的连接

````csharp
//canal 配置的 destination，默认为 example
var destination = "example";
//创建一个简单CanalClient连接对象（此对象不支持集群）传入参数分别为 canal地址、端口、destination、用户名、密码
var connector = CanalConnectors.NewSingleConnector("127.0.0.1", 11111, destination, "", "");
//连接 Canal
connector.Connect();
//订阅，同时传入Filter，如果不传则以Canal的Filter为准。Filter是一种过滤规则，通过该规则的表数据变更才会传递过来
connector.Subscribe(".*\\\\..*");
//获取消息但是不需要发送Ack来表示消费成功
connector.Get(batchSize);
//获取消息并且需要发送Ack表示消费成功
connector.GetWithoutAck(batchSize);
````

更多详情请查看 Sample

## 三.贡献代码

1.fork本项目

2.做出你的更改

3.提交 pull request