using TypeShim;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("browser")]

namespace QR.Wasm
{
    [TsExport]
    public class QRCode
    {
        public static string Generate(string text, int pixelsPerBlock)
        {
            return QRHelper.Generate(text, pixelsPerBlock);
        }
    }
}

