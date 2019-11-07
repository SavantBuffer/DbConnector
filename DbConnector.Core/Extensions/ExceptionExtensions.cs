//Copyright 2019 Robert Orama

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace DbConnector.Core.Extensions
{
    public static class ExceptionExtensions
    {
        public static Action<Exception> OnError;

        internal static void Log(this Exception ex, IDbConnectorLogger logger, bool isLoggingEnabled = true)
        {
            try
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
                if (isLoggingEnabled)
                {
                    if (logger != null)
                    {
                        logger.Log(ex);
                    }
                    else
                    {
                        OnError?.Invoke(ex);
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine(e.ToString());
#endif
            }
        }

        public static string SerializeToXml(this Exception ex)
        {
            try
            {
                XmlSerializableException toSerialize = new XmlSerializableException(ex);

                XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, toSerialize);
                    return textWriter.ToString();
                }
            }
            catch (Exception)
            {
                return "XML serialization failed!";
            }
        }
    }

    [Serializable]
    public class XmlSerializableException
    {
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public string Message { get; set; }
        public string SqlStatement { get; set; }
        public XmlSerializableException InnerException { get; set; }
        public int HResult { get; set; }
        public string HelpLink { get; set; }


        public XmlSerializableException()
        {
        }

        public XmlSerializableException(System.Exception ex)
        {
            StackTrace = ex.StackTrace;
            Source = ex.Source;
            Message = ex.Message;
            HResult = ex.HResult;
            HelpLink = ex.HelpLink;

            var prop = ex.GetType().GetProperty("Statement");

            if (prop != null)
            {
                var statement = prop.GetValue(ex);

                if (statement != null)
                {
                    SqlStatement = statement.GetType().GetProperty("SQL")?.GetValue(statement).ToString();
                }
            }

            if (ex.InnerException != null)
            {
                InnerException = new XmlSerializableException(ex.InnerException);
            }
        }
    }
}
