using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Novartment.Base.IO
{
	/// <summary>
	/// Основные данные о файле.
	/// </summary>
	/// <remarks>В отличие от System.IO.FileInfo дополнительно содержит базовый и относительный путь.</remarks>
	public class FileData
	{
		/// <summary>
		/// Инициализирует новый экземпляр FileData на основе указанных данных.
		/// </summary>
		/// <param name="basePath">Базовая часть пути к файлу.</param>
		/// <param name="relativeName">Относительная часть пути к файлу.</param>
		/// <param name="fullPath">Полный путь к файлу.</param>
		/// <param name="attributes">Атрибуты файла.</param>
		/// <param name="length">Размер файла в байтах.</param>
		/// <param name="creationTime">Время время создания файла.</param>
		/// <param name="lastAccessTime">Время последнего доступа к файлу.</param>
		/// <param name="lastWriteTime">Время последней операции записи в файл.</param>
		public FileData (
			string basePath,
			string relativeName,
			string fullPath,
			FileAttributes attributes,
			long length,
			DateTime creationTime,
			DateTime lastAccessTime,
			DateTime lastWriteTime)
		{
			if (basePath == null)
			{
				throw new ArgumentNullException (nameof (basePath));
			}

			if (relativeName == null)
			{
				throw new ArgumentNullException (nameof (relativeName));
			}

			if (fullPath == null)
			{
				throw new ArgumentNullException (nameof (fullPath));
			}

			Contract.EndContractBlock ();

			this.BasePath = basePath;
			this.RelativeName = relativeName;
			this.FullPath = fullPath;
			this.Attributes = attributes;
			this.Length = length;
			this.CreationTime = creationTime;
			this.LastAccessTime = lastAccessTime;
			this.LastWriteTime = lastWriteTime;
		}

		/// <summary>
		/// Получает базовую часть пути к файлу.
		/// </summary>
		public string BasePath { get; }

		/// <summary>
		/// Получает относительную часть пути к файлу.
		/// </summary>
		public string RelativeName { get; }

		/// <summary>
		/// Получает полный путь к файлу.
		/// </summary>
		public string FullPath { get; }

		/// <summary>
		/// Получает атрибуты файла.
		/// </summary>
		public FileAttributes Attributes { get; }

		/// <summary>
		/// Получает размер файла в байтах.
		/// </summary>
		public long Length { get; }

		/// <summary>
		/// Получает время время создания файла.
		/// </summary>
		public DateTime CreationTime { get; }

		/// <summary>
		/// Получает время последнего доступа к файлу.
		/// </summary>
		public DateTime LastAccessTime { get; }

		/// <summary>
		/// Получает время последней операции записи в файл.
		/// </summary>
		public DateTime LastWriteTime { get; }
	}
}
