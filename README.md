# CanalSharp

[![Latest version](https://img.shields.io/nuget/v/CanalSharp.svg)](https://www.nuget.org/packages/CanalSharp/) 

## 重构进度

目前重构的版本已经完全覆盖旧版本，且性能更高，代码更优美，实现了旧版本未实现的部分功能。支持最新的 Canal

English README.md  Will be provided after the refactoring is complete.

旧版本代码：https://github.com/dotnetcore/CanalSharp/tree/release/0.2.0

| Task                | Status   |
| ------------------- | ------ |
| protobuf 3 协议生成 | 已完成 |
| 对接 Canal          | 已完成 |
| 数据订阅封装        |        |
| 集群支持(Service 集群和 Client 集群)      |  已完成  |
| 数据发送到Kafka     |        |
| 数据发送到Redis     |        |

## 快速入门

>先决条件：安装Java环境和需要使用的数据库开启binlog

### 1.运行 Canal Server

（1）下载最新的 Canal Server https://github.com/alibaba/canal/releases/latest, 下载 `canal.deployer-版本号-SNAPSHOT.tar.gz` 文件

（2）配置

编辑文件 `conf/example/instance.properties`

设置 MySql 地址：`canal.instance.master.address=`

设置 MySql 用户：`canal.instance.dbUsername=`

设置 MySql 密码：`canal.instance.dbPassword=`

（3）运行
  进入 `bin` 目录，根据你的系统选择脚本运行。

### 2.使用

从 Nuget 安装

````shell
Install-Package CanalSharp
````

代码
````csharp
//初始化日志
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Debug)
        .AddFilter("System", LogLevel.Information)
        .AddConsole();
});

//创建连接
var conn=new SimpleCanalConnection(new SimpleCanalConnectionOptions(Canal Server 地址,端口 默认 11111,ClientId 自定义), loggerFactory.CreateLogger<SimpleCanalConnection>());

//连接到 Canal Server
await conn.ConnectAsync();
//订阅需要处理的数据
await conn.SubscribeAsync();
while (true)
{
    //获取数据
    var msg = await conn.GetAsync(1024);
    await Task.Delay(300);
}
````

>更详细的文档将在重构完成后提供
