// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
namespace QR.Wasm;
public partial class QRCodeInterop
{
    [JSExport]
    public static string Generate(string text, int pixelsPerBlock)
    {
        return QRCode.Generate(text, pixelsPerBlock);
    }
}
