using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker
{
    internal class GameSettingsRow
    {
        public string Name { get; set; }
        public string Args { get; set; }
        public bool IsIncluded { get; set; }
    }
}
