using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace Barcode
{
    public class OutBarcode
    {
        public void start()
        {
            BarcodeWriter qrcode = new BarcodeWriter
            {
                // 出力するコードの形式をQRコードに選択
                //Format = BarcodeFormat.QR_CODE,
                Format = BarcodeFormat.ITF,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    // QRコードの信頼性
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.H,
                    // 日本語を表示したい場合シフトJISを指定
                    //CharacterSet = "Shift_JIS",
                    // デフォルト
                    CharacterSet = "ISO-8859-1",
                    // QRコードのサイズ決定
                    Height = 160,
                    Width = 160,
                    // QRコード周囲の余白の大きさ
                    Margin = 4,
                    // バーコードのみ表示するか
                    // falseにするとテキストも表示する
                    PureBarcode = false
                }
            };

            // バーコードBitmapを作成
            using (var bitmap = qrcode.Write("109050000937500100000190"))
            {
                // 画像として保存
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Barcode.bmp");
                bitmap.Save(filePath, ImageFormat.Bmp);
            }
        }

    }
}
