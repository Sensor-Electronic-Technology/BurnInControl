using System.Text.Json;
using System.Threading.Channels;
using AsyncAwaitBestPractices;
using BurnInControl.Shared;
using BurnInControl.Shared.ComDefinitions.MessagePacket;
using BurnInControl.Shared.ComDefinitions.Station;
using ErrorOr;
using SerialPortLib;
using StationService.Infrastructure.SerialCom;

namespace BurnInControl.ConsoleTesting;

public class UsbTesting {
    SerialPortInput _serialPort= new SerialPortInput(true);
    
    public UsbTesting() {
        this._serialPort.SetPort("COM4",38400);
        this._serialPort.ConnectionStatusChanged += delegate(object sender, ConnectionStatusChangedEventArgs args)
        {
            Console.WriteLine("Connection Status: {0}", args.Connected);
        };
        this._serialPort.MessageLineReceived+= SerialPortOnMessageLineReceived;
    }
    
    private void SerialPortOnMessageLineReceived(Object sender, MessageReceivedLineEventArgs args) {
        /*if (args.Data.Contains('{')) {
            if (args.Data.IndexOf('{') == 0) {
                Console.WriteLine(args.Data);
            }
        }*/
        Console.WriteLine(args.Data);
    }
    
    public Task Send<TPacket>(StationMsgPrefix prefix,TPacket packet) where TPacket:IPacket {
        MessagePacket<TPacket> msgPacket = new MessagePacket<TPacket>() {
            Prefix = prefix,
            Packet = packet
        };
        var output = JsonSerializer.Serialize(msgPacket,
            new JsonSerializerOptions() {
                PropertyNamingPolicy =null,
                WriteIndented = false
            });
        Monitor.Enter(this._serialPort);//Lock object for thread safety
        try {
            this._serialPort.SendMessage(System.Text.Encoding.ASCII.GetBytes(output));//send bytes
        } catch(Exception e) {
            var exMessage = $"Exception: {e.Message}";
            if(e.InnerException != null) {
                exMessage += $" \n Inner: {e.InnerException.Message} ";
            }
            return Task.CompletedTask;
        }finally {
            Monitor.Exit(this._serialPort);
        }
        return Task.CompletedTask;
    }
    
    public Task Disconnect() {
        try {
            this._serialPort.Disconnect();
            return Task.CompletedTask;
        }  catch(Exception e) {
            var exMessage = $"Exception: {e.Message}";
            if(e.InnerException != null) {
                exMessage += $" \n Inner: {e.InnerException.Message} ";
            }
            Console.WriteLine(exMessage);
            return Task.CompletedTask;
        }
    }

    public Task Connect() {
        this._serialPort.Connect();
        return Task.CompletedTask;
    }
    

    

}