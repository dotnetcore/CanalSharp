using System.Net;

namespace CanalSharp.Client.Running
{
    public interface IClientRunningListener
    {
       /**
        * 触发现在轮到自己做为active，需要载入上一个active的上下文数据
        */
        SocketAddress ProcessActiveEnter();

        /**
         * 触发一下当前active模式失败
         */
        void ProcessActiveExit();
    }
}
