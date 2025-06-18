using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;

class Program
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

        byte[] buffer = new byte[256];
        int numBytesRead = await pipeServer.ReadAsync(buffer, 0, buffer.Length);

        string message = Encoding.UTF8.GetString(buffer, 0, numBytesRead);

        Console.WriteLine($"Received message: {message}");

        Console.WriteLine("Closing server");
    }
}
