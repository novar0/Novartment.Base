using System;
using System.Text;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class DataEntityBodyTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void LoadAndGetData ()
		{
			var template1Html = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>\r\n	<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" />\r\n	<title>Главная страница сайта ИТЦ</title>\r\n	<link href=\"/style/homepage.css\" rel=\"stylesheet\" type=\"text/css\" media=\"screen\" />\r\n</head>\r\n<body>\r\n	<h1><a href=\"#\">Исследовательско-технологический  центр</a></h1>\r\n	<p class=\"links\"><a href=\"/about/\">Подробнее<span style=\"color: #336699\">...</span></a></p>\r\n</body>\r\n</html>";
			var template1QuotedPrintable = "<!DOCTYPE=20html=20PUBLIC=20\"-//W3C//DTD=20XHTML=201.0=20Strict//EN\"=20\"htt=\r\np://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">=0D=0A<html=20xmlns=3D\"http=\r\n://www.w3.org/1999/xhtml\">=0D=0A<head>=0D=0A=09<meta=20http-equiv=3D\"conten=\r\nt-type\"=20content=3D\"text/html;=20charset=3Dutf-8\"=20/>=0D=0A=09<title>=D0=\r\n=93=D0=BB=D0=B0=D0=B2=D0=BD=D0=B0=D1=8F=20=D1=81=D1=82=D1=80=D0=B0=D0=BD=D0=\r\n=B8=D1=86=D0=B0=20=D1=81=D0=B0=D0=B9=D1=82=D0=B0=20=D0=98=D0=A2=D0=A6</titl=\r\ne>=0D=0A=09<link=20href=3D\"/style/homepage.css\"=20rel=3D\"stylesheet\"=20type=\r\n=3D\"text/css\"=20media=3D\"screen\"=20/>=0D=0A</head>=0D=0A<body>=0D=0A=09<h1>=\r\n<a=20href=3D\"#\">=D0=98=D1=81=D1=81=D0=BB=D0=B5=D0=B4=D0=BE=D0=B2=D0=B0=D1=\r\n=82=D0=B5=D0=BB=D1=8C=D1=81=D0=BA=D0=BE-=D1=82=D0=B5=D1=85=D0=BD=D0=BE=D0=\r\n=BB=D0=BE=D0=B3=D0=B8=D1=87=D0=B5=D1=81=D0=BA=D0=B8=D0=B9=20=20=D1=86=D0=B5=\r\n=D0=BD=D1=82=D1=80</a></h1>=0D=0A=09<p=20class=3D\"links\"><a=20href=3D\"/abou=\r\nt/\">=D0=9F=D0=BE=D0=B4=D1=80=D0=BE=D0=B1=D0=BD=D0=B5=D0=B5<span=20style=3D\"=\r\ncolor:=20#336699\">...</span></a></p>=0D=0A</body>=0D=0A</html>";
			var template1Base64 = "PCFET0NUWVBFIGh0bWwgUFVCTElDICItLy9XM0MvL0RURCBYSFRNTCAxLjAgU3RyaWN0Ly9FTiIg\r\nImh0dHA6Ly93d3cudzMub3JnL1RSL3hodG1sMS9EVEQveGh0bWwxLXN0cmljdC5kdGQiPg0KPGh0\r\nbWwgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkveGh0bWwiPg0KPGhlYWQ+DQoJPG1ldGEg\r\naHR0cC1lcXVpdj0iY29udGVudC10eXBlIiBjb250ZW50PSJ0ZXh0L2h0bWw7IGNoYXJzZXQ9dXRm\r\nLTgiIC8+DQoJPHRpdGxlPtCT0LvQsNCy0L3QsNGPINGB0YLRgNCw0L3QuNGG0LAg0YHQsNC50YLQ\r\nsCDQmNCi0KY8L3RpdGxlPg0KCTxsaW5rIGhyZWY9Ii9zdHlsZS9ob21lcGFnZS5jc3MiIHJlbD0i\r\nc3R5bGVzaGVldCIgdHlwZT0idGV4dC9jc3MiIG1lZGlhPSJzY3JlZW4iIC8+DQo8L2hlYWQ+DQo8\r\nYm9keT4NCgk8aDE+PGEgaHJlZj0iIyI+0JjRgdGB0LvQtdC00L7QstCw0YLQtdC70YzRgdC60L4t\r\n0YLQtdGF0L3QvtC70L7Qs9C40YfQtdGB0LrQuNC5ICDRhtC10L3RgtGAPC9hPjwvaDE+DQoJPHAg\r\nY2xhc3M9ImxpbmtzIj48YSBocmVmPSIvYWJvdXQvIj7Qn9C+0LTRgNC+0LHQvdC10LU8c3BhbiBz\r\ndHlsZT0iY29sb3I6ICMzMzY2OTkiPi4uLjwvc3Bhbj48L2E+PC9wPg0KPC9ib2R5Pg0KPC9odG1s\r\nPg==";

			// text/html в кодировке binary
			var body = new DataEntityBody (ContentTransferEncoding.Binary);
			var templateBytes = Encoding.UTF8.GetBytes (template1Html);
			var result = new byte[templateBytes.Length];
			body.LoadAsync (new ArrayBufferedSource (Encoding.UTF8.GetBytes (template1Html)), null).Wait ();
			var buf1 = body.GetDataSource ().ReadAllBytesAsync ().Result;
			Assert.Equal (templateBytes.Length, buf1.Length);
			for (int i = 0; i < templateBytes.Length; i++)
			{
				Assert.Equal (templateBytes[i], buf1.Span[i]);
			}

			// application/octet-stream в кодировке quoted-printable
			body = new DataEntityBody (ContentTransferEncoding.QuotedPrintable);
			body.LoadAsync (new ArrayBufferedSource (Encoding.ASCII.GetBytes (template1QuotedPrintable)), null).Wait ();
			Array.Clear (result, 0, result.Length);
			var buf2 = body.GetDataSource ().ReadAllBytesAsync ().Result;
			Assert.Equal (templateBytes.Length, buf2.Length);
			for (int i = 0; i < templateBytes.Length; i++)
			{
				Assert.Equal (templateBytes[i], buf2.Span[i]);
			}

			// text/plain в кодировке base64
			body = new DataEntityBody (ContentTransferEncoding.Base64);
			body.LoadAsync (new ArrayBufferedSource (Encoding.ASCII.GetBytes (template1Base64)), null).Wait ();
			Array.Clear (result, 0, result.Length);
			var buf3 = body.GetDataSource ().ReadAllBytesAsync ().Result;
			Assert.Equal (templateBytes.Length, buf3.Length);
			for (int i = 0; i < templateBytes.Length; i++)
			{
				Assert.Equal (templateBytes[i], buf3.Span[i]);
			}
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void SetDataAndSave ()
		{
			// application/octet-stream в кодировке 7-bit
			var body = new DataEntityBody (ContentTransferEncoding.SevenBit);
			body.SetDataAsync (new ArrayBufferedSource (new byte[] { 48, 49, 50 })).Wait ();
			var bytes = new BinaryDestinationMock (128);
			body.SaveAsync (bytes).Wait ();
			Assert.Equal (5, bytes.Count);
			Assert.Equal ("012\r\n", Encoding.ASCII.GetString (bytes.Buffer.Slice (0, bytes.Count)));

			// image/png в кодировке quoted-printable
			body = new DataEntityBody (ContentTransferEncoding.QuotedPrintable);
			body.SetDataAsync (new ArrayBufferedSource (new byte[] { 2, 3, 4 })).Wait ();
			bytes = new BinaryDestinationMock (128);
			body.SaveAsync (bytes).Wait ();
			Assert.Equal (11, bytes.Count);
			Assert.Equal ("=02=03=04\r\n", Encoding.ASCII.GetString (bytes.Buffer.Slice (0, bytes.Count)));

			// text/plain в кодировке base64
			body = new DataEntityBody (ContentTransferEncoding.Base64);
			body.SetDataAsync (new ArrayBufferedSource (new byte[] { 48, 49, 50 })).Wait ();
			bytes = new BinaryDestinationMock (128);
			body.SaveAsync (bytes).Wait ();
			Assert.Equal (6, bytes.Count);
			Assert.Equal ("MDEy\r\n", Encoding.ASCII.GetString (bytes.Buffer.Slice (0, bytes.Count)));
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void SetDataEqualsGetData ()
		{
			var rnd = new Random ();
			int size = 500 + rnd.Next (8192);
			var template = new byte[size];
			var result = new byte[size];
			for (int i = 0; i < size; i++)
			{
				template[i] = (byte)rnd.Next (byte.MaxValue + 1);
			}

			var srcStream = new ArrayBufferedSource (template);

			// application/octet-stream в кодировке base64
			var body = new DataEntityBody (ContentTransferEncoding.Base64);
			body.SetDataAsync (srcStream).Wait ();
			var buf = body.GetDataSource ().ReadAllBytesAsync ().Result;
			Assert.Equal (template.Length, buf.Length);
			for (int i = 0; i < template.Length; i++)
			{
				Assert.Equal (template[i], buf.Span[i]);
			}

			// application/octet-stream quoted-printable
			body = new DataEntityBody (ContentTransferEncoding.QuotedPrintable);
			srcStream = new ArrayBufferedSource (template);
			body.SetDataAsync (srcStream).Wait ();
			buf = body.GetDataSource ().ReadAllBytesAsync ().Result;
			Assert.Equal (template.Length, buf.Length);
			for (int i = 0; i < template.Length; i++)
			{
				Assert.Equal (template[i], buf.Span[i]);
			}

			// application/octet-stream в кодировке binary
			body = new DataEntityBody (ContentTransferEncoding.Binary);
			srcStream = new ArrayBufferedSource (template);
			body.SetDataAsync (srcStream).Wait ();
			buf = body.GetDataSource ().ReadAllBytesAsync ().Result;
			Assert.Equal (template.Length, buf.Length);
			for (int i = 0; i < template.Length; i++)
			{
				Assert.Equal (template[i], buf.Span[i]);
			}
		}
	}
}
