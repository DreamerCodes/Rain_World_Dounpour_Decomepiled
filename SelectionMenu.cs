using RWCustom;

public class SelectionMenu
{
	public int selectedItem;

	public string[] items;

	public string headline;

	public int lastVert;

	public bool lastEnterKey;

	public FLabel label;

	public SelectionMenu(string headline, string[] items)
	{
		this.headline = headline;
		this.items = items;
		label = new FLabel(Custom.GetFont(), "");
		label.x = 683f;
		label.y = 256f;
		Futile.stage.AddChild(label);
		UpdateLabel();
		lastEnterKey = true;
	}

	public void Update()
	{
		int num = 0;
		bool flag = false;
		if (num != lastVert)
		{
			ChangeSelection(-num);
		}
		if (flag && !lastEnterKey)
		{
			Select();
		}
		lastVert = num;
		lastEnterKey = flag;
	}

	private void ChangeSelection(int add)
	{
		selectedItem += add;
		if (selectedItem < 0)
		{
			selectedItem = items.Length - 1;
		}
		else if (selectedItem >= items.Length)
		{
			selectedItem = 0;
		}
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		string text = headline;
		text += "\r\n";
		text += "\r\n";
		for (int i = 0; i < items.Length; i++)
		{
			if (i == selectedItem)
			{
				text += ">";
			}
			text += items[i];
			if (i == selectedItem)
			{
				text += "<";
			}
			text += "\r\n";
		}
		label.text = text;
	}

	protected virtual void Select()
	{
	}
}
