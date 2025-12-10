using Chip8Vm.src;

class EmulatorMain
{
    static void Main(string[] args)
    {
        App emulator = new (args);
        emulator.Run();
    }
}