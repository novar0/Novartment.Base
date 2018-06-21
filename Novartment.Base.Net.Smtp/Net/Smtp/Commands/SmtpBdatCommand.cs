using System;
using System.Globalization;
using Novartment.Base.BinaryStreaming;

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

		internal void SetSource (IBufferedSource source)
		{
			this.SourceData = new SizeLimitedBufferedSource (source, this.Size);
		}

		internal static SmtpCommand Parse (ReadOnlySpan<char> value, BytesChunkEnumerator chunkEnumerator)
		{
			// Любая ошибка в обработке BDAT - очень нехороший случай,
			// потому что клиент будет слать данные не дожидаясь ответа об ошибке,
			// а мы не знаем сколько этих данных и будем считать их командами.
			// Поэтому проявляем максимальную толерантность к нарушению формата этой команды.
			var isSizeFound = chunkEnumerator.MoveToNextChunk (value, true, ' ');
			if (!isSizeFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Bdat, "Missed size parameter in 'BDAT' command.");
			}

			long size;
			bool isLast;
			var sizeStr = chunkEnumerator.GetString (value);
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
				isLast = chunkEnumerator.MoveToNextChunk (value, true, ' ');
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (
					SmtpCommandType.Bdat,
					FormattableString.Invariant ($"Unrecognized size parameter in 'BDAT' command. {excpt.Message}"));
			}

			return new SmtpBdatCommand (size, isLast);
		}

		public override string ToString ()
		{
			return this.IsLast ?
				FormattableString.Invariant ($"BDAT {this.Size} LAST\r\n") :
				FormattableString.Invariant ($"BDAT {this.Size}\r\n");
		}
	}
}
