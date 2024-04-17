using System;
using RWCustom;
using UnityEngine;

public class FlyGraphics : GraphicsModule
{
	public GenericBodyPart lowerBody;

	public Fly fly;

	public float[,] wings;

	public float horizontalSteeringCompensation;

	public float lastHorizontalSteeringCompensation;

	public float insectFlightWingSync;

	public float[] deathWingPositions;

	public FlyGraphics(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
		fly = ow as Fly;
		lowerBody = new GenericBodyPart(this, 5f, 0.5f, 1f, fly.mainBodyChunk);
		DEBUGLABELS = new DebugLabel[0];
		wings = new float[2, 2];
		insectFlightWingSync = UnityEngine.Random.value;
		deathWingPositions = new float[2]
		{
			UnityEngine.Random.value,
			UnityEngine.Random.value
		};
	}

	public override void Update()
	{
		base.Update();
		if (culled)
		{
			return;
		}
		lowerBody.Update();
		if (base.owner.firstChunk.submersion > 0f)
		{
			lowerBody.vel *= 0.9f;
		}
		else if (fly.CurrentBehavior == FlyAI.Behavior.Drop || fly.CurrentBehavior == FlyAI.Behavior.Burrow)
		{
			lowerBody.vel *= 0.9f;
		}
		else
		{
			lowerBody.vel.y -= fly.gravity * 2f;
		}
		lowerBody.PushOutOfTerrain(fly.room, fly.mainBodyChunk.pos);
		lowerBody.ConnectToPoint(fly.mainBodyChunk.pos, 10f, push: true, 0f, fly.mainBodyChunk.vel, 0f, 0f);
		if (fly.CurrentBehavior == FlyAI.Behavior.Chain)
		{
			lowerBody.vel *= 0f;
			if (fly.grasps[0] != null)
			{
				lowerBody.pos = fly.grasps[0].grabbed.firstChunk.pos;
			}
			else if (fly.burrowOrHangSpot.HasValue)
			{
				lowerBody.pos = fly.burrowOrHangSpot.Value;
			}
		}
		else if (base.owner.grabbedBy.Count > 0)
		{
			Vector2 vector = Custom.PerpendicularVector((base.owner.grabbedBy[0].grabber.mainBodyChunk.pos - fly.mainBodyChunk.pos).normalized) * 4f;
			vector.y = 0f - Mathf.Abs(vector.y);
			lowerBody.vel += vector;
		}
		if (lowerBody.pos.x == fly.mainBodyChunk.pos.x)
		{
			lowerBody.vel.x += Mathf.Lerp(-0.1f, 0.1f, UnityEngine.Random.value);
		}
		if (!fly.dead && base.owner.grabbedBy.Count > 0 && !(base.owner.grabbedBy[0].grabber is Fly))
		{
			wings[0, 1] = wings[0, 0];
			wings[1, 1] = wings[1, 0];
			wings[0, 0] = UnityEngine.Random.value;
			wings[1, 0] = UnityEngine.Random.value;
		}
		else if (fly.Consious)
		{
			lastHorizontalSteeringCompensation = horizontalSteeringCompensation;
			horizontalSteeringCompensation = Custom.DirVec(lowerBody.pos, fly.mainBodyChunk.pos).x - fly.dir.x;
			for (int i = 0; i < 2; i++)
			{
				if (fly.movMode == Fly.MovementMode.SwarmFlight)
				{
					wings[i, 0] = 0f;
					wings[i, 1] = 1f;
				}
				else if (fly.CurrentBehavior == FlyAI.Behavior.Drop || fly.CurrentBehavior == FlyAI.Behavior.Burrow)
				{
					wings[i, 1] = wings[i, 0];
					wings[i, 0] = 1f;
					horizontalSteeringCompensation = 0f;
				}
				else if (fly.CurrentBehavior == FlyAI.Behavior.Chain)
				{
					switch (fly.chainBehaviorVariation)
					{
					case 1:
					case 2:
						wings[i, 1] = wings[i, 0];
						wings[i, 0] = fly.flap;
						break;
					case 3:
						wings[i, 1] = 0f;
						wings[i, 0] = 1f;
						break;
					default:
						wings[i, 1] = wings[i, 0];
						wings[i, 0] = 1f;
						horizontalSteeringCompensation = 0f;
						break;
					}
				}
				else
				{
					wings[i, 1] = wings[i, 0];
					wings[i, 0] = fly.flap;
				}
			}
			insectFlightWingSync += UnityEngine.Random.value / 40f;
			if (insectFlightWingSync > 1f)
			{
				insectFlightWingSync -= 1f;
			}
		}
		else
		{
			lastHorizontalSteeringCompensation = horizontalSteeringCompensation;
			horizontalSteeringCompensation *= 0.999f;
			float y = Custom.RotateAroundOrigo(fly.mainBodyChunk.pos - fly.mainBodyChunk.lastPos, Custom.AimFromOneVectorToAnother(fly.mainBodyChunk.pos, lowerBody.pos)).y;
			for (int j = 0; j < 2; j++)
			{
				wings[j, 1] = wings[j, 0];
				wings[j, 0] = Mathf.Clamp(Mathf.Lerp(wings[j, 0], deathWingPositions[j] - y * 0.1f, 0.3f), 0f, 1f);
			}
		}
	}

	public override void SuckedIntoShortCut(Vector2 shortCutPosition)
	{
		base.SuckedIntoShortCut(shortCutPosition);
	}

	public override void Reset()
	{
		lowerBody.pos = fly.mainBodyChunk.pos;
		lowerBody.lastPos = fly.mainBodyChunk.lastPos;
		lowerBody.vel = fly.mainBodyChunk.vel;
		base.Reset();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[4];
		sLeaser.sprites[0] = new FSprite("FlyBody");
		sLeaser.sprites[0].color = new Color(0f, 0f, 1f);
		sLeaser.sprites[1] = new FSprite("FlyWing");
		sLeaser.sprites[1].anchorY = 0f;
		sLeaser.sprites[2] = new FSprite("FlyWing");
		sLeaser.sprites[2].anchorY = 0f;
		sLeaser.sprites[3] = new FSprite("FlyEyes");
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (!culled)
		{
			Vector2 vector = Vector2.Lerp(fly.bodyChunks[0].lastPos, fly.bodyChunks[0].pos, timeStacker) - camPos;
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[1].x = vector.x;
			sLeaser.sprites[1].y = vector.y;
			sLeaser.sprites[2].x = vector.x;
			sLeaser.sprites[2].y = vector.y;
			sLeaser.sprites[3].x = vector.x;
			sLeaser.sprites[3].y = vector.y;
			float num = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lowerBody.lastPos, lowerBody.pos, timeStacker), Vector2.Lerp(fly.bodyChunks[0].lastPos, fly.bodyChunks[0].pos, timeStacker));
			sLeaser.sprites[0].rotation = num;
			sLeaser.sprites[3].rotation = num;
			for (int i = 0; i < 2; i++)
			{
				float a = Mathf.Lerp(Mathf.Lerp(wings[i, 1], wings[i, 0], timeStacker), 0.5f, Mathf.Lerp(0.3f, 0f, Mathf.Lerp(fly.lastFlapDepth, fly.flapDepth, timeStacker)));
				float num2 = Mathf.Lerp(lastHorizontalSteeringCompensation, horizontalSteeringCompensation, timeStacker);
				num2 = ((num2 < 0f == (i == 0)) ? 0f : Mathf.Clamp(Mathf.Abs(num2 * 0.85f) - 0.1f, 0f, 1f));
				a = Mathf.Lerp(a, 0.5f, num2);
				a = Mathf.InverseLerp(0.01f, 0.99f, Mathf.Pow(a, 2f));
				sLeaser.sprites[1 + i].rotation = ((i == 0) ? (-1f) : 1f) * (40f + 150f * a) + num;
				sLeaser.sprites[1 + i].scaleX = ((i == 0) ? (-1f) : 1f) * ((fly.flapSpeed < 0f) ? 1f : (1f - 0.6f * (Mathf.Sin(Mathf.Lerp(wings[i, 1], wings[i, 0], timeStacker) * (float)Math.PI) * (1f - num2))));
			}
			sLeaser.sprites[1].isVisible = fly.bites == 3;
			sLeaser.sprites[2].isVisible = fly.bites > 1;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[i].color = palette.blackColor;
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
