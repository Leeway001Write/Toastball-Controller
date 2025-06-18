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

        IXbox360Controller controller1 = client.CreateXbox360Controller();
        IXbox360Controller controller2 = client.CreateXbox360Controller();
        controller1.Connect();
        controller2.Connect();

        Console.WriteLine("2 Xbox 360 Controllers connected!");

        while (true)
        {
            controller1.SetButtonState(Xbox360Button.RightShoulder, true);
            controller1.SetButtonState(Xbox360Button.A, true);
            controller2.SetButtonState(Xbox360Button.LeftShoulder, true);
            controller2.SetButtonState(Xbox360Button.A, true);
            Thread.Sleep(1000);

            controller1.SetButtonState(Xbox360Button.RightShoulder, false);
            controller1.SetButtonState(Xbox360Button.A, false);
            controller2.SetButtonState(Xbox360Button.LeftShoulder, false);
            controller2.SetButtonState(Xbox360Button.A, false);
            Thread.Sleep(500);
        }
    }
}
