using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace NetStudio.IPS.Entity;

internal class BindingListSync<T> : BindingList<T>
{
	private readonly SynchronizationContext? context;

	public BindingListSync()
	{
		context = SynchronizationContext.Current;
	}

	public BindingListSync(IList<T> list)
		: base(list)
	{
		context = SynchronizationContext.Current;
	}

	protected override void OnAddingNew(AddingNewEventArgs addingNewEventArgs_0)
	{
		 
		if (context == null)
		{
			base.OnAddingNew(addingNewEventArgs_0);
			return;
		}
		context.Send(delegate
		{
			base.OnAddingNew(addingNewEventArgs_0);
		}, null);
	}

	protected override void OnListChanged(ListChangedEventArgs listChangedEventArgs_0)
	{
	 
		if (context == null)
		{
			base.OnListChanged(listChangedEventArgs_0);
			return;
		}
		try
		{
			context.Send(delegate
			{
				base.OnListChanged(listChangedEventArgs_0);
			}, null);
		}
		catch (Exception)
		{
		}
	}
}
