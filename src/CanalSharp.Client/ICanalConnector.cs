// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CanalSharp.Protocol;

namespace CanalSharp.Client
{
    public interface ICanalConnector
    {
        /// <summary>
        /// Connect to the specified server.
        /// </summary>
        void Connect();

        /// <summary>
        /// Release connection
        /// </summary>
        void Disconnect();

        /// <summary>
        /// To check whether the connection is legal or not.
        /// The following situations are illegal:
        ///     a. Connection to Canal Server failed, and there is no connection available, returns false.
        ///     b. When chrrent client is running for preemption as a backup node (not a worker node), returns false.
        /// Supplementary explanation:
        ///     a. When current client become a backup node, all current operations on {@linkplain CanalConnector} 
        ///        are blocked until they are converted to working nodes. 
        ///     b. It is best for all business parties to periodically call CheckValid() mwthod,
        ///        such as calling the interrupt of the thread where CanalConnector is located, 
        ///        exiting CanalConnector directly, and dispose its own resources.
        /// </summary>
        /// <returns></returns>
        bool CheckValid();

        /// <summary>
        /// Client subscription
        /// When the subscription is repeated, the corresponding filter information will be updated.
        ///     a. If the filter information is empty, it'll use the filter information configured by Canal Server directly.
        ///     b. Otherwise, the filter information configured by Canal Server will be directly replaced, 
        ///        based on the version submitted this time.
        /// </summary>
        /// <param name="filter"></param>
        /// TODO: 后续可以考虑，如果本次提交的filter不为空，在执行过滤时，是对canal server filter + 本次filter的交集处理，达到只取1份binlog数据，多个客户端消费不同的表
        void Subscribe(string filter);

        /// <summary>
        ///  Subscribe. Does not submit client filters, which are subject to server filters.
        /// </summary>
        void Subscribe();

        /// <summary>
        /// Unsubscribe
        /// </summary>
        void UnSubscribe();

        /// <summary>
        /// Get data and confirm automatically
        /// The conditions returned by this method:
        ///     a. Try to get rhe specified batchSize records as many as possible without blocking.等待
        /// </summary>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        Message Get(int batchSize);

        /// <summary>
        /// Get data and confirm automatically
        /// The conditions returned by this method:
        ///     a. Get the specified batchSize records ot timeout.
        ///     b. If timeout = 0, it'll block unit it reaches the specified batchSize records.
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="timeout"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        Message Get(int batchSize, long? timeout, int? unit);

        /// <summary>
        /// Get without soecify position
        /// The conditions returned by this method:
        ///     a. Try to get rhe specified batchSize records as many as possible without blocking.
        /// Canal will save the client's lastst position.
        /// If it's the first fetch, it'll start output from the oldest record which saved in Canal.
        /// </summary>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        Message GetWithoutAck(int batchSize);

        /// <summary>
        /// Get without soecify position
        /// The conditions returned by this method:
        ///     a. Get the specified batchSize records ot timeout.
        ///     b. If timeout = 0, it'll block unit it reaches the specified batchSize records.
        /// Canal will save the client's lastst position.
        /// If it's the first fetch, it'll start output from the oldest record which saved in Canal.
        /// </summary>
        /// <param name="batchSize"></param>
        /// <param name="timeout"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        Message GetWithoutAck(int batchSize, long? timeout, int? unit);

        /// <summary>
        /// Confirm BatchId.
        /// All messages with BatchId that less than or equal to this BatchId will be confirmed automatically after confirmation.
        /// </summary>
        /// <param name="batchId"></param>
        void Ack(long batchId);

        /// <summary>
        /// Rollback to where {@link #ack} is not done.
        /// Specify the BatchId of the rollback.
        /// </summary>
        /// <param name="batchId"></param>
        void Rollback(long batchId);

        /// <summary>
        /// Rollack to where {@link #ack} is not done.
        /// The next time fetch, you can start wth the last place without {@link #ack}
        /// </summary>
        void Rollback();

        /// <summary>
        /// Interrupt blocking, used to gracefully stop the client.
        /// </summary>
        void StopRunning();
    }
}