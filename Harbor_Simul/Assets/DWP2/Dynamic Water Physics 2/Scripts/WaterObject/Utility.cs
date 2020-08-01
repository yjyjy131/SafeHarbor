using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DWP2
{
	public static class Utility 
	{
		/// <summary>
		/// Returns a rigidbody if one found on itself or one of the parents.
		/// </summary>
		public static Rigidbody FindRootRigidbody(this Transform transform)
		{
			if (transform.GetComponent<Rigidbody>())
			{
				return transform.GetComponent<Rigidbody>();
			}

			if (transform.parent != null)
			{
				return FindRootRigidbody(transform.parent);
			}

			return null;
		}
	}
}

