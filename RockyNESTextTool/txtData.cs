using System;
using System.ComponentModel;
using System.Linq.Expressions;

public class txtData : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private String Label;
    public String label
    {
        get { return this.Label; }
        set { this.SetAndNotify(ref this.Label, value, () => this.label); }

    }
    public String text { get; set; }
	public txtData()
	{
        label = "Welcome!";
        text = "open a file, or click add to get started!";

	}

    protected virtual void SetAndNotify<T>(ref T field, T value, Expression<Func<T>> property)
    {
        if (!object.ReferenceEquals(field, value))
        {
            field = value;
            this.OnPropertyChanged(property);
        }
    }

    protected virtual void OnPropertyChanged<T>(Expression<Func<T>> changedProperty)
    {
        if (PropertyChanged != null)
        {
            string name = ((MemberExpression)changedProperty.Body).Member.Name;
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }


    public override string ToString()
    {
        return label;
    }

}
