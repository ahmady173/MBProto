using MBProtoLib.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MBProtoLib.Core
{
    public class UserAuth
    {
        private Dictionary<uint, Type> constructors;
        private string connectionString;
        private string mbProtoConnectionString;

        public UserAuth(Dictionary<uint, Type> constructors, string connectionString, string mbProtoConnectionString)
        {
            this.constructors = constructors;
            this.connectionString = connectionString;
            this.mbProtoConnectionString = mbProtoConnectionString;
        }

        public byte[] Receive(byte[] Request)
        {
            MemoryStream ms = new MemoryStream(Request);

            using (BinaryReader messageReader = new BinaryReader(ms))
            {
                AuthObject data = Parse<AuthObject>(messageReader);
                return (data as AuthMethod).Do();
            }

        }
        private T Parse<T>(BinaryReader reader)
        {
            if (typeof(AuthObject).IsAssignableFrom(typeof(T)))
            {

                long auth_key_id = reader.ReadInt64();

                byte[] msg_key = reader.ReadBytes(16);

                byte[] cipherData = reader.ReadBytes((int)reader.BaseStream.Length - (int)reader.BaseStream.Position);
                byte[] plainData = ValidatePacket(auth_key_id, msg_key, cipherData);

                MemoryStream ms = new MemoryStream(plainData);
                using (BinaryReader messageReader = new BinaryReader(ms))
                {
                    return GetObject<T>(messageReader);
                }
            }
            else
            {
                //throw new Exception("invalid constructor code");
                throw new Exceptions.AuthException(new Exceptions.AuthException.ObjectNotFound());
            }
        }

        private byte[] ValidatePacket(long auth_key_id, byte[] msg_key, byte[] data)
        {
            int SessionTimeOutInSeccond = 8640000;//100 days
            int packetTimeOutInMiliSeccond = 100 * 1000;

            var db = new DataAccessDataContext(connectionString);

            if (!db.MBProto_user_sessionTbls.Any(c => c.authKeyID == auth_key_id))
            {
                throw new Exceptions.AuthException(new Exceptions.AuthException.AuthKeyInvalid());
            }

            var se = db.MBProto_user_sessionTbls.Single(c => c.authKeyID == auth_key_id);

            if (se.UserTbl.status != (byte)DeleteStatus.Active)
            {
                throw new Exceptions.AuthException(new Exceptions.AuthException.UserNotRegistered());
            }


            var dt = se.regDate;
            var nextDT = dt.AddSeconds(SessionTimeOutInSeccond);
            if (dt > nextDT)
            {
                db.MBProto_user_sessionTbls.DeleteOnSubmit(se);
                db.SubmitChanges();
                throw new Exceptions.AuthException(new Exceptions.AuthException.SessionExpired());
            }

            byte[] DiffKey = Convert.FromBase64String(se.diffKey);

            byte[] preRuntimeAESKey = new byte[DiffKey.Length + msg_key.Length];

            Buffer.BlockCopy(DiffKey, 0, preRuntimeAESKey, 0, DiffKey.Length);
            Buffer.BlockCopy(msg_key, 0, preRuntimeAESKey, DiffKey.Length, msg_key.Length);

            byte[] runtimeAESKey = SHA256.Create().ComputeHash(preRuntimeAESKey);

            byte[] plainData = new AES(runtimeAESKey).Decrypt(data);

            MemoryStream ms = new MemoryStream(plainData);
            using (BinaryReader messageReader = new BinaryReader(ms))
            {
                long salt = messageReader.ReadInt64();
                long sessionID = messageReader.ReadInt64();
                long time = messageReader.ReadInt64();
                int length = messageReader.ReadInt32();
                long sequence = messageReader.ReadInt64();

                if (se.sessionID != sessionID)
                {
                    throw new Exceptions.AuthException(new Exceptions.AuthException.PacketInvalid());
                }

                byte[] saltArray = BitConverter.GetBytes(salt);
                byte[] sessionIDArray = BitConverter.GetBytes(sessionID);
                byte[] timeArray = BitConverter.GetBytes(time);
                byte[] lengthArray = BitConverter.GetBytes(length);
                byte[] sequenceArray = BitConverter.GetBytes(sequence);

                byte[] clearMsgKey = new byte
                    [saltArray.Length
                    + sessionIDArray.Length
                    + timeArray.Length
                    + lengthArray.Length
                    + sequenceArray.Length];

                Buffer.BlockCopy(saltArray, 0, clearMsgKey, 0, saltArray.Length);
                Buffer.BlockCopy(sessionIDArray, 0, clearMsgKey, saltArray.Length, sessionIDArray.Length);
                Buffer.BlockCopy(timeArray, 0, clearMsgKey, saltArray.Length + sessionIDArray.Length, timeArray.Length);
                Buffer.BlockCopy(lengthArray, 0, clearMsgKey, saltArray.Length + sessionIDArray.Length + timeArray.Length, lengthArray.Length);
                Buffer.BlockCopy(sequenceArray, 0, clearMsgKey, saltArray.Length + sessionIDArray.Length + lengthArray.Length + timeArray.Length, sequenceArray.Length);

                byte[] generatedMsgKey = SHA256.Create().ComputeHash(clearMsgKey).Take(16).ToArray();

                long now = (long)Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1)).Milliseconds);
                long expDate = time + (long)packetTimeOutInMiliSeccond;

                if (now > expDate)
                {
                    throw new Exceptions.AuthException(new Exceptions.AuthException.PacketInvalid());
                }

                byte[] plainBytes = messageReader.ReadBytes((int)messageReader.BaseStream.Length - (int)messageReader.BaseStream.Position);

                MemoryStream mst = new MemoryStream(plainBytes);
                using (BinaryReader messageReadert = new BinaryReader(mst))
                {
                    uint code = messageReadert.ReadUInt32();
                    plainBytes = messageReadert.ReadBytes((int)messageReadert.BaseStream.Length - (int)messageReadert.BaseStream.Position);
                    byte[] codeArray = BitConverter.GetBytes(code);
                    byte[] result = new byte[plainBytes.Length + sessionIDArray.Length + codeArray.Length];
                    Buffer.BlockCopy(codeArray, 0, result, 0, codeArray.Length);
                    Buffer.BlockCopy(sessionIDArray, 0, result, codeArray.Length, sessionIDArray.Length);
                    Buffer.BlockCopy(plainBytes, 0, result, codeArray.Length + sessionIDArray.Length, plainBytes.Length);
                    return result;
                }

            }
        }

        private T GetObject<T>(BinaryReader reader)
        {
            uint dataCode = reader.ReadUInt32();
            if (!constructors.ContainsKey(dataCode))
            {
                throw new Exceptions.AuthException(new Exceptions.AuthException.ObjectNotFound());
            }

            Type constructorType = constructors[dataCode];
            if (!typeof(T).IsAssignableFrom(constructorType))
            {
                throw new Exceptions.AuthException(new Exceptions.AuthException.ObjectNotFound());
            }

            T obj = (T)Activator.CreateInstance(constructorType);
            ((AuthObject)(object)obj).Read(reader);
            return obj;
        }
        public static byte[] GenerateMessage(params object[] rows)
        {
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            {
                var msg = new AuthMessage(writer);
                foreach (var item in rows)
                {
                    msg.addRow(item);
                }

                return memory.ToArray();
            }
        }

        public static byte[] MakeResponse(string coreUrl, long userId, long sessionID, string diffKey, string result)
        {
            long salt = OTP.GenerateRandomID();
            long time = OTP.GetTime();
            long length = result.Length;

            byte[] saltArray = BitConverter.GetBytes(salt);
            byte[] timeArray = BitConverter.GetBytes(time);
            byte[] lengthArray = BitConverter.GetBytes(length);
            byte[] sessionIDArray = BitConverter.GetBytes(sessionID);
            byte[] DiffKey = Convert.FromBase64String(diffKey);


            byte[] clearMsgKey = new byte
                [saltArray.Length
                + sessionIDArray.Length
                + timeArray.Length
                + lengthArray.Length
                ];

            Buffer.BlockCopy(saltArray, 0, clearMsgKey, 0, saltArray.Length);
            Buffer.BlockCopy(sessionIDArray, 0, clearMsgKey, saltArray.Length, sessionIDArray.Length);
            Buffer.BlockCopy(timeArray, 0, clearMsgKey, saltArray.Length + sessionIDArray.Length, timeArray.Length);
            Buffer.BlockCopy(lengthArray, 0, clearMsgKey, saltArray.Length + sessionIDArray.Length + timeArray.Length, lengthArray.Length);

            byte[] preRuntimeAESKey = new byte[DiffKey.Length + sessionIDArray.Length];

            Buffer.BlockCopy(DiffKey, 0, preRuntimeAESKey, 0, DiffKey.Length);
            Buffer.BlockCopy(sessionIDArray, 0, preRuntimeAESKey, DiffKey.Length, sessionIDArray.Length);

            byte[] runtimeAESKey = SHA256.Create().ComputeHash(preRuntimeAESKey);

            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                var msg = new AuthMessage(writer);

                msg.addRow(clearMsgKey);
                msg.addRow(coreUrl);
                msg.addRow(result);

                return UserAuth.GenerateMessage(new AES(runtimeAESKey).Encrypt(ms.ToArray()));
            }
        }
    }
}
