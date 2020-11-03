using System;
using System.Security.Cryptography;

namespace CanalSharp.Utils
{
    public class SecurityUtil
    {
        public static byte[] Scramble411(byte[] data, byte[] seed)
        {
            var sha1 = SHA1.Create();
            var data1 = sha1.ComputeHash(data);

            sha1.Initialize();
            var data2 = sha1.ComputeHash(data1);

            sha1.Initialize();
            sha1.TransformBlock(seed, 0, seed.Length, null, 0);
            var pass3 = sha1.ComputeHash(data2);

            for (int i = 0; i < pass3.Length; i++)
            {
                pass3[i] = (byte)(pass3[i] ^ data1[i]);
            }

            return pass3;
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "").ToLower();
        }
    }
}