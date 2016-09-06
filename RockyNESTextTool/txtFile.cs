using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

public class txtFile
{
    public ObservableCollection<txtData> data;

	public txtFile()
	{
        data = new ObservableCollection<txtData>();
        
	}

    public void addData(txtData TD)
    {
        data.Add(TD);

    }
    public void removeDataAt(int index)
    {
        data.RemoveAt(index);

    }
    public txtData getTxtDataAt(int index)
    {

        return data[index];

    }
}
