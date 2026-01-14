using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Vm.src
{

    internal class App
    {
        // Conf
        public int Width { get; init; } = 64;
        public int Height { get; init; } = 32;
        public int Scaling { get; init; } = 15;
        public string Title { get; init; } = "Chip8";
        public int TickRate { get; init; } = 640;


        private Interpreter _interpreter;
        private Window _window;
        private byte[] _program = [];

        public App(string[] args) {
            string fileName = "program.ch8";
            if(args.Length > 1)
            {
                fileName = args[2];
            }

            if (!File.Exists(fileName))
            {
                System.Console.WriteLine($"File {fileName} not found !");
                System.Environment.Exit(-1);
            }

             _program = File.ReadAllBytes(fileName);

            _interpreter = new(_program);
            _window = new(Title, Width, Height, Scaling, TickRate);
        }

        public void Run()
        {
            _interpreter.Run();
            Loop();
        }

        private void Loop()
        {
            while (!_window.Exit)
            {
                _window.EventLoop();

                _interpreter.Step(_window.KeysPressed);
                if (_interpreter.DisplayUpdate == true)
                {
                    _window.UpdateBuffer(_interpreter.DisplayBuffer);
                    _interpreter.DisplayUpdate = false;
                }

                _window.DrawBuffer();
                _window.Present();
            }
        }
    }
}
