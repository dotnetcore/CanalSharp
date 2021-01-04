# 高可用

这里的高可用分为两类，客户端集群和服务端集群。都是采用冷备模式，因为对于 binlog 数据消费来说，并行处理将会带来数据顺序错乱的问题，当然你可以通过一些复杂的机制去实现，这里不做说明。集群部署需要 Zookeeper。

## 服务端集群

在 `conf/canal.properties` 文件中修改 zookeeper 地址

````
canal.zkServers=127.0.0.1:2181
````

集群中每个实例需，配置相同的 zookeeper 地址。

参考官方文档：[AdminGuide](https://github.com/alibaba/canal/wiki/AdminGuide)[|QuickStart](https://github.com/alibaba/canal/wiki/QuickStart)

## 客户端集群

客户端集群和服务端集群采用相同的模式，每个实例去抢占锁，获得了锁那么这个实例就运行获取数据，其他实例做冷备。

若正在运行消费数据的实例由于网络波动，导致和 zookeeper 失去连接，那么其他客户端实例不会立即抢占，会等待 60s 后才执行抢占，给与这个实例恢复的机会。

客户端集群使用的连接对象和快速入门中的不同：`ClusterCanalConnection`，但使用方法基本相同。

示例：

````csharp
//初始化日志
var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Debug)
                    .AddFilter("System", LogLevel.Information)
                    .AddConsole();
            });

var logger = loggerFactory.CreateLogger<Program>();
//设置zk地址和clientid，统一集群的client必须相同
var conn = new ClusterCanalConnection( new ClusterCanalOptions("localhost:2181", "12350")
//连接到Server                                      loggerFactory);
await conn.ConnectAsync();
//订阅
await conn.SubscribeAsync();
await conn.RollbackAsync(0);
while (true)
{
    try
    {
        //获取数据
        var msg = await conn.GetAsync(1024);
    }
    catch (Exception e)
    {
        _logger.LogError(e,"Error.");
        //发生异常执行重连，此方法只有集群连接对象才有
        await conn.ReConnectAsync();
    }

}
````

