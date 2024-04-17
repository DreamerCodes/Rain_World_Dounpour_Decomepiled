using System.IO;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SelectDialogPanel : Panel
{
	private string[] dialogPaths;

	private int perpage;

	private int currentOffset;

	public SelectDialogPanel(DevUI owner, DevUINode parentNode, Vector2 pos, string[] dialogNames)
		: base(owner, "Select_Dialog_Panel", parentNode, pos, new Vector2(305f, 420f), "Select dialog")
	{
		dialogPaths = dialogNames;
		currentOffset = 0;
		perpage = 36;
		PopulateDialogButtons(currentOffset);
	}

	public void PopulateDialogButtons(int offset)
	{
		currentOffset = offset;
		foreach (DevUINode subNode in subNodes)
		{
			subNode.ClearSprites();
		}
		subNodes.Clear();
		IntVector2 intVector = new IntVector2(0, 0);
		for (int i = currentOffset; i < dialogPaths.Length && i < currentOffset + perpage; i++)
		{
			string fileName = Path.GetFileName(dialogPaths[i]);
			subNodes.Add(new Button(owner, dialogPaths[i], this, new Vector2(5f + (float)intVector.x * 150f, size.y - 25f - 20f * (float)intVector.y), 145f, fileName));
			intVector.y++;
			if (intVector.y >= (int)Mathf.Floor((float)perpage / 2f))
			{
				intVector.x++;
				intVector.y = 0;
			}
		}
		subNodes.Add(new Button(owner, "BackPage99288..?/~", this, new Vector2(5f, size.y - 25f - 20f * ((float)(perpage / 2) + 1f)), 145f, "Previous"));
		subNodes.Add(new Button(owner, "NextPage99288..?/~", this, new Vector2(155f, size.y - 25f - 20f * ((float)(perpage / 2) + 1f)), 145f, "Next"));
	}

	public void PrevPage()
	{
		currentOffset -= perpage;
		if (currentOffset < 0)
		{
			currentOffset = 0;
		}
		PopulateDialogButtons(currentOffset);
	}

	public void NextPage()
	{
		currentOffset += perpage;
		if (currentOffset > dialogPaths.Length)
		{
			currentOffset = perpage * (int)Mathf.Floor((float)dialogPaths.Length / (float)perpage);
		}
		PopulateDialogButtons(currentOffset);
	}
}
