using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using SDL2;

namespace Chip8Vm.src
{

    internal class App
    {
        // Conf
        public int Width { get; init; } = 64;
        public int Height { get; init; } = 32;
        public int Scaling { get; init; } = 15;
        public string Title { get; init; } = "Chip8";

        private Dictionary<SDL.SDL_Keycode, byte> KeyMap = new()
        {
            {SDL.SDL_Keycode.SDLK_1, 1},
            {SDL.SDL_Keycode.SDLK_2, 2},
            {SDL.SDL_Keycode.SDLK_3, 3},
            {SDL.SDL_Keycode.SDLK_4, 0xc},

            {SDL.SDL_Keycode.SDLK_q, 4},
            {SDL.SDL_Keycode.SDLK_w, 5},
            {SDL.SDL_Keycode.SDLK_e, 6},
            {SDL.SDL_Keycode.SDLK_r, 0xd},

            {SDL.SDL_Keycode.SDLK_a, 7},
            {SDL.SDL_Keycode.SDLK_s, 8},
            {SDL.SDL_Keycode.SDLK_d, 9},
            {SDL.SDL_Keycode.SDLK_f, 0xe},

            {SDL.SDL_Keycode.SDLK_z, 0xa},
            {SDL.SDL_Keycode.SDLK_x, 0},
            {SDL.SDL_Keycode.SDLK_c, 0xb},
            {SDL.SDL_Keycode.SDLK_v, 0xf},
        };


        private Interpreter _interpreter;
        private Window _window;
        private byte[] _program = [];

        public App(string[] args) {
            string name = "chip8";
            if (!string.IsNullOrEmpty(Environment.ProcessPath))
                name = Environment.ProcessPath.Split("/").Last();
            
            string HelpMessage = $"Ussage: ./{name} {{ ROM filename }}";

            if(args.Length == 0)
            {
                System.Console.WriteLine(HelpMessage);
                System.Environment.Exit(-1);
            }

            string fileName = args[0];
            if (!File.Exists(fileName))
            {
                System.Console.WriteLine($"File {fileName} not found !");
                System.Environment.Exit(-1);
            }

            _program = File.ReadAllBytes(fileName);

            _interpreter = new(_program, false);
            _window = new(Title, Width, Height, Scaling, KeyMap);
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
