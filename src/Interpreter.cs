using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Vm.src
{
    internal class Interpreter
    {
        // Flags
        public bool DisplayUpdate       { get; set; }           = false;
        public Status InterpreterStatus { get; private set; }   = Status.Halted;
        public enum Status
        {
            Running,
            Halted,
            InvalidOpcode
        }

        // Display
        public bool[,] DisplayBuffer    { get; private set; }   = new bool[64,32];

        // Registers
        private UInt16 RegPC   = 0x200;           // begin execution at this address
        private UInt16 RegI    = 0;
        private byte[] RegV    = new byte[16];    // (mostly) general purpose registers

        // Memory
        private byte[]      Memory  = new byte[4096];
        private UInt16[]    Stack   = [];

        // Timers
        private byte DelayTimer = 0;
        private byte SoundTimer = 0;

        public Interpreter(byte[] program)
        {
            if (program.Length > (Memory.Length - 0x200))
            {
                throw new ArgumentException("Program is too large !!!");
            }

            program.CopyTo(Memory, 0x200);
        }

        public void Run()
        {
            if(InterpreterStatus != Status.InvalidOpcode) 
            {
                InterpreterStatus = Status.Running; 
            }
        }
        public void Halt()
        {
            if (InterpreterStatus != Status.InvalidOpcode)
            {
                InterpreterStatus = Status.Halted;
            }
        }
        public void Step()
        {
            switch (GetInstructionType(Memory[RegPC]))
            {
                case 0:
                    break;
                default:
                    System.Console.WriteLine("Invalid opcode !!!");
                    InterpreterStatus = Status.InvalidOpcode;
                    break;
            }
        }

        // Helper functions
        private void IncreasePC()
        {
            RegPC += 2;
        }

        private int GetInstructionType(byte opcode)
        {
            return -1;
        }
    }

}
