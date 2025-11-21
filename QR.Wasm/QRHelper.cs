using QRCoder;
using System;

namespace QR.Wasm
{
    internal sealed class QRHelper
    {
        internal static string Generate(string text, int pixelsPerBlock)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

            BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
            return Convert.ToBase64String(qrCode.GetGraphic(pixelsPerBlock));
        }
    }
}
