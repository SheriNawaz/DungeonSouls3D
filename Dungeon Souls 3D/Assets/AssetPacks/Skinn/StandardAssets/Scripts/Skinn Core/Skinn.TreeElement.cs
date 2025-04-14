using System;
using System.Collections.Generic;
using UnityEngine;

namespace CWM
{
	[Serializable]
	public class TreeElement
    {
		[SerializeField] string m_Name;
        [SerializeField] int m_ID;
        [SerializeField] int m_Depth;
		[NonSerialized] TreeElement m_Parent;
		[NonSerialized] List<TreeElement> m_Children;

		public int Depth
		{
			get { return m_Depth; }
			set { m_Depth = value; }
		}

		public TreeElement Parent
		{
			get { return m_Parent; }
			set { m_Parent = value; }
		}

		public List<TreeElement> Children
		{
			get { return m_Children; }
			set { m_Children = value; }
		}

		public bool HasChildren
		{
			get { return Children != null && Children.Count > 0; }
		}

		public string Name
		{
			get { return m_Name; } set { m_Name = value; }
		}

        public int ID
		{
			get { return m_ID; } set { m_ID = value; }
		}

		public TreeElement ()
		{
		}

		public TreeElement (string name, int depth, int id)
		{
			m_Name = name;
			m_ID = id;
			m_Depth = depth;
		}
    }

}


