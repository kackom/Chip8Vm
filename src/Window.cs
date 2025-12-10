using SDL2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Vm.src
{
    internal class Window
    {
        // Window config
        public Dictionary<SDL.SDL_Keycode, byte> KeyMap { get; init; } = [];
        public SDL.SDL_Color PixelColor { get; init; } = new SDL.SDL_Color() { r = 255, g = 255, b = 255, a = 255 };
        public SDL.SDL_Color BackgroundColor { get; init; } = new SDL.SDL_Color() { r = 21,  g = 21, b = 21, a = 255 };

        // Window status
        public bool Exit { get; private set; } = false;
        public List<byte> KeysPressed { get; private set; } = [];

        readonly public int Width;
        readonly public int Height;
        readonly public int Scaling;
        readonly public int TickRate;


        readonly private IntPtr _window = IntPtr.Zero;
        readonly private IntPtr _renderer = IntPtr.Zero;

        private SDL.SDL_Event _windowEvent;
        private UInt64 _frameStart, _frameEnd;
        private float _deltaTime;

        public Window(string title, int width, int height, int scaling, int tickRate)
        {
            this.Width = width;
            this.Height = height;
            this.Scaling = scaling;
            this.TickRate = tickRate;

            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0)
                throw new ApplicationException($"SDL_Init failed: {SDL.SDL_GetError()}\n");

            _window = SDL.SDL_CreateWindow(title, 
                                           SDL.SDL_WINDOWPOS_CENTERED, 
                                           SDL.SDL_WINDOWPOS_CENTERED, 
                                           width * scaling, 
                                           height * scaling, 
                                           SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (_window == IntPtr.Zero)
                throw new NullReferenceException($"SDL_CreateWindow failed: {SDL.SDL_GetError()}\n");

            _renderer = SDL.SDL_CreateRenderer( _window, 
                                                0, 
                                                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (_renderer == IntPtr.Zero)
                throw new NullReferenceException($"SDL_CreateRenderer failed: {SDL.SDL_GetError()}\n");

            if (SDL.SDL_RenderSetScale(_renderer, scaling, scaling) != 0)
                throw new ApplicationException($"SDL_RenderSetScale failed: {SDL.SDL_GetError()}\n");
        }

        ~Window()
        {
            SDL.SDL_DestroyRenderer (_renderer);
            SDL.SDL_DestroyWindow   (_window);

            SDL.SDL_Quit();
        }


        public void EventLoop()
        {
            _frameStart = SDL.SDL_GetPerformanceCounter();

            while (SDL.SDL_PollEvent(out _windowEvent) != 0)
            {
                byte key;

                switch (_windowEvent.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        Exit = true;
                        break;

                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        if (KeyMap.TryGetValue(_windowEvent.key.keysym.sym, out key))
                            KeysPressed.Add(key);
                        break;

                    case SDL.SDL_EventType.SDL_KEYUP:
                        if (KeyMap.TryGetValue(_windowEvent.key.keysym.sym, out key))
                            KeysPressed.Remove(key);
                        break;

                    default:
                        break;  
                }
            }
        }

        public void Draw(bool[,] pixelBuffer)
        {
            SDL.SDL_SetRenderDrawColor(_renderer, BackgroundColor.r, BackgroundColor.g, BackgroundColor.b, BackgroundColor.a);
            SDL.SDL_RenderClear(_renderer);

       
            List<SDL.SDL_Point> pixels = [];

            for(int y = 0; y < pixelBuffer.GetLength(1); y++)
            {
                for (int x = 0; x < pixelBuffer.GetLength(0); x++)
                {
                    if(pixelBuffer[x, y] == true)
                        pixels.Add(new SDL.SDL_Point() {x=x, y=y});
                }
            }

            SDL.SDL_SetRenderDrawColor(_renderer, PixelColor.r, PixelColor.g, PixelColor.b, PixelColor.a);
            SDL.SDL_RenderDrawPoints(_renderer, pixels.ToArray(), pixels.Count());
        }

        public void Present()
        {
            SDL.SDL_RenderPresent(_renderer);

            _frameEnd = SDL.SDL_GetPerformanceCounter();
            _deltaTime = (_frameEnd - _frameStart) / (float)SDL.SDL_GetPerformanceFrequency();
            SDL.SDL_Delay((uint)Math.Floor(1 / TickRate - _deltaTime * 1000.0));
        }

        virtual public void Init()
        {

        }

        virtual public void Loop()
        {

        }
    }
}
