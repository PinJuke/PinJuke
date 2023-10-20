using Microsoft.VisualBasic;
using PinJuke.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Configuration
{
    class IniDocument : IEnumerable<(string, IniSection)>
    {
        private readonly OrderedDictionary sectionByName = new();

        public IniSection this[string name]
        {
            get
            {
                return (IniSection?)sectionByName[name] ?? ((IniSection)(sectionByName[name] = new IniSection(name)));
            }
        }

        public IEnumerator<(string, IniSection)> GetEnumerator()
        {
            return new DictionaryEnumerator<string, IniSection>(sectionByName.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void MergeFrom(IniDocument otherDocument)
        {
            foreach (var (_, otherSection) in otherDocument)
            {
                this[otherSection.Name].MergeFrom(otherSection);
            }
        }
    }

    class IniSection : IEnumerable<(string, string)>
    {
        private readonly OrderedDictionary valueByName = new();

        public string Name { get; }

        public IniSection(string name)
        {
            this.Name = name;
        }

        public string? this[string name]
        {
            get
            {
                return (string?)valueByName[name];
            }
            set
            {
                if (value == null)
                {
                    valueByName.Remove(name);
                }
                else
                {
                    valueByName[name] = value;
                }
            }
        }

        public IEnumerator<(string, string)> GetEnumerator()
        {
            return new DictionaryEnumerator<string, string>(valueByName.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void MergeFrom(IniSection otherSection)
        {
            foreach (var (name, value) in otherSection)
            {
                this[name] = value;
            }
        }
    }

    class IniReader
    {
        public IniDocument Read(string filePath)
        {
            using var streamReader = new StreamReader(filePath, true);
            return Read(streamReader);
        }

        /// <summary>
        /// Some ideas taken from:
        /// https://github.com/FabioJe/INIParser/blob/5b3b91468190683e42c217093bee75e09208335c/INIParser/IniFile.cs
        /// </summary>
        public IniDocument Read(TextReader textReader)
        {
            IniDocument document = new();
            IniSection section = document[""];
            for (; ; )
            {
                var line = textReader.ReadLine();
                if (line == null)
                {
                    break;
                }
                line = line.Trim();
                if (line.Length == 0 || line[0] == ';')
                {
                    // Line is comment
                    continue;
                }
                if (line[0] == '[' && line[^1] == ']')
                {
                    // Line is section
                    var sectionName = line[1..^1];
                    section = document[sectionName];
                    continue;
                }
                var pair = line.Split('=', 2);
                if (pair.Length != 2)
                {
                    // Line is not recognized
                    continue;
                }
                section[pair[0]] = pair[1];
            }
            return document;
        }
    }
}
