using Ardalis.SmartEnum;
namespace BurnInControl.UI.Data;

public class LedShape : SmartEnum<LedShape,string>{
    public static readonly LedShape LedCircle=new LedShape(nameof(LedCircle), "led");
    public static readonly LedShape LedRect=new LedShape(nameof(LedRect), "led-rectangle");

    public LedShape(String name, String value) : base(name, value) {  }
}