using SoftCircuits.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PinJuke.Ini
{
    public class IniDocument : IEnumerable<KeyValuePair<string, IniSection>>
    {
        private readonly OrderedDictionary<string, IniSection> sectionByName = new();
        private readonly List<IniComment> footerComments = new();

        public IniSection this[string name]
        {
            get
            {
                return ProvideSection(name); // maybe rethink this
            }
        }

        public IniSection ProvideSection(string name)
        {
            var iniSection = sectionByName.GetValueOrDefault(name);
            if (iniSection == null)
            {
                iniSection = new(name);
                sectionByName[name] = iniSection;
            }
            return iniSection;
        }

        public void AddFooterComments(ICollection<IniComment> comments)
        {
            footerComments.AddRange(comments);
        }

        public IEnumerator<KeyValuePair<string, IniSection>> GetEnumerator()
        {
            return sectionByName.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void MergeFrom(IniDocument otherDocument)
        {
            foreach (var (name, otherSection) in otherDocument)
            {
                this[name].MergeFrom(otherSection);
            }
            footerComments.AddRange(otherDocument.footerComments);
        }

        public void WriteTo(TextWriter textWriter)
        {
            foreach (var (name, section) in sectionByName)
            {
                section.WriteTo(textWriter);
            }
            foreach (var comment in footerComments)
            {
                comment.WriteTo(textWriter);
            }
        }
    }

    public class IniSection : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly OrderedDictionary<string, IniEntry> iniEntryByName = new();
        private readonly List<IniComment> comments = new();

        public string Name { get; }

        public IniSection(string name)
        {
            Name = name;
        }

        public string? this[string name]
        {
            get
            {
                return iniEntryByName.GetValueOrDefault(name)?.Value;
            }
            set
            {
                if (value == null)
                {
                    iniEntryByName.Remove(name);
                }
                else
                {
                    ProvideEntry(name).Value = value;
                }
            }
        }

        public IniEntry ProvideEntry(string name)
        {
            var iniEntry = iniEntryByName.GetValueOrDefault(name);
            if (iniEntry == null)
            {
                iniEntry = new(name);
                iniEntryByName[name] = iniEntry;
            }
            return iniEntry;
        }

        public void AddComments(ICollection<IniComment> comments)
        {
            this.comments.AddRange(comments);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return iniEntryByName
                .Select(keyValuePair => new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void MergeFrom(IniSection otherSection)
        {
            comments.AddRange(otherSection.comments);
            foreach (var (name, otherEntry) in otherSection.iniEntryByName)
            {
                ProvideEntry(name).MergeFrom(otherEntry);
            }
        }

        public void WriteTo(TextWriter textWriter)
        {
            foreach (var comment in comments)
            {
                comment.WriteTo(textWriter);
            }

            if (Name.Length != 0)
            {
                textWriter.Write('[');
                textWriter.Write(Name);
                textWriter.Write(']');
                textWriter.WriteLine();
            }

            foreach (var (name, iniEntry) in iniEntryByName)
            {
                iniEntry.WriteTo(textWriter);
            }
        }
    }

    public class IniEntry
    {
        public string Name { get; }
        public string Value { get; set; } = "";

        private readonly List<IniComment> comments = new();

        public IniEntry(string name)
        {
            Name = name;
        }

        public void AddComments(ICollection<IniComment> comments)
        {
            this.comments.AddRange(comments);
        }

        public void MergeFrom(IniEntry otherEntry)
        {
            comments.AddRange(otherEntry.comments);
            Value = otherEntry.Value;
        }

        public void WriteTo(TextWriter textWriter)
        {
            foreach (var comment in comments)
            {
                comment.WriteTo(textWriter);
            }
            textWriter.Write(Name);
            textWriter.Write(" = ");
            textWriter.Write(Value);
            textWriter.WriteLine();
        }
    }

    public class IniComment
    {
        public string Line { get; }

        public IniComment(string line)
        {
            Line = line;
        }

        public void WriteTo(TextWriter textWriter)
        {
            textWriter.WriteLine(Line);
        }
    }

    public class IniIoException : Exception
    {
        public string FilePath { get;  }

        public IniIoException(string message, string filePath, Exception innerException) : base(message, innerException)
        {
            FilePath = filePath;
        }
    }

    public class IniReader
    {
        public IniDocument Read(string filePath)
        {
            try
            {
                using var streamReader = new StreamReader(filePath, true);
                return Read(streamReader);
            }
            catch (IOException ex)
            {
                throw new IniIoException(string.Format("Error reading file \"{0}\".", filePath), filePath, ex);
            }
        }

        public IniDocument Read(TextReader textReader)
        {
            var document = new IniDocument();
            var section = document.ProvideSection("");
            var comments = new List<IniComment>();
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
                    // Line is empty or a comment
                    comments.Add(new IniComment(line));
                    continue;
                }
                if (line[0] == '[' && line[^1] == ']')
                {
                    // Line is a section
                    var sectionName = line[1..^1];
                    section = document.ProvideSection(sectionName);
                    section.AddComments(comments);
                    comments.Clear();
                    continue;
                }
                var pair = line.Split('=', 2);
                if (pair.Length != 2)
                {
                    // Line is not recognized
                    comments.Add(new IniComment(line));
                    continue;
                }
                var name = pair[0].Trim();
                var value = pair[1].Trim();
                var entry = section.ProvideEntry(name);
                entry.Value = value;
                entry.AddComments(comments);
                comments.Clear();
            }
            document.AddFooterComments(comments);
            return document;
        }
    }
}
