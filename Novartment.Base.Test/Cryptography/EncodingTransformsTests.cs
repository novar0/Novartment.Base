using System;
using System.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class EncodingTransformsTests
	{
		// шаблон неструктурированных данных, передаваемых как application, image, audio или video
		private static readonly string Template1Html = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>\r\n	<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />\r\n	<title>Главная страница сайта ИТЦ</title>\r\n	<link href=\"/style/homepage.css\" rel=\"stylesheet\" type=\"text/css\" media=\"screen\" />\r\n</head>\r\n<body>\r\n	<h1><a href=\"#\">Исследовательско-технологический  центр</a></h1>\r\n	<p class=\"links\"><a href=\"/about/\">Подробнее<span style=\"color: #336699\">...</span></a></p>\r\n</body>\r\n</html>";
		private static readonly string Template1QuotedPrintable = "<!DOCTYPE=20html=20PUBLIC=20\"-//W3C//DTD=20XHTML=201.0=20Strict//EN\"=20\"htt=\r\np://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">=0D=0A<html=20xmlns=3D\"http=\r\n://www.w3.org/1999/xhtml\">=0D=0A<head>=0D=0A=09<meta=20http-equiv=3D\"conten=\r\nt-type\"=20content=3D\"text/html;=20charset=3Dutf-8\"=20/>=0D=0A=09<title>=D0=\r\n=93=D0=BB=D0=B0=D0=B2=D0=BD=D0=B0=D1=8F=20=D1=81=D1=82=D1=80=D0=B0=D0=BD=D0=\r\n=B8=D1=86=D0=B0=20=D1=81=D0=B0=D0=B9=D1=82=D0=B0=20=D0=98=D0=A2=D0=A6</titl=\r\ne>=0D=0A=09<link=20href=3D\"/style/homepage.css\"=20rel=3D\"stylesheet\"=20type=\r\n=3D\"text/css\"=20media=3D\"screen\"=20/>=0D=0A</head>=0D=0A<body>=0D=0A=09<h1>=\r\n<a=20href=3D\"#\">=D0=98=D1=81=D1=81=D0=BB=D0=B5=D0=B4=D0=BE=D0=B2=D0=B0=D1=\r\n=82=D0=B5=D0=BB=D1=8C=D1=81=D0=BA=D0=BE-=D1=82=D0=B5=D1=85=D0=BD=D0=BE=D0=\r\n=BB=D0=BE=D0=B3=D0=B8=D1=87=D0=B5=D1=81=D0=BA=D0=B8=D0=B9=20=20=D1=86=D0=B5=\r\n=D0=BD=D1=82=D1=80</a></h1>=0D=0A=09<p=20class=3D\"links\"><a=20href=3D\"/abou=\r\nt/\">=D0=9F=D0=BE=D0=B4=D1=80=D0=BE=D0=B1=D0=BD=D0=B5=D0=B5<span=20style=3D\"=\r\ncolor:=20#336699\">...</span></a></p>=0D=0A</body>=0D=0A</html>";
		private static readonly string Template1Base64 = "PCFET0NUWVBFIGh0bWwgUFVCTElDICItLy9XM0MvL0RURCBYSFRNTCAxLjAgU3RyaWN0Ly9FTiIg\r\nImh0dHA6Ly93d3cudzMub3JnL1RSL3hodG1sMS9EVEQveGh0bWwxLXN0cmljdC5kdGQiPg0KPGh0\r\nbWwgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGh0bWwiPg0KPGhlYWQ+DQoJPG1ldGEg\r\naHR0cC1lcXVpdj0iY29udGVudC10eXBlIiBjb250ZW50PSJ0ZXh0L2h0bWw7IGNoYXJzZXQ9dXRm\r\nLTgiIC8+DQoJPHRpdGxlPtCT0LvQsNCy0L3QsNGPINGB0YLRgNCw0L3QuNGG0LAg0YHQsNC50YLQ\r\nsCDQmNCi0KY8L3RpdGxlPg0KCTxsaW5rIGhyZWY9Ii9zdHlsZS9ob21lcGFnZS5jc3MiIHJlbD0i\r\nc3R5bGVzaGVldCIgdHlwZT0idGV4dC9jc3MiIG1lZGlhPSJzY3JlZW4iIC8+DQo8L2hlYWQ+DQo8\r\nYm9keT4NCgk8aDE+PGEgaHJlZj0iIyI+0JjRgdGB0LvQtdC00L7QstCw0YLQtdC70YzRgdC60L4t\r\n0YLQtdGF0L3QvtC70L7Qs9C40YfQtdGB0LrQuNC5ICDRhtC10L3RgtGAPC9hPjwvaDE+DQoJPHAg\r\nY2xhc3M9ImxpbmtzIj48YSBocmVmPSIvYWJvdXQvIj7Qn9C+0LTRgNC+0LHQvdC10LU8c3BhbiBz\r\ndHlsZT0iY29sb3I6ICMzMzY2OTkiPi4uLjwvc3Bhbj48L2E+PC9wPg0KPC9ib2R5Pg0KPC9odG1s\r\nPg==";

		// шаблон текста (в основном из ASCII символов), передаваемого как text. должен кодироваться с помощью Quoted Printable с минимальными искажениями
		private static readonly string Template2MostlyAsciiText = "Заголовок текста\r\n\r\n\r\n\r\n\r\n\r\nGetStringattObj.ContentDisposition.Param_FdileName ??\t\t\t\t\r\n\t\td in b   \r\n   ContentType.Param_Name01  23\r\n45\t6\r\n\t\tcoder.TransformFinalBlock (buf, nBlocks * coder.InputBlockSize, buf.Length - nBlocks * coder.InputBlockSize)";
		private static readonly string Template2QuotedPrintable = "=D0=97=D0=B0=D0=B3=D0=BE=D0=BB=D0=BE=D0=B2=D0=BE=D0=BA =D1=82=D0=B5=D0=BA=\r\n=D1=81=D1=82=D0=B0\r\n\r\n\r\n\r\n\r\n\r\nGetStringattObj.ContentDisposition.Param_FdileName ??\t\t\t=09\r\n\t\td in b  =20\r\n   ContentType.Param_Name01  23\r\n45\t6\r\n\t\tcoder.TransformFinalBlock (buf, nBlocks * coder.InputBlockSize, buf.Lengt=\r\nh - nBlocks * coder.InputBlockSize)";

		[Fact]
		[Trait ("Category", "Cryptography.CryptoTransform")]
		public void FromBase64Converter ()
		{
			ISpanCryptoTransform coder = new FromBase64Converter ();
			Assert.True (coder.CanTransformMultipleBlocks);
			var buf = Encoding.UTF8.GetBytes (Template1Base64);
			var result1 = new byte[buf.Length];
			var size = coder.TransformBlock (buf.AsSpan (0, 800), result1);
			var result2 = coder.TransformFinalBlock (buf.AsSpan (800, buf.Length - 800));
			var resutlStr = Encoding.UTF8.GetString (result1, 0, size) + Encoding.UTF8.GetString (result2.Span);
			Assert.Equal (Template1Html, resutlStr);
			coder.Dispose ();
		}

		[Fact]
		[Trait ("Category", "Cryptography.CryptoTransform")]
		public void ToQuotedPrintableWithLineBreaksTransform_Text ()
		{
			ISpanCryptoTransform coder = new ToQuotedPrintableWithLineBreaksConverter (true);
			Assert.True (coder.CanTransformMultipleBlocks);
			Assert.Equal (25, coder.InputBlockSize);
			Assert.Equal (78, coder.OutputBlockSize);
			var buf = Encoding.UTF8.GetBytes ("DOCTYPE-html PUBLIC*123456");
			var result1 = new byte[buf.Length * 4];
			int nBlocks = buf.Length / coder.InputBlockSize;
			var size = coder.TransformBlock (buf.AsSpan (0, nBlocks * coder.InputBlockSize), result1);
			var result2 = coder.TransformFinalBlock (buf.AsSpan (nBlocks * coder.InputBlockSize, buf.Length - (nBlocks * coder.InputBlockSize)));
			var resutlStr = Encoding.UTF8.GetString (result1, 0, size) + Encoding.UTF8.GetString (result2.Span);
			Assert.Equal ("DOCTYPE-html PUBLIC*123456", resutlStr);
			coder.Dispose ();

			coder = new ToQuotedPrintableWithLineBreaksConverter (true);
			buf = Encoding.UTF8.GetBytes (Template2MostlyAsciiText);
			result1 = new byte[buf.Length * 4];
			nBlocks = buf.Length / coder.InputBlockSize;
			size = coder.TransformBlock (buf.AsSpan (0, nBlocks * coder.InputBlockSize), result1);
			result2 = coder.TransformFinalBlock (buf.AsSpan (nBlocks * coder.InputBlockSize, buf.Length - (nBlocks * coder.InputBlockSize)));
			resutlStr = Encoding.UTF8.GetString (result1, 0, size) + Encoding.UTF8.GetString (result2.Span);
			Assert.Equal (Template2QuotedPrintable, resutlStr);
			coder.Dispose ();
		}

		[Fact]
		[Trait ("Category", "Cryptography.CryptoTransform")]
		public void FromQuotedPrintableTransform_Text ()
		{
			ISpanCryptoTransform coder = new FromQuotedPrintableConverter ();
			var buf = Encoding.UTF8.GetBytes (Template2QuotedPrintable);
			var result = new byte[buf.Length];
			var size = coder.TransformBlock (buf, result);
			Assert.Equal (Template2MostlyAsciiText, Encoding.UTF8.GetString (result, 0, size));
			coder.Dispose ();
		}

		[Fact]
		[Trait ("Category", "Cryptography.CryptoTransform")]
		public void ToQuotedPrintableWithLineBreaksTransform_Binary ()
		{
			ISpanCryptoTransform coder = new ToQuotedPrintableWithLineBreaksConverter (false);
			Assert.True (coder.CanTransformMultipleBlocks);
			Assert.Equal (25, coder.InputBlockSize);
			Assert.Equal (78, coder.OutputBlockSize);
			var buf = Encoding.UTF8.GetBytes (Template1Html);
			var result1 = new byte[buf.Length * 4];
			int nBlocks = buf.Length / coder.InputBlockSize;
			var size = coder.TransformBlock (buf.AsSpan (0, nBlocks * coder.InputBlockSize), result1);
			var result2 = coder.TransformFinalBlock (buf.AsSpan (nBlocks * coder.InputBlockSize, buf.Length - (nBlocks * coder.InputBlockSize)));
			var resutlStr = Encoding.UTF8.GetString (result1, 0, size) + Encoding.UTF8.GetString (result2.Span);
			Assert.Equal (Template1QuotedPrintable, resutlStr);
			coder.Dispose ();
		}

		[Fact]
		[Trait ("Category", "Cryptography.CryptoTransform")]
		public void FromQuotedPrintableTransform_Binary ()
		{
			ISpanCryptoTransform coder = new FromQuotedPrintableConverter ();
			var buf = Encoding.UTF8.GetBytes (Template1QuotedPrintable);
			var result = new byte[buf.Length];
			var size = coder.TransformBlock (buf, result);
			Assert.Equal (Template1Html, Encoding.UTF8.GetString (result, 0, size));
			coder.Dispose ();
		}

		[Fact]
		[Trait ("Category", "Cryptography.CryptoTransform")]
		public void ToBase64WithLineBreaksTransform ()
		{
			ISpanCryptoTransform coder = new ToBase64WithLineBreaksConverter ();
			Assert.True (coder.CanTransformMultipleBlocks);
			Assert.Equal (57, coder.InputBlockSize);
			Assert.Equal (78, coder.OutputBlockSize);
			var buf = Encoding.UTF8.GetBytes (Template1Html);
			var result1 = new byte[buf.Length * 2];
			var size = coder.TransformBlock (buf.AsSpan (0, 11 * 57), result1);
			var result2 = coder.TransformFinalBlock (buf.AsSpan (11 * 57, buf.Length - (11 * 57)));
			var resutlStr = Encoding.UTF8.GetString (result1, 0, size) + Encoding.UTF8.GetString (result2.Span);
			Assert.Equal (Template1Base64, resutlStr);
			coder.Dispose ();
		}
	}
}
