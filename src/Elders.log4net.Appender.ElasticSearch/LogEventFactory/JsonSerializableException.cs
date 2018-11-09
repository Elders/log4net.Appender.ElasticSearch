using System;
using System.Runtime.Serialization;

namespace log4net.Appender.ElasticSearch.LogEventFactory
{
    [DataContract(Name = "c60a96fe-1d5e-490f-b193-78e1f8931b2d")]
    public class SerializableException : Exception
    {
        SerializableException() { }

        public SerializableException(Exception ex)
        {
            ExType = ex.GetType();
            ExMessage = ex.Message;
            ExStackTrace = ex.StackTrace;
            ExSource = ex.Source;
            ExHelpLink = ex.HelpLink;

            if (ex.InnerException != null)
                ExInnerException = new SerializableException(ex.InnerException);
        }

        /// <summary>
        /// Serialization constructor.
        /// </summary>
        protected SerializableException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        [DataMember(Order = 1)]
        public Type ExType { get; private set; }

        [DataMember(Order = 2)]
        public string ExMessage { get; private set; }

        [DataMember(Order = 3)]
        public string ExStackTrace { get; private set; }

        [DataMember(Order = 4)]
        public string ExSource { get; private set; }

        [DataMember(Order = 5)]
        public string ExHelpLink { get; private set; }

        [DataMember(Order = 100)]
        public SerializableException ExInnerException { get; private set; }

        public override string ToString()
        {
            return ToString(true, true);
        }

        private String ToString(bool needFileLineInfo, bool needMessage)
        {
            String message = (needMessage ? Message : null);
            String result;

            if (message == null || message.Length <= 0)
                result = ExType.FullName;
            else
                result = ExType.FullName + ": " + ExMessage;

            if (ExInnerException != null)
                result = result + " ---> " + ExInnerException.ToString(needFileLineInfo, needMessage) + Environment.NewLine + "   --- End of inner exception stack trace ---";

            string stackTrace = ExStackTrace;
            if (!String.IsNullOrEmpty(stackTrace))
                result += Environment.NewLine + stackTrace;

            return result;
        }
    }
}
