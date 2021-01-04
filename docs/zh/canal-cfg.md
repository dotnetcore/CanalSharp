# Canal 配置

> 运行 Canal 需要安装 java8 环境

下载 canal, 访问 [release 页面](https://github.com/alibaba/canal/releases) , 选择需要的包下载, 如以 1.0.17 版本为例

```shell
wget https://github.com/alibaba/canal/releases/download/canal-1.0.17/canal.deployer-1.0.17.tar.gz
```

**解压缩**

```shell
mkdir /tmp/canal
tar zxvf canal.deployer-$version.tar.gz  -C /tmp/canal
```

**修改配置**

```shell
vi conf/example/instance.properties
```

```
## mysql serverId
canal.instance.mysql.slaveId = 1234
#position info，需要改成自己的数据库信息
canal.instance.master.address = 127.0.0.1:3306 
canal.instance.master.journal.name = 
canal.instance.master.position = 
canal.instance.master.timestamp = 
#canal.instance.standby.address = 
#canal.instance.standby.journal.name =
#canal.instance.standby.position = 
#canal.instance.standby.timestamp = 
#username/password，需要改成自己的数据库信息
canal.instance.dbUsername = canal  
canal.instance.dbPassword = canal
canal.instance.defaultDatabaseName =
canal.instance.connectionCharset = UTF-8
#table regex
canal.instance.filter.regex = .\*\\\\..\*
```

>canal.instance.connectionCharset 代表数据库的编码方式对应到 java 中的编码类型，比如 UTF-8，GBK , ISO-8859-1
>如果系统是1个 cpu，需要将 canal.instance.parser.parallel 设置为 false

**启动**

```shell
sh bin/startup.sh
```

> Windows 使用 startup.bat 启动

**查看 instance 的日志**

```shell
vi logs/example/example.log
```

**关闭**

```shell
sh bin/stop.sh
```

Canal 官方文档：https://github.com/alibaba/canal/wiki/QuickStart

Canal Docker 方式启动：https://github.com/alibaba/canal/wiki/Docker-QuickStart