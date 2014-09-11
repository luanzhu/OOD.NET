using System;

namespace OOD.Imp.Storage
{
	/*  Note: 
	 *   1. the B-tree used in the segment manager is not save in the database file. 
	 *      the tree aint with the page usage is save in another seperate file.
	 *   2. each node in the b-tree in segment manager is 24*order - 11, given
	 *      the fact each page size is 512 bytes, the b-tree order will be 21.
	 *   3. the total nodes is 1>>17(128K), but the page usage map itself will use
	 *      (128K/8)/512 = 32 pages. In other words, the first 32 pages is reserved for
	 *      usage map, the first b-tree node will start with page id 32.
	 *   4. Given the bitmap size is 2^17 bites, 2^14 bytes = 16K bytes, this bitmap will be kept in 
	 *      memory always.
	 *   5. the last legal page id is 128k.
	 */

	/// <summary>
	/// The bit map for the usage of pages in segment manager's b-tree in the system.
	/// </summary>
	public class PageBitmap
	{
		private			byte[]			m_Data;
		private			bool			m_Dirty;
		private			int		    	m_Size = 1 << 14;


		public PageBitmap()
		{
			this.m_Data = new byte[this.m_Size];
		}

		//Get the first free page in the system.
		public uint GetFirstFreePageID()
		{
			int BNo = -1;
			int offset = -1;

			//get byte number
			for (int i=0 ; i<this.m_Data.Length; i++)
			{
				if (this.m_Data[i] != 0xFF)
				{
					BNo = i;
					break;
				}
			}
			
			if (BNo == -1)
				throw new OOD.Exception.OutofSpaceError(
					this,
					"No free page!");

			//find the bit offset in the byte
			byte b = this.m_Data[BNo];
			for (int i=0; i<8; i++)
			{
				if ( ((byte)(b >> i) & 1) == 0)
				{
					offset = i;
					break;
				}
			}
			
			if (offset == -1)
				throw new OOD.Exception.ProgramLogicError(
					this,
					"NO bit is 0 in the target byte.");
			uint result = (uint)(BNo * 8 + offset);

			return result;
		}

		//Claim the page with pid as Page ID for use
		public void SetPageTaken(uint pid)
		{
			int BNo = (int) (pid / 8);
			int offset = (int)(pid % 8);

			if ((this.m_Data[BNo] & (1 << offset)) != 0)
				throw new OOD.Exception.ProgramLogicError(
					this,
					"Trying to use a taken page.");

			this.m_Data[BNo] = (byte) (this.m_Data[BNo] | 1 << offset);
			this.m_Dirty = true;
		}

		public void SetPageFree(uint pid)
		{
			int BNo = (int) (pid / 8);
			int offset = (int) (pid % 8);

			if ((this.m_Data[BNo] & (1 << offset)) == 0)
				throw new OOD.Exception.ProgramLogicError(
					this,
					"Trying to free a free page.");
			this.m_Data[BNo] = (byte)(this.m_Data[BNo] ^ (1 << offset));

			this.m_Dirty = true;
		}

		//Return true if page pid is free
		public bool PageFree(uint pid)
		{
			int BNo = (int) (pid / 8);
			int offset = (int) (pid % 8);

			return ((this.m_Data[BNo] & ( 1 << offset)) == 0);
		}

		public byte[] Data
		{
			get { return this.m_Data;}
		}

		public bool Dirty
		{
			get { return this.m_Dirty;}
			set { this.m_Dirty = value;}
		}

	}
}
