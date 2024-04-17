using RWCustom;
using UnityEngine;

namespace DevInterface;

public class ScavOutpostRepresentation : ResizeableObjectRepresentation
{
	public class ScavOutpostControlPanel : Panel
	{
		public class DepthControlSlider : Slider
		{
			public DepthControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "Direction_Slider":
					num = ((parentNode.parentNode as ScavOutpostRepresentation).pObj.data as PlacedObject.ScavengerOutpostData).direction;
					base.NumberText = ((int)Mathf.Lerp(-100f, 100f, num)).ToString();
					break;
				case "Skull_Seed_Slider":
					num = (float)((parentNode.parentNode as ScavOutpostRepresentation).pObj.data as PlacedObject.ScavengerOutpostData).skullSeed / 100f;
					base.NumberText = ((parentNode.parentNode as ScavOutpostRepresentation).pObj.data as PlacedObject.ScavengerOutpostData).skullSeed.ToString();
					break;
				case "Pearls_Seed_Slider":
					num = (float)((parentNode.parentNode as ScavOutpostRepresentation).pObj.data as PlacedObject.ScavengerOutpostData).pearlsSeed / 100f;
					base.NumberText = ((parentNode.parentNode as ScavOutpostRepresentation).pObj.data as PlacedObject.ScavengerOutpostData).pearlsSeed.ToString();
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Direction_Slider":
					((parentNode.parentNode as ScavOutpostRepresentation).pObj.data as PlacedObject.ScavengerOutpostData).direction = nubPos;
					break;
				case "Skull_Seed_Slider":
					((parentNode.parentNode as ScavOutpostRepresentation).pObj.data as PlacedObject.ScavengerOutpostData).skullSeed = (int)(nubPos * 100f);
					break;
				case "Pearls_Seed_Slider":
					((parentNode.parentNode as ScavOutpostRepresentation).pObj.data as PlacedObject.ScavengerOutpostData).pearlsSeed = (int)(nubPos * 100f);
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public ScavOutpostControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 75f), "Scavenger Outpost")
		{
			subNodes.Add(new DepthControlSlider(owner, "Direction_Slider", this, new Vector2(5f, 45f), "Direction: "));
			subNodes.Add(new DepthControlSlider(owner, "Skull_Seed_Slider", this, new Vector2(5f, 25f), "Skull Seed: "));
			subNodes.Add(new DepthControlSlider(owner, "Pearls_Seed_Slider", this, new Vector2(5f, 5f), "Pearls Seed: "));
		}
	}

	public ScavOutpostRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Scavenger Outpost", showRing: true)
	{
		subNodes.Add(new ScavOutpostControlPanel(owner, "Scav_Outpost_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as ScavOutpostControlPanel).pos = (pObj.data as PlacedObject.ScavengerOutpostData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(fSprites.Count - 1, absPos);
		fSprites[fSprites.Count - 1].scaleY = (subNodes[subNodes.Count - 1] as ScavOutpostControlPanel).pos.magnitude;
		fSprites[fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[subNodes.Count - 1] as ScavOutpostControlPanel).absPos);
		(pObj.data as PlacedObject.ScavengerOutpostData).panelPos = (subNodes[subNodes.Count - 1] as Panel).pos;
	}
}
