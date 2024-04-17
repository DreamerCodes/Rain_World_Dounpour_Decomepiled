using Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class KarmaFlower : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public class Part
	{
		public KarmaFlower owner;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public Part(KarmaFlower owner)
		{
			this.owner = owner;
			pos = owner.firstChunk.pos;
			lastPos = owner.firstChunk.pos;
			vel *= 0f;
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			if (owner.room.PointSubmerged(pos))
			{
				vel *= 0.7f;
			}
			else
			{
				vel *= 0.95f;
			}
		}

		public void Reset()
		{
			lastPos = owner.firstChunk.pos;
			pos = owner.firstChunk.pos;
			vel *= 0f;
		}
	}

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	public Vector2? growPos;

	public Vector2 hoverPos;

	public float hoverDirAdd;

	public Part[] petals;

	public Part[] stalk;

	private float movement;

	private float lastMovement;

	private float faceCamera;

	private bool removeRespawnFlowerFromMap;

	private Color stalkColor;

	public int bites = 4;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	public int StalkSprite => 0;

	public int RingSprite => 5;

	public int TotalSprites => 9;

	public int BitesLeft => bites;

	public int FoodPoints => 0;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public int PetalSprite(int p)
	{
		return 1 + p;
	}

	public int EffectSprite(int i)
	{
		return 6 + i;
	}

	public KarmaFlower(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), ModManager.MSC ? 5f : 2f, 0.05f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.93f;
		base.gravity = 0.6f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 0;
		base.waterFriction = 0.95f;
		base.buoyancy = 0.9f;
		faceCamera = Mathf.Lerp(0.2f, 0.8f, abstractPhysicalObject.world.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed));
		petals = new Part[4];
		for (int i = 0; i < petals.Length; i++)
		{
			petals[i] = new Part(this);
		}
		stalk = new Part[6];
		for (int j = 0; j < stalk.Length; j++)
		{
			stalk[j] = new Part(this);
		}
	}

	public void ResetParts()
	{
		for (int i = 0; i < petals.Length; i++)
		{
			petals[i].Reset();
		}
		for (int j = 0; j < stalk.Length; j++)
		{
			stalk[j].Reset();
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetParts();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastMovement = movement;
		movement = Mathf.InverseLerp(0f, 12f, Vector2.Distance(base.firstChunk.lastPos, base.firstChunk.pos));
		lastRotation = rotation;
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
			Consume();
			if (growPos.HasValue)
			{
				growPos = null;
			}
		}
		else if (!growPos.HasValue && base.firstChunk.ContactPoint.y == 0 && base.firstChunk.ContactPoint.x == 0)
		{
			rotation += base.firstChunk.pos - stalk[2].pos;
			rotation.Normalize();
		}
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		if (base.firstChunk.ContactPoint.y < 0)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * 0.1f * base.firstChunk.vel.x).normalized;
			base.firstChunk.vel.x *= 0.8f;
		}
		for (int i = 0; i < petals.Length; i++)
		{
			petals[i].Update();
			Vector2 vector = base.firstChunk.pos + rotation * 7f * 0.75f + Custom.FlattenVectorAlongAxis(Custom.DegToVec(Custom.VecToDeg(rotation) + 90f * (float)i) * 13f * 0.75f, Custom.VecToDeg(rotation), faceCamera);
			float val = Vector2.Dot((base.firstChunk.pos - petals[i].pos).normalized, (base.firstChunk.pos - vector).normalized);
			petals[i].vel = Vector2.Lerp(petals[i].vel, base.firstChunk.pos - base.firstChunk.lastPos, Custom.LerpMap(val, 1f, -1f, 0f, 1f));
			petals[i].vel += (vector - petals[i].pos) / Custom.LerpMap(val, -1f, 1f, 3f, 30f);
			petals[i].pos += (vector - petals[i].pos) / Custom.LerpMap(val, -1f, 1f, 3f, 60f);
			if (!Custom.DistLess(base.firstChunk.pos, petals[i].pos, 13.5f))
			{
				Vector2 vector2 = Custom.DirVec(petals[i].pos, base.firstChunk.pos);
				float num = Vector2.Distance(petals[i].pos, base.firstChunk.pos);
				petals[i].pos -= (13.5f - num) * vector2;
				petals[i].vel -= (13.5f - num) * vector2;
			}
		}
		for (int j = 0; j < stalk.Length; j++)
		{
			stalk[j].Update();
			if (!growPos.HasValue)
			{
				stalk[j].vel.y -= Mathf.InverseLerp(0f, stalk.Length - 1, j) * 0.4f;
			}
		}
		for (int k = 0; k < stalk.Length; k++)
		{
			ConnectStalkSegment(k);
		}
		for (int num2 = stalk.Length - 1; num2 >= 0; num2--)
		{
			ConnectStalkSegment(num2);
		}
		for (int l = 0; l < 4; l++)
		{
			Vector2 vector3 = base.firstChunk.pos - rotation * (3 + l) * 5f;
			float val2 = Vector2.Dot((base.firstChunk.pos - stalk[l].pos).normalized, (base.firstChunk.pos - vector3).normalized);
			stalk[l].vel = Vector2.Lerp(stalk[l].vel, base.firstChunk.pos - base.firstChunk.lastPos, Custom.LerpMap(val2, 1f, -1f, 0f, 1f) * Mathf.InverseLerp(4f, 0f, l));
			stalk[l].vel += (vector3 - stalk[l].pos) / Custom.LerpMap(val2, -1f, 1f, 3f, 30f) * Mathf.InverseLerp(4f, 0f, l);
			stalk[l].pos += (vector3 - stalk[l].pos) / Custom.LerpMap(val2, -1f, 1f, 3f, 60f) * Mathf.InverseLerp(4f, 0f, l);
		}
		for (int m = 0; m < stalk.Length; m++)
		{
			ConnectStalkSegment(m);
		}
		for (int num3 = stalk.Length - 1; num3 >= 0; num3--)
		{
			ConnectStalkSegment(num3);
		}
		if (growPos.HasValue)
		{
			stalk[stalk.Length - 1].pos = growPos.Value;
			stalk[stalk.Length - 1].vel *= 0f;
			base.firstChunk.vel.y += base.gravity;
			base.firstChunk.vel *= 0.7f;
			base.firstChunk.vel += (hoverPos - base.firstChunk.pos) / 20f;
			rotation = Custom.DegToVec(Custom.AimFromOneVectorToAnother(growPos.Value, base.firstChunk.pos) + hoverDirAdd);
			if (!room.game.rainWorld.safariMode && (!ModManager.MMF || MMF.cfgExtraTutorials.Value) && room.game.session is StoryGameSession && !(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.KarmaFlowerMessage && room.ViewedByAnyCamera(base.firstChunk.pos, 20f))
			{
				(room.game.session as StoryGameSession).saveState.deathPersistentSaveData.KarmaFlowerMessage = true;
				room.game.cameras[0].hud.textPrompt.AddMessage(room.game.rainWorld.inGameTranslator.Translate("Here is a strange energy"), 80, 120, darken: true, hideHud: true);
			}
		}
		for (int n = 2; n < stalk.Length; n++)
		{
			Vector2 vector4 = Custom.DirVec(stalk[n - 2].pos, stalk[n].pos);
			stalk[n].vel += vector4 * 2.3f;
			stalk[n - 2].vel -= vector4 * 2.3f;
		}
	}

	private void ConnectStalkSegment(int i)
	{
		float num = 5f;
		if (i == 0)
		{
			Vector2 vector = Custom.DirVec(stalk[i].pos, base.firstChunk.pos);
			float num2 = Vector2.Distance(stalk[i].pos, base.firstChunk.pos);
			stalk[i].pos -= (num - num2) * vector;
			stalk[i].vel -= (num - num2) * vector;
		}
		else
		{
			Vector2 vector2 = Custom.DirVec(stalk[i].pos, stalk[i - 1].pos);
			float num3 = Vector2.Distance(stalk[i].pos, stalk[i - 1].pos);
			stalk[i].pos -= (num - num3) * vector2 * 0.5f;
			stalk[i].vel -= (num - num3) * vector2 * 0.5f;
			stalk[i - 1].pos += (num - num3) * vector2 * 0.5f;
			stalk[i - 1].vel += (num - num3) * vector2 * 0.5f;
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (AbstrConsumable.originRoom < 0)
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			TryRoot(placeRoom);
			if (!AbstrConsumable.isConsumed)
			{
				AbstrConsumable.Consume();
			}
		}
		else if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			TryRoot(placeRoom);
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			rotation = Custom.RNV();
			lastRotation = rotation;
		}
		ResetParts();
	}

	private void TryRoot(Room placeRoom)
	{
		int x = placeRoom.GetTilePosition(base.firstChunk.pos).x;
		int num = placeRoom.GetTilePosition(base.firstChunk.pos).y;
		while (num >= 0 && num >= placeRoom.GetTilePosition(base.firstChunk.pos).y - 4)
		{
			if (!placeRoom.GetTile(x, num).Solid && placeRoom.GetTile(x, num - 1).Solid)
			{
				growPos = new Vector2(placeRoom.MiddleOfTile(x, num).x + Mathf.Lerp(-9f, 9f, placeRoom.game.SeededRandom((int)base.firstChunk.pos.x + (int)base.firstChunk.pos.y)), placeRoom.MiddleOfTile(x, num).y - 10f);
				hoverPos = new Vector2(growPos.Value.x + Mathf.Lerp(-7f, 7f, placeRoom.game.SeededRandom((int)base.firstChunk.pos.x - (int)base.firstChunk.pos.y)), growPos.Value.y + Mathf.Lerp(18f, 36f, placeRoom.game.SeededRandom((int)base.firstChunk.pos.y - (int)base.firstChunk.pos.x)));
				hoverDirAdd = Mathf.Lerp(-25f, 25f, placeRoom.game.SeededRandom((int)base.firstChunk.pos.x));
				base.firstChunk.HardSetPosition(hoverPos);
			}
			num--;
		}
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (growPos.HasValue)
		{
			growPos = null;
			Consume();
		}
	}

	private void Consume()
	{
		if (!AbstrConsumable.isConsumed)
		{
			AbstrConsumable.Consume();
		}
		if (!removeRespawnFlowerFromMap && AbstrConsumable.originRoom == -2)
		{
			Custom.Log("remove respawned karma flower from map");
			removeRespawnFlowerFromMap = true;
			if (abstractPhysicalObject.world != null && abstractPhysicalObject.world.game != null && abstractPhysicalObject.world.game.session != null && abstractPhysicalObject.world.game.session is StoryGameSession)
			{
				(abstractPhysicalObject.world.game.session as StoryGameSession).karmaFlowerMapPos = null;
				(abstractPhysicalObject.world.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaFlowerPosition = null;
			}
			if (abstractPhysicalObject.world != null && abstractPhysicalObject.world.game != null && abstractPhysicalObject.world.game.cameras[0].hud != null && abstractPhysicalObject.world.game.cameras[0].hud.map != null)
			{
				abstractPhysicalObject.world.game.cameras[0].hud.map.RemoveKarmaFlower();
			}
		}
		if (ModManager.Expedition && abstractPhysicalObject.world != null && abstractPhysicalObject.world.game != null && abstractPhysicalObject.world.game.rainWorld.ExpeditionMode)
		{
			ExpeditionGame.tempKarmaPos = null;
			abstractPhysicalObject.world.game.rainWorld.progression.currentSaveState.deathPersistentSaveData.karmaFlowerPosition = null;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[StalkSprite] = TriangleMesh.MakeLongMesh(stalk.Length, pointyTip: false, customColor: true);
		for (int i = 0; i < 4; i++)
		{
			sLeaser.sprites[PetalSprite(i)] = new FSprite("KarmaPetal");
			sLeaser.sprites[PetalSprite(i)].anchorY = 0f;
		}
		sLeaser.sprites[RingSprite] = TriangleMesh.MakeGridMesh("EndGameCircle", 5);
		for (int j = 0; j < 3; j++)
		{
			sLeaser.sprites[EffectSprite(j)] = new FSprite("Futile_White");
		}
		sLeaser.sprites[EffectSprite(0)].shader = rCam.room.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[EffectSprite(1)].shader = rCam.room.game.rainWorld.Shaders["FlatLightBehindTerrain"];
		sLeaser.sprites[EffectSprite(2)].shader = rCam.room.game.rainWorld.Shaders["GoldenGlow"];
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		bool flag = blink > 0 && Random.value < 0.5f;
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2[] array = new Vector2[4];
		Vector2 vector2 = vector;
		int num = 1;
		for (int i = 0; i < petals.Length; i++)
		{
			if (i < bites)
			{
				Vector2 vector3 = Vector2.Lerp(petals[i].lastPos, petals[i].pos, timeStacker);
				sLeaser.sprites[PetalSprite(i)].x = vector.x - camPos.x;
				sLeaser.sprites[PetalSprite(i)].y = vector.y - camPos.y;
				sLeaser.sprites[PetalSprite(i)].rotation = Custom.AimFromOneVectorToAnother(vector, vector3);
				sLeaser.sprites[PetalSprite(i)].scaleY = Vector2.Distance(vector, vector3) / 20f;
				sLeaser.sprites[PetalSprite(i)].scaleX = 0.375f;
				sLeaser.sprites[PetalSprite(i)].isVisible = true;
				sLeaser.sprites[PetalSprite(i)].color = (flag ? base.blinkColor : color);
				array[i] = vector3 + Custom.DirVec(vector, vector3) * 2f - camPos;
				vector2 += vector3;
				num++;
			}
			else
			{
				sLeaser.sprites[PetalSprite(i)].isVisible = false;
				array[i] = vector - camPos;
			}
		}
		vector2 /= (float)num;
		sLeaser.sprites[StalkSprite].color = (flag ? new Color(1f, 1f, 1f) : color);
		sLeaser.sprites[RingSprite].color = (flag ? new Color(1f, 1f, 1f) : color);
		TriangleMesh.QuadGridMesh(array, sLeaser.sprites[RingSprite] as TriangleMesh, 5);
		for (int j = 0; j < 3; j++)
		{
			sLeaser.sprites[EffectSprite(j)].x = vector2.x - camPos.x;
			sLeaser.sprites[EffectSprite(j)].y = vector2.y - camPos.y;
		}
		float t = Mathf.InverseLerp(0f, 4f, bites);
		sLeaser.sprites[EffectSprite(0)].scale = 75f * Mathf.Lerp(0.5f, 1f, t) / 16f;
		sLeaser.sprites[EffectSprite(0)].alpha = (flag ? 0f : (0.4f * (1f - Mathf.Lerp(lastMovement, movement, timeStacker)) * Mathf.Lerp(0.5f, 1f, t)));
		sLeaser.sprites[EffectSprite(0)].color = Custom.HSL2RGB(RainWorld.AntiGold.hue, 0.6f, 0.2f);
		sLeaser.sprites[EffectSprite(1)].scale = (flag ? 20f : 40f) * Mathf.Lerp(0.5f, 1f, t) / 16f;
		sLeaser.sprites[EffectSprite(1)].alpha = (flag ? 0.5f : 0.7f) * Mathf.Lerp(0.5f, 1f, t);
		sLeaser.sprites[EffectSprite(1)].color = (flag ? new Color(1f, 1f, 1f) : color);
		sLeaser.sprites[EffectSprite(2)].scale = 40f * Mathf.Lerp(0.5f, 1f, t) / 16f;
		sLeaser.sprites[EffectSprite(2)].alpha = (flag ? 0f : (0.8f * Mathf.Lerp(0.5f, 1f, t)));
		Vector2 vector4 = vector;
		float num2 = 0.75f;
		for (int k = 0; k < stalk.Length; k++)
		{
			Vector2 vector5 = Vector2.Lerp(stalk[k].lastPos, stalk[k].pos, timeStacker);
			Vector2 normalized = (vector5 - vector4).normalized;
			Vector2 vector6 = Custom.PerpendicularVector(normalized);
			float num3 = Vector2.Distance(vector5, vector4) / 5f;
			if (k == 0)
			{
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(k * 4, vector4 - vector6 * num2 - camPos);
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(k * 4 + 1, vector4 + vector6 * num2 - camPos);
			}
			else
			{
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(k * 4, vector4 - vector6 * num2 + normalized * num3 - camPos);
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(k * 4 + 1, vector4 + vector6 * num2 + normalized * num3 - camPos);
			}
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(k * 4 + 2, vector5 - vector6 * num2 - normalized * num3 - camPos);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(k * 4 + 3, vector5 + vector6 * num2 - normalized * num3 - camPos);
			vector4 = vector5;
		}
		for (int l = 0; l < (sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors.Length; l++)
		{
			float t2 = (float)l / (float)((sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).verticeColors[l] = Color.Lerp(flag ? new Color(1f, 1f, 1f) : color, stalkColor, t2);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = RainWorld.GoldRGB;
		stalkColor = Color.Lerp(palette.blackColor, palette.fogColor, 0.3f);
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
		}
		for (int j = 0; j < petals.Length; j++)
		{
			newContatiner.AddChild(sLeaser.sprites[PetalSprite(j)]);
		}
		newContatiner.AddChild(sLeaser.sprites[StalkSprite]);
		newContatiner.AddChild(sLeaser.sprites[RingSprite]);
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[EffectSprite(0)]);
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[EffectSprite(1)]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[EffectSprite(2)]);
	}

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Eat_Karma_Flower : SoundID.Slugcat_Bite_Karma_Flower, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites >= 1)
		{
			return;
		}
		(grasp.grabber as Player).ObjectEaten(this);
		if ((grasp.grabber as Player).room.game.session is StoryGameSession && !((grasp.grabber as Player).room.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma)
		{
			((grasp.grabber as Player).room.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = true;
			for (int i = 0; i < (grasp.grabber as Player).room.game.cameras.Length; i++)
			{
				if ((grasp.grabber as Player).room.game.cameras[i].followAbstractCreature == (grasp.grabber as Player).abstractCreature)
				{
					(grasp.grabber as Player).room.game.cameras[i].hud.karmaMeter.reinforceAnimation = 0;
					break;
				}
				if (ModManager.CoopAvailable)
				{
					(grasp.grabber as Player).room.game.cameras[i].hud.karmaMeter.reinforceAnimation = 0;
				}
			}
		}
		grasp.Release();
		Destroy();
	}

	public void ThrowByPlayer()
	{
	}
}
