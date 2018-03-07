using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace MBProtoLib.Exceptions
{

    public class AuthException : Exception
    {
        ErrorType _ErrorTypeProperties;

        public AuthException(ErrorType type)
        {
            _ErrorTypeProperties = type;
        }

        public T Serialize<T>()
        {

            if (typeof(T) == typeof(HttpStatusCode))
                return (T)Convert.ChangeType(_ErrorTypeProperties.Code, typeof(T));
            else if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(_ErrorTypeProperties.Message, typeof(T));

            throw new InvalidCastException("HttpStatusCode & string are valid types");
        }

        #region system

        public class PacketInvalid : ErrorType
        {
            public PacketInvalid() : base("PACKET_INVALID", HttpStatusCode.Unauthorized) { }
        }
        public class SessionExpired : ErrorType
        {
            public SessionExpired() : base("SESSION_EXPIRED", HttpStatusCode.Unauthorized) { }
        }
        public class AuthKeyInvalid : ErrorType
        {
            public AuthKeyInvalid() : base("AUTH_KEY_INVALID", HttpStatusCode.Unauthorized) { }
        }
        public class PhoneCodeHashEmpty : ErrorType
        {
            public PhoneCodeHashEmpty() : base("PHONE_CODE_HASH_EMPTY", HttpStatusCode.BadRequest) { }
        }
        public class PhoneCodeExpired : ErrorType
        {
            public PhoneCodeExpired() : base("PHONE_CODE_EXPIRED", HttpStatusCode.BadRequest) { }
        }
        public class PhoneCodeInvalid : ErrorType
        {
            public PhoneCodeInvalid() : base("PHONE_CODE_INVALID", HttpStatusCode.BadRequest) { }
        }
        public class PhoneNumberInvalid : ErrorType
        {
            public PhoneNumberInvalid(string mobile) : base("PHONE_NUMBER_INVALID_" + mobile, HttpStatusCode.BadRequest) { }
        }
        public class ObjectNotFound : ErrorType
        {
            public ObjectNotFound() : base("OBJECT_NOT_FOUND", HttpStatusCode.NotFound) { }
        }
        public class InternalServerError : ErrorType
        {
            public InternalServerError() : base("INTERNAL_SERVER_ERROR", HttpStatusCode.InternalServerError) { }
        }
        public class Flood : ErrorType
        {
            public Flood(int second) : base("FLOOD_WAIT_" + second, HttpStatusCode.NotAcceptable) { }
        }
        public class UserNotRegistered : ErrorType
        {
            public UserNotRegistered() : base("USER_NOT_REGISTERED", HttpStatusCode.BadRequest) { }
        }
        #endregion

        public abstract class ErrorType
        {
            string message = "";
            HttpStatusCode code;
            public HttpStatusCode Code { get { return code; } }
            public string Message { get { return message; } }

            public ErrorType(string message, HttpStatusCode code)
            {
                this.message = message;
                this.code = code;
            }
        }
    }

}