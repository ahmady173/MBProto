using MBProtoLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MBProtoLib.Core
{ 
    public class AuthMessage : IDisposable
    {
        BinaryWriter writer;

        public AuthMessage(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void addRow(object obj)
        {

            if (obj is AuthObject)
                ((AuthObject)(obj)).Write(writer);
            else if (obj is int)
                writer.Write((int)obj);
            else if (obj is long)
                writer.Write((long)obj);
            else if (obj is ulong)
                writer.Write((ulong)obj);
            else if (obj is string)
                Serializers.String.write(writer, (string)(obj));
            else if (obj is byte[])
            {
                foreach (var item in ((byte[])obj))
                {
                    writer.Write(item);
                }
            }

        }
        public void addRow(AuthObject obj)
        {
            obj.Write(writer);

        }
        public void addRow(int data)
        {
            writer.Write(data);

        }
        public void addRow(long data)
        {
            writer.Write(data);

        }
        public void addRow(ulong data)
        {
            writer.Write(data);

        }
        public void addRow(string data)
        {
            Serializers.String.write(writer,data);

        }
        public void addRow(byte[] data)
        {
            foreach (var item in data)
            {
                writer.Write(item);
            }
        }
        public void Dispose()
        {
            try
            {
                Dispose();
            }
            catch { }
        }
    }
}
