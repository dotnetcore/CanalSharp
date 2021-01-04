# 快速入门

## 安装

```shell
Install-Package CanalSharp
```

## 初始化日志

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

## 创建连接

```csharp
var conn=new SimpleCanalConnection(new SimpleCanalOptions("127.0.0.1",11111,1234),logger);
//连接到 Canal Server
await conn.ConnectAsync();
//订阅
await conn.SubscribeAsync();
```

## 获取数据

```csharp
var msg = await conn.GetAsync(1024);
```

Demo: [SimpleApp](https://github.com/dotnetcore/CanalSharp/blob/main/sample/CanalSharp.SimpleApp)