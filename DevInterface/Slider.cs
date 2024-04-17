using UnityEngine;

namespace DevInterface;

public class Slider : PositionedDevUINode, IDevUISignals
{
	public class SliderNub : RectangularDevUINode
	{
		public bool held;

		public float mousePosOffset;

		public SliderNub(DevUI owner, string IDstring, DevUINode parentNode)
			: base(owner, IDstring, parentNode, new Vector2(0f, 0f), new Vector2(8f, 16f))
		{
			fSprites.Add(new FSprite("pixel"));
			fSprites[fSprites.Count - 1].scaleY = size.y;
			fSprites[fSprites.Count - 1].scaleX = size.x;
			fSprites[fSprites.Count - 1].anchorX = 0f;
			fSprites[fSprites.Count - 1].anchorY = 0f;
			if (owner != null)
			{
				Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
			}
		}

		public override void Update()
		{
			base.Update();
			if (held)
			{
				fSprites[fSprites.Count - 1].color = new Color(0f, 0f, 1f);
			}
			else
			{
				fSprites[fSprites.Count - 1].color = (base.MouseOver ? new Color(1f, 0f, 0f) : new Color(0f, 0f, 0f));
			}
			if (owner != null && owner.mouseClick && base.MouseOver)
			{
				held = true;
				mousePosOffset = absPos.x - owner.mousePos.x;
			}
			if (held && (owner == null || !owner.mouseDown))
			{
				held = false;
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			MoveSprite(0, absPos);
		}
	}

	private bool inheritButton;

	public float titleWidth;

	private float SliderStartCoord
	{
		get
		{
			if (!inheritButton)
			{
				return titleWidth + 10f + 16f + 4f;
			}
			return titleWidth + 10f + 42f + 4f + 34f;
		}
	}

	public string NumberText
	{
		get
		{
			return (subNodes[1] as DevUILabel).Text;
		}
		set
		{
			(subNodes[1] as DevUILabel).Text = value;
		}
	}

	public Slider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title, bool inheritButton, float titleWidth)
		: base(owner, IDstring, parentNode, pos)
	{
		this.inheritButton = inheritButton;
		this.titleWidth = titleWidth;
		subNodes.Add(new DevUILabel(owner, "Title", this, new Vector2(0f, 0f), titleWidth, title));
		subNodes.Add(new DevUILabel(owner, "Number", this, new Vector2(titleWidth + 10f, 0f), inheritButton ? 42f : 16f, "0"));
		if (inheritButton)
		{
			subNodes.Add(new Button(owner, "Inherit_Button", this, new Vector2(titleWidth + 10f + 42f + 4f, 0f), 30f, "Inhrt"));
		}
		fSprites.Add(new FSprite("pixel"));
		fSprites[fSprites.Count - 1].scaleY = 16f;
		fSprites[fSprites.Count - 1].scaleX = 100f;
		fSprites[fSprites.Count - 1].anchorX = 0f;
		fSprites[fSprites.Count - 1].anchorY = 0f;
		fSprites[fSprites.Count - 1].color = new Color(1f, 1f, 1f);
		fSprites[fSprites.Count - 1].alpha = 0.5f;
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
		}
		fSprites.Add(new FSprite("pixel"));
		fSprites[fSprites.Count - 1].scaleY = 2f;
		fSprites[fSprites.Count - 1].scaleX = 100f;
		fSprites[fSprites.Count - 1].anchorX = 0f;
		fSprites[fSprites.Count - 1].anchorY = 0f;
		fSprites[fSprites.Count - 1].color = new Color(0f, 0f, 0f);
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
		}
		subNodes.Add(new SliderNub(owner, "Slider_Nub", this));
	}

	public override void Update()
	{
		base.Update();
		if (owner != null && (subNodes[inheritButton ? 3 : 2] as SliderNub).held)
		{
			NubDragged(Mathf.InverseLerp(absPos.x + SliderStartCoord, absPos.x + SliderStartCoord + 92f, owner.mousePos.x + (subNodes[inheritButton ? 3 : 2] as SliderNub).mousePosOffset));
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(0, absPos + new Vector2(SliderStartCoord, 0f));
		MoveSprite(1, absPos + new Vector2(SliderStartCoord, 7f));
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		ClickedResetToInherent();
	}

	public virtual void ClickedResetToInherent()
	{
	}

	public virtual void NubDragged(float nubPos)
	{
	}

	public void RefreshNubPos(float nubPos)
	{
		(subNodes[inheritButton ? 3 : 2] as SliderNub).Move(new Vector2(Mathf.Lerp(SliderStartCoord, SliderStartCoord + 92f, nubPos), 0f));
	}
}
