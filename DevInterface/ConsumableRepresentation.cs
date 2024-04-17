using System;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class ConsumableRepresentation : PlacedObjectRepresentation
{
	public class ConsumableControlPanel : Panel
	{
		public class ConsumableSlider : Slider
		{
			public ConsumableSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float nubPos = 0f;
				if (((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).minRegen == 0)
				{
					base.NumberText = "N/A";
					nubPos = 0f;
				}
				else
				{
					switch (IDstring)
					{
					case "Min_Regen_Slider":
						nubPos = (float)((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).minRegen / 50f;
						base.NumberText = ((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).minRegen.ToString();
						break;
					case "Max_Regen_Slider":
						nubPos = (float)((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).maxRegen / 50f;
						base.NumberText = ((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).maxRegen.ToString();
						break;
					}
				}
				RefreshNubPos(nubPos);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "Min_Regen_Slider":
					((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).minRegen = Math.Min((int)(nubPos * 50f), ((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).maxRegen);
					break;
				case "Max_Regen_Slider":
					((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).maxRegen = Math.Max((int)(nubPos * 50f), ((parentNode.parentNode as ConsumableRepresentation).pObj.data as PlacedObject.ConsumableObjectData).minRegen);
					break;
				}
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public ConsumableControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 55f), name)
		{
			subNodes.Add(new ConsumableSlider(owner, "Min_Regen_Slider", this, new Vector2(5f, 25f), "Min Cycles: "));
			subNodes.Add(new ConsumableSlider(owner, "Max_Regen_Slider", this, new Vector2(5f, 5f), "Max Cycles: "));
		}
	}

	public class DataPearlControlPanel : ConsumableControlPanel, IDevUISignals
	{
		public Button typeButton;

		public Button hiddenButton;

		public DataPearlControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name)
			: base(owner, IDstring, parentNode, pos, name)
		{
			size.y += 40f;
			typeButton = new Button(owner, "Pearl_Type_Button", this, new Vector2(5f, 45f), 240f, "TYPE");
			subNodes.Add(typeButton);
			hiddenButton = new Button(owner, "Pearl_Hidden_Button", this, new Vector2(5f, 65f), 240f, "Hidden");
			subNodes.Add(hiddenButton);
		}

		public override void Refresh()
		{
			base.Refresh();
			if (((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.DataPearlData).hidden)
			{
				hiddenButton.Text = "!HIDDEN!";
			}
			else
			{
				hiddenButton.Text = "Visible";
			}
			typeButton.Text = ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.DataPearlData).pearlType.ToString();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			PlacedObject.DataPearlData dataPearlData = (parentNode as ConsumableRepresentation).pObj.data as PlacedObject.DataPearlData;
			switch (sender.IDstring)
			{
			case "Pearl_Type_Button":
				if ((int)dataPearlData.pearlType >= ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.Count - 1)
				{
					dataPearlData.pearlType = new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.GetEntry(0));
				}
				else
				{
					dataPearlData.pearlType = new DataPearl.AbstractDataPearl.DataPearlType(ExtEnum<DataPearl.AbstractDataPearl.DataPearlType>.values.GetEntry(dataPearlData.pearlType.Index + 1));
				}
				break;
			case "Pearl_Hidden_Button":
				dataPearlData.hidden = !dataPearlData.hidden;
				break;
			}
			Refresh();
		}
	}

	public class VoidSpawnEggControlPanel : ConsumableControlPanel, IDevUISignals
	{
		public Button exitButton;

		public FSprite exitSprite;

		public int exitSpriteIndex;

		public VoidSpawnEggControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name)
			: base(owner, IDstring, parentNode, pos, name)
		{
			size.y += 20f;
			exitButton = new Button(owner, "Void_Spawn_Exit_Button", this, new Vector2(5f, 45f), 240f, "Exit: " + ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit);
			subNodes.Add(exitButton);
			exitSprite = new FSprite("pixel");
			fSprites.Add(exitSprite);
			exitSpriteIndex = fSprites.Count;
			owner.placedObjectsContainer.AddChild(exitSprite);
			exitSprite.anchorY = 0f;
			exitSprite.scaleX = 2f;
			exitSprite.color = new Color(1f, 0f, 0f);
		}

		public override void Refresh()
		{
			base.Refresh();
			exitButton.Text = "Exit: " + ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit;
			if (exitSprite != null)
			{
				int exit = ((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit;
				if (exit < 0 || exit >= owner.room.abstractRoom.connections.Length)
				{
					exitSprite.isVisible = false;
					return;
				}
				exitSprite.isVisible = true;
				Vector2 vector = (parentNode as PositionedDevUINode).absPos;
				Vector2 vector2 = owner.room.MiddleOfTile(owner.room.ShortcutLeadingToNode(exit).startCoord) - owner.room.game.cameras[0].pos;
				exitSprite.x = vector.x;
				exitSprite.y = vector.y;
				exitSprite.rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				exitSprite.scaleY = Vector2.Distance(vector, vector2);
			}
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			string iDstring = sender.IDstring;
			if (iDstring != null && iDstring == "Void_Spawn_Exit_Button")
			{
				((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit++;
				if (((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit >= owner.room.abstractRoom.connections.Length)
				{
					((parentNode as ConsumableRepresentation).pObj.data as PlacedObject.VoidSpawnEggData).exit = 0;
				}
			}
			Refresh();
		}
	}

	private ConsumableControlPanel controlPanel;

	public ConsumableRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, pObj.type.ToString())
	{
		if (pObj.type == PlacedObject.Type.VoidSpawnEgg)
		{
			controlPanel = new VoidSpawnEggControlPanel(owner, "Consumable_Panel", this, new Vector2(0f, 100f), "Consumable: " + pObj.type.ToString());
		}
		else if (pObj.type == PlacedObject.Type.DataPearl || pObj.type == PlacedObject.Type.UniqueDataPearl)
		{
			controlPanel = new DataPearlControlPanel(owner, "Consumable_Panel", this, new Vector2(0f, 100f), "Consumable: " + pObj.type.ToString());
		}
		else
		{
			controlPanel = new ConsumableControlPanel(owner, "Consumable_Panel", this, new Vector2(0f, 100f), "Consumable: " + pObj.type.ToString());
		}
		subNodes.Add(controlPanel);
		controlPanel.pos = (pObj.data as PlacedObject.ConsumableObjectData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
		fSprites[fSprites.Count - 1].anchorY = 0f;
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(fSprites.Count - 1, absPos);
		fSprites[fSprites.Count - 1].scaleY = controlPanel.pos.magnitude;
		fSprites[fSprites.Count - 1].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
		(pObj.data as PlacedObject.ConsumableObjectData).panelPos = (subNodes[subNodes.Count - 1] as Panel).pos;
	}
}
