namespace CanalSharp.Common
{
    public interface ICanalLifeCycle
    {
        void Start();

        void Stop();

        bool IsStart();
    }
}
