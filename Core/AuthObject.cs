using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MBProtoLib.Core
{
    public abstract class AuthObject 
    {
        public abstract void Write(BinaryWriter writer);
        public abstract void Read(BinaryReader reader);
        
    }
}