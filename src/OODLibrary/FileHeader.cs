using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// The class for file header.
	/// </summary>
	public class FileHeader
	{

		private		bool	m_normal_closed;
		private		uint	m_next_segment_id;
		private     uint	m_next_cid; //next class id
		private		uint	m_next_oid; //next object id
		private		short	m_version = 0;
		private		bool	m_dirty = false;
		private		uint	m_top_nodeSId_segTree; //the top node page id for the segment 
												   //b-tree which is used to manage the segments in the db file
		private		uint	m_top_nodeSId_spaceTree; //the top node page id for the free space tree
		private		uint	m_top_nodeSId_catalogTree;//the top node segment id for the catalog b-tree in the db file.
		
		private		bool	m_initialize_needed = false; // true if the db files are new and need to be initialized.


		public bool InitializeNeeded
		{
			get { return m_initialize_needed;}
		}

		public void SetInitializeNeeded()
		{
			m_initialize_needed = true;
		}

		public uint NextSegmentId
		{
			get { return m_next_segment_id;}
			set 
			{
				m_next_segment_id = value;
				m_dirty = true;
			}
		}

		public uint NextClassId
		{
			get { return m_next_cid;}
			set 
			{
				m_next_cid = value;
				m_dirty = true;
			}
		}

		public uint NextObjectId
		{
			get { return m_next_oid;}
			set
			{
				m_next_oid = value;
				m_dirty = true;
			}
		}

		public uint SegmentTreeTopNodeSId
		{
			get { return m_top_nodeSId_segTree;}
			set
			{
				m_top_nodeSId_segTree = value;
				m_dirty = true;
			}
		}

		public uint FreeSpaceTreeTopNodeSId
		{
			get { return m_top_nodeSId_spaceTree;}
			set 
			{
				m_top_nodeSId_spaceTree = value;
				m_dirty = true;
			}
		}

		public uint CatalogTreeTopNodeSId 
		{
			get { return m_top_nodeSId_catalogTree;}
			set 
			{
				m_top_nodeSId_catalogTree = value;
				m_dirty = true;
			}
		}

		public bool Dirty
		{
			get { return this.m_dirty;}
			set { this.m_dirty = value;}
		}


		public byte[] Serialize()
		{
			/* FORMAT:
			 * [normal closed?][version][next segment id][next class id][next object id]
			 *     1              2              4                 4             4 
			 * [top pid for segtree] [top pid for space tree] [ top sid for catalog tree]
			 *         4                      4                          4
			 */
			byte[] result = new byte[6 * 4 + 2 + 1];

			int pos = 0;

			result[pos++] = 0; //0 means not normal closed yet, set it to 1 is the last operation of the database.

			OOD.Utility.Bytes.Pack2(result, pos, m_version);
			pos += 2;

			OOD.Utility.Bytes.Pack4U(result, pos, m_next_segment_id);
			pos += 4;

			OOD.Utility.Bytes.Pack4U(result, pos, m_next_cid);
			pos+=4;

			OOD.Utility.Bytes.Pack4U(result, pos, m_next_oid);
			pos += 4;

			OOD.Utility.Bytes.Pack4U(result, pos, m_top_nodeSId_segTree);
			pos += 4;

			OOD.Utility.Bytes.Pack4U(result, pos, m_top_nodeSId_spaceTree);
			pos+=4;

			OOD.Utility.Bytes.Pack4U(result, pos, m_top_nodeSId_catalogTree);
			pos += 4;

			Debug.Assert(pos == result.Length);


			return result;
			
		}

		public void Deserialize(byte[] bytes, int offset, int count)
		{
			int pos = offset;
			m_normal_closed = (bytes[pos++] == 1);

			m_version = OOD.Utility.Bytes.Unpack2(bytes, pos);
			pos += 2;

			m_next_segment_id = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;

			m_next_cid = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;

			m_next_oid = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;

			m_top_nodeSId_segTree = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;

			m_top_nodeSId_spaceTree = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;

			m_top_nodeSId_catalogTree = OOD.Utility.Bytes.Unpack4U(bytes, pos);
			pos += 4;

			m_dirty = false;
			m_initialize_needed = false;
		}


		public void Deserialize(byte[] bytes)
		{
			Deserialize(bytes, 0, bytes.Length);
		}


		public FileHeader()
		{
		}
	}
}
