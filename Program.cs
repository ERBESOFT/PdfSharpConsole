// See https://aka.ms/new-console-template for more information
using ConsoleAppPdf.Properties;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
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

class FontResolver : IFontResolver
{
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        switch (familyName)
        {
            case "Bahnschrift":
            case "Oswald-Light":
            case "Oswald-Regular":
            case "Oswald-Bold":
                return new FontResolverInfo(familyName); 
        }

        // We pass all other font requests to the default handler.
        // When running on a web server without sufficient permission, you can return a default font at this stage.
        return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
    }

    static byte[] LoadFontData(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Test code to find the names of embedded fonts
        var ourResources = assembly.GetManifestResourceNames();

        using (Stream? stream = assembly.GetManifestResourceStream(name))
        {
            if (stream == null)
                throw new ArgumentException("No resource with name " + name);

            int count = (int)stream.Length;
            byte[] data = new byte[count];
            stream.Read(data, 0, count);
            return data;
        }
    }

    /// <summary>
    /// Return the font data for the fonts.
    /// </summary>
    public byte[]? GetFont(string fontName)
    {
        try
        {
            return LoadFontData($"ConsoleAppPdf.Resources.{fontName}.ttf");
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static FontResolver? OurGlobalFontResolver = null;

    /// <summary>
    /// Ensure the font resolver is only applied once (or an exception is thrown)
    /// </summary>
    internal static void Apply()
    {
        if (OurGlobalFontResolver == null || GlobalFontSettings.FontResolver == null)
        {
            if (OurGlobalFontResolver == null)
                OurGlobalFontResolver = new FontResolver();

            GlobalFontSettings.FontResolver = OurGlobalFontResolver;
        }
    }
}


