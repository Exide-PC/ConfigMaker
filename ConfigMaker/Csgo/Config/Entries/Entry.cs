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

using ConfigMaker.Csgo.Commands;
using System.Xml.Serialization;
using ConfigMaker.Csgo.Config.Enums;
using ConfigMaker.Csgo.Config.Entries.interfaces;

namespace ConfigMaker.Csgo.Config.Entries
{
    [XmlInclude(typeof(ParametrizedEntry<string[]>))]
    [XmlInclude(typeof(ParametrizedEntry<string>))]
    [XmlInclude(typeof(ParametrizedEntry<int[]>))]
    [XmlInclude(typeof(ParametrizedEntry<int>))]
    [XmlInclude(typeof(ParametrizedEntry<double[]>))]
    [XmlInclude(typeof(ParametrizedEntry<double>))]
    [XmlInclude(typeof(ParametrizedEntry<Entry[]>))]
    [XmlInclude(typeof(ParametrizedEntry<Entry>))]
    public class Entry : IEntry
    {
        public ConfigEntry PrimaryKey { get; set; }
        public Executable Cmd { get; set; }
        public EntryType Type { get; set; }
        public bool IsMetaScript { get; set; }
        public CommandCollection Dependencies { get; set; } = new CommandCollection();

        
    }
}
