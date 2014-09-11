using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// A b-tree node.
	/// </summary>
	public class BNode : Segment
	{

		private		short		m_order;
		private		IKey[]		m_keys;
		private		short		m_keyNums; //current key numbers
		private		uint[]		m_children;

		private		bool		m_leaf; // true if this node is a leaf



		public override void Initialize(uint segmentId)
		{
			this.m_segment_id = segmentId;
			this.m_order = 0;

			this.m_keys = null;
			this.m_children = null;

			this.m_keyNums = 0;
		}

		/* |[segment id][length]|[order][leaf][key numbers][key1 length][key1 content]...
		 *       4         4        2     1       2           2           key1 length
		 * [child1]....[child keyNumber + 1]
		 *    4               4
		 * 
		 * Note:
		 *	1. [segment id] and [length] will be saved by the segment manager, not by the node itself.
		 *  2. number of child pointer is always one bigger than number of keys in the node.
		 */ 
		public override byte[] Serialize()
		{
			int size = 0;
			//get the keys byte array first
			byte[][] Kbytes = new byte[m_keyNums][];
			for (int i=0; i<m_keyNums; i++)
			{
				Kbytes[i] = m_keys[i].Serialize();
				size += Kbytes[i].Length;
			}
			size += 2 + 1 + 2 + (m_keyNums << 1) + ((m_keyNums+1) << 2);

            byte[] result = new byte[size];
			int pos = 0;
			OOD.Utility.Bytes.Pack2(result,pos,m_order);
			pos += 2;
			result[pos++] = (byte)(m_leaf ? 1 : 0); //1 means true, 0 means false
			OOD.Utility.Bytes.Pack2(result,pos,m_keyNums);
			pos += 2;
			for (int i=0; i<m_keyNums; i++)
			{
				short length = (short)Kbytes[i].Length;
				OOD.Utility.Bytes.Pack2(result, pos, length);
				pos += 2;
				Array.Copy(Kbytes[i], 0, result, pos, length);
				pos += length;
			}
			for (int i=0; i<=m_keyNums; i++)
			{
				OOD.Utility.Bytes.Pack4U(result, pos, m_children[i]);
				pos += 4;
			}

			Debug.Assert(pos == size);

			return result;
		}

		/// <summary>
		/// Note:
		///  1. helper should be the realy key used in the b-tree.
		/// </summary>
		/// <param name="segmentId"></param>
		/// <param name="helper"></param>
		/// <param name="bytes"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		public override Segment Deserialize(uint segmentId, object helper, byte[] bytes, int offset, int count)
		{

			IKey keyFactory = (IKey)helper;
			int pos = offset;
			short order = OOD.Utility.Bytes.Unpack2(bytes, pos);

			BNode result = new BNode(segmentId, order);

			pos += 2;
			result.m_leaf = (bytes[pos++] == 1);
			result.m_keyNums = OOD.Utility.Bytes.Unpack2(bytes, pos);
			pos += 2;
			for (int i=0; i<result.m_keyNums; i++)
			{
				int length = OOD.Utility.Bytes.Unpack2(bytes, pos);
				pos += 2;
				result.m_keys[i] = keyFactory.Deserialize(bytes, pos, length);
				pos += length;
			}
			for (int i=0; i<=result.m_keyNums; i++)
			{
				result.m_children[i] = OOD.Utility.Bytes.Unpack4U(bytes, pos);
				pos += 4;
			}

			result.m_dirty = false;
			
			return result;
		}


		public override Segment Deserialize(uint segmentId, object helper, byte[] bytes)
		{
			return Deserialize(segmentId, helper, bytes, 0, bytes.Length);
		}

		
		/* constructors  */
		public BNode(uint segmentID, short order)
		{
			this.m_segment_id = segmentID;
			this.m_order = order;

			this.m_keys = new IKey[m_order - 1];
			this.m_children = new uint[m_order];

			this.m_keyNums = 0;

			this.m_dirty = true;
		}

		public BNode()
		{
			this.m_segment_id = 0;
			this.m_order = 0;
		}

		public void SetOrder(short order)
		{
			this.m_order = order;

			this.m_keys = new IKey[m_order - 1];
			this.m_children = new uint[m_order];

			this.m_keyNums = 0;

			this.m_dirty = true;
		}


		/* B-tree node related */
		public short Order
		{
			get { return this.m_order ; }
		}

		public override string ToString()
		{
			//used for debug
			string result="";
			for (int i=0; i<m_keyNums; i++)
				result += "         " + m_keys[i].ToString();
			result += "\r\n";
			for (int i=0; i<=m_keyNums; i++)
				result += "  " + m_children[i].ToString() + "  ";
            return result;
		}

		public bool Leaf
		{
			get { return this.m_leaf;}
			set 
			{ 
				this.m_leaf = value;
				this.m_dirty = true;
			}
		}

		public IKey GetKeyAt(int i)
		{
			Debug.Assert(i<this.m_keyNums);
			return this.m_keys[i];
		}

		public uint GetChildAt(int i)
		{
			Debug.Assert(i<=this.m_keyNums);
			return this.m_children[i];
		}

		public short KeyNums
		{
			get { return this.m_keyNums;}
		}

		public bool IsFull
		{
			get { return this.m_keyNums == m_order - 1;}
		}

		public bool IsUnderflow
		{
			get { return this.m_keyNums < (m_order - 1) >> 1;}
		}

		public ushort ReqMinimum
		{
			get { return (ushort)((m_order - 1) >> 1);}
		}

		/// <summary>
		/// Replace the data of the key at index i with the new data.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="keyWithNewData"></param>
		/// <returns></returns>
		public bool ReplaceData(int pos, IKey keyWithNewData)
		{
			Debug.Assert(pos < this.m_keys.Length);
			Debug.Assert(m_keys[pos].CompareTo(keyWithNewData) == 0);

			m_keys[pos].Data = keyWithNewData.Data;

			this.m_dirty = true;

			return true;
		}

		public bool ReplaceKeyDataWSuccessor(IKey oldKey, IKey successor, int pos)
		{
			Debug.Assert(pos < this.m_keys.Length);
			Debug.Assert(m_keys[pos].CompareTo(oldKey) == 0);
			Debug.Assert(oldKey.CompareTo(successor) < 0);

			m_keys[pos] = successor;

			this.m_dirty = true;

			return true;
		}

		/// <summary>
		/// Search the key in current node only, if key exits, return the key, otherwise, possibleNode is next node's 
		/// segment id where the key is possibly stored.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public IKey SearchKey(IKey target, ref int pos)
		{
			//Debug.Assert(this.m_keyNums >= ((m_order-1) >> 2));
			pos = 0;
			while (pos < this.m_keyNums && target.CompareTo(this.m_keys[pos])>0)
				pos ++;
			if (pos < this.m_keyNums && target.CompareTo(this.m_keys[pos])==0)
			{
				//found
				return this.m_keys[pos];
			}
			else
			{
				//not found
				return null;
			}
		}

		public void RemoveAtLeaf(IKey target, int pos)
		{
			Debug.Assert(this.m_keyNums <= (m_order -1));
			Debug.Assert(this.m_leaf);
			Debug.Assert(pos < m_keys.Length);

			Debug.Assert(m_keys[pos].CompareTo(target) == 0);

			for (int i=pos; i<this.m_keyNums - 1;i++)
			{
				m_keys[i] = m_keys[i+1];
			}
			this.m_keyNums--;

			this.m_dirty = true;
		}

		public void Remove(int pos)
		{
			Debug.Assert(this.m_keyNums <= m_order -1);
			Debug.Assert(pos < m_keys.Length);

			for (int i=pos; i<this.m_keyNums - 1;i++)
			{
				m_keys[i] = m_keys[i+1];
				m_children[i+1] = m_children[i+2];
			}
			this.m_keyNums--;

			this.m_dirty = true;
		}


		/// <summary>
		/// Insert new data into the leaf.
		/// </summary>
		/// <param name="newKey"></param>
		/// <param name="leftSon"></param>
		/// <param name="rightSon"></param>
		public void InsertAtLeaf(IKey newKey, int pos)
		{
			Debug.Assert(this.m_keyNums < m_order -1);
			Debug.Assert(this.m_leaf);
			Debug.Assert(pos < m_keys.Length);

			//shift later keys to the right
			for (int i=this.m_keyNums; i>pos; i--)
			{
				m_keys[i] = m_keys[i-1];
				m_children[i+1] = m_children[i];
			}
			           
			//insert the new key in the pos
			m_keys[pos] = newKey;
			m_children[pos] = 0;
			m_children[pos+1] = 0;
			m_keyNums ++;

			this.m_dirty = true;
		}

		/// <summary>
		/// Insert the first key into the B-tree node.
		/// </summary>
		/// <param name="first"></param>
		/// <param name="leftSon"></param>
		/// <param name="rightSon"></param>
		public void InsertFirstKey(IKey first, uint leftSon, uint rightSon)
		{
			Debug.Assert(this.m_keyNums == 0);

			m_keys[0] = first;
			m_children[0] = leftSon;
			m_children[1] = rightSon;

			this.m_keyNums ++;

			this.m_dirty = true;
		}

		/// <summary>
		/// insert a new key at internal node
		/// </summary>
		/// <param name="newKey"></param>
		/// <param name="pos"></param>
		/// <param name="rightSon"></param>
		public void InsertAtInternal(IKey newKey, int pos, uint rightSon)
		{
			Debug.Assert(this.m_leaf == false);
			Debug.Assert(this.IsFull == false);
			Debug.Assert(pos < m_keys.Length);
            
			for (int i=this.m_keyNums; i>pos; i--)
			{
				m_keys[i] = m_keys[i-1];
				m_children[i+1] = m_children[i];
			}

			//insert the new key along with the right pointer
			m_keys[pos] = newKey;
			m_children[pos+1] = rightSon;

			m_keyNums++;

			this.m_dirty = true;
		}

		/// <summary>
		/// split the leaf into two, leave left half in current leaf, and move right half into right node
		/// </summary>
		/// <param name="newKey"></param>
		/// <param name="median">the middle key is popped up</param>
		/// <param name="right"></param>
		public void SplitAtLeaf(IKey newKey, int pos, ref IKey median, ref BNode right)
		{
			Debug.Assert(this.m_leaf);
			Debug.Assert(this.IsFull);

			right.SetOrder(this.m_order);
			right.m_leaf = this.m_leaf;
			
			int mid = m_order >> 1;

			//copy current keys into a bigger array
			IKey[] temp = new IKey[m_order];
			Array.Copy(m_keys,0,temp,0,m_order - 1);
			//insert the newKey into the pos location
			//shift later keys to the right
			for (int i=this.m_keyNums; i>pos; i--)
			{
				temp[i] = temp[i-1];
			}

			temp[pos] = newKey;

			median = temp[mid];

			//copy left half to current leaf
			for (int i=0; i<mid; i++)
			{
				m_keys[i] = temp[i];
			}
			//copy right half to right leaf
			for (int i=mid+1; i<temp.Length; i++)
			{
				right.m_keys[i-mid-1] = temp[i];
			}

			m_keyNums = (short)mid;
			right.m_keyNums = (short)(m_order - 1 - mid);

			this.m_dirty = true;
			right.m_dirty = true;
		}

		/// <summary>
		/// Split the internal node with the newKey which is trying to insert this internal node
		/// </summary>
		/// <param name="newKey"></param>
		/// <param name="pos"></param>
		/// <param name="right"></param>
		/// <param name="newMedian"></param>
		/// <param name="newRight"></param>
		public void SplitAtInternal(IKey newKey, int pos, uint rightSon, out IKey newMedian, BNode newRight)
		{
			Debug.Assert(this.m_leaf == false);
			Debug.Assert(this.IsFull);

			IKey[] temp = new IKey[m_order];
			uint[] t_pointer = new uint[m_order + 1];

			//copy old data into new arrays
			Array.Copy(m_keys, 0, temp, 0, m_order-1);
			Array.Copy(m_children, 0, t_pointer, 0, m_order);

			//shift everything after this key right
			for (int i=this.m_keyNums; i>pos; i--)
			{
				temp[i] = temp[i-1];
				t_pointer[i+1] = t_pointer[i];
			}

			temp[pos] = newKey;
			t_pointer[pos+1] = rightSon;

			//copy left half back this node
			int mid = m_order >> 1;

			Array.Copy(temp,0,m_keys,0,mid);
			Array.Copy(t_pointer,0,m_children,0,mid+1);

			newMedian = temp[mid];

			Array.Copy(temp,mid+1,newRight.m_keys,0,m_order-mid-1);
			Array.Copy(t_pointer,mid+1,newRight.m_children,0,m_order-mid);

			m_keyNums = (short)mid;
            newRight.m_keyNums = (short)(m_order - mid -1);

			this.m_dirty = true;
			newRight.m_dirty = true;
		}

		/// <summary>
		/// Combine the left, right, along with the key in pos of this node into the new one, 
		/// and keep left, clear right one.
		/// </summary>
		/// <param name="pos">the location of the connecting key in this node(parent)</param>
		/// <param name="left"></param>
		/// <param name="right"></param>
		public void CombineChildren(int pos, BNode left, BNode right)
		{
			Debug.Assert(pos >=0 && pos < m_keys.Length);
			Debug.Assert(m_children[pos] == left.SegmentID);
			Debug.Assert(m_children[pos+1] == right.SegmentID);
			Debug.Assert(m_keys[pos].CompareTo(left.m_keys[left.m_keyNums-1])>0);
			Debug.Assert(m_keys[pos].CompareTo(right.m_keys[right.m_keyNums-1])<0);

			left.m_keys[left.m_keyNums] = m_keys[pos];
			left.m_children[left.m_keyNums+1] = right.m_children[0];
			left.m_keyNums ++ ;
			for (int i=0; i<right.m_keyNums; i++)
			{
				left.m_keys[left.m_keyNums++] = right.m_keys[i];
				left.m_children[left.m_keyNums] = right.m_children[i+1];
			}

			right.m_keyNums = 0;

			//remove pos record in the parent
			Remove(pos);

			this.m_dirty = true;
            left.m_dirty = true;
	        right.m_dirty = true;
		}


		/// <summary>
		/// Redistribute one from right to left
		/// </summary>
		/// <param name="pos">pos is pointing the middle connecting key in between the left and right.</param>
		/// <param name="left"></param>
		/// <param name="right"></param>
		public void RedistributeRight2Left(int pos, BNode left, BNode right)
		{
			Debug.Assert(pos >=0 && pos < m_keys.Length);
			Debug.Assert(m_children[pos] == left.SegmentID);
			Debug.Assert(m_children[pos+1] == right.SegmentID);
			Debug.Assert(m_keys[pos].CompareTo(left.m_keys[left.m_keyNums-1])>0);
			Debug.Assert(m_keys[pos].CompareTo(right.m_keys[right.m_keyNums-1])<0);

			//copy middle key in parent to tail of left
			left.m_keys[left.m_keyNums++] = m_keys[pos];
			left.m_children[left.m_keyNums] = right.m_children[0]; //move first pointer in right to be the last in new left
            
			//replace the middle key with the first key in right
			m_keys[pos] = right.m_keys[0];

			//remove the first key along with the first pointer from right
			for (int i=1; i<right.KeyNums; i++)
			{
				right.m_keys[i-1] = right.m_keys[i];
				right.m_children[i-1] = right.m_children[i];
			}
			right.m_keyNums --;
			//move the last one
			right.m_children[right.m_keyNums] = right.m_children[right.m_keyNums+1];

			this.m_dirty = true;
			left.m_dirty = true;
			right.m_dirty = true;
		}

		/// <summary>
		/// Distribute one key from left to right
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="left"></param>
		/// <param name="right"></param>
		public void RedistributeLeft2Right(int pos, BNode left, BNode right)
		{
			Debug.Assert(pos >=0 && pos < m_keys.Length);
			Debug.Assert(m_children[pos] == left.SegmentID);
			Debug.Assert(m_children[pos+1] == right.SegmentID);
			Debug.Assert(m_keys[pos].CompareTo(left.m_keys[left.m_keyNums-1])>0);
			Debug.Assert(m_keys[pos].CompareTo(right.m_keys[right.m_keyNums-1])<0);

			Debug.Assert(right.KeyNums < m_order - 1);

			//shift everything in right for the new key
			for (int i=right.KeyNums; i>0; i--)
			{
				right.m_keys[i] = right.m_keys[i-1];
				right.m_children[i+1] = right.m_children[i]; 
			}
			right.m_children[1] = right.m_children[0];

			//copy middle key in parent to the first one of right
			right.m_keys[0] = m_keys[pos];
			right.m_children[0] = left.m_children[left.KeyNums]; //copy last pointer to be the first pointer in new right node
			right.m_keyNums++;

			//copy the last key in left into parent
			m_keys[pos] = left.m_keys[left.m_keyNums-1];

			//remove the last key from left
			left.m_keyNums--;

			this.m_dirty = true;
			left.m_dirty = true;
			right.m_dirty = true;
		}
	}
}
