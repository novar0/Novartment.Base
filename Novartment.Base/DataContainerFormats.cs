using System.Collections.Generic;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Collections.Linq;

namespace Novartment.Base
{
	/// <summary>
	/// Названия стандартных предопределённых форматов данных для передачи между приложениями.
	/// </summary>
	public static class DataContainerFormats
	{
		/// <summary>Microsoft Windows bitmap data format.</summary>
		public static readonly string Bitmap = "Bitmap";

		/// <summary>Comma-separated value (CSV) data format.</summary>
		public static readonly string CommaSeparatedValue = "CSV";

		/// <summary>Device-independent bitmap (DIB) data format.</summary>
		public static readonly string Dib = "DeviceIndependentBitmap";

		/// <summary>Windows Data Interchange Format (DIF) data format.</summary>
		public static readonly string Dif = "DataInterchangeFormat";

		/// <summary>Windows enhanced metafile format.</summary>
		public static readonly string EnhancedMetafile = "EnhancedMetafile";

		/// <summary>Windows file drop format.</summary>
		public static readonly string FileDrop = "FileDrop";

		/// <summary>HTML data format.</summary>
		public static readonly string Html = "HTML Format";

		/// <summary>Windows locale (culture) data format.</summary>
		public static readonly string Locale = "Locale";

		/// <summary>Windows metafile picture data format.</summary>
		public static readonly string MetafilePicture = "MetaFilePict";

		/// <summary>Standard Windows OEM text data format.</summary>
		public static readonly string OemText = "OEMText";

		/// <summary>Windows palette data format.</summary>
		public static readonly string Palette = "Palette";

		/// <summary>Windows pen data format.</summary>
		public static readonly string PenData = "PenData";

		/// <summary>Resource Interchange File Format (RIFF) audio data format.</summary>
		public static readonly string Riff = "RiffAudio";

		/// <summary>Rich Text Format (RTF) data format.</summary>
		public static readonly string Rtf = "Rich Text Format";

		/// <summary>Windows symbolic link data format.</summary>
		public static readonly string SymbolicLink = "SymbolicLink";

		/// <summary>ANSI text data format.</summary>
		public static readonly string Text = "Text";

		/// <summary>Tagged Image File Format (TIFF) data format.</summary>
		public static readonly string Tiff = "TaggedImageFileFormat";

		/// <summary>Unicode text data format.</summary>
		public static readonly string UnicodeText = "UnicodeText";

		/// <summary>Wave audio data format.</summary>
		public static readonly string WaveAudio = "WaveAudio";

		/// <summary>Extensible Application Markup Language (XAML) data format.</summary>
		public static readonly string Xaml = "Xaml";

		/// <summary>Extensible Application Markup Language (XAML) package data format.</summary>
		public static readonly string XamlPackage = "XamlPackage";

		/// <summary>Data format that encapsulates any type of serializable data objects.</summary>
		public static readonly string Serializable = "PersistentObject";

		/// <summary>Locations of one or more existing shell namespace objects.</summary>
		public static readonly string ShellIdList = "Shell IDList Array";

		private static readonly string FileName = "FileName";
		private static readonly string FileNameW = "FileNameW";
		private static readonly string StringFormat = "System.String";
		private static readonly string DrawingBitmapFormat = "System.Drawing.Bitmap";
		private static readonly string DrawingImagingMetafileFormat = "System.Drawing.Imaging.Metafile";
		private static readonly string WindowsMediaImagingBitmapSource = "System.Windows.Media.Imaging.BitmapSource";

		/// <summary>
		/// Получение списка названий форматов, в которые возможно автоматическое конвертирование указанного формата.
		/// </summary>
		/// <param name="format">Название исходного формата.</param>
		/// <returns>Список названий форматов, в которые возможно автоматическое конвертирование указанного формата.</returns>
		public static IReadOnlyList<string> GetMappedFormats (string format)
		{
			if (format == null)
			{
				return ReadOnlyList.Empty<string> ();
			}

			if ((format == DataContainerFormats.Text) ||
				(format == DataContainerFormats.UnicodeText) ||
				(format == DataContainerFormats.StringFormat))
			{
				return new ReadOnlyArray<string> (new[] { DataContainerFormats.Text, DataContainerFormats.UnicodeText, DataContainerFormats.StringFormat });
			}

			if ((format == DataContainerFormats.FileDrop) ||
				(format == DataContainerFormats.FileName) ||
				(format == DataContainerFormats.FileNameW))
			{
				return new ReadOnlyArray<string> (new[] { DataContainerFormats.FileDrop, DataContainerFormats.FileNameW, DataContainerFormats.FileName });
			}

			if ((format == DataContainerFormats.Bitmap) ||
				(format == DataContainerFormats.WindowsMediaImagingBitmapSource) ||
				(format == DataContainerFormats.DrawingBitmapFormat))
			{
				return new ReadOnlyArray<string> (new[] { DataContainerFormats.Bitmap, DataContainerFormats.DrawingBitmapFormat, DataContainerFormats.WindowsMediaImagingBitmapSource });
			}

			if ((format == DataContainerFormats.EnhancedMetafile) ||
				(format == DataContainerFormats.DrawingImagingMetafileFormat))
			{
				return new ReadOnlyArray<string> (new[] { DataContainerFormats.EnhancedMetafile, DataContainerFormats.DrawingImagingMetafileFormat });
			}

			return ReadOnlyList.Repeat (format, 1);
		}
	}
}
