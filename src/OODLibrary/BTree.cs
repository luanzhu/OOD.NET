using System;
using System.Diagnostics;

namespace OOD.Imp.Storage
{
	/// <summary>
	/// A b-tree.
	/// </summary>
	public class BTree
	{

		private		uint				m_top_sid;
		private		ushort				m_order;
		private		SegmentManager		m_sgManager;
		private		IKey				m_keyFactory;
		private		BNode				m_nodeFactory;


		public BTree(uint topSid, SegmentManager sm, IKey keyFactory)
		{
			this.m_top_sid = topSid;
			this.m_sgManager = sm;
			this.m_keyFactory = keyFactory;
			this.m_nodeFactory = new BNode();
		}
#if DEBUG
		public	BNode		m_top
		{
			get 
			{
				return (BNode)m_sgManager.GetSegment(m_top_sid, m_nodeFactory, m_keyFactory);
			}
			set 
			{
				m_top_sid = value.SegmentID;
			}
		}
#else
				private		BNode		m_top
		{
			get 
			{
				return (BNode)m_sgManager.GetSegment(m_top_sid, m_nodeFactory, m_keyFactory);
			}
			set 
			{
				m_top_sid = value.SegmentID;
			}
		}
#endif
				 


		public ushort Order
		{
			get { return this.m_order;}
			set { this.m_order = value;}
		}

		public uint TopNodSegId
		{
			get { return m_top_sid;}
		}


		/// <summary>
		/// Search for target int this b-tree. target may have data field to be null, the result should have it non-null.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public IKey Search(IKey target)
		{
			IKey result = null;

			BNode n = m_top;
			bool finished = false;
			int pos = -1;
			while (!finished)
			{
				if (n.Leaf)
				{
					result = n.SearchKey(target, ref pos); //if key is found, result is not null, 
					                                       //otherwise, result is null and pos is the possible node where the key may be in
					if (result != null)
					{
						return result;
					}
					else
					{
						return null;
					}
				}
				else
				{
					result = n.SearchKey(target, ref pos);
					if (result == null)
					{
						uint nextNodeId = n.GetChildAt(pos);
						n = (BNode) m_sgManager.GetSegment(nextNodeId, m_nodeFactory, m_keyFactory);
					}
					else
					{
						return result;
					}
				}
			}
			return null;
		}


		/// <summary>
		/// Update the data part of the key.
		/// </summary>
		/// <param name="keyWithNewData"></param>
		/// <returns></returns>
		public bool UpdateData(IKey keyWithNewData)
		{
			IKey result = null;

			BNode n = m_top;
			bool finished = false;
			int pos = -1;
			while (!finished)
			{
				if (n.Leaf)
				{
					result = n.SearchKey(keyWithNewData, ref pos);
					if (result != null)
					{
						n.ReplaceData(pos, keyWithNewData);
						finished = true;
					}
					else
					{
						finished = true;
					}
				}
				else
				{
					result = n.SearchKey(keyWithNewData, ref pos);
					if (result == null)
					{
						uint nextNodeId = n.GetChildAt(pos);
						
						n = (BNode)m_sgManager.GetSegment(nextNodeId, m_nodeFactory, m_keyFactory);
					}
					else
					{
						n.ReplaceData(pos, keyWithNewData);
						finished = true;
					}
				}
			}
		
			return (result != null);
		}

		/// <summary>
		/// This event will be fired whenever the key in this node is moved to somewhere else.
		/// </summary>
		/// <remarks>This is only needed for OID indexing.</remarks>
		public event KeySegmentChangedEventHandler KeySegmentChanged;


		/// <summary>
		/// Insert the newKey into this B-tree, 
		/// </summary>
		/// <param name="newKey"></param>
		/// <returns></returns>
		public bool Insert(IKey newKey)
		{
			//find the leaf where the newKey should be in
			BNode n = m_top;
			System.Collections.Stack visited = new System.Collections.Stack();
			int pos = -1;
			while (!n.Leaf)
			{
				IKey temp = n.SearchKey(newKey, ref pos);
				if (temp == null)
				{
					uint nextNodeId = n.GetChildAt(pos);
					visited.Push(n);
					
					n = (BNode)m_sgManager.GetSegment(nextNodeId, m_nodeFactory, m_keyFactory);
				}
				else
					return false;
			}
			
			//now BNode n is the leaf where insert should happen
			IKey t_temp = n.SearchKey(newKey, ref pos);
			if (t_temp == null)
			{
				//not exists, go ahead to insert the new key
				if (!n.IsFull)
				{
					n.InsertAtLeaf(newKey, pos);

					return true;
				}
				else
				{
					//split needed for this node
					BNode right = new BNode();
					m_sgManager.GetNewSegment(right);
					IKey median = null;
					n.SplitAtLeaf(newKey, pos,  ref median, ref right); //this split is at leaf

					bool finished = false;					
					//now n holds the left half of the items, 
					//right holds the right half items, median is the middle key

					while (!finished)
					{			
						//parent is node middle key will be inserted
						BNode parent = (visited.Count >0 ? (BNode)visited.Pop() : null);

						if (parent == null)
						{
							//new top node is needed
							BNode new_top = new BNode();
							m_sgManager.GetNewSegment(new_top);
							new_top.SetOrder(m_top.Order);
							new_top.Leaf = false;
							new_top.InsertFirstKey(median, n.SegmentID, right.SegmentID);

							this.m_top_sid = new_top.SegmentID;

							return true;
						}
						else
						{
							IKey tt = parent.SearchKey(median, ref pos);
							if (tt != null)
								return false;

							if (!parent.IsFull)
							{
								parent.InsertAtInternal(median, pos, right.SegmentID);
								return true;
							}
							else
							{
								//parent is full again
								BNode newRight = new BNode();
								m_sgManager.GetNewSegment(newRight);
								newRight.SetOrder(parent.Order);
								newRight.Leaf = parent.Leaf;
								//this split will insert median into the parent, then split and new middle key is newMedian
								IKey newMedian;
								parent.SplitAtInternal(median, pos, right.SegmentID, out newMedian, newRight);

								n = parent;
								median = newMedian;
								right = newRight;
							}

						}

					}


				}
			}
			else
				return false;

			return false;
		}

		/// <summary>
		/// Given a key, search the node where the key is stored in the tree with root as the root node.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="key"></param>
		/// <param name="pos">the location the key is stored in</param>
		/// <param name="visisted">all the visisted parent is saved in the stack.</param>
		/// <returns></returns>
		public BNode FindNode(BNode root, IKey key, ref int pos, System.Collections.Stack visited, System.Collections.Stack via)
		{
			Debug.Assert(visited != null);

			BNode n = root;
			pos = -1;
			IKey temp = null;
			while (!n.Leaf)
			{
				temp = n.SearchKey(key, ref pos);
				if (temp == null)
				{
					uint nextNodeId = n.GetChildAt(pos);
					visited.Push(n);
					via.Push(pos);
					
					n = (BNode)m_sgManager.GetSegment(nextNodeId, m_nodeFactory, m_keyFactory);
				}
				else
					return n;
			}

			//n is leaf
			temp = n.SearchKey(key, ref pos);
			if (temp == null)
				return null;
			else
				return n;
		}

		/// <summary>
		/// Remove the key from the tree.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool Delete(IKey key)
		{
			int pos = -1;
			System.Collections.Stack visited = new System.Collections.Stack();
			System.Collections.Stack viaLinks = new System.Collections.Stack();
			
			//find the node which contains the key
			BNode n = FindNode(m_top, key, ref pos, visited, viaLinks);
			if (n == null)
			{
				return false;
			}
			else
			{
				if (n.Leaf)
				{
					n.RemoveAtLeaf(key, pos);
				}
				else
				{
					visited.Push(n);
					uint nextNodeId = n.GetChildAt(pos+1);
					viaLinks.Push(pos+1);
					BNode t_node = (BNode)m_sgManager.GetSegment(nextNodeId, m_nodeFactory, m_keyFactory);
					//find the leaf most leaf in its right sub-tree
					while (!t_node.Leaf)
					{
						visited.Push(t_node);
						nextNodeId = t_node.GetChildAt(0);
						viaLinks.Push(0);
						t_node = (BNode)m_sgManager.GetSegment(nextNodeId, m_nodeFactory, m_keyFactory);
					}
					Debug.Assert(t_node.Leaf);

					IKey successor = t_node.GetKeyAt(0);
                    
					//replace the key&data in n with the successor
					n.ReplaceKeyDataWSuccessor(key, successor, pos);

					//remove successor from the leaf node
					t_node.RemoveAtLeaf(successor, 0);

					n = t_node;
				}
			}

			//now n is the leaf node where the real deletion happens
			//visited keep all the parents visited so far, viaLinks keeps which links we followed
			while (n.IsUnderflow && n.SegmentID != m_top_sid)
			{

				BNode parent = (BNode)visited.Pop();
				//get left/right brother
				int followed = (int)viaLinks.Pop();
				BNode left = (followed>0? (BNode)m_sgManager.GetSegment(parent.GetChildAt(followed-1), m_nodeFactory, m_keyFactory) : null);
				BNode right = (followed<parent.KeyNums ? (BNode)m_sgManager.GetSegment(parent.GetChildAt(followed+1), m_nodeFactory, m_keyFactory) : null);

				Debug.Assert(left != null || right != null);

				bool combined = false;
				//try combin with right first
				if (right != null && right.KeyNums == right.ReqMinimum)
				{
					//combine with the right
					parent.CombineChildren(followed, n, right);
					Debug.Assert(right.KeyNums == 0);
					Debug.Assert(n.KeyNums > n.ReqMinimum);
					m_sgManager.FreeSegment(right);

					combined = true;

					if (parent.KeyNums == 0)
					{
						Debug.Assert(parent.Leaf == false);
						Debug.Assert(parent.SegmentID == this.m_top_sid);
						//tree will shrink
						this.m_top_sid = n.SegmentID;
						m_sgManager.FreeSegment(parent);
						break;
					}

				}
				else if (left != null && left.KeyNums == left.ReqMinimum)
				{
					//combine with the left
					parent.CombineChildren(followed-1, left, n);
					Debug.Assert(n.KeyNums == 0);
					Debug.Assert(left.KeyNums > left.ReqMinimum);
					m_sgManager.FreeSegment(n);

					combined = true;

					if (parent.KeyNums == 0)
					{
						Debug.Assert(parent.Leaf == false);
						Debug.Assert(parent.SegmentID == this.m_top_sid);
						//tree will shrink
						this.m_top_sid = left.SegmentID;
						m_sgManager.FreeSegment(parent);
						break;
					}

				}
				if (!combined)
				{
					//try redistrubute if combine is not possible
					if (right != null && right.KeyNums > right.ReqMinimum)
					{
						//redistribute one entry from right node
						parent.RedistributeRight2Left(followed, n, right);

					}
					else if (left != null &&  left.KeyNums > left.ReqMinimum)
					{
						//redistribute with left
						parent.RedistributeLeft2Right(followed-1, left, n);
					}

				}

				else
					n = parent;

			}

			return true;
		}

		#region enumerator

		public BTEnumerator GetEnumerator() 
		{
			return new BTEnumerator(this);
		}

		public class BTEnumerator 
		{
			private			BTree							m_tree;
			private			System.Collections.Queue		m_to_be_visited;
			private			IKey							m_current;
			private			BNode							m_current_node;
			private			int								m_current_key_no;

			public BTEnumerator(BTree tree) 
			{
				m_tree = tree;
				m_to_be_visited = new System.Collections.Queue();
				m_current = null;
				m_current_node = (BNode) m_tree.m_sgManager.GetSegment(m_tree.m_top_sid, m_tree.m_nodeFactory, m_tree.m_keyFactory);
				if (m_current_node.KeyNums >0)
				{
					for (int i=0; i<=m_current_node.KeyNums; i++)
					{
						m_to_be_visited.Enqueue(m_current_node.GetChildAt(i));
					}
				}
				m_current_key_no = -1;
			}

			public bool MoveNext() 
			{
				m_current_key_no ++;
				if (m_current_key_no < m_current_node.KeyNums)
				{
					m_current = m_current_node.GetKeyAt(m_current_key_no);
				}
				else
				{
					//this node is used up, try next one
					if (m_to_be_visited.Count >0)
					{
						m_current_node = (BNode)m_tree.m_sgManager.GetSegment(
							(uint)m_to_be_visited.Dequeue(),
							m_tree.m_nodeFactory,
							m_tree.m_keyFactory);
						Debug.Assert(m_current_node.KeyNums>0);
						if (!m_current_node.Leaf)
						{
							for (int i=0; i<=m_current_node.KeyNums; i++)
							{
								Debug.Assert(m_current_node.GetChildAt(i) >= 0);
								m_to_be_visited.Enqueue(m_current_node.GetChildAt(i));
							}
						}
						//locate the current to be the first one in the new node
						m_current_key_no = 0;
						m_current = m_current_node.GetKeyAt(0);
					}
					else
						m_current = null;
				}

				return m_current != null;
			}

			public IKey Current 
			{
				get 
				{
					return m_current;
				}
			}

		}
		#endregion
	}
}
