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
            PipeTransmissionMode.Message,
            PipeOptions.None
        );

        Console.WriteLine("Waiting for client...");
        await pipeServer.WaitForConnectionAsync();
        Console.WriteLine("Pipe connected!");



        using var vigemClient = new ViGEmClient();

        Dictionary<char, IXbox360Controller> controllers = new Dictionary<char, IXbox360Controller>();

        try {
            const int PACKET_SIZE = 5;
            byte[] buf = new byte[PACKET_SIZE];

            while (true)
            {
                int n = pipeServer.Read(buf, 0, PACKET_SIZE);
                if (n < PACKET_SIZE) {
                    Console.Write("ERROR: Partial read");
                }

                // Process packet
                byte plr = buf[0];
                byte msgType = buf[1];
                byte inputId = buf[2];
                sbyte valX = (sbyte) buf[3];
                sbyte valY = (sbyte) buf[4];

                Console.WriteLine("Plr=" + plr + ", type=" + msgType + ", inputID=" + inputId + ", x=" + valX + ", y=" + valY + "\t\t(" + buf + ")");
                // Console.WriteLine("RAW: " +
                //     string.Join(",", buf.Select(b => b.ToString()))
                // );


                /*
                byte[] buffer = new byte[256];
                int numBytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length);

                string message = Encoding.UTF8.GetString(buffer, 0, numBytesRead);

                Console.WriteLine($"Received message: {message}");
                if (message != "") {
                    char plrNumber = message[0];
                    if (message.Length == 1) {
                        Console.WriteLine("Player" + plrNumber + "action:");
                        IXbox360Controller controller;
                        if (!controllers.TryGetValue(plrNumber, out controller)) {
                            Console.WriteLine("\tCreate new controller");
                            controller = vigemClient.CreateXbox360Controller();
                            controller.Connect();
                            controllers[plrNumber] = controller;
                        }

                        Console.WriteLine("\tRelease buttons");
                        controller.SetButtonState(Xbox360Button.LeftShoulder, false);
                        controller.SetButtonState(Xbox360Button.RightShoulder, false);
                        controller.SetButtonState(Xbox360Button.Left, false);
                        controller.SetButtonState(Xbox360Button.Right, false);
                        controller.SetButtonState(Xbox360Button.A, false);
                        controller.SetButtonState(Xbox360Button.B, false);
                        controller.SetButtonState(Xbox360Button.Down, false);
                        controller.SetButtonState(Xbox360Button.Start, false);
                    } else if (message.Length == 3) {
                        char button = message[1];
                        char isPressed = message[2];;

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
                    } else if (message.Length == 4) {
                        // Joystick input
                        char axisId = message[1];
                        short axisX = JoyAxisToVigem(message[2]);
                        short axisY = JoyAxisToVigem(message[3]);

                        Console.WriteLine("Axes: " + message[2] + " " + message[3] + ", " + axisY);
                        controllers[plrNumber].SetAxisValue(Xbox360Axis.LeftThumbX, axisX);
                        controllers[plrNumber].SetAxisValue(Xbox360Axis.LeftThumbY, axisY);
                    } else {
                        Console.WriteLine("ERROR: Invalid WebSocket message (expected 1, 3, or 4 bytes):", message);
                    }
                }
                */
            }
        } finally {
            foreach (var controllerPair in controllers) {
                controllerPair.Value.Disconnect();
            }
        }
    }

    private static short JoyAxisToVigem(char num) {
        if (num == '0') {
            return -32768;
        }

        double input = (num - 4) / 4.0; // Convert 0 to 8 scale to -1 to 1

        return (short) Math.Floor(input * 32767);
    }   
}
