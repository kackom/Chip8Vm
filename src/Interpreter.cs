using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace Chip8Vm.src
{
    internal class Interpreter
    {
        // Const
        const int DisplayWidth = 64;
        const int DisplayHeight = 32;
        const int TickRate = 600;

        // Flags
        public bool DisplayUpdate { get; set; } = false;
        public Status InterpreterStatus { get; private set; } = Status.Halted;

        // Display
        public bool[,] DisplayBuffer { get; private set; } = new bool[DisplayWidth, DisplayHeight];

        // Registers
        private UInt16 RegPC = 0x200; // begin execution at 0x200 address
        private UInt16 RegI = 0;
        private byte[] RegV = new byte[16]; // (mostly) general purpose registers

        // Memory
        private byte[] Memory = new byte[4096];
        private Stack<UInt16> Stack = [];

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
                InterpreterStatus = Status.Running; 
        }
        public void Halt()
        {
            if (InterpreterStatus != Status.InvalidOpcode)
                InterpreterStatus = Status.Halted;
        }
        public void Step(List<byte> keys)
        {
            UInt16 opcode = FetchOpcode();

            switch (GetInstructionType(opcode))
            {
                case 0:
                    InstructionFamily0(opcode);
                    break;
                case 1:
                    RegPC = GetAddr(opcode);    // JMP
                    IncreasePC();

                    break;
                 case 2:
                    Stack.Push(RegPC);          // CALL
                    RegPC = GetAddr(opcode);

                    IncreasePC();
                    break;
                case 3:
                    if (RegV[GetX(opcode)] == GetConst(opcode))
                        IncreasePC();
                    IncreasePC();
                    break;
                case 4:
                    if (RegV[GetX(opcode)] != GetConst(opcode))
                        IncreasePC();
                    IncreasePC();
                    break;
                case 5:
                    if (RegV[GetX(opcode)] == RegV[GetY(opcode)])
                        IncreasePC();
                    IncreasePC();
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    InstructionFamily8(opcode);
                    break;
                case 9:
                    break;
                case 0xa:
                    break;
                case 0xb:
                    break;
                case 0xc:
                    break;
                case 0xd:
                    break;
                case 0xe:
                    InstructionFamilyE(opcode);
                    break;
                case 0xf:
                    InstructionFamilyF(opcode);
                    break;

                default:
                    System.Console.WriteLine("Invalid opcode !!!");
                    InterpreterStatus = Status.InvalidOpcode;
                    break;
            }
        }

        private void InstructionFamily0(UInt16 _opcode)
        {
            if(_opcode == 0x00e0) // Clear screen
            {
                Array.Clear(DisplayBuffer);
                DisplayUpdate = true;

                IncreasePC();
            }
            else if(_opcode == 0x00ee) // Return from subroutine
            {
                RegPC = Stack.Pop();

                IncreasePC();
            }
            else // Machine code execution attempted
            {
                InterpreterStatus = Status.InvalidOpcode;
            }
        }
        private void InstructionFamily8(UInt16 _opcode)
        {
           
        }

        private void InstructionFamilyE(UInt16 _opcode)
        {

        }
        
        private void InstructionFamilyF(UInt16 _opcode)
        {

        }

        // Helper functions
        private void IncreasePC()
        {
            RegPC += 2;
        }

        private UInt16 FetchOpcode()
        {
            return (UInt16)(Memory[RegPC]<<8 | Memory[RegPC+1]);
        }

        // Opcode decoding
        private int GetInstructionType(UInt16 opcode)
        {
            return opcode >> 12;
        }

        private UInt16 GetAddr(UInt16 opcode)
        {
            return (UInt16)(opcode & 0xfff);
        }

        private UInt16 GetX(UInt16 opcode)
        {
            return (UInt16)((opcode & 0xf00) >> 8);
        }

        private UInt16 GetY(UInt16 opcode)
        {
            return (UInt16)((opcode & 0xf0) >> 4);
        }

        private UInt16 GetConst(UInt16 opcode)
        {
            return (UInt16)(opcode & 0xff);
        }
    }

}
