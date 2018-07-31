/*
MIT License

Copyright (c) 2018 Exide-PC

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

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
                // Если пользователь особо одаренный, то он мог изменить в конфиге регистр кнопок
                string key = reader.Value.Trim().ToLower();
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
