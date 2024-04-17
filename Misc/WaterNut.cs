using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class WaterNut : Rock
{
	public class Stalk : UpdatableAndDeletable, IDrawable
	{
		public WaterNut nut;

		public Vector2[,] segments;

		public Vector2 rootPos;

		public Vector2 direction;

		public Vector2 nutPos;

		public Stalk(WaterNut nut, Room room)
		{
			this.nut = nut;
			nutPos = nut.firstChunk.pos;
			base.room = room;
			IntVector2 tilePosition = room.GetTilePosition(nut.firstChunk.pos);
			while (tilePosition.y >= 0 && !room.GetTile(tilePosition).Solid)
			{
				tilePosition.y--;
			}
			rootPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, -10f);
			segments = new Vector2[Custom.IntClamp((int)(Vector2.Distance(nut.firstChunk.pos, rootPos) / 15f), 4, 60), 3];
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 0] = Vector2.Lerp(rootPos, nut.firstChunk.pos, (float)i / (float)segments.GetLength(0));
				segments[i, 1] = segments[i, 0];
			}
			direction = Custom.DegToVec(Mathf.Lerp(-90f, 90f, room.game.SeededRandom((int)(nutPos.x + nutPos.y))));
			for (int j = 0; j < 100; j++)
			{
				Update(eu: false);
			}
			nut.ChangeCollisionLayer(0);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				segments[i, 1] = segments[i, 0];
				if (i == 0)
				{
					segments[i, 0] = rootPos;
					segments[i, 2] *= 0f;
				}
				else if (i == segments.GetLength(0) - 1 && nut != null)
				{
					segments[i, 0] = nut.firstChunk.pos;
					segments[i, 2] *= 0f;
				}
				else
				{
					segments[i, 0] += segments[i, 2];
					segments[i, 2] *= 0.7f;
					segments[i, 2].y += 0.3f;
					segments[i, 2] += direction * 0.4f * (1f - ((float)i + 1f) / (float)segments.GetLength(0));
				}
				if (i < segments.GetLength(0) - 1)
				{
					Vector2 normalized = (segments[i, 0] - segments[i + 1, 0]).normalized;
					float num = 15f;
					float num2 = Vector2.Distance(segments[i, 0], segments[i + 1, 0]);
					segments[i, 0] += normalized * (num - num2) * 0.5f;
					segments[i, 2] += normalized * (num - num2) * 0.5f;
					segments[i + 1, 0] -= normalized * (num - num2) * 0.5f;
					segments[i + 1, 2] -= normalized * (num - num2) * 0.5f;
				}
				if (i < segments.GetLength(0) - 2)
				{
					Vector2 normalized2 = (segments[i, 0] - segments[i + 2, 0]).normalized;
					segments[i, 2] += normalized2 * 1.5f;
					segments[i + 2, 2] -= normalized2 * 1.5f;
				}
				if (i == 0)
				{
					segments[i, 0] = rootPos;
					segments[i, 2] *= 0f;
				}
				if (Custom.DistLess(segments[i, 1], segments[i, 0], 10f))
				{
					segments[i, 1] = segments[i, 0];
				}
			}
			if (nut != null)
			{
				if (!Custom.DistLess(nutPos, nut.firstChunk.pos, (nut.grabbedBy.Count == 0) ? 100f : 20f) || nut.room != room || nut.slatedForDeletetion || nut.firstChunk.vel.magnitude > 15f)
				{
					nut.AbstrNut.Consume();
					nut.stalk = null;
					nut = null;
				}
				else
				{
					nut.firstChunk.vel.y += nut.gravity;
					nut.firstChunk.vel *= 0.6f;
					nut.firstChunk.vel += (nutPos - nut.firstChunk.pos) / 20f;
					nut.setRotation = Custom.DirVec(segments[segments.GetLength(0) - 2, 0], nut.firstChunk.pos);
				}
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: false);
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(segments[0, 1], segments[0, 0], timeStacker);
			for (int i = 0; i < segments.GetLength(0); i++)
			{
				Vector2 vector2 = Vector2.Lerp(segments[i, 1], segments[i, 0], timeStacker);
				Vector2 normalized = (vector2 - vector).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num = Vector2.Distance(vector2, vector) / 5f;
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, vector - vector3 * 0.5f + normalized * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, vector + vector3 * 0.5f + normalized * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * 0.5f - normalized * num - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * 0.5f - normalized * num - camPos);
				vector = vector2;
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			(sLeaser.sprites[0] as TriangleMesh).color = palette.blackColor;
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			sLeaser.sprites[0].RemoveFromContainer();
			rCam.ReturnFContainer("Items").AddChild(sLeaser.sprites[0]);
		}
	}

	public class AbstractWaterNut : AbstractConsumable
	{
		public bool swollen;

		public AbstractWaterNut(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int consumableIndex, PlacedObject.ConsumableObjectData consumableData, bool swollen)
			: base(world, AbstractObjectType.WaterNut, realizedObject, pos, ID, originRoom, consumableIndex, consumableData)
		{
			this.swollen = swollen;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}<oA>{5}", ID.ToString(), type.ToString(), pos.SaveToString(), originRoom, placedObjectIndex, swollen ? "1" : "0");
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public int swellCounter;

	public Stalk stalk;

	public AbstractWaterNut AbstrNut => abstractPhysicalObject as AbstractWaterNut;

	public WaterNut(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject, abstractPhysicalObject.world)
	{
		swellCounter = Random.Range(4, 120);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		bool flag = false;
		if (ModManager.MSC)
		{
			for (int i = 0; i < grabbedBy.Count; i++)
			{
				if (grabbedBy[i].grabber is Player && (grabbedBy[i].grabber as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
				{
					flag = true;
					break;
				}
			}
		}
		if (base.Submersion > 0f || flag)
		{
			swellCounter--;
			if (swellCounter < 1)
			{
				Swell();
			}
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			stalk = new Stalk(this, placeRoom);
			placeRoom.AddObject(stalk);
		}
		else if (!AbstrNut.isConsumed && AbstrNut.placedObjectIndex >= 0 && AbstrNut.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			base.firstChunk.HardSetPosition(placeRoom.roomSettings.placedObjects[AbstrNut.placedObjectIndex].pos);
			stalk = new Stalk(this, placeRoom);
			placeRoom.AddObject(stalk);
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
		rotationSpeed = 0f;
	}

	private void Swell()
	{
		if (grabbedBy.Count > 0)
		{
			grabbedBy[0].Release();
		}
		AbstrNut.swollen = true;
		room.PlaySound(SoundID.Water_Nut_Swell, base.firstChunk.pos);
		SwollenWaterNut swollenWaterNut = new SwollenWaterNut(abstractPhysicalObject);
		swollenWaterNut.plop = 0.01f;
		swollenWaterNut.lastPlop = 0f;
		swollenWaterNut.rotation = rotation;
		swollenWaterNut.lastRotation = lastRotation;
		swollenWaterNut.addAbstractEntity = true;
		room.AddObject(swollenWaterNut);
		swollenWaterNut.firstChunk.HardSetPosition(base.firstChunk.pos);
		swollenWaterNut.AbstrConsumable.isFresh = AbstrNut.isFresh;
		Destroy();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("JetFishEyeA");
		sLeaser.sprites[0].scaleX = 1.2f;
		sLeaser.sprites[0].scaleY = 1.4f;
		sLeaser.sprites[1] = new FSprite("tinyStar");
		sLeaser.sprites[1].scaleY = 2f;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		if (vibrate > 0)
		{
			vector += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
		}
		sLeaser.sprites[0].x = vector.x - camPos.x;
		sLeaser.sprites[0].y = vector.y - camPos.y;
		sLeaser.sprites[1].x = vector.x - camPos.x;
		sLeaser.sprites[1].y = vector.y - camPos.y;
		float num = Custom.VecToDeg(Vector3.Slerp(lastRotation, rotation, timeStacker));
		if (blink > 0 && Random.value < 0.5f)
		{
			sLeaser.sprites[1].color = base.blinkColor;
		}
		else
		{
			sLeaser.sprites[1].color = color;
		}
		sLeaser.sprites[0].rotation = num;
		sLeaser.sprites[1].rotation = num;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = palette.blackColor;
		color = Color.Lerp(new Color(0f, 0.4f, 1f), palette.blackColor, Mathf.Lerp(0f, 0.5f, rCam.PaletteDarkness()));
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
