using Ardalis.SmartEnum;
namespace BurnInControl.UI.Data;

public class LedColor : SmartEnum<LedColor,string>{
    public static readonly LedColor LedRed=new LedColor(nameof(LedRed), "led-red");
    public static readonly LedColor LedGreen=new LedColor(nameof(LedGreen), "led-green");
    public static readonly LedColor LedYellow=new LedColor(nameof(LedYellow), "led-yellow");
    public static readonly LedColor LedBlue=new LedColor(nameof(LedBlue), "led-blue");
    public static readonly LedColor LedOrange=new LedColor(nameof(LedOrange), "led-orange");

    public LedColor(String name, String value) : base(name, value) {  }
}