using System;
using System.IO;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// Disk file support for paging system.
	/// </summary>
	/// <remarks>
	/// The basic file read/write unit is the page.
	/// </remarks>
	public class DiskFile
	{

		public DiskFile(string FileName)
		{
			this.m_FileName = FileName;
			fd = new FileStream(
				this.m_FileName,
				FileMode.OpenOrCreate,
				FileAccess.ReadWrite,
				FileShare.None,
				5);
		}

		public void Flush()
		{
			fd.Flush();
		}

		public void Close()
		{

			fd.Close();
		}

		public long Length
		{
			get {return fd.Length;}
		}

		public void SetLength(long len)
		{
			fd.SetLength(len);
		}

		/// <summary>
		/// Synchronous read a page from disk file.
		/// </summary>
		/// <param name="buffer">The buffer which will hold the content of the page.</param>
		/// <param name="bufferOffset">start index of the buffer array.</param>
		/// <param name="fileOffset">start index of the file page.</param>
		/// <param name="count">Usually, count is the page size.</param>
		/// <returns>true if this is a new page in the file.</returns>
		public bool SynRead(byte[] buffer, int bufferOffset, long fileOffset, int count)
		{
			bool result = (this.fd.Length <= fileOffset ? true: false);

			fd.Seek(fileOffset, SeekOrigin.Begin);
			fd.Read(buffer, bufferOffset, count);

			return result;
		}

		public byte[] SynRead(long fileOffset, int length)
		{
			byte[] result = new byte[length];
			fd.Seek(fileOffset, SeekOrigin.Begin);
			fd.Read(result, 0, length);

			return result;
		}

		public void SynWrite(byte[] buffer, int bufferOffset, long fileOffset, int count)
		{
			fd.Seek(fileOffset, SeekOrigin.Begin);
			fd.Write(buffer, bufferOffset, count);
		}

		public void SynWriteByte(byte data, long fileOffset)
		{
			fd.Seek(fileOffset, SeekOrigin.Begin);
			fd.WriteByte(data);
		}

		private string m_FileName;
		private FileStream fd;
	}
}
