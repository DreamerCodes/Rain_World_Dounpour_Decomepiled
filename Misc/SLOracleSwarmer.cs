using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SLOracleSwarmer : OracleSwarmer
{
	public Oracle oracle;

	private bool triedFindingOracle;

	public bool hoverAtGrabablePos;

	public float blackMode;

	private Color blackColor;

	public SLOracleSwarmer(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		oracle = null;
		triedFindingOracle = false;
	}

	protected override void UpdateOtherSwarmers()
	{
		base.UpdateOtherSwarmers();
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (oracle != null)
			{
				break;
			}
			if (room.updateList[i] is Oracle)
			{
				oracle = room.updateList[i] as Oracle;
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		abstractPhysicalObject.destroyOnAbstraction = oracle != null;
		if (oracle == null && !triedFindingOracle)
		{
			for (int i = 0; i < room.updateList.Count; i++)
			{
				if (room.updateList[i] is Oracle)
				{
					oracle = room.updateList[i] as Oracle;
					break;
				}
			}
			triedFindingOracle = true;
		}
		if (blackMode < 1f)
		{
			blackMode = Mathf.Max(0f, blackMode - 1f / Mathf.Lerp(200f, 700f, Random.value));
		}
		if (room.gravity * affectedByGravity > 0.5f)
		{
			return;
		}
		affectedByGravity = Custom.LerpAndTick(affectedByGravity, (oracle != null) ? 0f : 1f, 0.04f, 1f / 30f);
		if (oracle != null)
		{
			Vector2 vector = default(Vector2);
			float num = 0f;
			Vector2 vector2 = default(Vector2);
			for (int num2 = otherSwarmers.Count - 1; num2 >= 0; num2--)
			{
				if (otherSwarmers[num2].slatedForDeletetion)
				{
					otherSwarmers.RemoveAt(num2);
				}
				else
				{
					float num3 = Mathf.Clamp(Vector2.Distance(base.firstChunk.pos, otherSwarmers[num2].firstChunk.pos) - 40f, -15f, 15f);
					if (num3 > 0f)
					{
						num3 /= 7f;
					}
					vector += Custom.DirVec(base.firstChunk.pos, otherSwarmers[num2].firstChunk.pos) * num3;
					num += Mathf.Abs(num3);
					vector2 += Custom.DirVec(base.firstChunk.pos, otherSwarmers[num2].firstChunk.pos + otherSwarmers[num2].firstChunk.vel.normalized * 120f) / Mathf.Pow(Mathf.Max(10f, Vector2.Distance(base.firstChunk.pos, otherSwarmers[num2].firstChunk.pos)), 4f);
				}
			}
			if (hoverAtGrabablePos)
			{
				vector2 += (new Vector2(1530f, 180f) - base.firstChunk.pos) / 100f;
				vector = vector2;
				num = 1f;
			}
			if (num > 0f)
			{
				vector /= num;
				drift = (drift + Custom.RNV() * Random.value * 0.22f + vector2 * 0.03f).normalized;
				base.firstChunk.vel += drift * 0.05f;
				base.firstChunk.vel += vector * 0.025f;
				base.firstChunk.vel += vector2.normalized * 0.05f;
			}
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, oracle.firstChunk.pos + new Vector2(0f, 100f)) * Mathf.InverseLerp(25f, 550f, Vector2.Distance(base.firstChunk.pos, oracle.firstChunk.pos + new Vector2(0f, 100f)));
			base.firstChunk.vel *= Custom.LerpMap(base.firstChunk.vel.magnitude, 0.2f, 3f, 1f, 0.9f);
		}
		direction = new Vector2(0f, 1f);
		lazyDirection = direction;
		revolveSpeed += Mathf.Lerp(-1f, 1f, Random.value) * 1f / 120f;
		revolveSpeed = Mathf.Clamp(revolveSpeed, -0.025f, 0.025f) * 0.99f;
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].color = Color.Lerp(Color.white, blackColor, blackMode);
		}
		sLeaser.sprites[0].alpha = 0.2f * (1f - blackMode);
		sLeaser.sprites[4].color = Color.Lerp(Color.white, blackColor, blackMode * 0.9f);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
	}

	public override void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites >= 1)
		{
			return;
		}
		(grasp.grabber as Player).ObjectEaten(this);
		if (!ModManager.MSC || !(grasp.grabber as Player).isNPC)
		{
			if (room.game.session is StoryGameSession)
			{
				(room.game.session as StoryGameSession).saveState.theGlow = true;
			}
		}
		else
		{
			((grasp.grabber as Player).State as PlayerNPCState).Glowing = true;
		}
		(grasp.grabber as Player).glowing = true;
		if (oracle != null)
		{
			oracle.GlowerEaten();
		}
		grasp.Release();
		Destroy();
	}

	public override void ExplodeSwarmer()
	{
		for (int i = 0; i < 10; i++)
		{
			Vector2 vector = Custom.RNV();
			room.AddObject(new Spark(base.firstChunk.pos + vector * Random.value * 40f, vector * Mathf.Lerp(4f, 30f, Random.value), Color.white, null, 4, 18));
		}
		if (oracle != null)
		{
			oracle.GlowerEaten();
		}
		Destroy();
	}
}
