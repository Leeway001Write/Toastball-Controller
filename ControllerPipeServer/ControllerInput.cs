using System;

using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

class ControllerInput
{
    static async Task Main()
    {
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



        using var vigemClient = new ViGEmClient();

        IXbox360Controller controller1 = vigemClient.CreateXbox360Controller();
        IXbox360Controller controller2 = vigemClient.CreateXbox360Controller();
        IXbox360Controller controller3 = vigemClient.CreateXbox360Controller();
        IXbox360Controller controller4 = vigemClient.CreateXbox360Controller();

        Dictionary<char, IXbox360Controller> controllers = new Dictionary<char, IXbox360Controller>();

        controllers['1'] = controller1;
        controllers['2'] = controller2;
        controllers['3'] = controller3;
        controllers['4'] = controller4;

        controller1.Connect();
        controller2.Connect();
        controller3.Connect();
        controller4.Connect();
        Thread.Sleep(100);
        Console.WriteLine("Controller connected");

        try {

            while (true) {
                byte[] buffer = new byte[256];
                int numBytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length);

                string message = Encoding.UTF8.GetString(buffer, 0, numBytesRead);

                Console.WriteLine($"Received message: {message}");
                if (message != "") {
                    char plrNumber = message[0];
                    char button = message[1];
                    char isPressed = message[2];

                    Console.WriteLine("plrNumber: " + plrNumber + ", button: " + button + ", isPressed: " + isPressed);

                    if (button == '0') {
                        // LEFT
                        if (isPressed == '1') {
                            controllers[plrNumber].SetButtonState(Xbox360Button.LeftShoulder, true);
                            controllers[plrNumber].SetButtonState(Xbox360Button.Left, true);
                        } else {
                            controllers[plrNumber].SetButtonState(Xbox360Button.LeftShoulder, false);
                            controllers[plrNumber].SetButtonState(Xbox360Button.Left, false);
                        }
                    } else if (button == '1') {
                        // RIGHT
                        if (isPressed == '1') {
                            controllers[plrNumber].SetButtonState(Xbox360Button.RightShoulder, true);
                            controllers[plrNumber].SetButtonState(Xbox360Button.Right, true);
                        } else {
                            controllers[plrNumber].SetButtonState(Xbox360Button.RightShoulder, false);
                            controllers[plrNumber].SetButtonState(Xbox360Button.Right, false);
                        }
                    } else if (button == '2') {
                        // BACK
                        if (isPressed == '1') {
                            controllers[plrNumber].SetButtonState(Xbox360Button.B, true);
                            controllers[plrNumber].SetButtonState(Xbox360Button.Start, true);
                        } else {
                            controllers[plrNumber].SetButtonState(Xbox360Button.B, false);
                            controllers[plrNumber].SetButtonState(Xbox360Button.Start, false);
                        }
                    } else if (button == '3') {
                        // MIDDLE
                        if (isPressed == '1') {
                            controllers[plrNumber].SetButtonState(Xbox360Button.Down, true);
                            controllers[plrNumber].SetButtonState(Xbox360Button.A, true);
                        } else {
                            controllers[plrNumber].SetButtonState(Xbox360Button.Down, false);
                            controllers[plrNumber].SetButtonState(Xbox360Button.A, false);
                        }
                    } else {
                        // NEXT
                        if (isPressed == '1') {
                            controllers[plrNumber].SetButtonState(Xbox360Button.A, true);
                        } else {
                            controllers[plrNumber].SetButtonState(Xbox360Button.A, false);
                        }
                    }
                }
            }
        } finally {
            controller1.Disconnect();
            controller2.Disconnect();
            controller3.Disconnect();
            controller4.Disconnect();
        }
    }
}
