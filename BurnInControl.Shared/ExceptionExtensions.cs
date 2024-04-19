using System.Text;

namespace BurnInControl.Shared;

public static class ExceptionExtensions {
    public static string ToErrorMessage(this Exception ex) {
        var sb = new StringBuilder();
        sb.AppendLine(ex.Message);
        if (ex.InnerException != null) {
            sb.AppendLine(ex.InnerException.Message);
        }
        return sb.ToString();
    }
}