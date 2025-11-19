namespace QR.Wasm
{
    internal sealed class QRHelper
    {
        internal static string Generate(string text, int pixelsPerBlock)
        {
            return QR.Core.Generator.Generate(text, pixelsPerBlock);
        }
    }
}
