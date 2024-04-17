using DevInterface;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class OEsphereRepresentation : PlacedObjectRepresentation
{
	public class OEspherePanel : Panel, IDevUISignals
	{
		public class OEsphereSlider : Slider
		{
			public OEsphereSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void NubDragged(float nubPos)
			{
				string iDstring = IDstring;
				if (iDstring != null && iDstring == "Depth_Slider")
				{
					((parentNode.parentNode as OEsphereRepresentation).pObj.data as PlacedObject.OEsphereData).depth = (int)(nubPos * 30f);
				}
				if (iDstring != null && iDstring == "lIntensity_Slider")
				{
					((parentNode.parentNode as OEsphereRepresentation).pObj.data as PlacedObject.OEsphereData).lIntensity = nubPos;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				_ = IDstring;
				if (IDstring == "Depth_Slider")
				{
					num = ((parentNode.parentNode as OEsphereRepresentation).pObj.data as PlacedObject.OEsphereData).depth;
					base.NumberText = (int)num + " ";
					RefreshNubPos(num * 0.03333f);
				}
				if (IDstring == "lIntensity_Slider")
				{
					num = ((parentNode.parentNode as OEsphereRepresentation).pObj.data as PlacedObject.OEsphereData).lIntensity;
					base.NumberText = (int)(num * 100f) + "% ";
					RefreshNubPos(num);
				}
			}
		}

		public OEspherePanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 65f), "OE Sphere")
		{
			subNodes.Add(new OEsphereSlider(owner, "Depth_Slider", this, new Vector2(5f, 25f), "Depth: "));
			subNodes.Add(new OEsphereSlider(owner, "lIntensity_Slider", this, new Vector2(5f, 45f), "Light: "));
		}

		public override void Move(Vector2 newPos)
		{
			base.Move(newPos);
			parentNode.Refresh();
		}

		public override void Refresh()
		{
			base.Refresh();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
		}
	}

	private OEsphere sphere;

	public OEsphereRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new Handle(owner, "Rad_Handle", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as Handle).pos = (pObj.data as PlacedObject.OEsphereData).handlePos;
		fSprites.Add(new FSprite("Futile_White"));
		owner.placedObjectsContainer.AddChild(fSprites[1]);
		fSprites[1].shader = owner.room.game.rainWorld.Shaders["VectorCircle"];
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[2]);
		fSprites[2].anchorY = 0f;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[3]);
		fSprites[3].anchorY = 0f;
		subNodes.Add(new OEspherePanel(owner, "OE_Sphere_Control_Panel", this, new Vector2(0f, 100f)));
		(subNodes[subNodes.Count - 1] as OEspherePanel).pos = (pObj.data as PlacedObject.OEsphereData).panelPos;
		for (int i = 0; i < owner.room.oeSpheres.Count; i++)
		{
			if (owner.room.oeSpheres[i].pos == pObj.pos)
			{
				sphere = owner.room.oeSpheres[i];
				break;
			}
		}
		if (sphere == null)
		{
			sphere = new OEsphere(pos, 100f, 0);
			owner.room.AddObject(sphere);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(1, absPos);
		fSprites[1].scale = (subNodes[0] as Handle).pos.magnitude / 8f;
		fSprites[1].alpha = 2f / (subNodes[0] as Handle).pos.magnitude;
		MoveSprite(2, absPos);
		fSprites[2].scaleY = (subNodes[0] as Handle).pos.magnitude;
		fSprites[2].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[0] as Handle).absPos);
		(pObj.data as PlacedObject.OEsphereData).handlePos = (subNodes[0] as Handle).pos;
		MoveSprite(3, absPos);
		fSprites[3].scaleY = (subNodes[1] as OEspherePanel).pos.magnitude;
		fSprites[3].rotation = Custom.AimFromOneVectorToAnother(absPos, (subNodes[1] as OEspherePanel).absPos);
		(pObj.data as PlacedObject.OEsphereData).handlePos = (subNodes[0] as Handle).pos;
		(pObj.data as PlacedObject.OEsphereData).panelPos = (subNodes[1] as Panel).pos;
		sphere.pos = pObj.pos;
		sphere.depth = (pObj.data as PlacedObject.OEsphereData).depth;
		sphere.lIntensity = (pObj.data as PlacedObject.OEsphereData).lIntensity;
		sphere.rad = (pObj.data as PlacedObject.OEsphereData).Rad;
	}
}
