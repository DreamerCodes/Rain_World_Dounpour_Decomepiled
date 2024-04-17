using RWCustom;
using UnityEngine;

namespace DevInterface;

public class MapRenderDefaultMaterial : DevUINode, IDevUISignals
{
	public class MapHandle : Handle
	{
		public Vector2 PanPos => (parentNode.parentNode as MapPage).panPos;

		public override Vector2 absPos
		{
			get
			{
				return base.absPos + PanPos;
			}
			set
			{
				base.absPos = value - PanPos;
			}
		}

		public MapHandle(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos)
		{
		}

		public override void Move(Vector2 newPos)
		{
			base.Move(newPos - PanPos);
		}

		public override void Update()
		{
			base.Update();
			if (base.MouseOver && owner != null && owner.mouseClick)
			{
				mouseOffset = pos + PanPos - owner.mousePos;
				dragged = true;
			}
		}
	}

	public class MapRenderDefaultMaterialPanel : Panel
	{
		public MapRenderDefaultMaterialPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, Vector2 size, string title)
			: base(owner, IDstring, parentNode, pos, size, title)
		{
			subNodes.Add(new Button(owner, "Remove_Def_Mat", this, new Vector2(5f, 30f), 100f, "Remove"));
			subNodes.Add(new Button(owner, "Material", this, new Vector2(5f, 10f), 100f, "Change Material"));
			fSprites.Add(new FSprite("pixel"));
			fSprites[fSprites.Count - 1].color = new Color(1f, 1f, 1f);
			fSprites[fSprites.Count - 1].anchorY = 0f;
			if (owner != null)
			{
				Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			MoveSprite(fSprites.Count - 1, absPos);
			fSprites[fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(absPos, (parentNode as PositionedDevUINode).absPos);
			fSprites[fSprites.Count - 1].scaleY = Vector2.Distance(absPos, (parentNode as PositionedDevUINode).absPos);
		}
	}

	public MapHandle handleA;

	public MapHandle handleB;

	public Vector2 lastAPos;

	public Vector2 lastBPos;

	private bool lastMouseOver;

	public FloatRect rect;

	public bool materialIsAir;

	public Vector2 PanPos => (parentNode as MapPage).panPos;

	private bool MouseOver
	{
		get
		{
			if (!(handleA.subNodes[0] as MapRenderDefaultMaterialPanel).MouseOver && !handleA.MouseOver)
			{
				return handleB.MouseOver;
			}
			return true;
		}
	}

	public MapRenderDefaultMaterial(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
		: base(owner, IDstring, parentNode)
	{
		handleA = new MapHandle(owner, "A", this, pos);
		handleB = new MapHandle(owner, "B", this, pos + new Vector2(30f, 30f));
		subNodes.Add(handleA);
		subNodes.Add(handleB);
		fSprites.Add(new FSprite("pixel"));
		fSprites[fSprites.Count - 1].anchorX = 0f;
		fSprites[fSprites.Count - 1].anchorY = 0f;
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
		}
		handleA.subNodes.Add(new MapRenderDefaultMaterialPanel(owner, "Def_Mat_Panel", handleA, new Vector2(-10f, -60f), new Vector2(200f, 50f), "Material Rect Settings"));
	}

	public override void Update()
	{
		base.Update();
		if (lastAPos != handleA.absPos || lastBPos != handleB.absPos || MouseOver != lastMouseOver)
		{
			Refresh();
			FloatRect floatRect = new FloatRect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
			if (handleA.pos.x < floatRect.left)
			{
				floatRect.left = handleA.pos.x;
			}
			if (handleB.pos.x < floatRect.left)
			{
				floatRect.left = handleB.pos.x;
			}
			if (handleA.pos.x > floatRect.right)
			{
				floatRect.right = handleA.pos.x;
			}
			if (handleB.pos.x > floatRect.right)
			{
				floatRect.right = handleB.pos.x;
			}
			if (handleA.pos.y < floatRect.bottom)
			{
				floatRect.bottom = handleA.pos.y;
			}
			if (handleB.pos.y < floatRect.bottom)
			{
				floatRect.bottom = handleB.pos.y;
			}
			if (handleA.pos.y > floatRect.top)
			{
				floatRect.top = handleA.pos.y;
			}
			if (handleB.pos.y > floatRect.top)
			{
				floatRect.top = handleB.pos.y;
			}
			rect = floatRect;
		}
		lastAPos = handleA.absPos;
		lastBPos = handleB.absPos;
		lastMouseOver = MouseOver;
	}

	public override void Refresh()
	{
		base.Refresh();
		fSprites[fSprites.Count - 1].x = handleA.pos.x + PanPos.x;
		fSprites[fSprites.Count - 1].y = handleA.pos.y + PanPos.y;
		fSprites[fSprites.Count - 1].scaleX = handleB.pos.x - handleA.pos.x;
		fSprites[fSprites.Count - 1].scaleY = handleB.pos.y - handleA.pos.y;
		fSprites[fSprites.Count - 1].color = (materialIsAir ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f));
		fSprites[fSprites.Count - 1].alpha = (MouseOver ? 0.5f : 0.25f);
	}

	public void Signal(DevUISignalType type, DevUINode sender, string message)
	{
		switch (sender.IDstring)
		{
		case "Material":
			materialIsAir = !materialIsAir;
			Refresh();
			break;
		case "Remove_Def_Mat":
			ClearSprites();
			parentNode.subNodes.Remove(this);
			parentNode.Refresh();
			break;
		}
	}
}
