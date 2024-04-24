using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace NetStudio.IPS.HMI;

public class BindingSourceRuntime : BindingSource
{
	private readonly SynchronizationContext? context;

	public BindingSourceRuntime()
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
		}
		else if (listChangedEventArgs_0.ListChangedType == ListChangedType.ItemChanged)
		{
			context.Post(delegate
			{
				base.OnListChanged(listChangedEventArgs_0);
			}, null);
		}
		else
		{
			context.Send(delegate
			{
				base.OnListChanged(listChangedEventArgs_0);
			}, null);
		}
	}
}
