using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Novartment.Base.IO
{
	/// <summary>
	/// Basic data about file.
	/// </summary>
	/// <remarks>
	/// Unlike System.IO.FileInfo additionally contains a base and relative path.
	/// </remarks>
	public sealed class FileData
	{
		/// <summary>
		/// Initializes a new instance of the FileData class
		/// with specified parameters.
		/// </summary>
		/// <param name="basePath">The base part of the path of the file.</param>
		/// <param name="relativeName">The relative part of the path of the file.</param>
		/// <param name="fullPath">The full path of the file.</param>
		/// <param name="attributes">The attributes for the file.</param>
		/// <param name="length">The size, in bytes, of the file.</param>
		/// <param name="creationTime">The creation time of the file.</param>
		/// <param name="lastAccessTime">The time the file was last accessed.</param>
		/// <param name="lastWriteTime">The time the file was last written to.</param>
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
		/// Gets the base part of the path of the file.
		/// </summary>
		public string BasePath { get; }

		/// <summary>
		/// Gets the relative part of the path of the file.
		/// </summary>
		public string RelativeName { get; }

		/// <summary>
		/// Gets the full path of the file.
		/// </summary>
		public string FullPath { get; }

		/// <summary>
		/// Gets the attributes for the file.
		/// </summary>
		public FileAttributes Attributes { get; }

		/// <summary>
		/// Gets the size, in bytes, of the file.
		/// </summary>
		public long Length { get; }

		/// <summary>
		/// Gets the creation time of the file.
		/// </summary>
		public DateTime CreationTime { get; }

		/// <summary>
		/// Gets the time the file was last accessed.
		/// </summary>
		public DateTime LastAccessTime { get; }

		/// <summary>
		/// Gets the time the file was last written to.
		/// </summary>
		public DateTime LastWriteTime { get; }
	}
}
