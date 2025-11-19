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

        Dictionary<byte, IXbox360Controller> controllers = new Dictionary<byte, IXbox360Controller>();

        try {
            const int PACKET_SIZE = 5;
            byte[] buf = new byte[PACKET_SIZE];

            while (true)
            {
                int n = pipeServer.Read(buf, 0, PACKET_SIZE);
                if (n < PACKET_SIZE) {
                    if (n == 0) {
                        Console.WriteLine("Pipe Closed.");
                        break;
                    }

                    Console.WriteLine("ERROR: Partial read");
                }

                // Process packet
                byte plr = buf[0];
                byte msgType = buf[1];
                byte inputId = buf[2];
                sbyte valX = (sbyte) buf[3];
                sbyte valY = (sbyte) buf[4];

                // Console.WriteLine("Plr=" + plr + ", type=" + msgType + ", inputID=" + inputId + ", x=" + valX + ", y=" + valY + "\t\t(" + buf + ")");
                // Console.WriteLine($"RAW buffer: {string.Join(",", buf)}");
                // Console.WriteLine("RAW: " +
                //     string.Join(",", buf.Select(b => b.ToString()))
                // );

                IXbox360Controller controller;
                switch (msgType)
                {
                    case (0x00):
                        // CONNECT
                        if (!controllers.TryGetValue(plr, out controller)) {
                            Console.WriteLine("\tConnect controller for player", plr);
                            controller = vigemClient.CreateXbox360Controller();
                            controller.Connect();
                            controllers[plr] = controller;
                        }
                        else
                        {
                            Console.WriteLine("\tPlayer reconnected:", plr);
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
                        break;

                    case (0x01):
                        // AXIS
                        // Console.WriteLine($"plr={plr}, dictionary keys=[{string.Join(",", controllers.Keys)}]");
                        Console.WriteLine(controllers.TryGetValue(plr, out controller));
                        try
                        {
                            controllers[plr].SetAxisValue(Xbox360Axis.LeftThumbX, inflateByteToShort(valX));
                            controllers[plr].SetAxisValue(Xbox360Axis.LeftThumbY, inflateByteToShort(valY));
                        }
                        catch
                        {
                            Console.WriteLine("Controller is not connected");
                        }
                        break;

                    case (0x02):
                        // BUTTON
                        break;

                    default:
                        Console.WriteLine("ERROR: Unknown Message Type");
                        break;
                }


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
                controllerPair.Value.SetButtonState(Xbox360Button.LeftShoulder, false);
                controllerPair.Value.SetButtonState(Xbox360Button.RightShoulder, false);
                controllerPair.Value.SetButtonState(Xbox360Button.Left, false);
                controllerPair.Value.SetButtonState(Xbox360Button.Right, false);
                controllerPair.Value.SetButtonState(Xbox360Button.A, false);
                controllerPair.Value.SetButtonState(Xbox360Button.B, false);
                controllerPair.Value.SetButtonState(Xbox360Button.Down, false);
                controllerPair.Value.SetButtonState(Xbox360Button.Start, false);
                
                controllerPair.Value.Disconnect();
            }
        }
    }

    private static short inflateByteToShort(sbyte num) {
        double normalized = num / 127.0;
        return (short) Math.Round(normalized * 32767);
    }   
}
