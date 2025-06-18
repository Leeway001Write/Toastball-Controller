// See https://aka.ms/new-console-template for more information
using System;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

class Program
{
    static void Main()
    {
        using var client = new ViGEmClient();

        IXbox360Controller controller = client.CreateXbox360Controller();
        controller.Connect();

        Console.WriteLine("Xbox 360 Controller connected!");

        while (true)
        {
            controller.SetButtonState(Xbox360Button.RightShoulder, true);
            controller.SetButtonState(Xbox360Button.A, true);
            Thread.Sleep(500);

            controller.SetButtonState(Xbox360Button.RightShoulder, false);
            controller.SetButtonState(Xbox360Button.A, false);
            Thread.Sleep(500);
        }
    }
}
