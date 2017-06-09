using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using Novartment.Base.Collections;

namespace Novartment.Base.IO
{
	/// <summary>
	/// Перечислитель объектов директории, включая содержимое вложенных директорий.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	public class FolderIterator :
		IEnumerable<FileData>
	{
		private readonly string _baseFolder;
		private readonly string _fileNameFilter;

		/// <summary>
		/// Инициализирует новый экземпляр FolderIterator для перечисления объектов в указанной директории.
		/// </summary>
		/// <param name="baseFolder">Директория, в которой производить перечисление содержимого.</param>
		public FolderIterator (string baseFolder)
			: this (baseFolder, null)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр FolderIterator для перечисления объектов с указанным именем в указанной директории.
		/// </summary>
		/// <param name="baseFolder">Директория, в которой производить перечисление содержимого.</param>
		/// <param name="fileNameFilter">
		/// Имя объекта, которое считается пригодным.
		/// Проверяется полное совпадение без учёта регистра.
		/// </param>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1062:Validate arguments of public methods",
			MessageId = "1",
			Justification = "'fileNameFilter' validated indirectly.")]
		public FolderIterator (string baseFolder, string fileNameFilter)
		{
			if (baseFolder == null)
			{
				throw new ArgumentNullException (nameof (baseFolder));
			}

			var idx = fileNameFilter?.IndexOfAny (Path.GetInvalidFileNameChars ()) ?? -1;
			if (idx >= 0)
			{
				throw new ArgumentOutOfRangeException (nameof (fileNameFilter));
			}

			Contract.EndContractBlock ();

			_baseFolder = baseFolder;
			_fileNameFilter = !string.IsNullOrWhiteSpace (_fileNameFilter) ? fileNameFilter : null;
		}

		/// <summary>
		/// Возвращает перечислитель, выполняющий итерацию элементов.
		/// Каждый элемент представляет собой относительный по отношению к базовому путь к файлу.
		/// </summary>
		/// <returns>Интерфейс, который может использоваться для перебора элементов.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Возвращает перечислитель, выполняющий итерацию элементов.
		/// Каждый элемент представляет собой относительный по отношению к базовому путь к файлу.
		/// </summary>
		/// <returns>Интерфейс, который может использоваться для перебора элементов.</returns>
		public IEnumerator<FileData> GetEnumerator ()
		{
			var stack = new ArrayList<string>
			{
				string.Empty
			};
			while (stack.TryTakeLast (out string item))
			{
				var dirInfo = new DirectoryInfo (Path.Combine (_baseFolder, item));
				foreach (var subitem in dirInfo.EnumerateFileSystemInfos ())
				{
					var subItemRelativePath = Path.Combine (item, subitem.Name);
					if ((subitem.Attributes & FileAttributes.Directory) != 0)
					{
						stack.Add (subItemRelativePath);
					}
					else
					{
						var isFilterMatched = (_fileNameFilter == null) || subitem.Name.Equals (_fileNameFilter, StringComparison.OrdinalIgnoreCase);
						if (isFilterMatched)
						{
							var fileData = new FileData (
								_baseFolder,
								subItemRelativePath,
								subitem.FullName,
								subitem.Attributes,
								((FileInfo)subitem).Length,
								subitem.CreationTime,
								subitem.LastAccessTime,
								subitem.LastWriteTime);
							yield return fileData;
						}
					}
				}
			}
		}
	}
}
