using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SDL2;

namespace Chip8Vm.src
{
    internal struct RGB
    {
        public byte r;
        public byte g;
        public byte b;
    }
    internal class Window
    {
        private IntPtr _window      = IntPtr.Zero;
        private IntPtr _renderer    = IntPtr.Zero;

        private SDL.SDL_Event _windowEvent;


        public bool closed;

        public RGB backgroundColor  = new RGB() {r=21, b=61, g=21};
        public RGB pixelColor       = new RGB() {r=255, b=255, g=255};


        public Window(string title, int width, int height, int scaling)
        {
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0)
                throw new ApplicationException($"SDL_Init failed: {SDL.SDL_GetError()}\n");

            _window =   SDL.SDL_CreateWindow(   title, 
                                                SDL.SDL_WINDOWPOS_CENTERED, 
                                                SDL.SDL_WINDOWPOS_CENTERED, 
                                                width * scaling, 
                                                height * scaling, 
                                                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN );
            if (_window == IntPtr.Zero)
                throw new NullReferenceException($"SDL_CreateWindow failed: {SDL.SDL_GetError()}\n");

            _renderer = SDL.SDL_CreateRenderer( _window, 
                                                0, 
                                                SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC );
            if (_renderer == IntPtr.Zero)
                throw new NullReferenceException($"SDL_CreateRenderer failed: {SDL.SDL_GetError()}\n");

            if (SDL.SDL_RenderSetScale(_renderer, scaling, scaling) != 0)
                throw new ApplicationException($"SDL_RenderSetScale failed: {SDL.SDL_GetError()}\n");
        }

        ~Window()
        {
            SDL.SDL_DestroyRenderer(_renderer);
            SDL.SDL_DestroyWindow(_window);

            SDL.SDL_Quit();
        }


        public void EventLoop(out bool exit, out List<byte> keyPressed)
        {
            exit = false;
            keyPressed = new List<byte> { };

            while (SDL.SDL_PollEvent(out _windowEvent) != 0)
            {
                if (_windowEvent.type == SDL.SDL_EventType.SDL_QUIT)
                    exit = true;
            }
        }

        public void RenderBuffer(bool[,] pixelBuffer)
        {
            SDL.SDL_SetRenderDrawColor(_renderer, backgroundColor.r, backgroundColor.g, backgroundColor.b, 255);
            SDL.SDL_RenderClear(_renderer);

            // Limits number of draw calls
            List<SDL.SDL_Point> pixels = new List<SDL.SDL_Point> { };

            for(int _y = 0; _y < pixelBuffer.GetLength(1); _y++)
            {
                for (int _x = 0; _x < pixelBuffer.GetLength(0); _x++)
                {
                    if(pixelBuffer[_x, _y] == true)
                        pixels.Add(new SDL.SDL_Point() {x=_x, y=_y});
                }
            }

            SDL.SDL_SetRenderDrawColor(_renderer, pixelColor.r, pixelColor.g, pixelColor.b, 255);
            SDL.SDL_RenderDrawPoints(_renderer, pixels.ToArray(), pixels.Count());
        }

        public void Present()
        {
            SDL.SDL_RenderPresent(_renderer);
        }
    }
}
