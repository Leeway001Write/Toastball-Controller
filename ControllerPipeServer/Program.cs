using System;

using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

class Program
{
    static async Task Main()
    {
        using var vigemClient = new ViGEmClient();

        IXbox360Controller controller1 = vigemClient.CreateXbox360Controller();

        Dictionary<int, IXbox360Controller> controllers = new Dictionary<int, IXbox360Controller>();

        controller1.Connect();
        Thread.Sleep(100);
        controller1.SetButtonState(Xbox360Button.A, true);
        Thread.Sleep(100);
        controller1.SetButtonState(Xbox360Button.A, false);
        Console.WriteLine("Controller connected");

        var pipeName = "ControllerInputPipe";

        var user = WindowsIdentity.GetCurrent().User;
        if (user == null) {
            Console.WriteLine("Pipe server for controller input failed to start, because the script could not get the current Windows user. User object: ");
            Console.WriteLine(user);
        }
        var pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(
            user,
            PipeAccessRights.FullControl,
            AccessControlType.Allow
        ));
        Console.WriteLine("Creating pipe server...");
        using var pipeServer = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous
        );

        Console.WriteLine("Waiting for client...");
        await pipeServer.WaitForConnectionAsync();
        Console.WriteLine("Pipe connected!");

        while (true) {
            byte[] buffer = new byte[256];
            int numBytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length);

            string message = Encoding.UTF8.GetString(buffer, 0, numBytesRead);

            Console.WriteLine($"Received message: {message}");

            char plrNumber = message[0];
            char button = message[1];
            char isPressed = message[2];

            //Console.WriteLine("plrNumber: " + plrNumber + ", button: " + button + ", isPressed: " + isPressed);

            if (button == '0') {
                // LEFT
                if (isPressed == '1') {
                    controller1.SetButtonState(Xbox360Button.LeftShoulder, true);
                } else {
                    controller1.SetButtonState(Xbox360Button.LeftShoulder, false);
                }
            } else if (button == '1') {
                // RIGHT
                if (isPressed == '1') {
                    controller1.SetButtonState(Xbox360Button.RightShoulder, true);
                } else {
                    controller1.SetButtonState(Xbox360Button.RightShoulder, false);
                }
            } else if (button == '2') {
                // BACK
                if (isPressed == '1') {
                    controller1.SetButtonState(Xbox360Button.B, true);
                } else {
                    controller1.SetButtonState(Xbox360Button.B, false);
                }
            } else if (button == '3') {
                // MIDDLE
                if (isPressed == '1') {
                    controller1.SetButtonState(Xbox360Button.X, true);
                } else {
                    controller1.SetButtonState(Xbox360Button.X, false);
                }
            } else {
                // NEXT
                if (isPressed == '1') {
                    controller1.SetButtonState(Xbox360Button.A, true);
                } else {
                    controller1.SetButtonState(Xbox360Button.A, false);
                }
            }
        }
    }
}
