/*
Based on DBC Viewer from TOM_RUS

This version produced by Kain Winterheart
http://www.facebook.com/kain.winterheart

For internal use only. ^^
*/

using System;
using System.IO;
using System.Xml;
using dbc2sql;

namespace dbc2html
{
    class Program
    {
        public static IWowClientDBReader m_reader;
        public static XmlDocument m_definitions;
        public static XmlElement m_definition;
        public static XmlNodeList m_fields;
        public static string m_dbcFile;
        public static string m_dbcName;
        public static string m_workingFolder;

        private static void LoadDefinitions()
        {
            m_definitions = new XmlDocument();
            m_definitions.Load(Path.Combine(m_workingFolder, "definitions.xml"));
        }

        private static XmlElement GetDefinition()
        {
            XmlNodeList definitions = m_definitions["DBFilesClient"].GetElementsByTagName(m_dbcName);

            if (definitions.Count == 1)
            {
                return ((XmlElement)definitions[0]);
            }
            else
            {
                Console.WriteLine("Can't load definition.");
                return null;
            }
        }

        private static void LoadFile(string file)
        {
            m_dbcFile = file;
            m_dbcName = Path.GetFileNameWithoutExtension(file);

            LoadDefinitions();

            m_definition = GetDefinition();

            if (m_definition == null)
            {
                Console.WriteLine("No definition available, exiting.");
                return;
            }
            else
            {
                Console.WriteLine("Processing " + m_dbcFile + "...");
            }

            processXML(file);
        }

        private static void processXML(string file)
        {
            try
            {
                m_reader = new DBCReader(file);
            }
            catch (Exception lastE)
            {
                Console.WriteLine(lastE.Message);
                return;
            }

            m_fields = m_definition.GetElementsByTagName("field");

            string[] types = new string[m_fields.Count];

            for (var j = 0; j < m_fields.Count; ++j)
                types[j] = m_fields[j].Attributes["type"].Value;

            XmlDocument acXmlOut = new XmlDocument();
            XmlNode acXmlOutRoot = acXmlOut.CreateElement("table");
            acXmlOut.AppendChild(acXmlOutRoot);

            XmlNode acXmlOutTR = acXmlOut.CreateElement("tr");
            for (var j = 0; j < m_fields.Count; ++j)
            {
                XmlNode acXmlOutTD = acXmlOut.CreateElement("td");
                acXmlOutTD.AppendChild(acXmlOut.CreateTextNode(m_fields[j].Attributes["name"].Value));
                acXmlOutTR.AppendChild(acXmlOutTD);
            }
            acXmlOutRoot.AppendChild(acXmlOutTR);

            for (var i = 0; i < m_reader.RecordsCount; ++i) // Add rows
            {
                var br = m_reader[i];
                acXmlOutTR = acXmlOut.CreateElement("tr");

                for (var j = 0; j < m_fields.Count; ++j)    // Add cells
                {
                    try
                    {
                        System.Object dbcValue = null;
                        switch (types[j])
                        {
                            case "long":
                                dbcValue = br.ReadInt64();
                                break;
                            case "ulong":
                                dbcValue = br.ReadUInt64();
                                break;
                            case "int":
                                dbcValue = br.ReadInt32();
                                break;
                            case "uint":
                                dbcValue = br.ReadUInt32();
                                break;
                            case "short":
                                dbcValue = br.ReadInt16();
                                break;
                            case "ushort":
                                dbcValue = br.ReadUInt16();
                                break;
                            case "sbyte":
                                dbcValue = br.ReadSByte();
                                break;
                            case "byte":
                                dbcValue = br.ReadByte();
                                break;
                            case "float":
                                dbcValue = br.ReadSingle();
                                break;
                            case "double":
                                dbcValue = br.ReadDouble();
                                break;
                            case "string":
                                dbcValue = m_reader is WDBReader ? br.ReadStringNull() : m_reader.StringTable[br.ReadInt32()];
                                break;
                            default:
                                break;
                        }
                        XmlNode acXmlOutTD = acXmlOut.CreateElement("td");
                        acXmlOutTD.AppendChild(acXmlOut.CreateTextNode(dbcValue.ToString()));
                        acXmlOutTR.AppendChild(acXmlOutTD);
                    }
                    catch (System.Exception lastE) { Console.WriteLine(lastE.Message); }
                }
                acXmlOutRoot.AppendChild(acXmlOutTR);
            }
            acXmlOut.Save(Path.Combine(m_workingFolder, Path.GetFileName(file) + ".csv.html"));
        }

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No input file specified, exiting.");
                return;
            }

            m_workingFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            LoadFile(args[0]);
        }
    }
}
