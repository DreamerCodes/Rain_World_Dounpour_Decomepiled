using System.Collections.Generic;
using OverseerHolograms;
using RWCustom;
using UnityEngine;

namespace ScavTradeInstruction;

public class ScavTradeInputInstructionController : OverseerTutorialBehavior.InputInstruction.InputInstructionController, IOwnAHoloImage
{
	public class Phase : ExtEnum<Phase>
	{
		public static readonly Phase None = new Phase("None", register: true);

		public static readonly Phase InitialProjections = new Phase("InitialProjections", register: true);

		public static readonly Phase PickupPearl = new Phase("PickupPearl", register: true);

		public static readonly Phase Projections = new Phase("Projections", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public OverseerTutorialBehavior.Eat.FoodArrow arrow;

	public OverseerImage.HoloImage holoImagePart;

	public OverseerImage.Frame imageFramePart;

	public Phase phase;

	public int showImage;

	public int imageCounter;

	public int showImageTimer;

	public List<OverseerImage.ImageID> images;

	public int CurrImageIndex => showImage;

	public int ShowTime => 0;

	public OverseerImage.ImageID CurrImage => images[showImage];

	public float ImmediatelyToContent => 0.5f;

	public ScavTradeInputInstructionController(OverseerTutorialBehavior.InputInstruction instructionHologram, OverseerTutorialBehavior tutBehavior)
		: base(instructionHologram, tutBehavior)
	{
		Custom.Log("new scav trade");
		arrow = new OverseerTutorialBehavior.Eat.FoodArrow(instructionHologram, instructionHologram.totalSprites);
		arrow.visible = false;
		instructionHologram.AddPart(arrow);
		phase = Phase.None;
		holoImagePart = new OverseerImage.HoloImage(instructionHologram, instructionHologram.totalSprites, this);
		instructionHologram.AddPart(holoImagePart);
		imageFramePart = new OverseerImage.Frame(instructionHologram, instructionHologram.totalSprites);
		instructionHologram.AddPart(imageFramePart);
		holoImagePart.visible = false;
		imageFramePart.visible = false;
		images = new List<OverseerImage.ImageID>
		{
			OverseerImage.ImageID.Scav_Outpost,
			OverseerImage.ImageID.Scav_And_Pearls,
			OverseerImage.ImageID.Scav_Slugcat_Trade
		};
	}

	public override void Update()
	{
		base.Update();
		if (base.overseer.AI.communication == null || base.overseer.AI.communication.inputInstruction == null || base.overseer.AI.communication.inputInstruction.slatedForDeletetion || !base.overseer.AI.communication.inputInstruction.activated || !(base.overseer.AI.communication.inputInstruction is ScavengerTradeInstructionTrigger))
		{
			instructionHologram.stillRelevant = false;
			return;
		}
		ScavengerTradeInstructionTrigger scavengerTradeInstructionTrigger = base.overseer.AI.communication.inputInstruction as ScavengerTradeInstructionTrigger;
		instructionHologram.showPickup = false;
		arrow.visible = false;
		holoImagePart.visible = false;
		imageFramePart.visible = false;
		if (phase == Phase.None)
		{
			if (scavengerTradeInstructionTrigger.pearl != null)
			{
				phase = Phase.InitialProjections;
			}
		}
		else if (phase == Phase.PickupPearl)
		{
			instructionHologram.showPickup = true;
			instructionHologram.closeToPos = scavengerTradeInstructionTrigger.pearl.firstChunk.pos + new Vector2(0f, 60f);
			arrow.visible = true;
			arrow.offset = scavengerTradeInstructionTrigger.pearl.firstChunk.pos + new Vector2(0f, 20f) - instructionHologram.pos;
			if (scavengerTradeInstructionTrigger.pearl.grabbedBy.Count > 0 && scavengerTradeInstructionTrigger.pearl.grabbedBy[0].grabber == base.overseer.AI.communication.player)
			{
				phase = Phase.Projections;
			}
		}
		else
		{
			if (!(phase == Phase.Projections) && !(phase == Phase.InitialProjections))
			{
				return;
			}
			holoImagePart.visible = true;
			imageFramePart.visible = true;
			if (holoImagePart.partFade > 0.9f)
			{
				showImageTimer++;
			}
			instructionHologram.hideInputs = true;
			instructionHologram.closeToPos = scavengerTradeInstructionTrigger.placedObject.pos;
			if (phase == Phase.InitialProjections)
			{
				arrow.visible = true;
				arrow.offset = scavengerTradeInstructionTrigger.pearl.firstChunk.pos + new Vector2(0f, 20f) - instructionHologram.pos;
				if (showImageTimer > 240)
				{
					showImageTimer = 0;
					phase = Phase.PickupPearl;
					instructionHologram.hideInputs = false;
				}
			}
			else if (showImageTimer == 220)
			{
				base.room.game.cameras[0].hud.textPrompt.AddMessage(base.room.game.rainWorld.inGameTranslator.Translate("These pearls are valued by the Scavengers"), 0, 200, darken: true, hideHud: true);
			}
			else if (showImageTimer > 400 && !scavengerTradeInstructionTrigger.completed)
			{
				showImageTimer = 0;
				scavengerTradeInstructionTrigger.completed = true;
				base.overseer.AI.communication.InWorldInputInstructionCompleted(scavengerTradeInstructionTrigger);
			}
			imageCounter++;
			if (imageCounter > 80)
			{
				imageCounter = 0;
				showImage++;
				if (showImage >= images.Count)
				{
					showImage = 0;
				}
			}
		}
	}
}
