using System;
using System.Threading.Tasks;
using CanalSharp.Connections.Enums;
using CanalSharp.Protocol;

namespace CanalSharp.Connections
{
    /// <summary>
    /// Canal Connection basic interface
    /// </summary>
    public interface ICanalConnection : IAsyncDisposable
    {
        /// <summary>
        /// Connect to canal server.
        /// </summary>
        /// <returns></returns>
        Task ConnectAsync();

        /// <summary>
        /// Subscribe to the data that needs to be received. Support repeat subscription to refresh filter setting.
        /// </summary>
        /// <param name="filter">Subscribe filter(Optional, default value is '.*\\..*')</param>
        /// <returns></returns>
        Task SubscribeAsync(string filter = ".*\\..*");

        /// <summary>
        /// UnSubscribe. If you need to unsubscribe, you should stop receiving data first.
        /// </summary>
        /// <param name="filter">UnSubscribe filter(Optional, default value is '.*\\..*')</param>
        /// <returns></returns>
        Task UnSubscribeAsync(string filter = ".*\\..*");

        Task DisConnectAsync();

        /// <summary>
        /// Ack has received data.
        /// </summary>
        /// <param name="batchId"></param>
        /// <returns></returns>
        Task AckAsync(long batchId);

        Task RollbackAsync(long batchId);

        /// <summary>
        /// Fetch data from Canal Server, have auto Ack and no timeout
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <returns></returns>
        Task<Message> GetAsync(int fetchSize);

        /// <summary>
        /// Fetch data from Canal Server, have auto Ack
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <param name="timeout">Fetch data timeout. If you set the value to null or less than or equal to 0, the value will be reset to -1. -1 means no timeout.</param>
        /// <param name="timeOutUnit">Timeout unit. Default value is <see cref="FetchDataTimeoutUnitType.Millisecond"/>.</param>
        /// <returns></returns>
        Task<Message> GetAsync(int fetchSize, long? timeout,
            FetchDataTimeoutUnitType timeOutUnit = FetchDataTimeoutUnitType.Millisecond);

        /// <summary>
        /// Fetch data from Canal Server, but have no auto Ack and no timeout
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <returns></returns>
        Task<Message> GetWithoutAckAsync(int fetchSize);

        /// <summary>
        /// Fetch data from Canal Server, but have no auto Ack
        /// </summary>
        /// <param name="fetchSize">Fetch data Size. If you set the value to null or less than or equal to 0, the value will be reset to 1000. </param>
        /// <param name="timeout">Fetch data timeout. If you set the value to null or less than or equal to 0, the value will be reset to -1. -1 means no timeout.</param>
        /// <param name="timeOutUnit">Timeout unit. Default value is <see cref="FetchDataTimeoutUnitType.Millisecond"/>.</param>
        /// <returns></returns>
        Task<Message> GetWithoutAckAsync(int fetchSize, long? timeout, FetchDataTimeoutUnitType timeOutUnit = FetchDataTimeoutUnitType.Millisecond);
    }
}