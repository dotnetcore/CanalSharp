namespace CanalSharp.Connections
{
    public class CanalOptionsBase
    {
        /// <summary>
        /// Canal Destination(Optional, default value is 'example')
        /// </summary>
        public string Destination { get; set; } = "example";

        /// <summary>
        /// Differentiate client instances(Require)
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Auth UserName(Optional, default value is empty string)
        /// </summary>
        public string UserName { get; set; } = "";

        /// <summary>
        /// Auth Password(Optional, default value is empty string)
        /// </summary>
        public string Password { get; set; } = "";

        /// <summary>
        /// The timeout of idle between client and server is 1 hour by default.(Optional, Unit: millisecond)
        /// </summary>
        public int IdleTimeout { get; set; } = 60 * 60 * 1000;

        /// <summary>
        /// Data read timeout.(Optional, default value is 60000, Unit: millisecond)
        /// </summary>
        public int SoTimeout { get; set; } = 60000;

        /// <summary>
        /// Automatically resolve the Entry object. If the maximum performance is considered, parsing can be delayed.(Optional, default value is false)
        /// </summary>
        public bool LazyParseEntry { get; set; } = false;
    }
}