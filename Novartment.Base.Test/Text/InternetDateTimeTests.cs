using System;
using System.Text;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class InternetDateTimeTests
	{
		[Fact]
		[Trait ("Category", "Text.InternetDateTime")]
		public void Parse ()
		{
			Assert.Equal (
				new DateTimeOffset (2012, 5, 15, 2, 49, 22, new TimeSpan (1, 0, 0)),
				InternetDateTime.Parse ("Tue, 15 May 2012 02:49:22 +0100"));
			Assert.Equal (
				new DateTimeOffset (2012, 5, 15, 2, 49, 22, new TimeSpan (1, 0, 0)),
				InternetDateTime.Parse ("15 May 2012 02:49:22 A"));
			Assert.Equal (
				new DateTimeOffset (1892, 6, 15, 23, 12, 0, new TimeSpan (-6, -30, 0)),
				InternetDateTime.Parse ("15 Jun 1892 23:12 -0630"));
		}

		[Fact]
		[Trait ("Category", "Text.InternetDateTime")]
		public void ParseException ()
		{
			Assert.Throws<FormatException> (() => InternetDateTime.Parse (string.Empty));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("first May 2012 02:49:22 +0100"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("Tue, 15"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("15 July 2012 02:49:22 +0100"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("15 May millenium 02:49:22 +0100"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("Tue, 15 May 2012"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("15 May 2012 a2:49:22 +0100"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("15 May 2012 02:b9:22 +0100"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("15 May 2012 02:49:c2 +0100"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("15 May 2012 02:49:22"));
			Assert.Throws<FormatException> (() => InternetDateTime.Parse ("15 May 2012 02:49:22 Moscow"));
		}

		[Fact]
		[Trait ("Category", "Text.InternetDateTime")]
		public void ToInternetString ()
		{
			Assert.Equal (
				"15 May 2012 07:49:22 +0600",
				new DateTimeOffset (2012, 5, 15, 7, 49, 22, new TimeSpan (6, 0, 0)).ToInternetString ());
			Assert.Equal (
				"15 May 2012 07:49:22 +0000",
				new DateTimeOffset (2012, 5, 15, 7, 49, 22, TimeSpan.Zero).ToInternetString ());
		}

		[Fact]
		[Trait ("Category", "Text.InternetDateTime")]
		public void ToInternetStringSpan ()
		{
			var buf = new char[100];

			var size = new DateTimeOffset (2012, 5, 15, 7, 49, 22, new TimeSpan (6, 0, 0)).ToInternetString (buf);
			Assert.Equal ("15 May 2012 07:49:22 +0600", new string (buf, 0, size));

			size = new DateTimeOffset (2012, 5, 15, 7, 49, 22, TimeSpan.Zero).ToInternetString (buf);
			Assert.Equal ("15 May 2012 07:49:22 +0000", new string (buf, 0, size));
		}

		[Fact]
		[Trait ("Category", "Text.InternetDateTime")]
		public void ToInternetUtf8String ()
		{
			var buf = new byte[100];

			var size = new DateTimeOffset (2012, 5, 15, 7, 49, 22, new TimeSpan (6, 0, 0)).ToInternetUtf8String (buf);
			Assert.Equal ("15 May 2012 07:49:22 +0600", Encoding.UTF8.GetString (buf, 0, size));

			size = new DateTimeOffset (2012, 5, 15, 7, 49, 22, TimeSpan.Zero).ToInternetUtf8String (buf);
			Assert.Equal ("15 May 2012 07:49:22 +0000", Encoding.UTF8.GetString (buf, 0, size));
		}
	}
}
