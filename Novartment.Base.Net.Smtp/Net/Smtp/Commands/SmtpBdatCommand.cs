using System;
using System.Globalization;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpBdatCommand : SmtpCommand
	{
		internal SmtpBdatCommand (long size, bool isLast)
			: base (SmtpCommandType.Bdat)
		{
			this.Size = size;
			this.IsLast = isLast;
		}

		internal SizeLimitedBufferedSource SourceData { get; private set; }

		internal long Size { get; }

		internal bool IsLast { get; }

		public override string ToString ()
		{
			return this.IsLast ?
				FormattableString.Invariant ($"BDAT {this.Size} LAST\r\n") :
				FormattableString.Invariant ($"BDAT {this.Size}\r\n");
		}

		internal static SmtpCommand Parse (ReadOnlySpan<char> value)
		{
			/*
			bdat-cmd = "BDAT" SP chunk-size [ SP end-marker ] CR LF
			chunk-size ::= 1*DIGIT
			end-marker ::= "LAST"

			Согласно стандарту, нельзя вернуть ошибку до начала передачи данных, поэтому
			любая ошибка в обработке BDAT - очень нехороший случай:
			клиент будет слать данные не дожидаясь ответа об ошибке,
			а мы не знаем сколько этих данных и будем считать их командами.
			Поэтому проявляем максимальную толерантность к нарушению формата этой команды.
			*/

			var pos = 0;
			var sizeTone = StructuredHeaderFieldLexicalToken.Parse (value, ref pos, AsciiCharClasses.Digit, false);
			if (sizeTone.TokenType != StructuredHeaderFieldLexicalTokenType.Value)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Bdat, "Unrecognized size parameter in 'BDAT' command.");
			}

			var sizeStr = value.Slice (sizeTone.Position, sizeTone.Length);
			long size;
			bool isLast;
			try
			{
#if NETCOREAPP2_1
				size = long.Parse (
					sizeStr,
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint,
					CultureInfo.InvariantCulture);
#else
				size = long.Parse (
					new string (sizeStr.ToArray ()),
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint,
					CultureInfo.InvariantCulture);
#endif

				// любые непробельные символы после размера считаем индикатором последней части
				isLast = StructuredHeaderFieldLexicalToken.Parse (value, ref pos, AsciiCharClasses.Visible, false).IsValid;
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (
					SmtpCommandType.Bdat,
					FormattableString.Invariant ($"Unrecognized size parameter in 'BDAT' command. {excpt.Message}"));
			}

			return new SmtpBdatCommand (size, isLast);
		}

		internal void SetSource (IBufferedSource source)
		{
			this.SourceData = new SizeLimitedBufferedSource (source, this.Size);
		}
	}
}
