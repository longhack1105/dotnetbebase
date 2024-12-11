using QRCoder;
using System.Drawing;

namespace TWChatAppApiMaster.Utils
{
    public class QRCodeGeneratorService
    {
        public Bitmap GenerateQRCode(string key)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(key, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
    }
}
