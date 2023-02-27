# CanalSharp

[![Latest version](https://img.shields.io/nuget/v/CanalSharp.svg)](https://www.nuget.org/packages/CanalSharp/) [![Member project of .NET Core Community](https://img.shields.io/badge/member%20project%20of-NCC-9e20c9.svg)](https://github.com/dotnetcore)

CanalSharp 是阿里巴巴开源项目 mysql 数据库 binlog 的增量订阅&消费组件 Canal 的 **.NET 客户端**。在数据库中，**更改数据捕获**（**CDC**）是一组软件设计模式，用于确定和跟踪已更改的数据，以便可以使用已更改的数据来采取措施，Canal 便是 mysql 数据库的一种 cdc 组件。

## 快速入门

### 安装

```shell
Install-Package CanalSharp
```

### 初始化日志

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddFilter("Microsoft", LogLevel.Debug)
        .AddFilter("System", LogLevel.Information)
        .AddConsole();
});
var logger= loggerFactory.CreateLogger<SimpleCanalConnection>();
```

CanalSharp 使用 **Microsoft.Extensions.Logging.Abstractions** ，因为目前主流日志组件，如：nlog、serilog 等，全部支持此日志抽象接入，也就是说你可以通过安装 nlog、serilog 对其的适配，来使用它们，无论是 Console App 或则是 Web App。

### 创建连接

```csharp
var conn=new SimpleCanalConnection(new SimpleCanalOptions("127.0.0.1",11111,1234),logger);
//连接到 Canal Server
await conn.ConnectAsync();
//订阅
await conn.SubscribeAsync();
```

### 获取数据

```csharp
var msg = await conn.GetAsync(1024);
```

## 支持版本

| CanalSharp | Canal |
|------------|-------|
| 1.2.0      | 1.1.6 |

Mysql 版本由 Canal 决定。

## 文档

Github: [docs](https://github.com/dotnetcore/CanalSharp/tree/main/docs/zh)

WebSite: [Canal Document](https://canalsharp.azurewebsites.net/zh/) (推荐)

## 问题反馈

请通过 [Issue](https://github.com/dotnetcore/CanalSharp/issues/new) 向我们提交问题反馈，在提交时尽可能提供详细的信息，以便我们进行排查和解决。

## 贡献代码

如果你有一些好的想法，欢迎您提交 [Pull Request](https://github.com/dotnetcore/canalsharp/pulls) 或者 [Issue](https://github.com/dotnetcore/CanalSharp/issues/new)

## 重构进度

目前重构的版本已经完全覆盖旧版本，且性能更高，代码更优美，实现了旧版本未实现的部分功能，支持最新的 Canal。

| Task                | Status        |
| ------------------- |---------------|
| protobuf 3 协议生成 | 已完成           |
| 对接 Canal          | 已完成           |
| 数据订阅封装        | TODO          |
| 集群支持(Service 集群和 Client 集群)      | 已完成           |
| 数据发送到Kafka     | 直接通过 Canal 发送 |
| 数据发送到Redis     | 直接通过 Canal 发送 |

