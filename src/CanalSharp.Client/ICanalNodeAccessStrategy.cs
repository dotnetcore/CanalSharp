using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CanalSharp.Abstract
{
    public interface ICanalNodeAccessStrategy
    {
        SocketAddress CurrentNode();

        SocketAddress NextNode();
    }
}
