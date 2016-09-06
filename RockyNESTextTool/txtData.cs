/*
 * Text Data
 * 
 * John welter
 * 2016
 * 
 * a block of text. includes a label and text data.
 * 
 * Label - the label of the text block to reference to
 * text - body of text. made up of characters (lower case) and commands (upper case)
 * 
 * can notify the main program if the label has changed to change it in the selection tabel as well
 * 
 * default construction uses program initialization text
 * 
*/
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
