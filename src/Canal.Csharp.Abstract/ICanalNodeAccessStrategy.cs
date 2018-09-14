using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Canal.Csharp.Abstract
{
    public interface ICanalNodeAccessStrategy
    {
        SocketAddress CurrentNode();

        SocketAddress NextNode();
    }
}
