using Microsoft.Win32;
using SDL2;
using System;
using System.Collections;
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

        const int InstructionPerSecond = 600;

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


        private Random _generator = new();

        // Debug
        public void PrintStatus()
        {
            // Reg dump
            Console.WriteLine("\nV Reg: ");
            foreach(var reg in RegV)
            {
                Console.Write(reg.ToString() + " ");
            }
            Console.WriteLine("PC Reg: " + RegPC.ToString());
            Console.WriteLine("I Reg: " + RegI.ToString());

            // 
            Console.WriteLine("OPCODE: " + FetchOpcode().ToString("X"));

            Console.Write("Interpreter status: ");
            switch (InterpreterStatus)
            {
                case Status.Halted:
                    Console.WriteLine("Execustion halted");
                    break;
                case Status.InvalidOpcode:
                    Console.WriteLine("Invalid opcode!");
                    break;
                default:
                    Console.WriteLine("Running");
                    break;
            }
        }

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

            PrintStatus();
            switch (GetInstructionFamily(opcode))
            {
                case 0:
                    {
                        InstructionFamily0(opcode);
                    }
                    break;
                case 1:
                    {
                        RegPC = GetAddr(opcode);
                    }
                    break;
                case 2:
                    {
                        Stack.Push(RegPC);      
                        RegPC = GetAddr(opcode);
                    }
                    break;
                case 3:
                    {
                        if (RegV[GetX(opcode)] == GetConst(opcode))
                            IncreasePC();
                        IncreasePC();
                    }
                    break;
                case 4:
                    {
                        if (RegV[GetX(opcode)] != GetConst(opcode))
                            IncreasePC();
                        IncreasePC();
                    }
                    break;
                case 5:
                    {
                        if (RegV[GetX(opcode)] == RegV[GetY(opcode)])
                            IncreasePC();
                        IncreasePC();
                    }
                    break;
                case 6:
                    {
                        RegV[GetX(opcode)] = GetConst(opcode);
                        IncreasePC();
                    }
                    break;
                case 7:
                    {
                        RegV[GetX(opcode)] += GetConst(opcode);
                        IncreasePC();
                    }
                    break;
                case 8:
                    {
                        InstructionFamily8(opcode);
                    }
                    break;
                case 9:
                    {
                        if (RegV[GetX(opcode)] != RegV[GetY(opcode)])
                            IncreasePC();
                        IncreasePC();
                    }
                    break;
                case 0xa:
                    {
                        RegI = GetAddr(opcode);
                        IncreasePC();
                    }
                    break;
                case 0xb:
                    {
                        RegPC = (ushort)(GetAddr(opcode) + RegV[0]);
                    }
                    break;
                case 0xc:
                    {
                        RegV[GetX(opcode)] = (byte)_generator.Next(0, GetConst(opcode));
                        IncreasePC();
                    }
                    break;
                case 0xd:
                    {
                        RegV[0xf] = 0;

                        for(byte i = 0; i < GetN(opcode); i++)
                        {
                            byte pixel = Memory[RegI + i];
                            for(byte j = 8;j > 0;)
                            {
                                --j;    // Nice one c#

                                bool prev = DisplayBuffer[RegV[GetX(opcode)] + j, RegV[GetY(opcode)] + i];

                                DisplayBuffer[RegV[GetX(opcode)] + j, RegV[GetY(opcode)] + i] ^= (pixel % 2) != 0;
                                pixel >>= 1;

                                if (prev && !DisplayBuffer[RegV[GetX(opcode)] + j, RegV[GetY(opcode)] + i])
                                        RegV[0xf] = 1;
                            }
                        }
                        DisplayUpdate = true;
                        IncreasePC();
                    }
                    break;
                case 0xe:
                    {
                        InstructionFamilyE(opcode, keys);
                    }
                    break;
                case 0xf:
                    {
                        InstructionFamilyF(opcode, keys);
                    }
                    break;
                default:
                    {
                        System.Console.WriteLine("Invalid opcode !!!");
                        InterpreterStatus = Status.InvalidOpcode;
                    }
                    break;
            }
        }

        private void InstructionFamily0(UInt16 _opcode)
        {
            switch(_opcode & 0x00ff)
            {
                case 0xe0:
                    {
                        Array.Clear(DisplayBuffer);
                        DisplayUpdate = true;

                        IncreasePC();
                    }
                    break;
                case 0xee:
                    {
                        RegPC = Stack.Pop();

                        IncreasePC();
                    }
                    break;
                default:
                    {
                        System.Console.WriteLine("Invalid opcode !!!");
                        InterpreterStatus = Status.InvalidOpcode;
                    }
                    break;
            }
        }
        private void InstructionFamily8(UInt16 _opcode)
        {
           switch(_opcode & 0x000f)
            {
                case 0:
                    {
                        RegV[GetX(_opcode)] = RegV[GetY(_opcode)];
                        IncreasePC();
                    }
                    break;
                case 1:
                    {
                        RegV[GetX(_opcode)] = (byte)(RegV[GetX(_opcode)] | RegV[GetY(_opcode)]);
                        IncreasePC();
                    }
                    break;
                case 2:
                    {
                        RegV[GetX(_opcode)] = (byte)(RegV[GetX(_opcode)] & RegV[GetY(_opcode)]);
                        IncreasePC();
                    }
                    break;
                case 3:
                    {
                        RegV[GetX(_opcode)] = (byte)(RegV[GetX(_opcode)] ^ RegV[GetY(_opcode)]);
                        IncreasePC();
                    }
                    break;
                case 4:
                    {
                        RegV[0xf] = 0;
                        byte prev = RegV[GetX(_opcode)];

                        RegV[GetX(_opcode)] += RegV[GetY(_opcode)];

                        if(prev >= RegV[GetX(_opcode)] && RegV[GetY(_opcode)] != 0)
                            RegV[0xf] = 1;

                        IncreasePC();
                    }
                    break;
                case 5:
                    {
                        RegV[0xf] = 0;
                        byte prev = RegV[GetX(_opcode)];

                        RegV[GetX(_opcode)] -= RegV[GetY(_opcode)];

                        if(prev <= RegV[GetX(_opcode)] && RegV[GetY(_opcode)] != 0)
                            RegV[0xf] = 1;

                        IncreasePC();
                    }
                    break;
                case 6:
                    {
                        RegV[0xf] = 0;
                        if((RegV[GetY(_opcode)] & 1) != 0)
                            RegV[0xf] = 1;

                        RegV[GetX(_opcode)] = (byte)(RegV[GetY(_opcode)] >> 1);
                        IncreasePC();
                    }
                    break;
                case 7:
                    {
                        RegV[0xf] = 0;
                        byte prev = RegV[GetY(_opcode)];

                        RegV[GetX(_opcode)] = (byte)(RegV[GetY(_opcode)] - RegV[GetX(_opcode)]);

                        if(prev <= RegV[GetY(_opcode)] && RegV[GetX(_opcode)] != 0)
                            RegV[0xf] = 1;

                        IncreasePC();
                    }
                    break;
                case 0xe:
                    {
                        RegV[0xf] = 0;
                        if((RegV[GetY(_opcode)] & 0x80) != 0)
                            RegV[0xf] = 1;

                        RegV[GetX(_opcode)] = (byte)(RegV[GetY(_opcode)] << 1);
                        IncreasePC();
                    }
                    break;
                default:
                    {
                        System.Console.WriteLine("Invalid opcode !!!");
                        InterpreterStatus = Status.InvalidOpcode;
                    }
                    break;
            }
        }

        private void InstructionFamilyE(UInt16 _opcode, List<byte> _keys)
        {
            switch(_opcode & 0x00ff)
            {
                case 0x9e:
                    {
                        if(_keys.Contains(RegV[GetX(_opcode)]))
                            IncreasePC();
                        IncreasePC();
                    }
                    break;
                case 0xa1:
                    {
                        if(!_keys.Contains(RegV[GetX(_opcode)]))
                            IncreasePC();
                        IncreasePC();
                    }
                    break;
                default:
                    {
                        System.Console.WriteLine("Invalid opcode !!!");
                        InterpreterStatus = Status.InvalidOpcode;
                    }
                    break;
            }
        }
        
        private void InstructionFamilyF(UInt16 _opcode, List<byte> _keys)
        {
            switch(_opcode & 0x00ff)
            {
                case 7:
                    {
                        RegV[GetX(_opcode)] = DelayTimer;
                        IncreasePC();
                    }
                    break;
                case 0xa:
                    {
                        if(_keys.Count() > 0)
                        {
                            RegV[GetX(_opcode)] = _keys.First();
                            IncreasePC();
                        }
                    }
                    break;
                case 0x15:
                    {
                        DelayTimer = RegV[GetX(_opcode)];
                        IncreasePC();
                    }
                    break;
                case 0x18:
                    {
                        SoundTimer = RegV[GetX(_opcode)];
                        IncreasePC();
                    }
                    break;
                case 0x1e:
                    {
                        RegI += RegV[GetX(_opcode)];
                        IncreasePC();
                    }
                    break;
                case 0x29:
                    {
                        RegI = (ushort)(RegV[GetX(_opcode)] * 5);
                        IncreasePC();
                    }
                    break;
                case 0x33:
                    {
                        var dec = RegV[GetX(_opcode)];

                        Memory[RegI] = (byte)(dec / 100);
                        dec %= 100;
                        Memory[RegI + 1] = (byte)(dec / 10);
                        dec %= 10;
                        Memory[RegI + 2] = (byte)(dec / 1);

                        IncreasePC();
                    }
                    break;
                case 0x55:
                    {
                        for(var i = 0;i <= GetX(_opcode); i++)
                            Memory[RegI + i] = RegV[GetX(_opcode)];
                        
                        RegI = (ushort)(RegI + 1 + GetX(_opcode));

                        IncreasePC();
                    }
                    break;
                case 0x65:
                    {
                        for(var i = 0;i <= GetX(_opcode); i++)
                            RegV[GetX(_opcode)] = Memory[RegI + i];
                            
                        RegI = (ushort)(RegI + 1 + GetX(_opcode));

                        IncreasePC();
                    }
                    break;
                default:
                    {
                        System.Console.WriteLine("Invalid opcode !!!");
                        InterpreterStatus = Status.InvalidOpcode;
                    }
                    break;
            }
        }

        // Helpers
        private void IncreasePC()
        {
            RegPC += 2;
        }

        private UInt16 FetchOpcode()
        {
            return (UInt16)(Memory[RegPC]<<8 | Memory[RegPC+1]);
        }


        // Opcode decoding
        private int GetInstructionFamily(UInt16 opcode)
        {
            return opcode >> 12;
        }

        private UInt16 GetAddr(UInt16 opcode)
        {
            return (UInt16)(opcode & 0x0fff);
        }
        private byte GetConst(UInt16 opcode)
        {
            return (byte)(opcode & 0x00ff);
        }

        private byte GetX(UInt16 opcode)
        {
            return (byte)((opcode & 0x0f00) >> 8);
        }

        private byte GetY(UInt16 opcode)
        {
            return (byte)((opcode & 0x00f0) >> 4);
        }

        private byte GetN(UInt16 opcode)
        {
            return (byte)(opcode & 0x000f);
        }
    }
}