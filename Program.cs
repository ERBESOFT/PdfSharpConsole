// See https://aka.ms/new-console-template for more information
using ConsoleAppPdf.Properties;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using ConsoleAppPdf;
using System.Reflection;
using System.Text.RegularExpressions;


static void DrawTableWithRowNumbers(XGraphics gfx, int rows, int columns, double startX, double startY, double cellWidth, double cellHeight, XPen borderPen, XFont font)
{
    for (int row = 0; row < rows; row++)
    {
        for (int col = 0; col < columns; col++)
        {
            double x = startX + (col * cellWidth);
            double y = startY + (row * cellHeight);

            // Hücreyi çiz
            gfx.DrawRectangle(borderPen, x, y, cellWidth, cellHeight);

            // İlk sütun için satır numarasını sağa dayalı olarak ekle
            if (col == 0)
            {
                string rowNumber = (row + 1).ToString();
                XRect textRect = new XRect(x, y, cellWidth, cellHeight);
                gfx.DrawString(rowNumber, font, XBrushes.Black, textRect, XStringFormats.CenterRight);
            }
        }
    }
}

void DrawRoundedRectangle(XGraphics gfx, XRect rect, double radius, XBrush brush)
{
    double diameter = radius * 2;

    // Yuvarlatılmış köşe yayları ve düz çizgilerle bir yol oluştur
    var path = new XGraphicsPath();

    // Yaylar ve çizgiler
    path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); // Sol üst köşe
    path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90); // Sağ üst köşe
    path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90); // Sağ alt köşe
    path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90); // Sol alt köşe

    // Yolu kapat
    path.CloseFigure();

    // Yuvarlatılmış dikdörtgeni çiz ve doldur
    gfx.DrawPath(brush, path);

    gfx.DrawPath(XPens.Black, path);
}

static void WrapPargraph(XGraphics gfx, XRect rect)
{
    // Dikdörtgeni çiz
    gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, rect.X, rect.Y, rect.Width, rect.Height);

    // Uzun metin
    string longText = "PdfSharp, PDF dosyaları oluşturmak ve düzenlemek için kullanılan güçlü bir kütüphanedir. "
                    + "Bu örnek, bir dikdörtgenin içine sığdırılmış ve otomatik olarak kaydırılmış uzun bir metni göstermektedir.";

    // Yazı tipi
    XFont font = new XFont("Oswald-Bold", 12, XFontStyleEx.Bold);


    // Metni dikdörtgene sığacak şekilde kaydır
    XTextFormatter tf = new XTextFormatter(gfx);

    XRect textRect = new XRect(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10);
    tf.Alignment = XParagraphAlignment.Justify; // Metni sola hizala
    tf.DrawString(longText, font, XBrushes.Black, textRect);
}

FontResolver.Apply();
// Yeni bir PDF belgesi oluştur
PdfDocument document = new PdfDocument();
document.Info.Title = "Rounded Rectangle Example";

// Bir sayfa ekle
PdfPage page = document.AddPage();

XFont font = new XFont("Times New Roman", 12, XFontStyleEx.Bold);

// Çizim için XGraphics nesnesi oluştur
using (XGraphics gfx = XGraphics.FromPdfPage(page))
{
    // Yuvarlak köşeli dikdörtgen çizmek için yöntem
    DrawRoundedRectangle(gfx, new XRect(50, 50, 200, 100), 20, XBrushes.CornflowerBlue);

    DrawTableWithRowNumbers(gfx, 5, 5, 10, 10, 100, 30, XPens.Gray, font);

    WrapPargraph(gfx, new XRect(10, 300, 300, 300));
}

// PDF'i kaydet
document.Save("filePath.pdf");
Console.WriteLine($"PDF başarıyla kaydedildi");



