using UnityEngine;

namespace Menu;

public class GradientsContainer : MenuContainer
{
	public float alpha;

	public GradientsContainer(Menu menu, MenuObject owner, Vector2 pos, float alpha)
		: base(menu, owner, pos)
	{
		this.alpha = alpha;
	}

	public void CreateButtonGradients()
	{
		for (int i = 0; i < owner.subObjects.Count; i++)
		{
			if (owner.subObjects[i] is SimpleButton && !(owner.subObjects[i] is InputOptionsMenu.InputSelectButton))
			{
				subObjects.Add(new DarkGradient(menu, this, (owner.subObjects[i] as SimpleButton).pos + (owner.subObjects[i] as SimpleButton).size / 2f, 105f, 50f, alpha));
			}
			else if (owner.subObjects[i] is HoldButton)
			{
				subObjects.Add(new DarkGradient(menu, this, (owner.subObjects[i] as HoldButton).pos, 200f, 200f, Mathf.Pow(alpha, 0.65f)));
			}
		}
	}
}
