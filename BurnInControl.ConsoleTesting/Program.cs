// See https://aka.ms/new-console-template for more information

var numberstr="123.434e-3";
var number=double.Parse(numberstr);

var digitialString = "D55";
var digitalNumber = int.Parse(digitialString.Substring(1));
Console.WriteLine("Number: "+number);
Console.WriteLine("Digital Number: "+digitalNumber);