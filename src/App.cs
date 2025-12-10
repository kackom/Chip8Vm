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
        public int Scaling { get; init; } = 10;
        public string Title { get; init; } = "Chip8";
        public int TickRate { get; init; } = 600;


        private Interpreter _interpreter;
        private Window _window;
        private byte[] _program = [];

        public App(string[] args) {
            string fileName = "program.ch8";

            try
            {
                _program = File.ReadAllBytes(fileName);
            }
            catch (DirectoryNotFoundException e)
            {
                System.Console.WriteLine(e.Message);
                return;
            }

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
                    _window.Draw(_interpreter.DisplayBuffer);
                    _interpreter.DisplayUpdate = false;
                }

                _window.Present();
            }
        }
    }
}
