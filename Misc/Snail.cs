using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;

public class Snail : Creature
{
	public SnailAI AI;

	public float size;

	public Color[] shellColor;

	private Vector2 breathingForce;

	public Vector2? suckPoint;

	public bool triggered;

	public int triggerTicker;

	public int allowedInNonAccesibleTile;

	public IntVector2? dropToTile;

	public float clickCounter;

	private float[] miniClicks;

	private bool justClicked;

	public bool bloated;

	public Vector2 shellDirection;

	private float lastOutOfShell;

	public float outOfShell
	{
		get
		{
			if (!AI.move)
			{
				return 0f;
			}
			return 1f;
		}
	}

	public Snail(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		Random.State state = Random.state;
		Random.InitState(abstractCreature.ID.RandomSeed);
		float num = Mathf.Lerp(0.2f, 0.4f, Random.value);
		size = Mathf.Lerp(1f - num, 1f + num, world.game.SeededRandom(abstractCreature.ID.RandomSeed));
		shellColor = new Color[2];
		float num2 = Mathf.Lerp(0.85f, 1.1f, Random.value);
		if (num2 > 1f)
		{
			num2 -= 1f;
		}
		float num3 = Mathf.Lerp(0.85f, 1.1f, Random.value);
		if (num3 > 1f)
		{
			num3 -= 1f;
		}
		shellColor[0] = Custom.HSL2RGB(num2, Mathf.Lerp(0.7f, 1f, 1f - Mathf.Pow(Random.value, 3f)), Mathf.Lerp(0f, 0.3f, Mathf.Pow(Random.value, 2f)));
		shellColor[1] = Custom.HSL2RGB(Mathf.Lerp(num3, num2, Mathf.Pow(Random.value, 3f)), Mathf.Lerp(0.7f, 1f, 1f - Mathf.Pow(Random.value, 3f)), Mathf.Lerp(0.05f, 1f, Mathf.Pow(Random.value, 3f)));
		shellColor[0] = Color.Lerp(shellColor[0], shellColor[1], (Random.value < 0.8f) ? Mathf.Pow(Random.value, 3f) : 1f);
		for (int i = 0; i < 2; i++)
		{
			shellColor[i] = new Color(Mathf.Max(shellColor[i].r, 0.007843138f), Mathf.Max(shellColor[i].g, 0.007843138f), Mathf.Max(shellColor[i].b, 0.007843138f));
		}
		miniClicks = new float[5];
		for (int j = 0; j < miniClicks.Length; j++)
		{
			miniClicks[j] = Mathf.Lerp(1f / (float)miniClicks.Length * (float)j, 1f / (float)miniClicks.Length * ((float)j + 1f), Random.value);
		}
		Random.state = state;
		float num4 = 0.32f;
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 4f * size, num4 / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 9f * Mathf.Min(1f, size), num4 / 2f);
		bodyChunkConnections = new BodyChunkConnection[2];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 0f, BodyChunkConnection.Type.Pull, 1f, 0.5f);
		bodyChunkConnections[1] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 2f * size, BodyChunkConnection.Type.Push, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.8f;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new SnailGraphics(this);
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (placeRoom.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template))
		{
			suckPoint = base.mainBodyChunk.pos;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (outOfShell != lastOutOfShell)
		{
			room.PlaySound((outOfShell > 0.5f) ? SoundID.Snail_Emerge_From_Shell : SoundID.Snail_Withdraw_In_Shell, base.mainBodyChunk);
		}
		lastOutOfShell = outOfShell;
		bloated = false;
		if (triggerTicker > 0)
		{
			triggerTicker--;
			if (triggerTicker == 0)
			{
				triggered = true;
			}
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		if (base.Consious && dropToTile.HasValue)
		{
			Drop();
		}
		else if (base.Consious && room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template))
		{
			Act();
		}
		else if (allowedInNonAccesibleTile < 1)
		{
			suckPoint = null;
		}
		if (triggered && !base.dead && (base.bodyChunks[1].ContactPoint.y < 0 || (base.mainBodyChunk.submersion > 0f && Random.value < 0.05f)))
		{
			Click();
		}
		if (allowedInNonAccesibleTile > 0)
		{
			allowedInNonAccesibleTile--;
		}
		if (suckPoint.HasValue)
		{
			if (Custom.DistLess(base.mainBodyChunk.pos, suckPoint.Value, 15f) && base.mainBodyChunk.vel.magnitude < 6f)
			{
				base.mainBodyChunk.vel += (suckPoint.Value - base.mainBodyChunk.pos) * 0.5f;
			}
			else
			{
				suckPoint = null;
			}
		}
	}

	private void Act()
	{
		if (!suckPoint.HasValue && !base.safariControlled)
		{
			FindSuckPoint();
			return;
		}
		bodyChunkConnections[0].distance = (AI.move ? (12f * Mathf.Lerp(size, 1f, 0.5f)) : (7f * Mathf.Lerp(size, 1f, 0.5f)));
		justClicked = false;
		float num = clickCounter;
		if (AI.scared > 0.3f && !base.safariControlled)
		{
			clickCounter += Mathf.InverseLerp(0.3f, 0.9f, AI.scared) / 200f;
		}
		else if (clickCounter > 0.92f)
		{
			clickCounter += 0.0033333334f;
		}
		else
		{
			clickCounter -= 0.01f;
		}
		clickCounter = Mathf.Clamp(clickCounter, 0f, 1f);
		if (num < clickCounter)
		{
			for (int i = 0; i < miniClicks.Length; i++)
			{
				if (num < miniClicks[i] && clickCounter >= miniClicks[i])
				{
					MiniClick();
					break;
				}
			}
		}
		if (clickCounter == 1f && Random.value < 2f / 3f)
		{
			Click();
			return;
		}
		base.bodyChunks[0].vel.y += Mathf.Lerp(base.gravity, base.gravity - base.buoyancy, base.bodyChunks[0].submersion);
		base.bodyChunks[0].vel *= 0.5f;
		base.bodyChunks[1].vel.y += Mathf.Lerp(base.gravity, base.gravity - base.buoyancy, base.bodyChunks[0].submersion) * 0.5f;
		base.bodyChunks[1].vel *= 0.9f;
		breathingForce *= 0.96f;
		breathingForce += Custom.DegToVec(Random.value * 360f) / 60f;
		breathingForce = Vector2.ClampMagnitude(breathingForce, 1f);
		base.bodyChunks[1].vel += breathingForce * 0.1f;
		base.abstractCreature.abstractAI.RealAI.Update();
		if (base.safariControlled)
		{
			if (inputWithDiagonals.HasValue)
			{
				shellDirection = new Vector2(0f - (float)inputWithDiagonals.Value.x, 0f - (float)inputWithDiagonals.Value.y);
				suckPoint = base.mainBodyChunk.pos - shellDirection * 6f;
				base.bodyChunks[0].vel = shellDirection * -2.5f;
				base.bodyChunks[1].vel = shellDirection * 0.5f;
				if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
				{
					enteringShortCut = room.GetTilePosition(base.bodyChunks[0].pos);
				}
				if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
				{
					clickCounter += 0.3f;
					MiniClick();
				}
				else if (inputWithDiagonals.Value.y < 0)
				{
					base.GoThroughFloors = true;
				}
				else
				{
					base.GoThroughFloors = false;
				}
			}
		}
		else
		{
			if (!AI.move)
			{
				return;
			}
			MovementConnection movementConnection = (AI.pathFinder as SnailPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos), actuallyFollowingThisPath: true);
			if (room == null || !(movementConnection != default(MovementConnection)))
			{
				return;
			}
			if (movementConnection.type == MovementConnection.MovementType.Slope || movementConnection.type == MovementConnection.MovementType.CeilingSlope || movementConnection.type == MovementConnection.MovementType.OpenDiagonal)
			{
				allowedInNonAccesibleTile = 20;
			}
			else if (movementConnection.type == MovementConnection.MovementType.DropToFloor)
			{
				dropToTile = movementConnection.DestTile;
			}
			shellDirection = new Vector2(0f, 0f);
			if (movementConnection.type == MovementConnection.MovementType.Standard)
			{
				if (movementConnection.startCoord.x != movementConnection.destinationCoord.x)
				{
					if (IsTileSolid(0, 0, -1))
					{
						shellDirection.y = 1f;
					}
					else if (IsTileSolid(0, 0, 1))
					{
						shellDirection.y = -1f;
					}
				}
				else if (IsTileSolid(0, -1, 0) && !IsTileSolid(0, 1, 0))
				{
					shellDirection.x = 1f;
				}
				else if (IsTileSolid(0, 1, 0) && !IsTileSolid(0, -1, 0))
				{
					shellDirection.x = -1f;
				}
			}
			else if (movementConnection.type == MovementConnection.MovementType.Slope)
			{
				shellDirection = Custom.PerpendicularVector(IntVector2.ToVector2(movementConnection.DestTile - movementConnection.StartTile).normalized);
				if (movementConnection.destinationCoord.x < movementConnection.startCoord.x)
				{
					shellDirection *= -1f;
				}
			}
			else if (movementConnection.type == MovementConnection.MovementType.CeilingSlope)
			{
				shellDirection = -Custom.PerpendicularVector(IntVector2.ToVector2(movementConnection.DestTile - movementConnection.StartTile).normalized);
				if (movementConnection.destinationCoord.x < movementConnection.startCoord.x)
				{
					shellDirection *= -1f;
				}
			}
			if (base.bodyChunks[1].ContactPoint.x != 0 && movementConnection.startCoord.x != movementConnection.destinationCoord.x && movementConnection.startCoord.y == movementConnection.destinationCoord.y)
			{
				base.bodyChunks[1].vel.y -= (base.bodyChunks[1].pos.y - room.MiddleOfTile(movementConnection.destinationCoord).y) * 0.2f;
			}
			else if (base.bodyChunks[1].ContactPoint.y != 0 && movementConnection.startCoord.x == movementConnection.destinationCoord.x && movementConnection.startCoord.y != movementConnection.destinationCoord.y)
			{
				base.bodyChunks[1].vel.x -= (base.bodyChunks[1].pos.x - room.MiddleOfTile(movementConnection.destinationCoord).x) * 0.2f;
			}
			base.bodyChunks[1].vel += shellDirection * 0.75f;
			if ((movementConnection.type == MovementConnection.MovementType.ShortCut || !movementConnection.destinationCoord.TileDefined) && room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				enteringShortCut = room.GetTilePosition(base.bodyChunks[0].pos);
				return;
			}
			base.GoThroughFloors = movementConnection.destinationCoord.y < movementConnection.startCoord.y;
			if (room.aimap.getAItile(base.mainBodyChunk.pos).acc == AItile.Accessibility.Climb && !room.GetTile(base.mainBodyChunk.pos).verticalBeam)
			{
				shellDirection = new Vector2(0f, 1.8f);
			}
			Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(movementConnection.destinationCoord) - shellDirection * 5f) * 1.1f;
			base.bodyChunks[1].vel -= vector * 0.2f;
			if (Custom.DistLess(base.mainBodyChunk.pos, suckPoint.Value + vector, 10f))
			{
				suckPoint += vector;
			}
			else
			{
				suckPoint += Custom.DirVec(suckPoint.Value, base.mainBodyChunk.pos);
			}
		}
	}

	private void Drop()
	{
		base.mainBodyChunk.vel.x += Mathf.Clamp(room.MiddleOfTile(dropToTile.Value).x - base.mainBodyChunk.pos.x, -5f, 5f) * 0.1f;
		suckPoint = null;
		for (int i = 0; i < 2; i++)
		{
			if (base.bodyChunks[i].ContactPoint.y < 0)
			{
				dropToTile = null;
				break;
			}
		}
	}

	private void FindSuckPoint()
	{
		base.mainBodyChunk.vel.y -= 0.3f;
		if (base.mainBodyChunk.ContactPoint.y < 0 && room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template))
		{
			room.PlaySound(SoundID.Snail_Attatch_To_Surface, base.mainBodyChunk);
			suckPoint = room.MiddleOfTile(base.mainBodyChunk.pos);
		}
	}

	public void Click()
	{
		if (triggerTicker > 0)
		{
			return;
		}
		if (room.BeingViewed)
		{
			if (base.bodyChunks[1].submersion == 1f)
			{
				room.AddObject(new ShockWave(base.bodyChunks[1].pos, 160f * size, 0.07f, 9));
			}
			else
			{
				room.AddObject(new ShockWave(base.bodyChunks[1].pos, 100f * size, 0.07f, 6));
				for (int i = 0; i < 10; i++)
				{
					room.AddObject(new WaterDrip(base.bodyChunks[1].pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(4f, 21f, Random.value), waterColor: false));
				}
			}
		}
		Stun(60);
		if (ModManager.MSC && room.game.IsStorySession && room.game.GetStorySession.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && room.world.region != null && room.world.region.name == "DS")
		{
			Vector2 vector = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.lastPos, 0.35f);
			Color color = shellColor[0];
			room.AddObject(new Explosion(room, this, vector, 7, 280f, 4.2f, 50f, 280f, 0.25f, this, 0.7f, 160f, 1f));
			room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, color));
			room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
			room.AddObject(new ExplosionSpikes(room, vector, 14, 30f, 9f, 7f, 170f, color));
			room.AddObject(new ShockWave(vector, 240f, 0.045f, 5));
			for (int j = 0; j < 10; j++)
			{
				Vector2 vector2 = Custom.RNV();
				if (room.GetTile(vector + vector2 * 20f).Solid)
				{
					if (!room.GetTile(vector - vector2 * 20f).Solid)
					{
						vector2 *= -1f;
					}
					else
					{
						vector2 = Custom.RNV();
					}
				}
				for (int k = 0; k < 3; k++)
				{
					room.AddObject(new Spark(vector + vector2 * Mathf.Lerp(30f, 60f, Random.value), vector2 * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(color, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
				}
				room.AddObject(new Explosion.FlashingSmoke(vector + vector2 * 40f * Random.value, vector2 * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), color, Random.Range(3, 11)));
			}
			for (int l = 0; l < abstractPhysicalObject.stuckObjects.Count; l++)
			{
				abstractPhysicalObject.stuckObjects[l].Deactivate();
			}
			room.PlaySound(SoundID.Bomb_Explode, vector);
			room.InGameNoise(new InGameNoise(vector, 9000f, this, 1f));
		}
		clickCounter = 0f;
		room.PlaySound(SoundID.Snail_Pop, base.mainBodyChunk);
		float num = 60f * size;
		for (int m = 0; m < room.physicalObjects.Length; m++)
		{
			foreach (PhysicalObject item in room.physicalObjects[m])
			{
				if (item == this)
				{
					continue;
				}
				BodyChunk[] array = item.bodyChunks;
				foreach (BodyChunk bodyChunk in array)
				{
					float num2 = 1f + bodyChunk.submersion * base.bodyChunks[1].submersion * 4.5f;
					if (!Custom.DistLess(bodyChunk.pos, base.bodyChunks[1].pos, num * num2 + bodyChunk.rad + base.bodyChunks[1].rad) || !room.VisualContact(bodyChunk.pos, base.bodyChunks[1].pos))
					{
						continue;
					}
					float num3 = Mathf.InverseLerp(num * num2 + bodyChunk.rad + base.bodyChunks[1].rad, (num * num2 + bodyChunk.rad + base.bodyChunks[1].rad) / 2f, Vector2.Distance(bodyChunk.pos, base.bodyChunks[1].pos));
					bodyChunk.vel += Custom.DirVec(base.bodyChunks[1].pos + new Vector2(0f, IsTileSolid(1, 0, -1) ? (-20f) : 0f), bodyChunk.pos) * num3 * num2 * 3f / bodyChunk.mass;
					if (ModManager.MSC && item is Player && (item as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Saint)
					{
						(item as Player).SaintStagger(1500);
					}
					else if (item is Player)
					{
						(item as Player).Stun((int)(60f * num3));
					}
					else if (item is Creature)
					{
						(item as Creature).Stun((int)(((ModManager.MMF && MMF.cfgIncreaseStuns.Value) ? 300f : 60f) * num3));
					}
					if (item is Leech)
					{
						if (Random.value < 1f / 30f || Custom.DistLess(base.bodyChunks[1].pos, bodyChunk.pos, base.bodyChunks[1].rad + bodyChunk.rad + 5f))
						{
							(item as Leech).Die();
						}
						else
						{
							(item as Leech).Stun((int)(num3 * bodyChunk.submersion * Mathf.Lerp(800f, 900f, Random.value)));
						}
					}
				}
			}
		}
		if (room.waterObject != null)
		{
			float num4 = 1f + base.bodyChunks[1].submersion * 1.5f;
			room.waterObject.Explosion(base.bodyChunks[1].pos, num * num4 * 1.2f, num4 * 3f);
		}
		suckPoint = null;
		for (int num5 = 0; num5 < 2; num5++)
		{
			if (IsTileSolid(num5, 0, -1))
			{
				base.bodyChunks[num5].vel += Custom.DegToVec(-50f + 100f * Random.value) * 10f;
			}
			else
			{
				base.bodyChunks[num5].vel += Custom.DegToVec(Random.value * 360f) * 10f;
			}
		}
		VibrateLeeches(1000f);
		justClicked = true;
		bloated = true;
		triggered = false;
	}

	private void MiniClick()
	{
		room.PlaySound(SoundID.Snail_Warning_Click, base.mainBodyChunk);
		base.bodyChunks[1].vel += Custom.DegToVec(Random.value * 360f) * 3f;
		bloated = true;
		for (int i = 0; i < 3; i++)
		{
			if (room.BeingViewed && base.bodyChunks[1].submersion < 0.5f)
			{
				room.AddObject(new WaterDrip(base.bodyChunks[1].pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(4f, 21f, Random.value), waterColor: false));
			}
			else
			{
				room.AddObject(new Bubble(base.bodyChunks[1].pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(4f, 11f, Random.value), bottomBubble: false, fakeWaterBubble: false));
			}
		}
		VibrateLeeches(250f);
	}

	private void VibrateLeeches(float rad)
	{
		if (!(base.bodyChunks[1].submersion > 0.5f))
		{
			return;
		}
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Leech && room.abstractRoom.creatures[i].realizedCreature != null && room.abstractRoom.creatures[i].realizedCreature.room == room && Custom.DistLess(base.mainBodyChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, rad) && (Custom.DistLess(base.mainBodyChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, rad / 4f) || room.VisualContact(base.mainBodyChunk.pos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos)))
			{
				(room.abstractRoom.creatures[i].realizedCreature as Leech).HeardSnailClick(base.mainBodyChunk.pos);
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (triggered)
		{
			Click();
		}
		else if (base.Consious)
		{
			if (otherObject is Snail)
			{
				AI.CollideWithSnail();
				if (AI.move && room.aimap.getAItile(otherObject.firstChunk.pos).narrowSpace)
				{
					clickCounter += 0.005f;
				}
			}
			else if (otherObject is Creature)
			{
				clickCounter += 0.025f;
			}
		}
		if (suckPoint.HasValue && !(otherObject is Snail) && !Custom.DistLess(base.mainBodyChunk.pos, suckPoint.Value, 6f))
		{
			suckPoint = null;
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (!justClicked && speed > 20f && firstContact)
		{
			triggered = true;
		}
		else if (!justClicked && speed > 5f && clickCounter < 0.92f)
		{
			clickCounter = 0.92f;
		}
		if (speed > 1.5f && firstContact)
		{
			float num = Mathf.InverseLerp(6f, 14f, speed);
			if (num < 1f)
			{
				room.PlaySound(SoundID.Snail_Light_Terrain_Impact, base.mainBodyChunk, loop: false, 1f - num, 1f);
			}
			if (num > 0f)
			{
				room.PlaySound(SoundID.Snail_Heavy_Terrain_Impact, base.mainBodyChunk, loop: false, num, 1f);
			}
		}
		base.TerrainImpact(chunk, direction, speed, firstContact);
	}

	public override void Die()
	{
		base.Die();
	}

	public override Color ShortCutColor()
	{
		return Color.Lerp(shellColor[0], shellColor[1], 0.3f);
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		if (!base.dead && ((double)damage >= 0.1 || stunBonus > 10f))
		{
			triggered = true;
		}
		else if (!justClicked && clickCounter < 0.92f)
		{
			clickCounter = 0.92f;
		}
		base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
	}

	public override void Stun(int st)
	{
		base.Stun(st);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + (float)i) * 15f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 2f;
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}
}
