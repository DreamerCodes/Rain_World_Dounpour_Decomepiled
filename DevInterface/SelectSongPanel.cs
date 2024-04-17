using RWCustom;
using UnityEngine;

namespace DevInterface;

public class SelectSongPanel : Panel
{
	public SelectSongPanel(DevUI owner, DevUINode parentNode, Vector2 pos, string[] songNames)
		: base(owner, "Select_Song_Panel", parentNode, pos, new Vector2(20f + (float)Mathf.RoundToInt((float)songNames.Length / 32f + 0.5f) * 150f, 700f), "Select a song")
	{
		IntVector2 intVector = new IntVector2(0, 0);
		for (int i = 0; i < songNames.Length; i++)
		{
			subNodes.Add(new Button(owner, songNames[i], this, new Vector2(5f + (float)intVector.x * 150f, size.y - 25f - 20f * (float)intVector.y), 145f, songNames[i]));
			intVector.y++;
			if (intVector.y > 33)
			{
				intVector.x++;
				intVector.y = 0;
			}
		}
	}
}
