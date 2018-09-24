

using System.Net;

namespace CanalSharp.Client.Impl.Running
{

    /// <summary>
    /// 触发一下mainstem发生切换
    /// </summary>
    public interface IClientRunningListener
    {
        /// <summary>
        /// 触发现在轮到自己做为active，需要载入上一个active的上下文数据
        /// </summary>
        /// <returns></returns>
        SocketAddress ProcessActiveEnter();

        /// <summary>
        /// 触发一下当前active模式失败
        /// </summary>
        void ProcessActiveExit();
    }
}