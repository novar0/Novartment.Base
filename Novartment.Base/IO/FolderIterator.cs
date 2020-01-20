using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Novartment.Base.Collections;

namespace Novartment.Base.IO
{
	/// <summary>
	/// An e\Enumerator of directory objects, including the contents of nested directories.
	/// </summary>
	public class FolderIterator :
		IEnumerable<FileData>
	{
		private readonly string _baseFolder;
		private readonly string _fileNameFilter;

		/// <summary>
		/// Initializes a new instance of the FolderIterator class
		/// for enumeration in the specified directory.
		/// </summary>
		/// <param name="baseFolder">The directory whose content will be enumerated.</param>
		public FolderIterator (string baseFolder)
			: this (baseFolder, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the FolderIterator class
		/// for enumeration of the objects with specified name in the specified directory.
		/// Инициализирует новый экземпляр FolderIterator для перечисления объектов с указанным именем в указанной директории.
		/// </summary>
		/// <param name="baseFolder">The directory whose content will be enumerated.</param>
		/// <param name="fileNameFilter">
		/// The name of the object that is considered suitable.
		/// A full match is checked, case-insensitive.
		/// </param>
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
		/// Returns an enumerator for the directory objects.
		/// </summary>
		/// <returns>An enumerator for the directory objects.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Returns an enumerator for the directory objects.
		/// </summary>
		/// <returns>An enumerator for the directory objects.</returns>
		public IEnumerator<FileData> GetEnumerator ()
		{
			var stack = new ArrayList<string>
			{
				string.Empty,
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
