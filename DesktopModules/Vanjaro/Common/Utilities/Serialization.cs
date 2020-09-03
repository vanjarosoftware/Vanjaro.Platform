using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace Vanjaro.Common.Utilities
{
    public static class Serializer
    {
        public enum SerializationType
        {
            JSON = 0, Xml = 1
        }

        public static string ToJSON(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("Object");
            }

            try
            {
                JavaScriptSerializer Serializer = new JavaScriptSerializer();
                return Serializer.Serialize(obj);
            }
            catch { return string.Empty; }
        }

        public static string ToXml(object obj)
        {
            XmlSerializer xser = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            StringBuilder sb = new System.Text.StringBuilder();
            StringWriter sw = new StringWriter(sb);
            xser.Serialize(sw, obj);

            return sb.ToString();
        }

        public static object ToObject(string SerializedData, Type ObjectType, SerializationType SerialType)
        {
            if (string.IsNullOrEmpty(SerializedData))
            {
                throw new ArgumentNullException("SerializedData");
            }

            if (ObjectType == null)
            {
                throw new ArgumentNullException("ObjectType");
            }

            switch (SerialType)
            {
                case SerializationType.JSON:
                    {
                        try
                        {
                            JavaScriptSerializer Serializer = new JavaScriptSerializer();
                            return Serializer.DeserializeObject(SerializedData);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                    }
                case SerializationType.Xml:
                    {
                        try
                        {
                            XmlSerializer xser = new XmlSerializer(ObjectType);
                            StringReader sr = new StringReader(SerializedData);
                            return xser.Deserialize(sr);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                default:
                    {
                        return null;
                    }
            }
        }
        public static object ToObject<T>(string SerializedData, SerializationType SerialType)
        {
            if (string.IsNullOrEmpty(SerializedData))
            {
                throw new ArgumentNullException("SerializedData");
            }

            switch (SerialType)
            {
                case SerializationType.JSON:
                    {
                        try
                        {
                            JavaScriptSerializer Serializer = new JavaScriptSerializer();
                            return Serializer.Deserialize<T>(SerializedData);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                    }
                case SerializationType.Xml:
                    {
                        try
                        {
                            XmlSerializer xser = new XmlSerializer(typeof(T));
                            StringReader sr = new StringReader(SerializedData);
                            return xser.Deserialize(sr);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
