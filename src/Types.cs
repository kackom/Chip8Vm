using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Vm.src
{
    public enum Status
    {
        Running,
        Halted,
        InvalidOpcode
    }
}
