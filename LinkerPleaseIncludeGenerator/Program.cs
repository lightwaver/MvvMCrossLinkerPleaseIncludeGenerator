using LinkerPleaseIncludeGenerator.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LinkerPleaseIncludeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Doit(args[0], args[1]);
        }

        private void Doit(string path, string outputpath)
        {
            List<AttributeResultEntry> typePropertyList = new List<AttributeResultEntry>();
            List<string> attributeNames = new List<string>();
            foreach (var file in Directory.GetFiles(path, "*.axml"))
            {
                Console.WriteLine($"processing {file}");
                XmlDocument doc = new XmlDocument();
                doc.Load(file);

                var result = ProcessNode(doc);

                foreach (var res in result)
                {
                    if (typePropertyList.Find(t => res.Property == t.Property && res.TypeName == t.TypeName) != null) continue;
                    typePropertyList.Add(res);

                    if (!attributeNames.Contains(res.MvxTag))
                    {
                        attributeNames.Add(res.MvxTag);
                    }
                }
            }

            Console.WriteLine("used Attributes:");
            foreach (var item in attributeNames)
            {
                Console.WriteLine(item);
            }

            typePropertyList.Sort((t1, t2) =>
            {
                var typecomp = string.Compare(t1?.TypeName, t2?.TypeName);
                if (typecomp != 0) return typecomp;
                return string.Compare(t1?.Property, t2?.Property);
            });

            using (var sw = new StreamWriter(outputpath, false, Encoding.UTF8))
            {
                string lastType = null;

                sw.WriteLine(Settings.Default.FilePrefix);

                void Write(AttributeResultEntry r, string txt) =>
                    sw.WriteLine(txt
                    .Replace("{Type}", r.TypeName)
                    .Replace("{TypeWithoutDots}", r.TypeName.Replace(".", "_"))
                    .Replace("{Property}", r.Property)
                    );

                var eventNames = Settings.Default.Events.Split(";\r\n \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var ignoreables = Settings.Default.Ignore.Split(";\r\n \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (var res in typePropertyList)
                {
                    if (res.TypeName.Contains("."))
                        res.TypeName = res.TypeName.Substring(res.TypeName.LastIndexOf('.') + 1);

                    Console.WriteLine($"found {res.TypeName} value: {res.Property}");

                    if (lastType != res.TypeName)
                    {
                        if (!string.IsNullOrEmpty(lastType))
                            Write(res, Settings.Default.TypeSuffix);

                        lastType = res.TypeName;

                        Write(res, Settings.Default.TypePrefix);
                    }

                    if (string.IsNullOrEmpty(res.Property.Trim())) continue;
                    if (Array.IndexOf(ignoreables, $"{res.TypeName}.{res.Property}") >= 0
                        || Array.IndexOf(ignoreables, res.Property) >= 0)
                        continue;

                    if (Array.IndexOf(eventNames, res.Property) >= 0)
                    {
                        Write(res, Settings.Default.EventTemplate);
                    }
                    else
                    {
                        Write(res, Settings.Default.PropertyTemplate);
                    }
                }

                sw.WriteLine(Settings.Default.TypeSuffix);
                sw.WriteLine(Settings.Default.FileSuffix);
            }
            //if (Debugger.IsAttached)
            //    Console.ReadLine();
        }

        internal class AttributeResultEntry
        {
            internal string TypeName;
            internal string Property;
            internal string MvxTag;
        }

        public IEnumerable<AttributeResultEntry> ProcessNode(XmlNode node)
        {
            if (node.Attributes != null)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (att.Name.Contains(Settings.Default.attributeToSearch))
                    {
                        if (att.Value.StartsWith("@")) continue;

                        yield return new AttributeResultEntry
                        {
                            TypeName = node.Name,
                            Property = ParseAttribute(att.Value),
                            MvxTag = att.Name
                        };
                    }
                }
            }
            if (node.HasChildNodes)
            {
                foreach (XmlNode child in node.ChildNodes)
                {
                    foreach (var childres in ProcessNode(child))
                    {
                        yield return childres;
                    }
                }
            }
            yield break;
        }

        private string ParseAttribute(string value)
        {
            return value.Substring(0, value.IndexOf(' '));
        }
    }
}
