// See https://aka.ms/new-console-template for more information

using BurnInControl.Data.ComponentConfiguration.ProbeController;
using System.Text.Json;
ProbeControllerConfig config = new ProbeControllerConfig();

var json=JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine("ProbeControllerConfig:");
Console.WriteLine(json);


/*var numberstr="123.434e-3";
var number=double.Parse(numberstr);

var digitialString = "D55";
var digitalNumber = int.Parse(digitialString.Substring(1));
Console.WriteLine("Number: "+number);
Console.WriteLine("Digital Number: "+digitalNumber);*/