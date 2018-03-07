using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MBProtoLib.Core
{
    public abstract class AuthMethod:AuthObject
    {
        public int apiVersion;
        public abstract byte[] Do();
    }
}