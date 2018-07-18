using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ConfigMaker.Csgo.Config
{
    public class KeySequence: IEquatable<KeySequence>, IXmlSerializable
    {
        public string[] Keys { get; set; } = null;

        public string this[int index] => this.Keys[index];

        public KeySequence()
        {

        }

        public KeySequence(IEnumerable<string> keys)
        {
            this.Keys = keys.Select(key => key.Trim().ToLower()).ToArray();
        }

        public KeySequence(params string[] keys): this((IEnumerable<string>) keys)
        {
            //this.Keys = keys.Select(key => key.Trim().ToLower()).ToArray();
        }

        public override int GetHashCode()
        {
            string keysInStr = string.Join(" ", this.Keys.Select(k => k));
            return keysInStr.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as KeySequence);
        }

        public bool Equals(KeySequence other)
        {
            if (this.Keys.Length != other.Keys.Length) return false;

            for (int i = 0; i < this.Keys.Length; i++)
                if (this.Keys[i] != other.Keys[i]) return false;

            return true;
        }

        public override string ToString()
        {
            return string.Join("_", this.Keys);
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            int keyCount = reader.AttributeCount;
            List<string> keys = new List<string>();
            reader.MoveToFirstAttribute();

            for (int i = 0; i < keyCount; i++)
            {
                string key = reader.Value;
                keys.Add(key);
            }

            this.Keys = keys.ToArray();
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            for (int i = 0; i < this.Keys.Length; i++)
            {
                writer.WriteAttributeString($"Key{i}", this.Keys[i]);
            }
        }
    }
}
