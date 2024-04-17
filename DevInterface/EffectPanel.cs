using UnityEngine;

namespace DevInterface;

public class EffectPanel : Panel
{
	public class EffectPanelSlider : Slider
	{
		public RoomSettings.RoomEffect effect => (parentNode as EffectPanel).effect;

		public EffectPanelSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
			: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
		{
		}

		public override void Refresh()
		{
			base.Refresh();
			float num = 0f;
			string iDstring = IDstring;
			if (iDstring != null && iDstring == "Amount_Slider")
			{
				num = effect.amount;
			}
			base.NumberText = (int)(num * 100f) + "%";
			RefreshNubPos(num);
		}

		public override void NubDragged(float nubPos)
		{
			if (!effect.inherited)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Amount_Slider")
				{
					effect.amount = nubPos;
					if (effect.type == RoomSettings.RoomEffect.Type.VoidMelt)
					{
						owner.room.game.cameras[0].levelGraphic.alpha = effect.amount;
						if (owner.room.game.cameras[0].fullScreenEffect != null)
						{
							owner.room.game.cameras[0].fullScreenEffect.alpha = effect.amount;
						}
					}
					if (effect.type == RoomSettings.RoomEffect.Type.HeatWave)
					{
						owner.room.game.cameras[0].levelGraphic.alpha = effect.amount / 2f;
					}
				}
			}
			Refresh();
		}
	}

	public RoomSettings.RoomEffect effect;

	public EffectPanel(DevUI owner, DevUINode parentNode, Vector2 pos, RoomSettings.RoomEffect effect)
		: base(owner, "Effect_Panel_" + effect.type.ToString(), parentNode, pos, new Vector2(250f, 30f), "")
	{
		this.effect = effect;
		Move(effect.panelPosition);
		subNodes.Add(new EffectPanelSlider(owner, "Amount_Slider", this, new Vector2(5f, 5f), "Amount: "));
	}

	public override void Move(Vector2 newPos)
	{
		base.Move(newPos);
		effect.panelPosition = absPos;
	}

	public override void Refresh()
	{
		base.Refresh();
		base.Title = "EFFECT " + (effect.inherited ? "<T> " : "") + effect.type.ToString();
		fSprites[0].color = (effect.overWrite ? new Color(0.5f, 0f, 0f) : (effect.inherited ? new Color(0f, 0f, 0.5f) : new Color(0f, 0.5f, 0f)));
	}
}
