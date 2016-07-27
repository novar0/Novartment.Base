﻿using System;
using System.Text;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности с простым (discrete) текстовым содержимым,
	/// хранящимся в виде массива байтов.
	/// </summary>
	public class TextEntityBody : DataEntityBody,
		IDiscreteEntityBody
	{
		/// <summary>
		/// Набор символов содержимого по умолчанию.
		/// RFC 2046 4.1.2. The default character set, US-ASCII.
		/// </summary>
		[SuppressMessage ("Microsoft.Security",
			"CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
			Justification = "Encoding is immutable.")]
		public static readonly Encoding DefaultEncoding = Encoding.GetEncoding ("us-ascii");

		/// <summary>
		/// Получает кодировку символов, используемую в содержимом.
		/// Соответствует параметру "charset" поля заголовка "Content-Type" определённому в RFC 2046.
		/// </summary>
		public Encoding Encoding { get; }

		/// <summary>
		/// Инициализирует новый экземпляр класса TextEntityBody
		/// использующий указанные кодировки символов и передачи содержимого.
		/// </summary>
		/// <param name="encoding">Кодировка символов, используемая в содержимом.</param>
		/// <param name="transferEncoding">Кодировка передачи содержимого.</param>
		public TextEntityBody (Encoding encoding, ContentTransferEncoding transferEncoding)
			: base (transferEncoding)
		{
			this.Encoding = encoding ?? Encoding.UTF8;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса TextEntityBody
		/// использующий указанный набор символов и кодировку передачи содержимого.
		/// </summary>
		/// <param name="charset">Набор символов, используемая в содержимом.</param>
		/// <param name="transferEncoding">Кодировка передачи содержимого.</param>
		[SuppressMessage ("Microsoft.Maintainability",
			"CA1502:AvoidExcessiveComplexity",
			Justification = "Method not too complex.")]
		public TextEntityBody (string charset, ContentTransferEncoding transferEncoding)
			: base (transferEncoding)
		{
			if (charset == null)
			{
				this.Encoding = Encoding.UTF8;
			}
			else
			{
				// Handle custome/extended charsets, just remove "x-" from start.
				var isExtendedCharset = charset.StartsWith ("x-", StringComparison.OrdinalIgnoreCase);
				if (isExtendedCharset)
				{
					charset = charset.Substring (2);
				}

				// Cp1252 is not IANA registered, some mail clients send it, it equal to windows-1252.
				switch (charset.ToUpperInvariant ())
				{
					case "CP874": charset = "windows-874"; break;
					case "CP1250": charset = "windows-1250"; break;
					case "CP1251": charset = "windows-1251"; break;
					case "CP1252": charset = "windows-1252"; break;
					case "CP1253": charset = "windows-1253"; break;
					case "CP1254": charset = "windows-1254"; break;
					case "CP1255": charset = "windows-1255"; break;
					case "CP1256": charset = "windows-1256"; break;
					case "CP1257": charset = "windows-1257"; break;
					case "CP1258": charset = "windows-1258"; break;
				}
				this.Encoding = Encoding.GetEncoding (charset);
			}
		}

		/// <summary>
		/// Получает текст, который содержит тело сущности.
		/// </summary>
		public string GetText ()
		{
			var dataSrc = GetDataSource ();
			return BufferedSourceExtensions.ReadAllTextAsync (dataSrc, this.Encoding, CancellationToken.None).Result;
		}

		/// <summary>
		/// Устанавливает текст, который содержит тело сущности.
		/// </summary>
		/// <param name="value">Текст, который будет содержать тело сущности.</param>
		public void SetText (string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}
			Contract.EndContractBlock ();

			var bytes = this.Encoding.GetBytes (value);
			var dataSrc = new ArrayBufferedSource (bytes, 0, bytes.Length);
			SetDataAsync (dataSrc, CancellationToken.None).Wait ();
		}
	}
}
