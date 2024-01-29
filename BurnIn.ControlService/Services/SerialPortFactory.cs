using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.InteropServices;
namespace BurnIn.ControlService.Services;

public class SerialPortFactory {
    private const int BaudRate = 38400;
    /*private readonly Func<string, bool> _portPredicate = port => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? port.Contains("USB") : port.Contains("COM");*/

    public SerialPort Create() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            var portName=this.FindPort();
            if (!string.IsNullOrEmpty(portName)) {
                return new SerialPort(portName);
            } else {
                return new SerialPort();
            }
        } else {
            return new SerialPort("COM3", BaudRate);
        }
    }
    
    private string FindPort() {
        Process process = new Process();
        string fileName =@"/home/setiburnin/Documents/test.sh";
        //Console.WriteLine();
        ProcessStartInfo startInfo = new ProcessStartInfo { 
            FileName = fileName,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        process.StartInfo = startInfo;
        process.Start();
        var result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        var lines=result.Split('\n');
        var arduino=lines.FirstOrDefault(e => e.Contains("Arduino"));
        if (!string.IsNullOrEmpty(arduino)) {
            Console.WriteLine($"Found Arduino: ");
            int index=arduino.IndexOf('-');
            if (index >= 0) {
                var portName=arduino.Substring(0, index - 1);
                return portName;
            } else {
                return string.Empty;
            }
        } else {
            return string.Empty;
        }
    }
}