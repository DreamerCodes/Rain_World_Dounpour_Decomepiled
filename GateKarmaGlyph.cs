using Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class GateKarmaGlyph : CosmeticSprite
{
	public bool side;

	public RegionGate gate;

	public RegionGate.GateRequirement requirement;

	private float fade;

	private float lastFade;

	private float flicker;

	private float goalFade;

	private Color paletteShortcutColor;

	private Color myDefaultColor;

	private float sinAdder;

	public bool symbolDirty = true;

	public Color col;

	public Color lastCol;

	private int animationIndex;

	public int animationTicker;

	public bool animationFinished;

	private int glyphIndex;

	private int[] citizensIDSequence;

	public OracleChatLabel mismatchLabel;

	private bool controllingRobo;

	public float redSine;

	public Color GetToColor
	{
		get
		{
			if (!gate.EnergyEnoughToOpen || (animationFinished && ShouldPlayCitizensIDAnimation() < 0))
			{
				return Color.Lerp(myDefaultColor, new Color(1f, 0f, 0f), 0.4f + 0.5f * Mathf.Sin(sinAdder / 12f));
			}
			return myDefaultColor;
		}
	}

	public void UpdateDefaultColor()
	{
		if (gate is ElectricGate)
		{
			myDefaultColor = new Color(1f, 0.75f, 0.5f, 1f);
		}
		else
		{
			myDefaultColor = paletteShortcutColor;
		}
		if (gate.unlocked)
		{
			myDefaultColor = Color.Lerp(myDefaultColor, new Color(0.2f, 0.8f, 1f, 1f), 0.6f);
		}
	}

	public GateKarmaGlyph(bool side, RegionGate gate, RegionGate.GateRequirement requirement)
	{
		this.side = side;
		this.gate = gate;
		this.requirement = requirement;
		pos = gate.room.MiddleOfTile(side ? 28 : 19, 14);
		lastPos = pos;
		col = GetToColor;
		lastCol = col;
		fade = 1f;
		lastFade = 1f;
		if (!ModManager.MSC || !(requirement == MoreSlugcatsEnums.GateRequirement.RoboLock))
		{
			return;
		}
		if (ModManager.Expedition && gate.room.game.rainWorld.ExpeditionMode && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer && gate.room.world.region.name == "UW" && gate.room.abstractRoom.name.Contains("LC"))
		{
			this.requirement = RegionGate.GateRequirement.OneKarma;
			return;
		}
		Random.InitState(50);
		citizensIDSequence = new int[9];
		for (int i = 0; i < citizensIDSequence.Length; i++)
		{
			if (gate.room.game.IsStorySession && !(gate.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark)
			{
				citizensIDSequence[i] = 0;
			}
			else
			{
				citizensIDSequence[i] = Random.Range(0, 14);
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastFade = fade;
		if (gate.mode == RegionGate.Mode.MiddleClosed || gate.mode == RegionGate.Mode.Closed || gate.mode == RegionGate.Mode.Broken)
		{
			if (gate.letThroughDir == side || gate is ElectricGate)
			{
				bool flag = false;
				if (gate is ElectricGate)
				{
					for (int i = 0; i < (gate as ElectricGate).lampsOn.Length; i++)
					{
						if (flag)
						{
							break;
						}
						flag = (gate as ElectricGate).lampsOn[i];
					}
				}
				if (flag)
				{
					goalFade = Mathf.Max(0f, goalFade - 0.025f);
				}
				else
				{
					goalFade = (gate.EnergyEnoughToOpen ? Mathf.InverseLerp((gate is ElectricGate) ? 10f : 40f, 0f, gate.startCounter) : 0.82f);
				}
			}
			else
			{
				goalFade = Mathf.Min(gate.EnergyEnoughToOpen ? 1f : 0.82f, goalFade + 1f / 30f);
			}
		}
		else
		{
			goalFade = Mathf.Max(0f, goalFade - 0.025f);
		}
		fade = Custom.LerpAndTick(fade, Mathf.Min(goalFade, 1f - flicker), 0.01f, 0.05f);
		if (Random.value < 1f / ((flicker == 0f) ? Mathf.Lerp(30f, 780f, goalFade) : 30f))
		{
			flicker = Random.value;
		}
		if (Random.value < 1f / 70f && !gate.EnergyEnoughToOpen)
		{
			flicker = Mathf.Max(flicker, Random.value);
		}
		if (flicker > 0f)
		{
			flicker = Mathf.Max(0f, flicker - 0.05f);
		}
		lastCol = col;
		col = Color.Lerp(col, GetToColor, 0.2f);
		if (requirement == RegionGate.GateRequirement.DemoLock || (ModManager.MSC && requirement == MoreSlugcatsEnums.GateRequirement.OELock))
		{
			redSine += 1f;
			col = new Color(1f, Mathf.Sin(redSine / 25f) * 0.5f + 0.5f, Mathf.Sin(redSine / 25f) * 0.5f + 0.5f);
		}
		if (!gate.EnergyEnoughToOpen || (animationFinished && ShouldPlayCitizensIDAnimation() < 0))
		{
			sinAdder += 1f;
		}
		if (ModManager.MSC && ShouldPlayCitizensIDAnimation() != 0)
		{
			for (int j = 0; j < gate.room.game.Players.Count; j++)
			{
				if (gate.room.game.Players[j].realizedCreature != null && (gate.room.game.Players[j].realizedCreature as Player).myRobot != null)
				{
					(gate.room.game.Players[j].realizedCreature as Player).myRobot.lockTarget = new Vector2(pos.x, pos.y + 40f);
					controllingRobo = true;
				}
			}
			if (room.game.GetStorySession.saveState.hasRobo)
			{
				animationTicker++;
				if (animationTicker % 3 == 0 && !animationFinished)
				{
					animationIndex++;
				}
				if (animationTicker % 15 == 0)
				{
					glyphIndex++;
					if (glyphIndex < 10)
					{
						room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Data_Bit, pos, 1f, 0.5f + Random.value * 2f);
					}
				}
				if (animationIndex > 9)
				{
					animationIndex = 0;
				}
				if (glyphIndex >= 10)
				{
					animationFinished = true;
				}
			}
			else
			{
				animationFinished = true;
			}
			if (animationFinished && mismatchLabel == null && ShouldPlayCitizensIDAnimation() < 0)
			{
				mismatchLabel = new OracleChatLabel(null);
				mismatchLabel.color = Color.red;
				mismatchLabel.inverted = true;
				mismatchLabel.pos = new Vector2(pos.x + (float)(side ? 150 : (-150)), pos.y - 50f);
				if (!room.game.GetStorySession.saveState.hasRobo)
				{
					mismatchLabel.NewPhrase(50);
				}
				else
				{
					mismatchLabel.NewPhrase(51);
				}
				gate.room.AddObject(mismatchLabel);
			}
		}
		else
		{
			if (!ModManager.MSC)
			{
				return;
			}
			if (controllingRobo)
			{
				for (int k = 0; k < gate.room.game.Players.Count; k++)
				{
					if (gate.room.game.Players[k].realizedCreature != null && (gate.room.game.Players[k].realizedCreature as Player).myRobot != null)
					{
						(gate.room.game.Players[k].realizedCreature as Player).myRobot.lockTarget = null;
						controllingRobo = false;
					}
				}
			}
			if (mismatchLabel != null)
			{
				mismatchLabel.Destroy();
				mismatchLabel = null;
			}
			animationTicker = 0;
			glyphIndex = -1;
			animationFinished = false;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (ModManager.MSC && requirement == MoreSlugcatsEnums.GateRequirement.RoboLock)
		{
			sLeaser.sprites = new FSprite[11];
		}
		else
		{
			sLeaser.sprites = new FSprite[2];
		}
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["LightSource"];
		sLeaser.sprites[1] = new FSprite("pixel");
		sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["GateHologram"];
		sLeaser.sprites[1].anchorY = 0.75f;
		if (ModManager.MSC && requirement == MoreSlugcatsEnums.GateRequirement.RoboLock)
		{
			for (int i = 2; i < 11; i++)
			{
				sLeaser.sprites[i] = new FSprite("pixel");
				sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["GateHologram"];
				sLeaser.sprites[i].scale = 3f;
			}
		}
		symbolDirty = true;
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Lerp(lastFade, fade, timeStacker);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[i].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[i].isVisible = num > 0f;
			sLeaser.sprites[i].color = Color.Lerp(lastCol, col, timeStacker);
		}
		if (symbolDirty)
		{
			if (gate.unlocked)
			{
				sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbol0");
			}
			else
			{
				sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbol" + requirement.value);
			}
			symbolDirty = false;
		}
		sLeaser.sprites[0].scale = Mathf.Lerp(200f, 300f, num) / 16f;
		sLeaser.sprites[0].alpha = num * Mathf.Lerp(0.55f, 0.6f, Random.value);
		sLeaser.sprites[1].alpha = num * 0.9f;
		if (ModManager.MSC && requirement == MoreSlugcatsEnums.GateRequirement.RoboLock)
		{
			Vector2 vector = new Vector2(sLeaser.sprites[0].x - 8f, sLeaser.sprites[0].y - 5f);
			for (int j = 2; j < 11; j++)
			{
				sLeaser.sprites[j].x = vector.x + (float)((j - 2) % 3 * 9);
				sLeaser.sprites[j].y = vector.y + (float)((j - 2) / 3 * 9);
				if (gate.unlocked)
				{
					sLeaser.sprites[j].alpha = 0f;
					sLeaser.sprites[j].isVisible = false;
				}
				else
				{
					sLeaser.sprites[j].alpha = num * 0.9f;
					sLeaser.sprites[j].isVisible = num > 0f;
				}
				sLeaser.sprites[j].color = Color.Lerp(lastCol, col, timeStacker);
				if (glyphIndex < j - 2)
				{
					sLeaser.sprites[j].element = Futile.atlasManager.GetElementWithName("pixel");
					sLeaser.sprites[j].scale = 3f;
				}
				else if (glyphIndex == j - 2)
				{
					sLeaser.sprites[j].element = Futile.atlasManager.GetElementWithName("TinyGlyph" + Random.Range(0, 14));
					sLeaser.sprites[j].scale = 1f;
				}
				else if (glyphIndex > j - 2)
				{
					sLeaser.sprites[j].element = Futile.atlasManager.GetElementWithName("TinyGlyph" + citizensIDSequence[j - 2]);
					sLeaser.sprites[j].scale = 1f;
				}
			}
			if (ShouldPlayCitizensIDAnimation() != 0)
			{
				if (animationIndex > 0)
				{
					sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbol" + requirement.value + "-" + animationIndex);
				}
				else
				{
					sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("gateSymbol" + requirement.value);
				}
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
		paletteShortcutColor = palette.texture.GetPixel(13, 7);
		UpdateDefaultColor();
	}

	public int ShouldAnimate()
	{
		if (requirement != MoreSlugcatsEnums.GateRequirement.RoboLock || gate.mode != RegionGate.Mode.MiddleClosed || !gate.EnergyEnoughToOpen || gate.unlocked || gate.letThroughDir == side)
		{
			return 0;
		}
		int num = gate.PlayersInZone();
		if (num > 0 && num < 3)
		{
			gate.letThroughDir = num == 1;
			if (!gate.dontOpen && !gate.MeetRequirement)
			{
				return -1;
			}
			return 1;
		}
		return 0;
	}

	public int ShouldPlayCitizensIDAnimation()
	{
		if (!ModManager.MSC || requirement != MoreSlugcatsEnums.GateRequirement.RoboLock)
		{
			return 0;
		}
		if (gate.mode != RegionGate.Mode.MiddleClosed || !gate.EnergyEnoughToOpen || gate.unlocked || gate.letThroughDir == side)
		{
			return 0;
		}
		int num = gate.PlayersInZone();
		if (num > 0 && num < 3)
		{
			gate.letThroughDir = num == 1;
			if (gate.dontOpen || gate.MeetRequirement)
			{
				return 1;
			}
			return -1;
		}
		return 0;
	}
}
