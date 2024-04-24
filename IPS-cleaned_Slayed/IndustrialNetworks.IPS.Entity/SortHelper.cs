using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NetStudio.IPS.Entity;

public static class SortHelper
{
	public static void SortAscending<T, P>(this BindingList<T> bindingList, Func<T, P> sortProperty)
	{
		
		bindingList.Sort(null, (T gparam_0, T gparam_1) => ((IComparable<P>)(object)sortProperty(gparam_0)).CompareTo(sortProperty(gparam_1)));
	}

	public static void SortDescending<T, P>(this BindingList<T> bindingList, Func<T, P> sortProperty)
	{
		
		bindingList.Sort(null, (T gparam_0, T gparam_1) => ((IComparable<P>)(object)sortProperty(gparam_1)).CompareTo(sortProperty(gparam_0)));
	}

	public static void Sort<T>(this BindingList<T> bindingList)
	{
		bindingList.Sort(null, null);
	}

	public static void Sort<T>(this BindingList<T> bindingList, IComparer<T> comparer)
	{
		bindingList.Sort(comparer, null);
	}

	public static void Sort<T>(this BindingList<T> bindingList, Comparison<T> comparison)
	{
		bindingList.Sort(null, comparison);
	}

	private static void Sort<T>(this BindingList<T> bindingList, IComparer<T> p_Comparer, Comparison<T> p_Comparison)
	{
		 
		List<T> sortList = new List<T>();
		bindingList.ForEach(delegate(T item)
		{
			sortList.Add(item);
		});
		if (p_Comparison == null)
		{
			sortList.Sort(p_Comparer);
		}
		else
		{
			sortList.Sort(p_Comparison);
		}
		bool raiseListChangedEvents = bindingList.RaiseListChangedEvents;
		bindingList.RaiseListChangedEvents = false;
		try
		{
			bindingList.Clear();
			sortList.ForEach(delegate(T item)
			{
				bindingList.Add(item);
			});
		}
		finally
		{
			bindingList.RaiseListChangedEvents = raiseListChangedEvents;
			bindingList.ResetBindings();
		}
	}

	public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		foreach (T item in source)
		{
			action(item);
		}
	}
}
