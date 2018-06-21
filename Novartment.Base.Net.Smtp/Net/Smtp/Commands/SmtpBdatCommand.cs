using System;
using System.Globalization;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpBdatCommand : SmtpCommand
	{
		internal SmtpBdatCommand (IBufferedSource source, long size, bool isLast)
			: base (SmtpCommandType.Bdat)
		{
			this.SourceData = new SizeLimitedBufferedSource (source, size);
			this.Size = size;
			this.IsLast = isLast;
		}

		internal SizeLimitedBufferedSource SourceData { get; }

		internal long Size { get; }

		internal bool IsLast { get; }

		internal static SmtpCommand Parse (BytesChunkEnumerator chunkEnumerator, IBufferedSource source)
		{
			// Любая ошибка в обработке BDAT - очень нехороший случай,
			// потому что клиент будет слать данные не дожидаясь ответа об ошибке,
			// а мы не знаем сколько этих данных и будем считать их командами.
			// Поэтому проявляем максимальную толерантность к нарушению формата этой команды.
			var isSizeFound = chunkEnumerator.MoveToNextChunk (0x20, 0x20);
			if (!isSizeFound)
			{
				return new SmtpInvalidSyntaxCommand (SmtpCommandType.Bdat, "Missed size parameter in 'BDAT' command.");
			}

			long size;
			bool isLast;
			var sizeStr = chunkEnumerator.GetString ();
			try
			{
				size = long.Parse (
					sizeStr,
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowDecimalPoint,
					CultureInfo.InvariantCulture);

				// любые непробельные символы после размера считаем индикатором последней части
				isLast = chunkEnumerator.MoveToNextChunk (0x20, 0x20);
			}
			catch (FormatException excpt)
			{
				return new SmtpInvalidSyntaxCommand (
					SmtpCommandType.Bdat,
					FormattableString.Invariant ($"Unrecognized size parameter '{sizeStr}' in 'BDAT' command. {excpt.Message}"));
			}

			return new SmtpBdatCommand (source, size, isLast);
		}

		public override string ToString ()
		{
			return this.IsLast ?
				FormattableString.Invariant ($"BDAT {this.Size} LAST\r\n") :
				FormattableString.Invariant ($"BDAT {this.Size}\r\n");
		}
	}
}
