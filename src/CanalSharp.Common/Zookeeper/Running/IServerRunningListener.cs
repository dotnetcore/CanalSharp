namespace CanalSharp.Common.Zookeeper.Running
{
    public interface IServerRunningListener
    {
        /**
        * 启动时回调做点事情
        */
        void ProcessStart();

        /**
         * 关闭时回调做点事情
         */
        void ProcessStop();

        /**
         * 触发现在轮到自己做为active，需要载入上一个active的上下文数据
         */
        void ProcessActiveEnter();

        /**
         * 触发一下当前active模式失败
         */
        void ProcessActiveExit();
    }
}
