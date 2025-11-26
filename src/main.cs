using Chip8Vm.src;

class EmulatorMain
{
    static void Main(string[] args)
    {
        // TODO: arg parsing, conf loader
        byte[] program = [];

        try
        {
            program = File.ReadAllBytes("./program.bin");
        }
        catch (DirectoryNotFoundException e)
        {
            System.Console.WriteLine(e.Message);
            return;
        }

        Window      EmulatorWindow      = new("Chip8", 640, 320, 10);
        Interpreter EmulatorInterpreter = new(program);

        EmulatorInterpreter.Run();

        while (!EmulatorWindow.Exit)
        {
            EmulatorWindow.EventLoop();

            EmulatorInterpreter.Step();

            if (EmulatorInterpreter.DisplayUpdate == true)
            {
                EmulatorWindow.Draw(EmulatorInterpreter.DisplayBuffer);
                EmulatorInterpreter.DisplayUpdate = false;
            }

            EmulatorWindow.Present();
        }
    }
}