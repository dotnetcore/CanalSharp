using System.Net;

namespace CanalSharp.Client
{
    public interface ICanalNodeAccessStrategy
    {
        SocketAddress CurrentNode();

        SocketAddress NextNode();
    }
}