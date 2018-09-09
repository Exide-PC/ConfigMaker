using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigMaker.Mvvm.Models
{
    public class VirtualKeyboardModel : BindableBase
    {
        public enum KeyState
        {
            FirstInSequence,
            SecondInSequence,
            InCurrentSequence
        }

        public Dictionary<string, KeyState> KeyStates { get; } = new Dictionary<string, KeyState>();
    }
}
