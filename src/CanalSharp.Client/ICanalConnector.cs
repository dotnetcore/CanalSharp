using System;
using System.Threading.Tasks;
using CanalSharp.Protocol;

namespace CanalSharp.Client
{
    public interface ICanalConnector
    {
        /// <summary>
        /// 链接对应的canal server
        /// </summary>
        Task Connect();

        /// <summary>
        /// 释放链接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 检查下链接是否合法
        /// 几种case下链接不合法:
        /// 1. 链接canal server失败，一直没有一个可用的链接，返回false
        /// 2. 当前客户端在进行running抢占的时候，做为备份节点存在，非处于工作节点，返回false
        ///  说明：
        /// a. 当前客户端一旦做为备份节点存在，当前所有的对{@linkplain CanalConnector}的操作都会处于阻塞状态，直到转为工作节点
        /// b. 所以业务方最好定时调用checkValid()方法用，比如调用CanalConnector所在线程的interrupt，直接退出CanalConnector，并根据自己的需要退出自己的资源
        /// </summary>
        /// <returns></returns>
        bool CheckValid();

        /// <summary>
        /// 客户端订阅，重复订阅时会更新对应的filter信息
        ///  a. 如果本次订阅中filter信息为空，则直接使用canal server服务端配置的filter信息
        ///  b. 如果本次订阅中filter信息不为空，目前会直接替换canal server服务端配置的filter信息，以本次提交的为准
        /// </summary>
        /// <param name="filter"></param>
        /// TODO: 后续可以考虑，如果本次提交的filter不为空，在执行过滤时，是对canal server filter + 本次filter的交集处理，达到只取1份binlog数据，多个客户端消费不同的表
        Task Subscribe(string filter);

        /// <summary>
        ///  客户端订阅，不提交客户端filter，以服务端的filter为准
        /// </summary>
        Task Subscribe();

        /// <summary>
        /// 取消订阅
        /// </summary>
        void UnSubscribe();

        /// <summary>
        /// 获取数据，自动进行确认，该方法返回的条件：尝试拿batchSize条记录，有多少取多少，不会阻塞等待
        /// </summary>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        Message Get(int batchSize);
        /// <summary>
        /// 获取数据，自动进行确认
        ///  该方法返回的条件：
        /// a. 拿够batchSize条记录或者超过timeout时间
        ///  b. 如果timeout=0，则阻塞至拿到batchSize记录才返回
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="timeout"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        Message Get(int batchSize, long timeout, TimeSpan unit);

        /// <summary>
        /// 不指定 position 获取事件，该方法返回的条件: 尝试拿batchSize条记录，有多少取多少，不会阻塞等待
        ///  canal 会记住此 client 最新的position
        /// 如果是第一次 fetch，则会从 canal 中保存的最老一条数据开始输出。
        /// </summary>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        Message GetWithoutAck(int batchSize);

        /// <summary>
        ///  该方法返回的条件：
        ///   a. 拿够batchSize条记录或者超过timeout时间
        ///  b. 如果timeout=0，则阻塞至拿到batchSize记录才返回
        /// canal 会记住此 client 最新的position。
        /// 如果是第一次 fetch，则会从 canal 中保存的最老一条数据开始输出。
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="timeout"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        Message GetWithoutAck(int batchSize, long? timeout, int? unit);

        /// <summary>
        /// 进行 batch id 的确认。确认之后，小于等于此 batchId 的 Message 都会被确认。
        /// </summary>
        /// <param name="batchId"></param>
        void Ack(long batchId);

        /// <summary>
        /// 回滚到未进行 {@link #ack} 的地方，指定回滚具体的batchId
        /// </summary>
        /// <param name="batchId"></param>
        void Rollback(long batchId);

        /// <summary>
        /// 回滚到未进行 {@link #ack} 的地方，下次fetch的时候，可以从最后一个没有 {@link #ack} 的地方开始拿
        /// </summary>
        void Rollback();

        /// <summary>
        /// 中断的阻塞，用于优雅停止client
        /// </summary>
        void StopRunning();
    }
}
