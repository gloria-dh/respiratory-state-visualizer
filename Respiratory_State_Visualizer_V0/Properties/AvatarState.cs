using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Respiratory_State_Visualizer_V0.Properties
{
    internal class AvatarState
    {
        internal State displayState {  get; set; }
    }

    internal enum State { calm, holding_breath, hyperventilating }
}
