using System;
using System.Collections.Generic;
using LizardCosmetics;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class LizardGraphics : GraphicsModule, ILookingAtCreatures
{
	public struct IndividualVariations
	{
		public float headSize;

		public float fatness;

		public float tailLength;

		public float tailFatness;

		public float tailColor;

		public IndividualVariations(float headSize, float fatness, float tailLength, float tailFatness, float tailColor)
		{
			this.headSize = headSize;
			this.fatness = fatness;
			this.tailLength = tailLength;
			this.tailFatness = tailFatness;
			this.tailColor = tailColor;
		}
	}

	public struct LizardSpineData
	{
		public float f;

		public Vector2 pos;

		public Vector2 outerPos;

		public Vector2 dir;

		public Vector2 perp;

		public float depthRotation;

		public float rad;

		public LizardSpineData(float f, Vector2 pos, Vector2 outerPos, Vector2 dir, Vector2 perp, float depthRotation, float rad)
		{
			this.f = f;
			this.pos = pos;
			this.outerPos = outerPos;
			this.dir = dir;
			this.perp = perp;
			this.depthRotation = depthRotation;
			this.rad = rad;
		}
	}

	public Lizard lizard;

	public LizardLimb[] limbs;

	public Vector2[,] drawPositions;

	public TailSegment[] tail;

	public GenericBodyPart[] tongue;

	public int legsGrabbing;

	public int frontLegsGrabbing;

	public int hindLegsGrabbing;

	public int noGripCounter;

	public float frontBob;

	public float hindBob;

	public float depthRotation;

	public float lastDepthRotation;

	public float headDepthRotation;

	public float lastHeadDepthRotation;

	public GenericBodyPart head;

	private bool stunnedLastUpdate;

	private bool rotateWhileStunned;

	private float lastBlink;

	private float blink;

	private int flicker;

	private float flickerColor;

	private int freeze;

	public int whiteFlicker;

	private bool everySecondDraw;

	private bool debugVisualization;

	private float lastBreath;

	private float breath;

	private Vector2 tailDirection;

	public CreatureLooker creatureLooker;

	public Vector2 lookPos;

	private bool visualizeVision;

	private float scanning;

	public RoomPalette palette;

	public float headColorSetter;

	private Vector2[,] eyes;

	private float eyeBeamsActive;

	public Color whiteCamoColor = new Color(0f, 0f, 0f);

	private float whiteCamoColorAmount = -1f;

	private float whiteCamoColorAmountDrag = 1f;

	private Color whitePickUpColor;

	private float whiteDominanceHue;

	private Color ivarBodyColor = new Color(0f, 0f, 0f);

	private int whiteGlitchFit;

	private bool blackSalamander;

	public int extraSprites;

	public List<Template> cosmetics;

	public SnowAccumulation snowAccCosmetic;

	public float bodyLength;

	public float tailLength;

	public float headConnectionRad;

	public float showDominance;

	public LightSource lightSource;

	private DynamicSoundLoop soundLoop;

	private float bellyDragVolume;

	public float blackLizardLightUpHead;

	public float voiceVisualizationIntensity;

	public float lastVoiceVisualizationIntensity;

	public float voiceVisualization;

	public float lastVoiceVisualization;

	public IndividualVariations iVars;

	public int overrideHeadGraphic;

	public float Camouflaged
	{
		get
		{
			if (whiteCamoColorAmount == -1f)
			{
				return 1f;
			}
			return whiteCamoColorAmount;
		}
	}

	public Color effectColor
	{
		get
		{
			if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
			{
				return palette.blackColor;
			}
			if (snowAccCosmetic != null)
			{
				return Color.Lerp(lizard.effectColor, Color.Lerp(Color.white, whiteCamoColor, 0.5f), Mathf.Min(1f, snowAccCosmetic.DebrisSaturation * 1.5f));
			}
			return lizard.effectColor;
		}
	}

	public Color SalamanderColor
	{
		get
		{
			if (blackSalamander)
			{
				return Color.Lerp(palette.blackColor, effectColor, 0.1f);
			}
			return Color.Lerp(new Color(0.9f, 0.9f, 0.95f), effectColor, 0.06f);
		}
	}

	public int startOfExtraSprites => SpriteTongueStart + (visualizeVision ? 2 : 0) + ((tongue != null) ? 2 : 0);

	public float BodyAndTailLength => bodyLength + tailLength;

	public float BubbleIntensity => lizard.bubbleIntensity * 0.5f + voiceVisualizationIntensity;

	public bool Caramel
	{
		get
		{
			if (ModManager.MSC)
			{
				return lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard;
			}
			return false;
		}
	}

	public int SpriteBodyCirclesStart => 0;

	public int SpriteBodyCirclesEnd => SpriteBodyCirclesStart + 5;

	public int SpriteBodyMesh => SpriteBodyCirclesEnd;

	public int SpriteTail => SpriteBodyMesh + 1;

	public int SpriteLimbsStart => SpriteTail + 1;

	public int SpriteLimbsEnd => SpriteLimbsStart + limbs.Length;

	public int SpriteHeadStart => SpriteLimbsEnd;

	public int SpriteHeadEnd => SpriteHeadStart + 5;

	public int SpriteLimbsColorStart => SpriteHeadEnd;

	public int SpriteLimbsColorEnd => SpriteLimbsColorStart + limbs.Length;

	public int SpriteTongueStart => SpriteLimbsColorEnd;

	public int SpriteTongueEnd => SpriteTongueStart + 2;

	public int TotalSprites => startOfExtraSprites + extraSprites;

	public int SpriteVisionEnd => TotalSprites;

	public int SpriteVisionStart => SpriteVisionEnd - 2;

	public int DebugLimbsStart => 0;

	public int DebugLimbsEnd => DebugLimbsStart + limbs.Length;

	public int DebugBodyChunksStart => DebugLimbsEnd;

	public int DebugBodyChunksEnd => DebugBodyChunksStart + lizard.bodyChunks.Length;

	public int DebugHead => DebugBodyChunksEnd;

	public int DebugGrabPosStart => DebugHead + 1;

	public int DebugGrabPosEnd => DebugGrabPosStart + limbs.Length;

	public int TotalDebugSprites => DebugGrabPosEnd;

	private Color HeadColor1
	{
		get
		{
			if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
			{
				return Color.Lerp(new Color(1f, 1f, 1f), whiteCamoColor, whiteCamoColorAmount);
			}
			if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
			{
				return Color.Lerp(palette.blackColor, new Color(0.5f, 0.5f, 0.5f), blackLizardLightUpHead);
			}
			if (lizard.Template.type == CreatureTemplate.Type.Salamander)
			{
				return SalamanderColor;
			}
			if (snowAccCosmetic != null)
			{
				return Color.Lerp(palette.blackColor, effectColor, Mathf.Min(1f, snowAccCosmetic.DebrisSaturation * 1.5f));
			}
			return palette.blackColor;
		}
	}

	private Color HeadColor2
	{
		get
		{
			if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
			{
				return Color.Lerp(palette.blackColor, whiteCamoColor, whiteCamoColorAmount);
			}
			if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
			{
				return Color.Lerp(palette.blackColor, new Color(0.5f, 0.5f, 0.5f), blackLizardLightUpHead);
			}
			if (lizard.Template.type == CreatureTemplate.Type.Salamander)
			{
				return SalamanderColor;
			}
			return effectColor;
		}
	}

	public LizardGraphics(PhysicalObject ow)
		: base(ow, internalContainers: true)
	{
		List<BodyPart> list = new List<BodyPart>();
		lizard = ow as Lizard;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(lizard.abstractCreature.ID.RandomSeed);
		iVars = GenerateIvars();
		limbs = new LizardLimb[Caramel ? 6 : 4];
		for (int i = 0; i < limbs.Length; i++)
		{
			int num = ((i >= 2) ? 2 : 0);
			if (Caramel)
			{
				num = i / 2;
			}
			limbs[i] = new LizardLimb(this, base.owner.bodyChunks[num], i, 2.5f, 0.7f, 0.99f, lizard.lizardParams.limbSpeed, lizard.lizardParams.limbQuickness, (i % 2 == 1) ? limbs[i - 1] : null);
			list.Add(limbs[i]);
		}
		tail = new TailSegment[lizard.lizardParams.tailSegments];
		for (int j = 0; j < lizard.lizardParams.tailSegments; j++)
		{
			float num2 = 8f * lizard.lizardParams.bodySizeFac;
			num2 *= (float)(lizard.lizardParams.tailSegments - j) / (float)lizard.lizardParams.tailSegments;
			float num3 = (((j > 0) ? 8f : 16f) + num2) / 2f;
			num3 *= lizard.lizardParams.tailLengthFactor * iVars.tailLength;
			tail[j] = new TailSegment(this, num2, num3, (j > 0) ? tail[j - 1] : null, 0.85f, 1f, 0.4f, pullInPreviousPosition: false);
			list.Add(tail[j]);
		}
		if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
		{
			for (int k = 0; k < lizard.lizardParams.tailSegments; k++)
			{
				float num4 = Mathf.InverseLerp(0f, lizard.lizardParams.tailSegments - 1, k);
				tail[k].rad += Mathf.Sin(Mathf.Pow(num4, 0.7f) * (float)Math.PI) * 2.5f;
				tail[k].rad *= 1f - Mathf.Sin(Mathf.InverseLerp(0f, 0.4f, num4) * (float)Math.PI) * 0.5f;
			}
		}
		tailLength = 0f;
		for (int l = 0; l < tail.Length; l++)
		{
			tailLength += tail[l].connectionRad;
		}
		if (lizard.tongue != null)
		{
			tongue = new GenericBodyPart[lizard.lizardParams.tongueSegments];
			for (int m = 0; m < tongue.Length; m++)
			{
				tongue[m] = new GenericBodyPart(this, 1f, 1f, 0.9f, lizard.mainBodyChunk);
			}
		}
		head = new GenericBodyPart(this, 6f * lizard.lizardParams.headSize, 0.5f, 0.99f, lizard.bodyChunks[0]);
		list.Add(head);
		headConnectionRad = 11f * lizard.lizardParams.headSize;
		bodyLength = headConnectionRad * 0.5f;
		for (int n = 0; n < lizard.bodyChunkConnections.Length; n++)
		{
			bodyLength += lizard.bodyChunkConnections[n].distance;
		}
		drawPositions = new Vector2[base.owner.bodyChunks.Length, 2];
		if (lizard.room != null && lizard.room.world.region != null)
		{
			blackSalamander = UnityEngine.Random.value < lizard.room.world.region.regionParams.blackSalamanderChance;
		}
		else
		{
			blackSalamander = UnityEngine.Random.value < 1f / 3f;
		}
		cosmetics = new List<Template>();
		int num5 = startOfExtraSprites;
		if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
		{
			num5 = AddCosmetic(num5, new AxolotlGills(this, num5));
			num5 = AddCosmetic(num5, new TailGeckoScales(this, num5));
			if (UnityEngine.Random.value < 0.75f)
			{
				num5 = AddCosmetic(num5, new LongShoulderScales(this, num5));
				num5 = AddCosmetic(num5, new TailFin(this, num5));
			}
			else
			{
				num5 = AddCosmetic(num5, new ShortBodyScales(this, num5));
				num5 = ((!(UnityEngine.Random.value < 0.75f)) ? AddCosmetic(num5, new TailTuft(this, num5)) : AddCosmetic(num5, new TailFin(this, num5)));
			}
		}
		else if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
		{
			num5 = ((!(UnityEngine.Random.value < 0.175f)) ? AddCosmetic(num5, new SpineSpikes(this, num5)) : AddCosmetic(num5, new WingScales(this, num5)));
			num5 = AddCosmetic(num5, new TailTuft(this, num5));
		}
		if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
		{
			if (UnityEngine.Random.value < 0.75f)
			{
				num5 = AddCosmetic(num5, new WingScales(this, num5));
			}
			num5 = ((!(UnityEngine.Random.value < 0.5f) || iVars.tailColor != 0f) ? AddCosmetic(num5, new TailGeckoScales(this, num5)) : AddCosmetic(num5, new TailTuft(this, num5)));
			num5 = AddCosmetic(num5, new JumpRings(this, num5));
		}
		else if (lizard.Template.type != CreatureTemplate.Type.WhiteLizard)
		{
			int num6 = 0;
			bool flag = false;
			bool flag2 = false;
			if (Caramel && UnityEngine.Random.value < 0.6f)
			{
				num5 = AddCosmetic(num5, new BodyStripes(this, num5));
				num6++;
			}
			else if (UnityEngine.Random.value < 1f / 15f || (UnityEngine.Random.value < 0.8f && lizard.Template.type == CreatureTemplate.Type.GreenLizard) || (UnityEngine.Random.value < 0.7f && lizard.Template.type == CreatureTemplate.Type.BlackLizard))
			{
				num5 = AddCosmetic(num5, new SpineSpikes(this, num5));
				num6++;
			}
			else if (UnityEngine.Random.value < 1f / 30f && !Caramel)
			{
				num5 = AddCosmetic(num5, new BumpHawk(this, num5));
				num6++;
			}
			else if ((UnityEngine.Random.value < 1f / 21f || (lizard.Template.type == CreatureTemplate.Type.PinkLizard && UnityEngine.Random.value < 0.5f) || (lizard.Template.type == CreatureTemplate.Type.RedLizard && UnityEngine.Random.value < 0.9f)) && lizard.Template.type != CreatureTemplate.Type.Salamander)
			{
				num5 = AddCosmetic(num5, new LongShoulderScales(this, num5));
				flag = true;
				num6++;
			}
			else if ((UnityEngine.Random.value < 0.0625f || (lizard.Template.type == CreatureTemplate.Type.BlueLizard && UnityEngine.Random.value < 0.5f)) && lizard.Template.type != CreatureTemplate.Type.Salamander)
			{
				num5 = AddCosmetic(num5, new ShortBodyScales(this, num5));
				flag2 = true;
				num6++;
			}
			else if (lizard.Template.type == CreatureTemplate.Type.GreenLizard && UnityEngine.Random.value < 0.5f)
			{
				num5 = AddCosmetic(num5, new ShortBodyScales(this, num5));
				flag2 = true;
				num6++;
			}
			if (lizard.Template.type != CreatureTemplate.Type.Salamander)
			{
				if (Caramel && UnityEngine.Random.value < 0.5f)
				{
					num5 = AddCosmetic(num5, new TailTuft(this, num5));
				}
				else if (UnityEngine.Random.value < 1f / 9f || (num6 == 0 && UnityEngine.Random.value < 0.7f) || (lizard.Template.type == CreatureTemplate.Type.PinkLizard && UnityEngine.Random.value < 0.6f) || (lizard.Template.type == CreatureTemplate.Type.BlueLizard && UnityEngine.Random.value < 0.96f))
				{
					num5 = AddCosmetic(num5, new TailTuft(this, num5));
				}
				else if (num6 < 2 && lizard.Template.type == CreatureTemplate.Type.GreenLizard && UnityEngine.Random.value < 0.7f)
				{
					if (UnityEngine.Random.value < 0.5f || flag || flag2)
					{
						num5 = AddCosmetic(num5, new TailTuft(this, num5));
					}
					else
					{
						num5 = AddCosmetic(num5, new LongShoulderScales(this, num5));
						flag = true;
						num6++;
					}
				}
			}
			if (UnityEngine.Random.value < ((num6 == 0) ? 0.7f : 0.1f) && lizard.Template.type != CreatureTemplate.Type.Salamander && lizard.Template.type != CreatureTemplate.Type.YellowLizard && ((!flag && UnityEngine.Random.value < 0.9f) || UnityEngine.Random.value < 1f / 30f))
			{
				num5 = AddCosmetic(num5, new LongHeadScales(this, num5));
			}
			if (lizard.Template.type == CreatureTemplate.Type.Salamander)
			{
				num5 = AddCosmetic(num5, new AxolotlGills(this, num5));
				num5 = AddCosmetic(num5, new TailFin(this, num5));
			}
			else if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
			{
				num5 = AddCosmetic(num5, new Whiskers(this, num5));
			}
			else if (lizard.Template.type == CreatureTemplate.Type.YellowLizard)
			{
				num5 = AddCosmetic(num5, new Antennae(this, num5));
				if (num6 == 0 && UnityEngine.Random.value < 0.6f)
				{
					num5 = AddCosmetic(num5, new ShortBodyScales(this, num5));
					flag2 = true;
					num6++;
				}
			}
			else if (lizard.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard))
			{
				num5 = AddCosmetic(num5, new LongShoulderScales(this, num5));
				flag = true;
				num6++;
				num5 = AddCosmetic(num5, new SpineSpikes(this, num5));
				num6++;
				num5 = ((!(UnityEngine.Random.value < 0.5f)) ? AddCosmetic(num5, new TailTuft(this, num5)) : AddCosmetic(num5, new TailFin(this, num5)));
			}
			if (num6 == 0 && Caramel)
			{
				num5 = AddCosmetic(num5, new BumpHawk(this, num5));
				num6++;
			}
		}
		else
		{
			if (UnityEngine.Random.value < 0.4f)
			{
				num5 = AddCosmetic(num5, new BumpHawk(this, num5));
			}
			else if (UnityEngine.Random.value < 0.4f)
			{
				num5 = AddCosmetic(num5, new ShortBodyScales(this, num5));
			}
			else if (UnityEngine.Random.value < 0.2f)
			{
				num5 = AddCosmetic(num5, new LongShoulderScales(this, num5));
			}
			else if (UnityEngine.Random.value < 0.2f)
			{
				num5 = AddCosmetic(num5, new LongHeadScales(this, num5));
			}
			if (UnityEngine.Random.value < 0.5f)
			{
				num5 = AddCosmetic(num5, new TailTuft(this, num5));
			}
		}
		if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
		{
			snowAccCosmetic = new SnowAccumulation(this, num5);
			num5 = AddCosmetic(num5, snowAccCosmetic);
		}
		soundLoop = new ChunkDynamicSoundLoop(lizard.mainBodyChunk);
		blink = UnityEngine.Random.value;
		lastBlink = blink;
		lastBreath = breath;
		breath = UnityEngine.Random.value;
		tailDirection = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
		eyes = new Vector2[2, 3];
		bodyParts = list.ToArray();
		for (int num7 = 0; num7 < bodyParts.Length; num7++)
		{
			bodyParts[num7].bodyPartArrayIndex = num7;
		}
		overrideHeadGraphic = -1;
		if (ModManager.MSC)
		{
			if (lizard.abstractCreature.Winterized || (lizard.room != null && lizard.room.game.IsStorySession && lizard.room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint))
			{
				overrideHeadGraphic = UnityEngine.Random.Range(5, 8);
			}
			if (lizard.Template.type == CreatureTemplate.Type.Salamander || lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
			{
				overrideHeadGraphic = -1;
			}
			if (Caramel)
			{
				if (UnityEngine.Random.value < 0.25f)
				{
					overrideHeadGraphic = 3;
				}
				else
				{
					overrideHeadGraphic = 4;
				}
			}
		}
		if (ModManager.MSC)
		{
			if (Caramel)
			{
				float num8 = UnityEngine.Random.Range(0.7f, 1f);
				if (num8 >= 0.8f)
				{
					ivarBodyColor = new HSLColor(UnityEngine.Random.Range(0.075f, 0.125f), UnityEngine.Random.Range(0.4f, 0.9f), num8).rgb;
					lizard.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.1f, 0.03f, 0.2f), 0.55f, Custom.ClampedRandomVariation(0.55f, 0.05f, 0.2f));
				}
				else
				{
					ivarBodyColor = new HSLColor(UnityEngine.Random.Range(0.075f, 0.125f), UnityEngine.Random.Range(0.3f, 0.5f), num8).rgb;
				}
			}
			if (lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
			{
				ivarBodyColor = Color.white;
			}
		}
		UnityEngine.Random.state = state;
	}

	private IndividualVariations GenerateIvars()
	{
		float headSize = Custom.ClampedRandomVariation(0.5f, 0.07f, 0.5f) * 2f;
		if (UnityEngine.Random.value < 0.5f)
		{
			headSize = 1f;
		}
		float num = Custom.ClampedRandomVariation(0.5f, 0.12f, 0.5f) * 2f;
		float num2 = Custom.ClampedRandomVariation(0.5f, 0.2f, 0.3f) * 2f;
		float num3 = Custom.ClampedRandomVariation(0.45f, 0.1f, 0.3f) * 2f;
		float tailColor = 0f;
		if (lizard.Template.type != CreatureTemplate.Type.WhiteLizard && UnityEngine.Random.value > 0.5f)
		{
			tailColor = UnityEngine.Random.value;
		}
		if (lizard.Template.type == CreatureTemplate.Type.RedLizard)
		{
			num = Mathf.Min(1f, num);
			num3 = Mathf.Min(1f, num3);
		}
		else if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
		{
			num = Custom.ClampedRandomVariation(0.45f, 0.06f, 0.5f) * 2f;
		}
		else if (ModManager.MSC && (Caramel || lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
		{
			num = Mathf.Min(0.8f, num);
			num3 = Mathf.Min(0.9f, num3);
		}
		return new IndividualVariations(headSize, num, num2, num3, tailColor);
	}

	private int AddCosmetic(int spriteIndex, Template cosmetic)
	{
		cosmetics.Add(cosmetic);
		spriteIndex += cosmetic.numberOfSprites;
		extraSprites += cosmetic.numberOfSprites;
		return spriteIndex;
	}

	public void CamoAmountControlled()
	{
		if (!lizard.safariControlled)
		{
			whiteCamoColorAmountDrag = Mathf.Lerp(whiteCamoColorAmountDrag, Mathf.InverseLerp(0.65f, 0.4f, lizard.AI.runSpeed), UnityEngine.Random.value);
		}
		else if (!lizard.inputWithDiagonals.HasValue || !lizard.inputWithDiagonals.Value.thrw)
		{
			whiteCamoColorAmountDrag = 0f;
		}
		else if (lizard.mainBodyChunk.vel.magnitude > 0f)
		{
			whiteCamoColorAmountDrag = Mathf.InverseLerp(0.65f, 0.4f, lizard.mainBodyChunk.vel.magnitude / 5f);
		}
		else
		{
			whiteCamoColorAmountDrag = 1f;
		}
	}

	public float BodyChunkDisplayRad(int index)
	{
		return lizard.bodyChunks[index].rad * (1f / lizard.lizardParams.bodyRadFac);
	}

	public override void Update()
	{
		base.Update();
		lastVoiceVisualizationIntensity = voiceVisualizationIntensity;
		voiceVisualizationIntensity = (lizard.voice.MakingASound ? 1f : 0f);
		lastVoiceVisualization = voiceVisualization;
		voiceVisualization = lizard.voice.VisualizedIntensity;
		if (lizard.animation == Lizard.Animation.Lounge)
		{
			soundLoop.sound = SoundID.Lizard_Lounge_Attack_LOOP;
			soundLoop.Pitch = 1f;
			soundLoop.Volume = 1f;
		}
		else
		{
			soundLoop.sound = SoundID.Lizard_Belly_Drag_LOOP;
			soundLoop.Pitch = 1f;
			soundLoop.Volume = 1f;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < 3; i++)
			{
				if (lizard.bodyChunks[i].ContactPoint.x != 0 || lizard.room.GetTile(lizard.bodyChunks[i].pos - new Vector2(BodyChunkDisplayRad(i) + 6f, 0f)).Solid || lizard.room.GetTile(lizard.bodyChunks[i].pos + new Vector2(BodyChunkDisplayRad(i) + 6f, 0f)).Solid)
				{
					num += ((lizard.bodyChunks[i].ContactPoint.x != 0) ? 1f : 0.5f) / 3f;
					num2 += Mathf.Abs(lizard.bodyChunks[i].vel.y) / 3f;
				}
				if (lizard.bodyChunks[i].ContactPoint.y != 0 || lizard.room.GetTile(lizard.bodyChunks[i].pos - new Vector2(0f, BodyChunkDisplayRad(i) + 6f)).Solid || lizard.room.GetTile(lizard.bodyChunks[i].pos + new Vector2(0f, BodyChunkDisplayRad(i) + 6f)).Solid)
				{
					num += ((lizard.bodyChunks[i].ContactPoint.y != 0) ? 1f : 0.5f) / 3f;
					num3 += Mathf.Abs(lizard.bodyChunks[i].vel.x) / 3f;
				}
			}
			for (int j = 0; j < tail.Length; j++)
			{
				if (!(num <= 1f))
				{
					break;
				}
				if (tail[j].terrainContact)
				{
					num += 0.5f / (float)tail.Length;
				}
			}
			bellyDragVolume = Mathf.Lerp(bellyDragVolume, Mathf.Clamp(num, 0f, 1f), 0.2f);
			soundLoop.Pitch = Mathf.Lerp(0.3f, 2.2f, Mathf.Lerp(Mathf.InverseLerp(0.2f, 8f, Mathf.Max(num2, num3)), 0.5f, 0.3f));
			soundLoop.Volume = bellyDragVolume * Mathf.InverseLerp(0.2f, 2f, Mathf.Max(num2, num3));
		}
		soundLoop.Update();
		showDominance = Mathf.Clamp(showDominance - 1f / Mathf.Lerp(60f, 120f, UnityEngine.Random.value), 0f, 1f);
		if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
		{
			if (lizard.dead)
			{
				whiteCamoColorAmount = Mathf.Lerp(whiteCamoColorAmount, 0.3f, 0.01f);
			}
			else
			{
				if ((lizard.State as LizardState).health < 0.6f && UnityEngine.Random.value * 1.5f < (lizard.State as LizardState).health && UnityEngine.Random.value < 1f / (lizard.Stunned ? 10f : 40f))
				{
					whiteGlitchFit = (int)Mathf.Lerp(5f, 40f, (1f - (lizard.State as LizardState).health) * UnityEngine.Random.value);
				}
				if (whiteGlitchFit == 0 && lizard.Stunned && UnityEngine.Random.value < 0.05f)
				{
					whiteGlitchFit = 2;
				}
				if (whiteGlitchFit > 0)
				{
					whiteGlitchFit--;
					float f = 1f - (lizard.State as LizardState).health;
					if (UnityEngine.Random.value < 0.2f)
					{
						whiteCamoColorAmountDrag = 1f;
					}
					if (UnityEngine.Random.value < 0.2f)
					{
						whiteCamoColorAmount = 1f;
					}
					if (UnityEngine.Random.value < 0.5f)
					{
						whiteCamoColor = Color.Lerp(whiteCamoColor, new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value), Mathf.Pow(f, 0.2f) * Mathf.Pow(UnityEngine.Random.value, 0.1f));
					}
					if (UnityEngine.Random.value < 1f / 3f)
					{
						whitePickUpColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
					}
				}
				else if (showDominance > 0f)
				{
					whiteDominanceHue += UnityEngine.Random.value * Mathf.Pow(showDominance, 2f) * 0.2f;
					if (whiteDominanceHue > 1f)
					{
						whiteDominanceHue -= 1f;
					}
					whiteCamoColor = Color.Lerp(whiteCamoColor, Custom.HSL2RGB(whiteDominanceHue, 1f, 0.5f), Mathf.InverseLerp(0.5f, 1f, Mathf.Pow(showDominance, 0.5f)) * UnityEngine.Random.value);
					whiteCamoColorAmount = Mathf.Lerp(whiteCamoColorAmount, 1f - Mathf.Sin(Mathf.InverseLerp(0f, 1.1f, Mathf.Pow(showDominance, 0.5f)) * (float)Math.PI), 0.1f);
				}
				else
				{
					if (lizard.animation == Lizard.Animation.ShootTongue || lizard.animation == Lizard.Animation.PrepareToLounge || lizard.animation == Lizard.Animation.Lounge)
					{
						whiteCamoColorAmountDrag = 0f;
					}
					else if (UnityEngine.Random.value < 0.1f)
					{
						CamoAmountControlled();
					}
					whiteCamoColorAmount = Mathf.Clamp(Mathf.Lerp(whiteCamoColorAmount, whiteCamoColorAmountDrag, 0.1f * UnityEngine.Random.value), 0.15f, 1f);
					whiteCamoColor = Color.Lerp(whiteCamoColor, whitePickUpColor, 0.1f);
				}
			}
		}
		else if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
		{
			if (lizard.bubble > 0)
			{
				blackLizardLightUpHead = Mathf.Min(blackLizardLightUpHead + 0.1f, 1f);
			}
			else
			{
				blackLizardLightUpHead *= 0.9f;
			}
		}
		else if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
		{
			whiteCamoColor = Color.Lerp(whiteCamoColor, whitePickUpColor, 0.1f);
		}
		if (!culled)
		{
			for (int k = 0; k < cosmetics.Count; k++)
			{
				cosmetics[k].Update();
			}
		}
		if (whiteFlicker > 0)
		{
			whiteFlicker--;
		}
		if (freeze > 0)
		{
			freeze--;
			return;
		}
		if (creatureLooker == null)
		{
			creatureLooker = new CreatureLooker(this, lizard.AI.tracker, lizard, 0.3f, lizard.lizardParams.framesBetweenLookFocusChange);
		}
		else
		{
			creatureLooker.Update();
		}
		if (lizard.animation == Lizard.Animation.PreySpotted || lizard.animation == Lizard.Animation.PreyReSpotted)
		{
			creatureLooker.ReevaluateLookObject(lizard.AI.preyTracker.MostAttractivePrey, 10f);
		}
		else if (lizard.animation == Lizard.Animation.ThreatSpotted || lizard.animation == Lizard.Animation.ThreatReSpotted)
		{
			creatureLooker.ReevaluateLookObject(lizard.AI.threatTracker.mostThreateningCreature, 10f);
		}
		if (creatureLooker.lookCreature != null)
		{
			if (creatureLooker.lookCreature.VisualContact)
			{
				lookPos = Custom.MoveTowards(lookPos, creatureLooker.lookCreature.representedCreature.realizedCreature.mainBodyChunk.pos, Mathf.Lerp(100f, 20f, lizard.lizardParams.neckStiffness));
			}
			else
			{
				lookPos = Custom.MoveTowards(lookPos, lizard.room.MiddleOfTile(creatureLooker.lookCreature.BestGuessForPosition().Tile), Mathf.Lerp(100f, 20f, lizard.lizardParams.neckStiffness));
			}
		}
		float num4 = 180f;
		if (creatureLooker.lookCreature != null && lizard.Consious)
		{
			num4 -= 80f;
			if (creatureLooker.lookCreature.VisualContact)
			{
				num4 -= 50f;
				if (lizard.Template.CreatureRelationship(creatureLooker.lookCreature.representedCreature.creatureTemplate).type == CreatureTemplate.Relationship.Type.Eats || lizard.Template.CreatureRelationship(creatureLooker.lookCreature.representedCreature.creatureTemplate).intensity > 0.5f)
				{
					eyeBeamsActive = 1f;
				}
				else
				{
					eyeBeamsActive = Mathf.Clamp(eyeBeamsActive + 1f / 30f, 0f, 1f);
				}
			}
			else
			{
				eyeBeamsActive = Mathf.Clamp(eyeBeamsActive - 0.0125f, 0f, 1f);
			}
		}
		else
		{
			eyeBeamsActive = Mathf.Clamp(eyeBeamsActive - 0.05f, 0f, 1f);
		}
		Vector2 vector = lizard.bodyChunks[0].pos + Custom.DirVec(lizard.bodyChunks[1].pos, lizard.bodyChunks[0].pos) * 300f;
		for (int l = 0; l < 2; l++)
		{
			eyes[l, 1] = eyes[l, 0];
			eyes[l, 2] = Vector2.Lerp(eyes[l, 2], Vector2.ClampMagnitude(eyes[l, 2] + Custom.DegToVec(UnityEngine.Random.value * 360f) * num4 * 0.5f * UnityEngine.Random.value, num4), 0.5f);
			if (l == 0)
			{
				eyes[l, 0] = Vector2.Lerp(eyes[l, 0], lookPos + eyes[l, 2], 0.6f);
			}
			else
			{
				eyes[l, 0] = Vector2.Lerp(eyes[l, 0], Vector2.Lerp(lookPos, vector, Mathf.InverseLerp(25f, 180f, num4)) + eyes[l, 2], 0.3f);
			}
			if (creatureLooker.lookCreature != null && creatureLooker.lookCreature.VisualContact)
			{
				eyes[l, 2] *= 0.5f;
			}
			if (Vector2.Dot((lizard.bodyChunks[0].pos - lizard.bodyChunks[1].pos).normalized, (eyes[l, 0] - lizard.bodyChunks[1].pos).normalized) < lizard.lizardParams.periferalVisionAngle)
			{
				eyes[l, 0] = Vector2.Lerp(eyes[l, 0], vector, 0.1f);
				eyes[l, 2] *= 0.85f;
			}
			if (!Custom.DistLess(eyes[l, 0], vector, 300f))
			{
				eyes[l, 0] = Vector2.Lerp(eyes[l, 0], vector, 0.1f);
			}
			if (Custom.DistLess(eyes[l, 0], lizard.bodyChunks[0].pos, 100f))
			{
				eyes[l, 0] = lizard.bodyChunks[0].pos + Custom.DirVec(lizard.bodyChunks[0].pos, eyes[l, 0]) * 100f;
			}
		}
		if (DEBUGLABELS != null)
		{
			DEBUGLABELS[0].label.text = "like player: " + lizard.AI.LikeOfPlayer(lizard.AI.tracker.RepresentationForCreature(lizard.room.game.Players[0], addIfMissing: false));
			if (lizard.AI.tracker.RepresentationForCreature(lizard.room.game.Players[0], addIfMissing: false) != null && lizard.AI.tracker.RepresentationForCreature(lizard.room.game.Players[0], addIfMissing: false).dynamicRelationship != null)
			{
				DEBUGLABELS[1].label.text = "Player rel : " + lizard.AI.tracker.RepresentationForCreature(lizard.room.game.Players[0], addIfMissing: false).dynamicRelationship.currentRelationship.type.ToString() + " " + lizard.AI.tracker.RepresentationForCreature(lizard.room.game.Players[0], addIfMissing: false).dynamicRelationship.currentRelationship.intensity;
			}
			else
			{
				DEBUGLABELS[1].label.text = "~";
			}
			DEBUGLABELS[2].label.text = "behav: " + lizard.AI.behavior.ToString() + "    run speed: " + lizard.AI.runSpeed;
			DEBUGLABELS[3].label.text = "migrate to: " + lizard.abstractCreature.world.GetAbstractRoom(lizard.abstractCreature.abstractAI.MigrationDestination).name;
		}
		if (lizard.animation == Lizard.Animation.PrepareToLounge)
		{
			headColorSetter = Mathf.Clamp(headColorSetter - 1f / ((float)lizard.lizardParams.preLoungeCrouch / 2f), -1f, 1f);
		}
		else if (lizard.animation == Lizard.Animation.Lounge)
		{
			headColorSetter = 1f;
			if (!debugVisualization)
			{
				lizard.room.AddObject(new LizardBubble(this, 0f, 1f, voiceVisualization * 10f));
			}
			if (lizard.Template.type == CreatureTemplate.Type.GreenLizard)
			{
				for (int m = 0; m < lizard.bodyChunks.Length; m++)
				{
					BodyChunk bodyChunk = lizard.bodyChunks[m];
					if (bodyChunk.ContactPoint.y < 0 && bodyChunk.vel.magnitude > 7f)
					{
						for (int n = 0; n < 3; n++)
						{
							Vector2 vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 10f;
							vel.y = Mathf.Abs(vel.y);
							vel.x += bodyChunk.vel.x;
							lizard.room.AddObject(new WaterDrip(bodyChunk.pos + new Vector2(0f, -1f) * BodyChunkDisplayRad(m) * 0.9f, vel, waterColor: false));
						}
					}
				}
			}
		}
		else if (lizard.animation == Lizard.Animation.ThreatSpotted || lizard.animation == Lizard.Animation.ThreatReSpotted)
		{
			if (lizard.timeInAnimation < 10 || UnityEngine.Random.value < 1f / 6f)
			{
				LizardBubble lizardBubble = new LizardBubble(this, 0f, 0.5f, Mathf.Pow(voiceVisualization, 2f) * 20f);
				lizardBubble.vel = Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(17f, 21f, UnityEngine.Random.value);
				lizardBubble.lifeTime = (int)Mathf.Lerp(10f, 15f, UnityEngine.Random.value);
				lizard.room.AddObject(lizardBubble);
			}
		}
		else
		{
			headColorSetter = Mathf.Lerp(headColorSetter, 0f, 0.2f);
		}
		if (lizard.postLoungeStun > 0)
		{
			headColorSetter = Mathf.Lerp(headColorSetter, -1f, 0.8f);
		}
		scanning += 0.05f;
		if (scanning > 1f)
		{
			scanning -= 1f;
		}
		if (lizard.Stunned && !stunnedLastUpdate)
		{
			rotateWhileStunned = true;
		}
		stunnedLastUpdate = lizard.Stunned;
		for (int num5 = 0; num5 < base.owner.bodyChunks.Length; num5++)
		{
			drawPositions[num5, 1] = drawPositions[num5, 0];
			drawPositions[num5, 0] = base.owner.bodyChunks[num5].pos;
		}
		float num6 = (4f + 7f / lizard.lizardParams.walkBob) / 2f;
		frontBob = (frontBob * num6 + (float)(frontLegsGrabbing - 1)) / (num6 + 1f);
		hindBob = (hindBob * num6 + (float)(hindLegsGrabbing - 1)) / (num6 + 1f);
		drawPositions[0, 0].y += frontBob * lizard.lizardParams.walkBob;
		drawPositions[1, 0].y += frontBob + hindBob * lizard.lizardParams.walkBob * 0.5f;
		drawPositions[2, 0].y += hindBob * lizard.lizardParams.walkBob;
		if (lizard.animation == Lizard.Animation.ShootTongue)
		{
			float num7 = ((lizard.timeInAnimation < lizard.timeToRemainInAnimation / 2 - 2) ? 2f : (-1f));
			drawPositions[0, 0] += Custom.DirVec(drawPositions[0, 0], lizard.bodyChunks[2].pos) * ((float)lizard.timeInAnimation / (float)lizard.lizardParams.tongueWarmUp) * 10f * num7;
			drawPositions[2, 0] += Custom.DirVec(drawPositions[2, 0], lizard.bodyChunks[0].pos) * ((float)lizard.timeInAnimation / (float)lizard.lizardParams.tongueWarmUp) * 10f * num7;
		}
		if (flicker > 0)
		{
			flicker--;
			blink = UnityEngine.Random.value;
			lastBlink = UnityEngine.Random.value;
		}
		else
		{
			lastBlink = blink;
			if (!lizard.Consious)
			{
				blink = Mathf.Lerp(blink - Mathf.Floor(blink), 0.25f, 0.02f);
			}
			else
			{
				blink += Mathf.Lerp(0.0025f, 0.07f, Mathf.Max(lizard.AI.excitement, showDominance)) + UnityEngine.Random.value * 0.001f;
			}
		}
		if (lightSource != null)
		{
			lightSource.stayAlive = true;
			lightSource.setPos = head.pos;
			lightSource.setRad = 100f;
			if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
			{
				lightSource.color = new Color(1f, 1f, 1f);
				lightSource.setAlpha = 0.35f * blackLizardLightUpHead;
			}
			else
			{
				lightSource.setAlpha = 0.6f * (1f - whiteCamoColorAmount);
				lightSource.color = effectColor;
			}
			if (lightSource.slatedForDeletetion || lizard.room.Darkness(head.pos) == 0f)
			{
				lightSource = null;
			}
		}
		else if (lizard.room.Darkness(head.pos) > 0f && lizard.Template.type != CreatureTemplate.Type.Salamander)
		{
			lightSource = new LightSource(head.pos, environmentalLight: false, effectColor, lizard);
			lightSource.requireUpKeep = true;
			lizard.room.AddObject(lightSource);
		}
		lastBreath = breath;
		if (!lizard.dead)
		{
			breath += 0.0125f;
		}
		if (lizard.Consious && UnityEngine.Random.value < 0.05f)
		{
			tailDirection = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
		}
		float num8 = 0f;
		legsGrabbing = 0;
		frontLegsGrabbing = 0;
		hindLegsGrabbing = 0;
		bool flag = true;
		for (int num9 = 0; num9 < limbs.Length; num9++)
		{
			if (lizard.Consious && lizard.swim > 0.5f)
			{
				if (lizard.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
				{
					if (!lizard.salamanderLurk)
					{
						Vector2 vector2 = Custom.DirVec(drawPositions[2, 0], drawPositions[0, 0]);
						Vector2 vector3 = Custom.PerpendicularVector(Custom.DirVec(drawPositions[2, 0], drawPositions[0, 0]));
						float num10 = (float)num9 / (float)limbs.Length + breath * 1.5f;
						limbs[num9].vel += vector2 * Mathf.Sin(num10 * (float)Math.PI * 2f) * 3f;
						if (Mathf.Sin(num10 * (float)Math.PI * 2f) > 0f)
						{
							limbs[num9].vel *= 0.7f;
							limbs[num9].vel += (Custom.ClosestPointOnLine(drawPositions[1, 0] - vector2, drawPositions[1, 0] + vector2, limbs[num9].pos) + vector3 * ((num9 % 2 == 0) ? (-5f) : 5f) - limbs[num9].pos) / 5f;
						}
						else
						{
							limbs[num9].vel += vector3 * (0.5f + 0.5f * Mathf.Cos(num10 * (float)Math.PI * 2f)) * 1.5f * ((num9 % 2 == 0) ? (-1f) : 1f);
						}
					}
				}
				else
				{
					limbs[num9].vel.x += Mathf.Sin(((float)num9 / (float)limbs.Length * (lizard.lizardParams.smoothenLegMovement ? 1f : 0.1f) + breath * 2f) * (float)Math.PI * 2f) * lizard.swim * 0.5f;
				}
			}
			limbs[num9].Update();
			if (limbs[num9].gripCounter >= lizard.lizardParams.limbGripDelay)
			{
				legsGrabbing++;
				if (num9 < 2)
				{
					frontLegsGrabbing++;
				}
				else if (num9 >= limbs.Length - 2)
				{
					hindLegsGrabbing++;
				}
			}
			if (limbs[num9].gripCounter > 0)
			{
				flag = false;
			}
			float num11 = Custom.DistanceToLine(limbs[num9].pos, limbs[num9].connection.pos, limbs[num9].connection.rotationChunk.pos);
			num11 *= ((num9 > 1) ? 1f : (-1f));
			if (Mathf.Abs(num11) > 10f)
			{
				num8 += Mathf.Sign(num11);
			}
			limbs[num9].flip = Mathf.Lerp(limbs[num9].flip, (num11 < 0f) ? 1f : (-1f), 0.3f);
		}
		num8 = Mathf.Clamp(num8, -1f, 1f);
		if (lizard.Consious)
		{
			num8 = Mathf.Lerp(num8, (drawPositions[0, 0].x > drawPositions[1, 0].x) ? (-1f) : 1f, lizard.swim);
		}
		lastDepthRotation = depthRotation;
		if (!lizard.Stunned || rotateWhileStunned)
		{
			depthRotation = Mathf.Lerp(depthRotation, num8, 0.1f);
		}
		lastHeadDepthRotation = headDepthRotation;
		float f2 = Vector2.Dot((lookPos - drawPositions[0, 0]).normalized, (head.pos - drawPositions[0, 0]).normalized);
		f2 = Mathf.InverseLerp(0f, 0.6f, Mathf.Abs(f2));
		headDepthRotation = Mathf.Lerp(headDepthRotation, depthRotation * f2, 0.5f);
		if (flag)
		{
			noGripCounter++;
		}
		else
		{
			noGripCounter = 0;
		}
		tail[0].connectedPoint = drawPositions[2, 0];
		float stiffness = Mathf.Lerp(lizard.lizardParams.tailStiffness, 900f, showDominance);
		if (lizard.Consious && lizard.swim > 0.5f && !lizard.salamanderLurk)
		{
			for (int num12 = 0; num12 < tail.Length; num12++)
			{
				UpdateTailSegment(num12, stiffness);
			}
			if ((lizard.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)) && !lizard.salamanderLurk)
			{
				Vector2 vector4 = Custom.PerpendicularVector(Custom.DirVec(lizard.bodyChunks[2].pos, lizard.bodyChunks[0].pos));
				drawPositions[0, 0] += vector4 * Mathf.Sin(breath * (float)Math.PI * 2f * 3f) * 3.5f * lizard.swim;
				drawPositions[1, 0] += vector4 * Mathf.Sin((breath + 0.4f) * (float)Math.PI * 2f * 3f) * 3f * lizard.swim;
				drawPositions[2, 0] += vector4 * Mathf.Sin((breath + 0.8f) * (float)Math.PI * 2f * 3f) * 2f * lizard.swim;
			}
		}
		else
		{
			for (int num13 = tail.Length - 1; num13 >= 0; num13--)
			{
				UpdateTailSegment(num13, stiffness);
			}
		}
		Vector2 vector5 = head.pos - head.lastPos;
		head.Update();
		if (lizard.grasps[0] != null && lizard.grasps[0].grabbed is Creature)
		{
			head.pos = lizard.grasps[0].grabbed.bodyChunks[lizard.grasps[0].chunkGrabbed].pos;
		}
		else if (lizard.Consious)
		{
			Vector2 vector6 = Custom.DirVec(drawPositions[0, 0], lookPos);
			if (lizard.upcomingConnections.Count > 0)
			{
				vector6 = Vector3.Slerp(vector6, Custom.DirVec(drawPositions[0, 0], lizard.room.MiddleOfTile(lizard.upcomingConnections[lizard.upcomingConnections.Count - 1].destinationCoord)), 0.15f + lizard.lizardParams.neckStiffness * 0.15f);
			}
			float num14 = Vector2.Dot(vector6, Custom.DirVec(drawPositions[1, 0], drawPositions[0, 0]));
			num14 = Mathf.Clamp(0f - num14, 0f, 1f);
			num14 = 1f - num14;
			num14 = Mathf.Pow(num14, 3f);
			num14 = 1f - num14;
			num14 *= 1f - lizard.lizardParams.neckStiffness;
			if (num14 > 0f)
			{
				vector6 = Vector3.Slerp(vector6, Custom.DirVec(drawPositions[1, 0], drawPositions[0, 0]), num14);
			}
			head.vel += vector6 * Mathf.Lerp(6f, 2f, lizard.lizardParams.neckStiffness);
		}
		if (lizard.tongue != null && lizard.tongue.Out)
		{
			head.vel = Custom.DirVec(drawPositions[0, 0], lizard.tongue.pos) * 5f;
		}
		head.ConnectToPoint(drawPositions[0, 0] + Custom.DirVec(drawPositions[1, 0], drawPositions[0, 0]) * 12f * lizard.lizardParams.headSize, headConnectionRad, push: false, 0.2f, base.owner.bodyChunks[0].vel, 0.5f, 0.1f);
		if (!Custom.DistLess(head.pos, head.lastPos + vector5, 40f))
		{
			for (int num15 = 0; num15 < 1 + (int)(Mathf.InverseLerp(10f, 180f, vector5.magnitude) * 4f); num15++)
			{
				if (!debugVisualization)
				{
					lizard.room.AddObject(new LizardBubble(this, 1f, UnityEngine.Random.value, 3f + voiceVisualization * 10f));
				}
			}
		}
		if (lizard.bubble > 0)
		{
			head.pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 5f * BubbleIntensity;
		}
		if (lizard.bubble > 0 || voiceVisualization > 0.5f)
		{
			for (int num16 = 0; num16 < UnityEngine.Random.Range(1, 1 + (int)(BubbleIntensity * 3f)); num16++)
			{
				if (!debugVisualization)
				{
					lizard.room.AddObject(new LizardBubble(this, BubbleIntensity, 0f, Mathf.Pow(voiceVisualization, 2f) * 20f));
				}
			}
		}
		if (lizard.tongue != null)
		{
			for (int num17 = 0; num17 < tongue.Length; num17++)
			{
				if (!lizard.tongue.Out)
				{
					tongue[num17].pos = lizard.tongue.pos;
					tongue[num17].lastPos = lizard.tongue.lastPos;
					tongue[num17].vel *= 0f;
					continue;
				}
				tongue[num17].Update();
				if (UnityEngine.Random.value < 0.01f && tongue[num17].vel.magnitude > 10f)
				{
					lizard.room.AddObject(new WaterDrip(tongue[num17].pos, tongue[num17].vel * UnityEngine.Random.value, waterColor: false));
				}
				float num18 = (float)num17 / (float)(tongue.Length - 1);
				float num19 = Mathf.Abs((float)num17 - ((float)tongue.Length - 1f) / 2f) / (((float)tongue.Length - 1f) / 2f);
				float f3 = num19;
				if (lizard.tongue.state == LizardTongue.State.Retracting)
				{
					f3 = 1f;
				}
				Vector2 vector7 = drawPositions[0, 0] + (Vector2)Vector3.Slerp(Custom.DirVec(drawPositions[0, 0], lizard.tongue.graphPos[0]), Custom.DirVec(drawPositions[1, 0], drawPositions[0, 0]), Mathf.Pow(1f - num18, 1f + Mathf.Pow(lizard.tongue.Stretched, 2f) * 10f) * 0.5f) * Vector2.Distance(drawPositions[0, 0], lizard.tongue.graphPos[0]) * num18;
				tongue[num17].pos = Vector2.Lerp(tongue[num17].pos, vector7, Mathf.Max(lizard.tongue.Stretched * 0.05f + lizard.tongue.SuperStretched * 0.2f, Mathf.Pow(f3, 3f)));
				tongue[num17].vel += (vector7 - tongue[num17].pos) * (lizard.tongue.Stretched * 0.2f + lizard.tongue.SuperStretched * 0.3f);
				if (lizard.Template.type == CreatureTemplate.Type.BlueLizard || lizard.Template.type == CreatureTemplate.Type.CyanLizard)
				{
					tongue[num17].rad = (2f + Mathf.Lerp(3f, 1f, num19) * (1f - Mathf.Pow(lizard.tongue.CombinedStretched, 0.5f))) / (1f + num18 * 0.5f + (lizard.tongue.SuperStretched + 0.5f) * 0.5f * (1f - num19));
				}
				else if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
				{
					tongue[num17].rad = (2f + 0.75f * (1f - lizard.tongue.CombinedStretched) + Mathf.Lerp(8f, 1f, num19) * (1f - Mathf.Pow(lizard.tongue.CombinedStretched, 0.5f))) / (1f + num18 * 0.5f + (lizard.tongue.SuperStretched + 0.5f) * 3f * (1f - num19));
				}
				else if (lizard.Template.type == CreatureTemplate.Type.Salamander)
				{
					tongue[num17].rad = (3f + 0.5f * (1f - lizard.tongue.CombinedStretched) + Mathf.Lerp(8f, 1f, num19) * (1f - Mathf.Pow(lizard.tongue.CombinedStretched, 0.3f))) / (1f + num18 * 0.5f + (lizard.tongue.SuperStretched + 0.5f) * 2f * (1f - num19));
				}
				tongue[num17].PushOutOfTerrain(lizard.room, lizard.mainBodyChunk.pos);
			}
		}
		if (voiceVisualization > 0.2f)
		{
			for (int num20 = 0; num20 < 3; num20++)
			{
				drawPositions[num20, 0] += Custom.RNV() * 8f * Mathf.Pow(Mathf.InverseLerp(0.2f, 1f, voiceVisualization), 0.5f) / (1 + num20);
			}
		}
	}

	private void UpdateTailSegment(int i, float stiffness)
	{
		tail[i].Update();
		if (lizard.room.PointSubmerged(tail[i].pos))
		{
			tail[i].vel *= 0.8f;
		}
		else
		{
			tail[i].vel.y -= 0.9f * Mathf.Pow((float)i / (float)(tail.Length - 1), 3f);
		}
		if (!Custom.DistLess(tail[i].pos, drawPositions[2, 0], 15f * (float)(i + 1)))
		{
			tail[i].pos = base.owner.bodyChunks[2].pos + Custom.DirVec(drawPositions[2, 0], tail[i].pos) * 15f * (i + 1) * (ModManager.MMF ? base.owner.room.gravity : 1f);
		}
		Vector2 a = drawPositions[1, 0];
		if (i == 1)
		{
			a = drawPositions[2, 0];
		}
		else if (i > 1)
		{
			a = tail[i - 2].pos;
		}
		a = Vector2.Lerp(a, drawPositions[1, 0], 0.2f);
		tail[i].vel += Custom.DirVec(a, tail[i].pos) * stiffness * Mathf.Pow(lizard.lizardParams.tailStiffnessDecline, i) / Vector2.Distance(a, tail[i].pos);
		if (i == 0)
		{
			tail[i].vel += tailDirection;
		}
		if (lizard.Consious && lizard.swim > 0.5f)
		{
			if (lizard.Template.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard))
			{
				if (!lizard.salamanderLurk)
				{
					tail[i].vel += (Custom.DirVec(drawPositions[1, 0], drawPositions[2, 0]) * 0.2f + Custom.PerpendicularVector((tail[i].pos - a).normalized) * Mathf.Sin((breath * 7f - (float)i / (float)tail.Length) * 1.5f * (float)Math.PI)) * Mathf.InverseLerp(0.5f, 1f, lizard.swim) * Mathf.Pow(0.85f, i) * 4f;
				}
			}
			else
			{
				tail[i].vel *= 1f - 0.5f * Mathf.InverseLerp(0.5f, 1f, lizard.swim);
				tail[i].vel += (Custom.DirVec(drawPositions[1, 0], drawPositions[2, 0]) + Custom.PerpendicularVector((tail[i].pos - a).normalized) * Mathf.Sin((breath * 5f - (float)i / (float)tail.Length) * 0.3f * (float)Math.PI)) * Mathf.InverseLerp(0.5f, 1f, lizard.swim) * Mathf.Pow(0.85f, i) * 7f;
			}
		}
		else if (showDominance > 0f)
		{
			tail[i].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * showDominance * UnityEngine.Random.value * 2f;
		}
	}

	public override void Reset()
	{
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			drawPositions[i, 0] = base.owner.bodyChunks[i].pos;
			drawPositions[i, 1] = base.owner.bodyChunks[i].pos;
		}
		for (int j = 0; j < tail.Length; j++)
		{
			tail[j].Reset(base.owner.bodyChunks[1].pos);
		}
		for (int k = 0; k < limbs.Length; k++)
		{
			limbs[k].Reset(base.owner.bodyChunks[1].pos);
		}
		for (int l = 0; l < cosmetics.Count; l++)
		{
			cosmetics[l].Reset();
		}
		head.Reset(base.owner.bodyChunks[0].pos);
	}

	public override void SuckedIntoShortCut(Vector2 shortCutPosition)
	{
		for (int i = 0; i < tail.Length; i++)
		{
			tail[i].lastPos = tail[i].pos;
			tail[i].vel *= 0.5f;
			tail[i].pos = (tail[i].pos * 5f + shortCutPosition) / 6f;
		}
		for (int j = 0; j < limbs.Length; j++)
		{
			limbs[j].lastPos = limbs[j].pos;
			limbs[j].vel *= 0.5f;
			limbs[j].pos = (limbs[j].pos * 5f + shortCutPosition) / 6f;
		}
	}

	public void Stun(int st)
	{
		flicker = Custom.IntClamp(st / 3, 3, 15);
	}

	public void WhiteFlicker(int fl)
	{
		if (fl > whiteFlicker)
		{
			whiteFlicker = fl;
		}
		everySecondDraw = false;
	}

	public void TerrainImpact(int chunk, IntVector2 direction, float speed)
	{
		if (debugVisualization || (!(UnityEngine.Random.value < 0.6f) && chunk != 0))
		{
			return;
		}
		for (int i = 0; i < 1 + (int)(Mathf.InverseLerp(20f, 90f, speed) * 3f); i++)
		{
			LizardBubble lizardBubble = new LizardBubble(this, 1f, UnityEngine.Random.value, speed / 15f + Mathf.Pow(voiceVisualization, 2f) * 20f);
			lizard.room.AddObject(lizardBubble);
			if (direction.x != 0)
			{
				lizardBubble.vel.x = Mathf.Abs(lizardBubble.vel.x) * (0f - (float)direction.x);
			}
			if (direction.y != 0)
			{
				lizardBubble.vel.y = Mathf.Abs(lizardBubble.vel.y) * (0f - (float)direction.y);
			}
		}
	}

	public void CreatureSpotted(bool firstSpot, Tracker.CreatureRepresentation crit)
	{
		if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
		{
			blackLizardLightUpHead = Mathf.Min(blackLizardLightUpHead + 0.5f, 1f);
		}
		if (creatureLooker != null)
		{
			creatureLooker.ReevaluateLookObject(crit, firstSpot ? 2f : 4f);
		}
		if (lizard.Template.CreatureRelationship(crit.representedCreature.realizedCreature.Template).type == CreatureTemplate.Relationship.Type.Eats || lizard.Template.CreatureRelationship(crit.representedCreature.realizedCreature.Template).intensity > 0.5f || (creatureLooker != null && creatureLooker.lookCreature == null))
		{
			eyes[UnityEngine.Random.Range(0, 2), 0] = crit.representedCreature.realizedCreature.mainBodyChunk.pos;
			eyeBeamsActive = Mathf.Clamp(eyeBeamsActive + 0.1f, 0f, 1f);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (debugVisualization)
		{
			sLeaser.sprites = new FSprite[TotalDebugSprites];
			for (int i = DebugBodyChunksStart; i < DebugBodyChunksEnd; i++)
			{
				FSprite fSprite = new FSprite("pixel");
				sLeaser.sprites[i] = fSprite;
				fSprite.color = ((i == DebugBodyChunksStart) ? new Color(1f, 1f, 1f) : effectColor);
			}
			sLeaser.sprites[DebugHead] = new FSprite("pixel");
			sLeaser.sprites[DebugHead].color = Color.Lerp(new Color(1f, 1f, 1f), effectColor, 0.5f);
			sLeaser.sprites[DebugHead].scale = head.rad * 2f;
			for (int j = DebugLimbsStart; j < DebugLimbsEnd; j++)
			{
				FSprite fSprite2 = new FSprite("pixel");
				sLeaser.sprites[j] = fSprite2;
				fSprite2.color = Color.Lerp(new Color(0f, 0f, 0f), effectColor, 0.5f);
				fSprite2.scale = limbs[j].rad * 2f;
			}
			for (int k = DebugGrabPosStart; k < DebugGrabPosEnd; k++)
			{
				FSprite fSprite3 = new FSprite("pixel");
				sLeaser.sprites[k] = fSprite3;
				fSprite3.color = new Color(0f, 0f, 1f);
				fSprite3.scale = 10f;
				fSprite3.alpha = 0.5f;
			}
		}
		else
		{
			visualizeVision = rCam.room.game.setupValues.lizardLaserEyes;
			sLeaser.sprites = new FSprite[TotalSprites];
			for (int l = 0; l < cosmetics.Count; l++)
			{
				cosmetics[l].InitiateSprites(sLeaser, rCam);
			}
			if (visualizeVision)
			{
				for (int m = SpriteVisionStart; m < SpriteVisionEnd; m++)
				{
					FSprite fSprite4 = new FSprite("Futile_White");
					sLeaser.sprites[m] = fSprite4;
					fSprite4.color = effectColor;
					fSprite4.scaleX = 0.08f;
					fSprite4.anchorY = 0f;
					fSprite4.shader = lizard.room.game.rainWorld.Shaders["LizardLaser"];
				}
			}
			if (lizard.tongue != null)
			{
				FSprite fSprite5 = new FSprite("Circle20");
				sLeaser.sprites[SpriteTongueStart + 1] = fSprite5;
				fSprite5.anchorY = 0f;
				TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[(tongue.Length - 1) * 4 + 1];
				for (int n = 0; n < tongue.Length - 1; n++)
				{
					int num = n * 4;
					for (int num2 = 0; num2 < 4; num2++)
					{
						array[num + num2] = new TriangleMesh.Triangle(num + num2, num + num2 + 1, num + num2 + 2);
					}
				}
				array[(tongue.Length - 1) * 4] = new TriangleMesh.Triangle((tongue.Length - 1) * 4, (tongue.Length - 1) * 4 + 1, (tongue.Length - 1) * 4 + 2);
				TriangleMesh triangleMesh = new TriangleMesh("Futile_White", array, lizard.Template.type == CreatureTemplate.Type.CyanLizard);
				sLeaser.sprites[SpriteTongueStart] = triangleMesh;
			}
			int num3 = SpriteLimbsColorStart - SpriteLimbsStart;
			for (int num4 = SpriteLimbsStart; num4 < SpriteLimbsEnd; num4++)
			{
				FSprite fSprite6 = new FSprite("pixel");
				sLeaser.sprites[num4] = fSprite6;
				fSprite6.x = -10000f;
				fSprite6.scale = lizard.lizardParams.limbSize;
				fSprite6.color = new Color(0.1f, 0.1f, 0.1f, 1f);
				fSprite6 = new FSprite("pixel");
				sLeaser.sprites[num4 + num3] = fSprite6;
				fSprite6.x = -10000f;
				fSprite6.scale = lizard.lizardParams.limbSize;
				fSprite6.color = effectColor;
			}
			for (int num5 = SpriteBodyCirclesStart; num5 < SpriteBodyCirclesEnd; num5++)
			{
				FSprite fSprite7 = new FSprite("Circle20");
				sLeaser.sprites[num5] = fSprite7;
				fSprite7.x = -10000f;
				fSprite7.color = new Color(0.1f, 0.1f, 0.1f, 1f);
			}
			TriangleMesh.Triangle[] array2 = new TriangleMesh.Triangle[8];
			for (int num6 = 0; num6 < 4; num6++)
			{
				int num7 = num6 * 4;
				array2[num6 * 2] = new TriangleMesh.Triangle(num7, num7 + 1, num7 + 2);
				array2[num6 * 2 + 1] = new TriangleMesh.Triangle(num7 + 1, num7 + 2, num7 + 3);
			}
			TriangleMesh triangleMesh2 = new TriangleMesh("Futile_White", array2, customColor: false);
			sLeaser.sprites[SpriteBodyMesh] = triangleMesh2;
			array2 = new TriangleMesh.Triangle[(tail.Length - 1) * 4 + 1];
			for (int num8 = 0; num8 < tail.Length - 1; num8++)
			{
				int num9 = num8 * 4;
				for (int num10 = 0; num10 < 4; num10++)
				{
					array2[num9 + num10] = new TriangleMesh.Triangle(num9 + num10, num9 + num10 + 1, num9 + num10 + 2);
				}
			}
			array2[(tail.Length - 1) * 4] = new TriangleMesh.Triangle((tail.Length - 1) * 4, (tail.Length - 1) * 4 + 1, (tail.Length - 1) * 4 + 2);
			triangleMesh2 = ((iVars.tailColor != 0f) ? new TriangleMesh("Futile_White", array2, customColor: true) : new TriangleMesh("Futile_White", array2, customColor: false));
			sLeaser.sprites[SpriteTail] = triangleMesh2;
			for (int num11 = SpriteHeadStart; num11 < SpriteHeadEnd; num11++)
			{
				FSprite fSprite8 = new FSprite("pixel");
				sLeaser.sprites[num11] = fSprite8;
				fSprite8.anchorY = ((lizard.Template.type == CreatureTemplate.Type.GreenLizard) ? 0.55f : 0.7f);
			}
			if (lizard.lizardParams.headGraphics[4] == 3)
			{
				sLeaser.sprites[SpriteHeadEnd - 1].anchorY = 0.75f;
			}
		}
		if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
		{
			sLeaser.sprites[SpriteHeadEnd - 1].isVisible = false;
		}
		sLeaser.containers = new FContainer[1]
		{
			new FContainer()
		};
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (culled)
		{
			return;
		}
		if (freeze > 0)
		{
			timeStacker = 1f;
		}
		if (debugVisualization)
		{
			for (int i = DebugBodyChunksStart; i < DebugBodyChunksEnd; i++)
			{
				sLeaser.sprites[i].x = base.owner.bodyChunks[i - DebugBodyChunksStart].pos.x - camPos.x;
				sLeaser.sprites[i].y = base.owner.bodyChunks[i - DebugBodyChunksStart].pos.y - camPos.y;
				sLeaser.sprites[i].scale = BodyChunkDisplayRad(i - DebugBodyChunksStart) * lizard.bodyChunks[i - DebugBodyChunksStart].terrainSqueeze * 2f;
			}
		}
		else
		{
			for (int j = 0; j < cosmetics.Count; j++)
			{
				cosmetics[j].DrawSprites(sLeaser, rCam, timeStacker, camPos);
			}
			if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard || (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
			{
				ColorBody(sLeaser, DynamicBodyColor(0f));
				Color color = rCam.PixelColorAtCoordinate(lizard.mainBodyChunk.pos);
				Color color2 = rCam.PixelColorAtCoordinate(lizard.bodyChunks[1].pos);
				Color color3 = rCam.PixelColorAtCoordinate(lizard.bodyChunks[2].pos);
				if (color == color2)
				{
					whitePickUpColor = color;
				}
				else if (color2 == color3)
				{
					whitePickUpColor = color2;
				}
				else if (color3 == color)
				{
					whitePickUpColor = color3;
				}
				else
				{
					whitePickUpColor = (color + color2 + color3) / 3f;
				}
				if (whiteCamoColorAmount == -1f)
				{
					whiteCamoColor = whitePickUpColor;
					whiteCamoColorAmount = 1f;
				}
			}
			float num = Mathf.Lerp(lastHeadDepthRotation, headDepthRotation, timeStacker);
			float num2 = Mathf.Lerp(lizard.lastJawOpen, lizard.JawOpen, timeStacker);
			if (lizard.JawReadyForBite && lizard.Consious)
			{
				num2 += UnityEngine.Random.value * 0.2f;
			}
			num2 = Mathf.Lerp(num2, Mathf.Lerp(lastVoiceVisualization, voiceVisualization, timeStacker) + 0.2f, Mathf.Lerp(lastVoiceVisualizationIntensity, voiceVisualizationIntensity, timeStacker) * 0.8f);
			num2 = Mathf.Clamp(num2, 0f, 1f);
			float num3 = 5f;
			Vector2 a = Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker);
			a = Vector2.Lerp(a, Vector2.Lerp(head.lastPos, head.pos, timeStacker), 0.2f);
			float num4 = (Mathf.Sin(Mathf.Lerp(lastBreath, breath, timeStacker) * (float)Math.PI * 2f) + 1f) * 0.5f * Mathf.Pow(1f - lizard.AI.runSpeed, 2f);
			for (int k = SpriteBodyCirclesStart; k < SpriteBodyCirclesEnd - 1; k++)
			{
				int num5 = ((k < 2) ? 1 : 2);
				Vector2 vector = BodyPosition(k, timeStacker);
				if (lizard.animation == Lizard.Animation.ThreatSpotted || lizard.animation == Lizard.Animation.ThreatReSpotted)
				{
					vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 2f;
				}
				float num6 = BodyChunkDisplayRad(num5);
				if (k % 2 == 0)
				{
					num6 = (num6 + BodyChunkDisplayRad(num5 - 1)) / 2f;
				}
				num6 *= 1f + num4 * (float)(3 - k) * 0.1f * ((k == 0) ? 0.5f : 1f);
				num6 *= iVars.fatness;
				Vector2 normalized = (a - vector).normalized;
				Vector2 vector2 = Custom.PerpendicularVector(normalized);
				float num7 = Vector2.Distance(vector, a);
				(sLeaser.sprites[SpriteBodyMesh] as TriangleMesh).MoveVertice(k * 4, vector + normalized * num7 - vector2 * num3 - camPos);
				(sLeaser.sprites[SpriteBodyMesh] as TriangleMesh).MoveVertice(k * 4 + 1, vector + normalized * num7 + vector2 * num3 - camPos);
				(sLeaser.sprites[SpriteBodyMesh] as TriangleMesh).MoveVertice(k * 4 + 2, vector - vector2 * num6 - camPos);
				(sLeaser.sprites[SpriteBodyMesh] as TriangleMesh).MoveVertice(k * 4 + 3, vector + vector2 * num6 - camPos);
				num3 = num6;
				a = vector;
				sLeaser.sprites[k].scale = num6 / 10f;
				sLeaser.sprites[k].x = vector.x - camPos.x;
				sLeaser.sprites[k].y = vector.y - camPos.y;
			}
			int num8 = SpriteLimbsColorStart - SpriteLimbsStart;
			for (int l = SpriteLimbsStart; l < SpriteLimbsEnd; l++)
			{
				Vector2 vector3 = Vector2.Lerp(limbs[l - SpriteLimbsStart].lastPos, limbs[l - SpriteLimbsStart].pos, timeStacker);
				int num9 = ((l >= SpriteLimbsStart + 2) ? 2 : 0);
				if (limbs.Length > 4)
				{
					num9 = Math.Min((int)((float)(l - SpriteLimbsStart) / 2f), 2);
				}
				Vector2 vector4 = Vector2.Lerp(drawPositions[num9, 1], drawPositions[num9, 0], timeStacker);
				if (l < SpriteLimbsStart + 2)
				{
					vector4 = Vector2.Lerp(vector4, Vector2.Lerp(drawPositions[1, 1], drawPositions[1, 0], timeStacker), 0.2f);
				}
				int val = (int)(Vector2.Distance(vector3, vector4) / (4f * lizard.lizardParams.limbSize)) + 1;
				val = Custom.IntClamp(val, 1, 9);
				val += 9 * (2 - (int)Mathf.Clamp(Mathf.Abs(limbs[l - SpriteLimbsStart].flip) * 3f, 0f, 2f));
				if (l >= SpriteLimbsStart + 2)
				{
					val += 27;
				}
				sLeaser.sprites[l].x = vector3.x - camPos.x;
				sLeaser.sprites[l].y = vector3.y - camPos.y;
				sLeaser.sprites[l].rotation = Custom.AimFromOneVectorToAnother(vector4, vector3) - 90f;
				sLeaser.sprites[l].scaleY = Mathf.Sign(limbs[l - SpriteLimbsStart].flip) * lizard.lizardParams.limbThickness;
				if (val < 10)
				{
					sLeaser.sprites[l].element = Futile.atlasManager.GetElementWithName("LizardArm_0" + val);
				}
				else
				{
					sLeaser.sprites[l].element = Futile.atlasManager.GetElementWithName("LizardArm_" + val);
				}
				sLeaser.sprites[l + num8].x = vector3.x - camPos.x;
				sLeaser.sprites[l + num8].y = vector3.y - camPos.y;
				sLeaser.sprites[l + num8].rotation = Custom.AimFromOneVectorToAnother(vector4, vector3) - 90f;
				sLeaser.sprites[l + num8].scaleY = Mathf.Sign(limbs[l - SpriteLimbsStart].flip) * lizard.lizardParams.limbThickness;
				if (val < 10)
				{
					sLeaser.sprites[l + num8].element = Futile.atlasManager.GetElementWithName("LizardArmColor_0" + val);
				}
				else
				{
					sLeaser.sprites[l + num8].element = Futile.atlasManager.GetElementWithName("LizardArmColor_" + val);
				}
				if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard && l >= SpriteLimbsStart + 2)
				{
					sLeaser.sprites[l].isVisible = false;
					sLeaser.sprites[l + num8].isVisible = false;
				}
				else
				{
					sLeaser.sprites[l].isVisible = true;
					sLeaser.sprites[l + num8].isVisible = true;
				}
			}
			if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard || lizard.Template.type == CreatureTemplate.Type.BlackLizard || lizard.Template.type == CreatureTemplate.Type.CyanLizard)
			{
				for (int m = SpriteLimbsStart; m < SpriteLimbsEnd; m++)
				{
					sLeaser.sprites[m + num8].alpha = Mathf.Sin(whiteCamoColorAmount * (float)Math.PI) * 0.3f;
					sLeaser.sprites[m + num8].color = palette.blackColor;
				}
			}
			else if (lizard.Template.type == CreatureTemplate.Type.Salamander)
			{
				for (int n = SpriteLimbsStart; n < SpriteLimbsEnd; n++)
				{
					if (n % 2 == 1)
					{
						sLeaser.sprites[n + num8].alpha = Mathf.Lerp(0.3f, 0.1f, Mathf.Abs(Mathf.Lerp(lastDepthRotation, depthRotation, timeStacker)));
					}
					else
					{
						sLeaser.sprites[n + num8].alpha = 0.3f;
					}
					sLeaser.sprites[n + num8].color = (blackSalamander ? effectColor : palette.blackColor);
				}
			}
			else
			{
				for (int num10 = SpriteLimbsStart; num10 < SpriteLimbsEnd; num10 += 2)
				{
					sLeaser.sprites[num10 + num8].alpha = Mathf.Lerp(1f, 0.3f, Mathf.Abs(Mathf.Lerp(lastDepthRotation, depthRotation, timeStacker)));
				}
			}
			a = Vector2.Lerp(drawPositions[2, 1], drawPositions[2, 0], timeStacker);
			num3 = BodyChunkDisplayRad(2) * iVars.fatness;
			for (int num11 = 0; num11 < tail.Length; num11++)
			{
				Vector2 vector5 = Vector2.Lerp(tail[num11].lastPos, tail[num11].pos, timeStacker);
				Vector2 normalized2 = (vector5 - a).normalized;
				Vector2 vector6 = Custom.PerpendicularVector(normalized2);
				float num12 = Vector2.Distance(vector5, a) / 5f;
				(sLeaser.sprites[SpriteTail] as TriangleMesh).MoveVertice(num11 * 4, a - vector6 * (num3 + tail[num11].StretchedRad) * 0.5f * iVars.fatness * iVars.tailFatness + normalized2 * num12 - camPos);
				(sLeaser.sprites[SpriteTail] as TriangleMesh).MoveVertice(num11 * 4 + 1, a + vector6 * (num3 + tail[num11].StretchedRad) * 0.5f * iVars.fatness * iVars.tailFatness + normalized2 * num12 - camPos);
				if (num11 < tail.Length - 1)
				{
					(sLeaser.sprites[SpriteTail] as TriangleMesh).MoveVertice(num11 * 4 + 2, vector5 - vector6 * tail[num11].StretchedRad * iVars.fatness * iVars.tailFatness - normalized2 * num12 - camPos);
					(sLeaser.sprites[SpriteTail] as TriangleMesh).MoveVertice(num11 * 4 + 3, vector5 + vector6 * tail[num11].StretchedRad * iVars.fatness * iVars.tailFatness - normalized2 * num12 - camPos);
				}
				else
				{
					(sLeaser.sprites[SpriteTail] as TriangleMesh).MoveVertice(num11 * 4 + 2, vector5 - camPos);
				}
				num3 = tail[num11].StretchedRad * iVars.fatness * iVars.tailFatness;
				a = vector5;
			}
			Vector2 vector7 = Vector2.Lerp(Vector2.Lerp(head.lastPos, head.pos, timeStacker), Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker), 0.2f);
			if (lizard.Consious)
			{
				if (lizard.bubble > 0)
				{
					vector7 += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * BubbleIntensity * lizard.SnakeCoil;
				}
				if (whiteFlicker > 0)
				{
					vector7 += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * 2f * lizard.SnakeCoil;
				}
				if (lizard.animation == Lizard.Animation.PreySpotted || lizard.animation == Lizard.Animation.PreyReSpotted || lizard.animation == Lizard.Animation.ThreatSpotted || lizard.animation == Lizard.Animation.ThreatReSpotted)
				{
					vector7 += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * (1f + 2f * Mathf.Lerp(lastVoiceVisualization, voiceVisualization, timeStacker)) * lizard.SnakeCoil;
				}
			}
			float num13 = Custom.AimFromOneVectorToAnother(Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker), Vector2.Lerp(head.lastPos, head.pos, timeStacker));
			Vector2 normalized3 = Custom.PerpendicularVector(Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker) - Vector2.Lerp(head.lastPos, head.pos, timeStacker)).normalized;
			for (int num14 = SpriteHeadStart; num14 < SpriteHeadEnd; num14++)
			{
				if (num14 > SpriteHeadStart + 1)
				{
					sLeaser.sprites[num14].x = vector7.x + normalized3.x * num2 * num * lizard.lizardParams.jawOpenMoveJawsApart * (1f - lizard.lizardParams.jawOpenLowerJawFac) - camPos.x;
					sLeaser.sprites[num14].y = vector7.y + normalized3.y * num2 * num * lizard.lizardParams.jawOpenMoveJawsApart * (1f - lizard.lizardParams.jawOpenLowerJawFac) - camPos.y;
					sLeaser.sprites[num14].rotation = num13 + lizard.lizardParams.jawOpenAngle * (1f - lizard.lizardParams.jawOpenLowerJawFac) * num2 * num;
				}
				else
				{
					sLeaser.sprites[num14].x = vector7.x - normalized3.x * num2 * num * lizard.lizardParams.jawOpenMoveJawsApart * lizard.lizardParams.jawOpenLowerJawFac - camPos.x;
					sLeaser.sprites[num14].y = vector7.y - normalized3.y * num2 * num * lizard.lizardParams.jawOpenMoveJawsApart * lizard.lizardParams.jawOpenLowerJawFac - camPos.y;
					sLeaser.sprites[num14].rotation = num13 - lizard.lizardParams.jawOpenAngle * lizard.lizardParams.jawOpenLowerJawFac * num2 * num;
				}
				sLeaser.sprites[num14].scaleX = Mathf.Sign(num) * lizard.lizardParams.headSize * iVars.headSize;
				sLeaser.sprites[num14].scaleY = lizard.lizardParams.headSize * iVars.headSize;
			}
			int num15 = 3 - (int)(Mathf.Abs(num) * 3.9f);
			sLeaser.sprites[SpriteHeadStart].element = Futile.atlasManager.GetElementWithName("LizardJaw" + num15 + "." + lizard.lizardParams.headGraphics[0]);
			sLeaser.sprites[SpriteHeadStart + 1].element = Futile.atlasManager.GetElementWithName("LizardLowerTeeth" + num15 + "." + lizard.lizardParams.headGraphics[1]);
			sLeaser.sprites[SpriteHeadStart + 2].element = Futile.atlasManager.GetElementWithName("LizardUpperTeeth" + num15 + "." + lizard.lizardParams.headGraphics[2]);
			if (overrideHeadGraphic >= 0)
			{
				sLeaser.sprites[SpriteHeadStart + 3].element = Futile.atlasManager.GetElementWithName("LizardHead" + num15 + "." + overrideHeadGraphic);
			}
			else
			{
				sLeaser.sprites[SpriteHeadStart + 3].element = Futile.atlasManager.GetElementWithName("LizardHead" + num15 + "." + lizard.lizardParams.headGraphics[3]);
			}
			sLeaser.sprites[SpriteHeadStart + 4].element = Futile.atlasManager.GetElementWithName("LizardEyes" + num15 + "." + lizard.lizardParams.headGraphics[4]);
			if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
			{
				sLeaser.sprites[SpriteHeadStart + 4].color = effectColor;
				sLeaser.sprites[SpriteHeadStart + 1].color = HeadColor(timeStacker);
				sLeaser.sprites[SpriteHeadStart + 2].color = HeadColor(timeStacker);
			}
			else
			{
				if (lizard.Template.type == CreatureTemplate.Type.BlackLizard)
				{
					sLeaser.sprites[SpriteHeadStart + 2].color = Color.Lerp(palette.blackColor, new Color(0.5f, 0.5f, 0.5f), Mathf.Pow(blackLizardLightUpHead, 1f - 0.95f * num2));
				}
				sLeaser.sprites[SpriteHeadStart].color = HeadColor(timeStacker);
				sLeaser.sprites[SpriteHeadStart + 3].color = HeadColor(timeStacker);
			}
			if (lizard.tongue != null)
			{
				if (lizard.tongue.Out)
				{
					sLeaser.sprites[SpriteTongueStart + 1].isVisible = true;
					sLeaser.sprites[SpriteTongueStart].isVisible = true;
					Vector2 vector8 = Vector2.Lerp(lizard.tongue.graphPos[1], lizard.tongue.graphPos[0], timeStacker);
					Vector2 vector9 = Vector2.Lerp(tongue[tongue.Length - 2].lastPos, tongue[tongue.Length - 2].pos, timeStacker);
					sLeaser.sprites[SpriteTongueStart + 1].x = vector8.x - camPos.x;
					sLeaser.sprites[SpriteTongueStart + 1].y = vector8.y - camPos.y;
					sLeaser.sprites[SpriteTongueStart + 1].rotation = Custom.AimFromOneVectorToAnother(vector8, vector9);
					sLeaser.sprites[SpriteTongueStart + 1].scaleX = 0.05f * ((lizard.Template.type == CreatureTemplate.Type.BlueLizard) ? 4f : 5f);
					sLeaser.sprites[SpriteTongueStart + 1].scaleY = 0.05f * Vector2.Distance(vector8, vector9);
					a = Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker);
					num3 = 8f;
					for (int num16 = 0; num16 < tongue.Length; num16++)
					{
						Vector2 vector10 = Vector2.Lerp(tongue[num16].lastPos, tongue[num16].pos, timeStacker);
						Vector2 normalized4 = (vector10 - a).normalized;
						Vector2 vector11 = Custom.PerpendicularVector(normalized4);
						float num17 = Vector2.Distance(vector10, a) / 5f;
						float rad = tongue[num16].rad;
						(sLeaser.sprites[SpriteTongueStart] as TriangleMesh).MoveVertice(num16 * 4, a - vector11 * (num3 + rad) * 0.5f + normalized4 * num17 - camPos);
						(sLeaser.sprites[SpriteTongueStart] as TriangleMesh).MoveVertice(num16 * 4 + 1, a + vector11 * (num3 + rad) * 0.5f + normalized4 * num17 - camPos);
						if (num16 < tongue.Length - 1)
						{
							(sLeaser.sprites[SpriteTongueStart] as TriangleMesh).MoveVertice(num16 * 4 + 2, vector10 - vector11 * rad - normalized4 * num17 - camPos);
							(sLeaser.sprites[SpriteTongueStart] as TriangleMesh).MoveVertice(num16 * 4 + 3, vector10 + vector11 * rad - normalized4 * num17 - camPos);
						}
						else
						{
							(sLeaser.sprites[SpriteTongueStart] as TriangleMesh).MoveVertice(num16 * 4 + 2, vector10 - camPos);
						}
						num3 = rad;
						a = vector10;
					}
					if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
					{
						for (int num18 = 0; num18 < (sLeaser.sprites[SpriteTongueStart] as TriangleMesh).verticeColors.Length; num18++)
						{
							(sLeaser.sprites[SpriteTongueStart] as TriangleMesh).verticeColors[num18] = Color.Lerp(HeadColor(timeStacker), palette.blackColor, Mathf.InverseLerp(0f, (sLeaser.sprites[SpriteTongueStart] as TriangleMesh).verticeColors.Length - 1, num18));
						}
					}
				}
				else
				{
					sLeaser.sprites[SpriteTongueStart + 1].isVisible = false;
					sLeaser.sprites[SpriteTongueStart].isVisible = false;
				}
			}
			if (visualizeVision && lizard.room != null)
			{
				if (eyeBeamsActive >= 0.2f)
				{
					for (int num19 = 0; num19 < 2; num19++)
					{
						Vector2 vector12 = vector7 + Custom.DirVec(vector7, Vector2.Lerp(eyes[num19, 1], eyes[num19, 0], timeStacker)) * 10000f;
						Vector2 vector13 = Vector2.Lerp(lizard.bodyChunks[0].lastPos, lizard.bodyChunks[0].pos, timeStacker);
						Vector2 vector14 = Vector2.Lerp(lizard.bodyChunks[1].lastPos, lizard.bodyChunks[1].pos, timeStacker);
						if (Vector2.Dot((vector13 - vector14).normalized, (vector12 - vector14).normalized) < lizard.lizardParams.periferalVisionAngle)
						{
							vector12 = vector7 + Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector14, vector13) + lizard.lizardParams.periferalVisionAngle * Mathf.Sign(Custom.DistanceToLine(vector12, vector13, vector14))) * 10000f;
						}
						float num20 = Vector2.Distance(vector12, vector7);
						Vector2 normalized5 = (vector12 - vector7).normalized;
						for (float num21 = 0f; num21 < num20; num21 += 10f)
						{
							if (lizard.room.GetTile(vector7 + normalized5 * num21).Terrain == Room.Tile.TerrainType.Solid)
							{
								vector12 = vector7 + normalized5 * num21;
								break;
							}
						}
						if (creatureLooker.lookCreature != null && creatureLooker.lookCreature.VisualContact && creatureLooker.lookCreature.representedCreature.realizedCreature != null)
						{
							for (int num22 = 0; num22 < creatureLooker.lookCreature.representedCreature.realizedCreature.bodyChunks.Length; num22++)
							{
								float num23 = Custom.CirclesCollisionTime(vector7.x, vector7.y, creatureLooker.lookCreature.representedCreature.realizedCreature.bodyChunks[num22].pos.x, creatureLooker.lookCreature.representedCreature.realizedCreature.bodyChunks[num22].pos.y, vector12.x - vector7.x, vector12.y - vector7.y, 0f, creatureLooker.lookCreature.representedCreature.realizedCreature.bodyChunks[num22].rad);
								if (num23 > 0f && num23 < 1f)
								{
									vector12 = Vector2.Lerp(vector7, vector12, num23);
								}
							}
						}
						sLeaser.sprites[SpriteVisionEnd - 1 - num19].scaleY = Vector2.Distance(vector7, vector12) * 0.06f;
						sLeaser.sprites[SpriteVisionEnd - 1 - num19].isVisible = true;
						sLeaser.sprites[SpriteVisionEnd - 1 - num19].alpha = Mathf.InverseLerp(0.2f, 1f, eyeBeamsActive);
						sLeaser.sprites[SpriteVisionEnd - 1 - num19].x = vector12.x - camPos.x;
						sLeaser.sprites[SpriteVisionEnd - 1 - num19].y = vector12.y - camPos.y;
						sLeaser.sprites[SpriteVisionEnd - 1 - num19].rotation = Custom.AimFromOneVectorToAnother(vector12, vector7);
					}
				}
				else
				{
					sLeaser.sprites[SpriteVisionEnd - 1].isVisible = false;
					sLeaser.sprites[SpriteVisionEnd - 2].isVisible = false;
				}
			}
			if (flicker > 10)
			{
				flickerColor = UnityEngine.Random.value;
			}
		}
		if (UnityEngine.Random.value > 0.025f)
		{
			everySecondDraw = !everySecondDraw;
		}
	}

	public Color HeadColor(float timeStacker)
	{
		if (whiteFlicker > 0 && (whiteFlicker > 15 || everySecondDraw))
		{
			return new Color(1f, 1f, 1f);
		}
		float a = 1f - Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(lastBlink, blink, timeStacker) * 2f * (float)Math.PI), 1.5f + lizard.AI.excitement * 1.5f);
		if (headColorSetter != 0f)
		{
			a = Mathf.Lerp(a, (headColorSetter > 0f) ? 1f : 0f, Mathf.Abs(headColorSetter));
		}
		if (flicker > 10)
		{
			a = flickerColor;
		}
		a = Mathf.Lerp(a, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastVoiceVisualization, voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(lastVoiceVisualizationIntensity, voiceVisualizationIntensity, timeStacker));
		return Color.Lerp(HeadColor1, HeadColor2, a);
	}

	public float HeadRotation(float timeStacker)
	{
		float num = Custom.AimFromOneVectorToAnother(Vector2.Lerp(drawPositions[0, 1], drawPositions[0, 0], timeStacker), Vector2.Lerp(head.lastPos, head.pos, timeStacker));
		float num2 = Mathf.Lerp(lastHeadDepthRotation, headDepthRotation, timeStacker);
		float num3 = Mathf.Clamp(Mathf.Lerp(lizard.lastJawOpen, lizard.JawOpen, timeStacker), 0f, 1f);
		return num + lizard.lizardParams.jawOpenAngle * (1f - lizard.lizardParams.jawOpenLowerJawFac) * num3 * num2;
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		this.palette = palette;
		if (debugVisualization)
		{
			return;
		}
		if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
		{
			ColorBody(sLeaser, new Color(1f, 1f, 1f));
		}
		else if (ModManager.MSC && (Caramel || lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
		{
			ColorBody(sLeaser, ivarBodyColor);
		}
		else if (lizard.Template.type == CreatureTemplate.Type.Salamander)
		{
			ColorBody(sLeaser, SalamanderColor);
		}
		else
		{
			ColorBody(sLeaser, palette.blackColor);
		}
		sLeaser.sprites[SpriteHeadStart + 1].color = palette.blackColor;
		sLeaser.sprites[SpriteHeadStart + 2].color = palette.blackColor;
		sLeaser.sprites[SpriteHeadStart + 4].color = palette.blackColor;
		if (lizard.tongue != null)
		{
			sLeaser.sprites[SpriteTongueStart].color = palette.blackColor;
			sLeaser.sprites[SpriteTongueStart + 1].color = palette.blackColor;
		}
		for (int i = 0; i < cosmetics.Count; i++)
		{
			cosmetics[i].ApplyPalette(sLeaser, rCam, palette);
		}
		if (iVars.tailColor > 0f)
		{
			for (int j = 0; j < (sLeaser.sprites[SpriteTail] as TriangleMesh).verticeColors.Length; j++)
			{
				float t = (float)(j / 2) * 2f / (float)((sLeaser.sprites[SpriteTail] as TriangleMesh).verticeColors.Length - 1);
				(sLeaser.sprites[SpriteTail] as TriangleMesh).verticeColors[j] = BodyColor(Mathf.Lerp(bodyLength / BodyAndTailLength, 1f, t));
			}
		}
		if (lizard.Template.type == CreatureTemplate.Type.Salamander && blackSalamander)
		{
			sLeaser.sprites[SpriteHeadStart + 4].color = effectColor;
		}
		else if (lizard.Template.type == CreatureTemplate.Type.CyanLizard)
		{
			sLeaser.sprites[SpriteHeadStart].color = palette.blackColor;
			sLeaser.sprites[SpriteHeadStart + 3].color = palette.blackColor;
		}
	}

	public Color DynamicBodyColor(float f)
	{
		if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
		{
			return Color.Lerp(new Color(1f, 1f, 1f), whiteCamoColor, whiteCamoColorAmount);
		}
		if (lizard.Template.type == CreatureTemplate.Type.Salamander)
		{
			return SalamanderColor;
		}
		if (ModManager.MSC && lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard && snowAccCosmetic != null)
		{
			return Color.Lerp(ivarBodyColor, Color.Lerp(ivarBodyColor, whiteCamoColor, 0.5f), Mathf.Min(1f, snowAccCosmetic.DebrisSaturation * 1.5f));
		}
		return palette.blackColor;
	}

	public Color BodyColor(float f)
	{
		if (ModManager.MSC && (Caramel || lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard) && (f < bodyLength / BodyAndTailLength || iVars.tailColor == 0f))
		{
			return ivarBodyColor;
		}
		if (lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
		{
			return DynamicBodyColor(f);
		}
		if (lizard.Template.type == CreatureTemplate.Type.Salamander)
		{
			return SalamanderColor;
		}
		if (f < bodyLength / BodyAndTailLength || iVars.tailColor == 0f)
		{
			return palette.blackColor;
		}
		float value = Mathf.InverseLerp(bodyLength / BodyAndTailLength, 1f, f);
		float f2 = Mathf.Clamp(Mathf.InverseLerp(lizard.lizardParams.tailColorationStart, 0.95f, value), 0f, 1f);
		f2 = Mathf.Pow(f2, lizard.lizardParams.tailColorationExponent) * iVars.tailColor;
		if (ModManager.MSC && (Caramel || lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
		{
			return Color.Lerp(ivarBodyColor, effectColor, f2);
		}
		return Color.Lerp(palette.blackColor, effectColor, f2);
	}

	private void ColorBody(RoomCamera.SpriteLeaser sLeaser, Color col)
	{
		sLeaser.sprites[SpriteBodyMesh].color = col;
		sLeaser.sprites[SpriteTail].color = col;
		for (int i = SpriteBodyCirclesStart; i < SpriteBodyCirclesEnd; i++)
		{
			sLeaser.sprites[i].color = col;
		}
		for (int j = SpriteLimbsStart; j < SpriteLimbsEnd; j++)
		{
			sLeaser.sprites[j].color = col;
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		if (debugVisualization)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
			return;
		}
		sLeaser.RemoveAllSpritesFromContainer();
		if (visualizeVision)
		{
			for (int i = 0; i < 2; i++)
			{
				rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[SpriteVisionEnd - 1 - i]);
			}
		}
		for (int j = 0; j < cosmetics.Count; j++)
		{
			if (cosmetics[j].spritesOverlap == Template.SpritesOverlap.Behind)
			{
				cosmetics[j].AddToContainer(sLeaser, rCam, newContatiner);
			}
		}
		int num = SpriteLimbsColorStart - SpriteLimbsStart;
		for (int k = 7; k < 11; k++)
		{
			newContatiner.AddChild(sLeaser.sprites[k + num]);
		}
		for (int l = SpriteLimbsStart; l < SpriteLimbsEnd; l += 2)
		{
			newContatiner.AddChild(sLeaser.sprites[l]);
			newContatiner.AddChild(sLeaser.sprites[l + num]);
		}
		newContatiner.AddChild(sLeaser.sprites[SpriteBodyMesh]);
		for (int m = SpriteBodyCirclesStart; m < SpriteBodyCirclesEnd; m++)
		{
			newContatiner.AddChild(sLeaser.sprites[m]);
		}
		newContatiner.AddChild(sLeaser.sprites[SpriteTail]);
		for (int n = SpriteLimbsStart + 1; n < SpriteLimbsEnd; n += 2)
		{
			newContatiner.AddChild(sLeaser.sprites[n]);
			newContatiner.AddChild(sLeaser.sprites[n + num]);
		}
		for (int num2 = 0; num2 < cosmetics.Count; num2++)
		{
			if (cosmetics[num2].spritesOverlap == Template.SpritesOverlap.BehindHead)
			{
				cosmetics[num2].AddToContainer(sLeaser, rCam, newContatiner);
			}
		}
		newContatiner.AddChild(sLeaser.sprites[SpriteHeadStart]);
		newContatiner.AddChild(sLeaser.sprites[SpriteHeadStart + 1]);
		if (lizard.tongue != null)
		{
			for (int num3 = 0; num3 < 2; num3++)
			{
				newContatiner.AddChild(sLeaser.sprites[SpriteTongueStart + num3]);
			}
		}
		newContatiner.AddChild(sLeaser.containers[0]);
		newContatiner.AddChild(sLeaser.sprites[SpriteHeadStart + 2]);
		newContatiner.AddChild(sLeaser.sprites[SpriteHeadStart + 3]);
		newContatiner.AddChild(sLeaser.sprites[SpriteHeadStart + 4]);
		for (int num4 = 0; num4 < cosmetics.Count; num4++)
		{
			if (cosmetics[num4].spritesOverlap == Template.SpritesOverlap.InFront)
			{
				cosmetics[num4].AddToContainer(sLeaser, rCam, newContatiner);
			}
		}
	}

	public float CreatureInterestBonus(Tracker.CreatureRepresentation crit, float score)
	{
		float value = Vector2.Dot((lizard.bodyChunks[0].pos - lizard.bodyChunks[1].pos).normalized, (lizard.room.MiddleOfTile(crit.BestGuessForPosition()) - lizard.bodyChunks[1].pos).normalized);
		return score * Mathf.Lerp(Mathf.Clamp(value, 0.1f, 1f), 1f, 0.5f);
	}

	public Tracker.CreatureRepresentation ForcedLookCreature()
	{
		return lizard.AI.focusCreature;
	}

	public void LookAtNothing()
	{
		lookPos = drawPositions[0, 0] + Custom.DirVec(drawPositions[1, 0], drawPositions[0, 0]) * 100f + Custom.DegToVec(UnityEngine.Random.value * 360f) * 100f * UnityEngine.Random.value;
	}

	public LizardSpineData SpinePosition(float s, float timeStacker)
	{
		float num = lizard.bodyChunkConnections[0].distance + lizard.bodyChunkConnections[1].distance;
		Vector2 vector;
		float a;
		Vector2 vector2;
		Vector2 vector3;
		float b;
		float t;
		if (s < num / (num + tailLength))
		{
			float num2 = Mathf.InverseLerp(0f, num / (num + tailLength), s);
			int num3 = Mathf.FloorToInt(num2 * 4f - 1f);
			int num4 = Mathf.FloorToInt(num2 * 4f);
			if (num3 < 0)
			{
				vector = Vector2.Lerp(BodyPosition(0, timeStacker), Vector2.Lerp(head.lastPos, head.pos, timeStacker), 0.5f);
				a = head.rad;
			}
			else
			{
				vector = BodyPosition(num3, timeStacker);
				a = BodyChunkDisplayRad((num3 < 2) ? 1 : 2) * iVars.fatness;
			}
			vector2 = ((num4 + 1 >= 4) ? Vector2.Lerp(tail[0].lastPos, tail[0].pos, timeStacker) : BodyPosition(num4 + 1, timeStacker));
			vector3 = BodyPosition(num4, timeStacker);
			b = BodyChunkDisplayRad((num4 < 2) ? 1 : 2);
			t = Mathf.InverseLerp(num3 + 1, num4 + 1, num2 * 4f);
		}
		else
		{
			float num5 = Mathf.InverseLerp(num / (num + tailLength), 1f, s);
			int num6 = Mathf.FloorToInt(num5 * (float)tail.Length - 1f);
			int num7 = Mathf.FloorToInt(num5 * (float)tail.Length);
			if (num7 > tail.Length - 1)
			{
				num7 = tail.Length - 1;
			}
			if (num6 < 0)
			{
				vector = BodyPosition(3, timeStacker);
				a = BodyChunkDisplayRad(2) * iVars.fatness;
			}
			else
			{
				vector = Vector2.Lerp(tail[num6].lastPos, tail[num6].pos, timeStacker);
				a = tail[num6].StretchedRad * iVars.fatness * iVars.tailFatness;
			}
			vector2 = Vector2.Lerp(tail[Math.Min(num7 + 1, tail.Length - 1)].lastPos, tail[Math.Min(num7 + 1, tail.Length - 1)].pos, timeStacker);
			vector3 = Vector2.Lerp(tail[num7].lastPos, tail[num7].pos, timeStacker);
			b = tail[num7].StretchedRad;
			t = Mathf.InverseLerp(num6 + 1, num7 + 1, num5 * (float)tail.Length);
		}
		Vector2 normalized = Vector2.Lerp(vector3 - vector, vector2 - vector3, t).normalized;
		if (normalized.x == 0f && normalized.y == 0f)
		{
			normalized = (tail[tail.Length - 1].pos - tail[tail.Length - 2].pos).normalized;
		}
		Vector2 vector4 = Custom.PerpendicularVector(normalized);
		float num8 = Mathf.Lerp(a, b, t);
		float f = Mathf.Lerp(lastDepthRotation, depthRotation, timeStacker);
		f = Mathf.Pow(Mathf.Abs(f), Mathf.Lerp(1.2f, 0.3f, Mathf.Pow(s, 0.5f))) * Mathf.Sign(f);
		Vector2 outerPos = Vector2.Lerp(vector, vector3, t) + vector4 * f * num8;
		return new LizardSpineData(s, Vector2.Lerp(vector, vector3, t), outerPos, normalized, vector4, f, num8);
	}

	private Vector2 BodyPosition(int p, float timeStacker)
	{
		if (p > 3)
		{
			return new Vector2(100f, 100f);
		}
		int num = ((p < 2) ? 1 : 2);
		if (p % 2 == 1)
		{
			return Vector2.Lerp(drawPositions[num, 1], drawPositions[num, 0], timeStacker);
		}
		Vector2 vector = Vector2.Lerp(drawPositions[num, 1], drawPositions[num, 0], timeStacker);
		Vector2 vector2 = Vector2.Lerp(tail[0].lastPos, tail[0].pos, timeStacker);
		if (num == 1)
		{
			vector2 = Vector2.Lerp(drawPositions[2, 1], drawPositions[2, 0], timeStacker);
		}
		Vector2 vector3 = Vector2.Lerp(drawPositions[num - 1, 1], drawPositions[num - 1, 0], timeStacker);
		Vector3 normalized = Vector3.Slerp(vector - vector3, vector2 - vector, 0.35f).normalized;
		normalized *= -0.5f * Vector2.Distance(vector, vector3);
		return new Vector2(vector.x + normalized.x, vector.y + normalized.y);
	}
}
