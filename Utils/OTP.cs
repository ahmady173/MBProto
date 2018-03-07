using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Web;

namespace MBProtoLib.Utils
{
    public static class OTP
    {
        public const int SECRET_LENGTH = 20;
        private const string
        MSG_SECRETLENGTH = "Abc872??/027643**&1jdn78//diuj!(`~";

        private static int[] dd = new int[10] { 0, 2, 4, 6, 8, 1, 3, 5, 7, 9 };

        private static byte[] secretKey = new byte[SECRET_LENGTH]
        {
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
            0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40, 0x41, 0x42, 0x43
        };
        private static int checksum(int Code_Digits)
        {
            int d1 = (Code_Digits / 1000000) % 10;
            int d2 = (Code_Digits / 100000) % 10;
            int d3 = (Code_Digits / 10000) % 10;
            int d4 = (Code_Digits / 1000) % 10;
            int d5 = (Code_Digits / 100) % 10;
            int d6 = (Code_Digits / 10) % 10;
            int d7 = Code_Digits % 10;
            return (10 - ((dd[d1] + d2 + dd[d3] + d4 + dd[d5] + d6 + dd[d7]) % 10)) % 10;
        }

        /// <summary>
        /// Formats the OTP. This is the OTP algorithm.
        /// </summary>
        /// <param name="hmac">HMAC value</param>
        /// <returns>8 digits OTP</returns>
        private static string FormatOTP(byte[] hmac)
        {
            int offset = hmac[19] & 0xf;
            int bin_code = (hmac[offset] & 0x7f) << 24
                | (hmac[offset + 1] & 0xff) << 16
                | (hmac[offset + 2] & 0xff) << 8
                | (hmac[offset + 3] & 0xff);
            int Code_Digits = bin_code % 10000000;
            int csum = checksum(Code_Digits);
            int OTP = Code_Digits * 10 + csum;

            return ByteArrayToString(SHA1.Create().ComputeHash(BitConverter.GetBytes(OTP))).ToLower();
        }

        private static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex.Replace("-", "");
        }

        /// <summary>
        /// Sets the OTP secret
        /// </summary>
        public static byte[] Secret
        {
            set
            {
                if (value.Length < SECRET_LENGTH)
                {
                    throw new Exception(MSG_SECRETLENGTH);
                }

                secretKey = value;
            }
        }


        //public static string GenerateOTP(long counter)
        //{
        //    HMACSHA1 hmacSha1 = new HMACSHA1(secretKey);

        //    byte[] hmac_result = hmacSha1.ComputeHash(BitConverter.GetBytes(counter));

        //    return FormatOTP(hmac_result);
        //}
        public static string GenerateOTP()
        {

            HMACSHA1 hmacSha1 = new HMACSHA1(secretKey);

            byte[] hmac_result = hmacSha1.ComputeHash(BitConverter.GetBytes(GenerateRandomID()));

            return FormatOTP(hmac_result);
        }
        /// <summary>
        /// generate 64 bit salt
        /// </summary>
        /// <returns></returns>
        private static long GenerateSalt()
        {
            long time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).Ticks);
            var random = new Random((int)time);

            long Salt = ((time) << 32) | ((time % 1000) << 22) | ((long)random.Next(524288) << 2);
            return Salt;
        }

        public static long GetTime()
        {
            return Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).Milliseconds);
        }

        public static long GetCounter()
        {
            Thread.Sleep(new Random().Next(0, 2));
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

        /// <summary>
        /// generate 64 bit sessionID
        /// </summary>
        /// <returns></returns>
        public static long GenerateRandomID()
        {
            long l1 = GenerateSalt() * GetCounter();

            var random = new Random((int)l1);
            long rand = (((long)random.Next()) << 32) | l1 | ((long)random.Next());
            return rand;
        }
    }
}