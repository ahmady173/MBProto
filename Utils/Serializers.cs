using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MBProtoLib.Utils
{
    public class Serializers
    {

        public static class String
        {
            public static string read(BinaryReader reader)
            {
                Int32 stringLenght = reader.ReadInt32();
                if (stringLenght == 0)
                    return "";
                try
                {
                    byte[] data = reader.ReadBytes(stringLenght);
                    return Encoding.UTF8.GetString(data, 0, data.Length);
                }
                catch
                {
                    return "";
                }
            }

            public static void write(BinaryWriter writer, string str)
            {
                byte[] data = Encoding.UTF8.GetBytes(str);
                writer.Write(data.Length);
                writer.Write(Encoding.UTF8.GetBytes(str));
            }
        }

    }
}
