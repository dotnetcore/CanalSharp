# 应答机制

应答机制可以保证消费数据的准确性，Canal 服务端会记录 Client 消费的进度，需要客户端发送 ACK 消息，服务端才会更新进度。类似于在消息队列中的 ACK 机制，如 RabbitMQ。

## 自动应答

````csharp
await conn.GetAsync(1024);//获取数据并自动应答
````

`GetAsync()` 会在获取数据后，自动向 Server 发送 ack 消息。

## 手动应答

````csharp
var msg = await conn.GetWithoutAckAsync(1024);//获取数据
await conn.AckAsync(msg.Id);//手动应答
await conn.RollbackAsync(msg.Id);//回滚
````



