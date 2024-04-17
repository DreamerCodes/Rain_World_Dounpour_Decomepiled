using RWCustom;
using UnityEngine;

public class LizardGraphicsDebug : GraphicsModule
{
	public Lizard lizard;

	public LizardLimb[] limbs;

	private Vector2[,] drawPositions;

	private TailSegment[] tail;

	public int legsGrabbing;

	public int frontLegsGrabbing;

	public int hindLegsGrabbing;

	public int noGripCounter;

	public float frontBob;

	public float hindBob;

	public LizardGraphicsDebug(PhysicalObject ow)
		: base(ow, internalContainers: true)
	{
		lizard = ow as Lizard;
		limbs = new LizardLimb[4];
		for (int i = 0; i < 4; i++)
		{
			limbs[i] = new LizardLimb(this, base.owner.bodyChunks[(i >= 2) ? 2 : 0], i, 2.5f, 0.7f, 0.99f, lizard.lizardParams.limbSpeed, lizard.lizardParams.limbQuickness, (i % 2 == 1) ? limbs[i - 1] : null);
		}
		tail = new TailSegment[lizard.lizardParams.tailSegments];
		for (int j = 0; j < lizard.lizardParams.tailSegments; j++)
		{
			float num = 7f * lizard.lizardParams.bodySizeFac;
			num *= ((float)(lizard.lizardParams.tailSegments - j) / (float)lizard.lizardParams.tailSegments * 4f + 1f) / 5f;
			float num2 = (((j > 0) ? 7f : 14f) + num) / 2f;
			num2 *= lizard.lizardParams.tailLengthFactor;
			tail[j] = new TailSegment(this, num, num2, (j > 0) ? tail[j - 1] : null, 0.85f, 1f, 0.4f, pullInPreviousPosition: false);
		}
		drawPositions = new Vector2[base.owner.bodyChunks.Length, 2];
		DEBUGLABELS = new DebugLabel[1];
		DEBUGLABELS[0] = new DebugLabel(ow, new Vector2(40f, 50f));
	}

	public override void Update()
	{
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			drawPositions[i, 1] = drawPositions[i, 0];
			drawPositions[i, 0] = base.owner.bodyChunks[i].pos;
		}
		float num = (4f + 7f / lizard.lizardParams.walkBob) / 2f;
		frontBob = (frontBob * num + (float)(frontLegsGrabbing - 1)) / (num + 1f);
		hindBob = (hindBob * num + (float)(hindLegsGrabbing - 1)) / (num + 1f);
		drawPositions[0, 0].y += frontBob * lizard.lizardParams.walkBob;
		drawPositions[1, 0].y += frontBob + hindBob * lizard.lizardParams.walkBob * 0.5f;
		drawPositions[2, 0].y += hindBob * lizard.lizardParams.walkBob;
		legsGrabbing = 0;
		frontLegsGrabbing = 0;
		hindLegsGrabbing = 0;
		bool flag = true;
		for (int j = 0; j < limbs.Length; j++)
		{
			limbs[j].Update();
			if (limbs[j].gripCounter >= lizard.lizardParams.limbGripDelay)
			{
				legsGrabbing++;
				if (j < 2)
				{
					frontLegsGrabbing++;
				}
				else
				{
					hindLegsGrabbing++;
				}
			}
			if (limbs[j].gripCounter > 0)
			{
				flag = false;
			}
		}
		if (flag)
		{
			noGripCounter++;
		}
		else
		{
			noGripCounter = 0;
		}
		tail[0].connectedPoint = drawPositions[2, 0];
		float tailStiffness = lizard.lizardParams.tailStiffness;
		for (int num2 = tail.Length - 1; num2 >= 0; num2--)
		{
			tail[num2].Update();
			tail[num2].vel.y -= 0.9f * Mathf.Pow((float)num2 / (float)(tail.Length - 1), 3f);
			if (!Custom.DistLess(tail[num2].pos, drawPositions[2, 0], 15f * (float)(num2 + 1)))
			{
				tail[num2].pos = base.owner.bodyChunks[2].pos + Custom.DirVec(drawPositions[2, 0], tail[num2].pos) * 15f * (num2 + 1);
			}
			Vector2 a = drawPositions[1, 0];
			if (num2 == 1)
			{
				a = drawPositions[2, 0];
			}
			else if (num2 > 1)
			{
				a = tail[num2 - 2].pos;
			}
			a = Vector2.Lerp(a, drawPositions[1, 0], 0.2f);
			tail[num2].vel += Custom.DirVec(a, tail[num2].pos) * tailStiffness * Mathf.Pow(lizard.lizardParams.tailStiffnessDecline, num2) / Vector2.Distance(a, tail[num2].pos);
		}
	}

	public override void Reset()
	{
	}

	public override void SuckedIntoShortCut(Vector2 shortCutPosition)
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3 + limbs.Length + tail.Length];
		for (int i = 0; i < 3; i++)
		{
			FSprite fSprite = new FSprite("pixel");
			sLeaser.sprites[i] = fSprite;
			rCam.ReturnFContainer("Midground").AddChild(fSprite);
			fSprite.x = -10000f;
			fSprite.color = lizard.lizardParams.standardColor;
		}
		sLeaser.sprites[0].color = (lizard.lizardParams.standardColor + new Color(1f, 1f, 1f)) / 2f;
		for (int j = 3; j < limbs.Length + 3; j++)
		{
			FSprite fSprite2 = new FSprite("pixel");
			sLeaser.sprites[j] = fSprite2;
			fSprite2.x = -10000f;
			fSprite2.color = new Color(1f, 0f, 0f);
			fSprite2.scale = limbs[j - 3].rad * 2f;
		}
		for (int k = limbs.Length + 3; k < limbs.Length + 3 + tail.Length; k++)
		{
			FSprite fSprite3 = new FSprite("pixel");
			sLeaser.sprites[k] = fSprite3;
			rCam.ReturnFContainer("Midground").AddChild(fSprite3);
			fSprite3.x = -10000f;
			fSprite3.color = (lizard.lizardParams.standardColor + new Color(0f, 0f, 0f)) / 2f;
			fSprite3.scale = tail[k - limbs.Length - 3].rad * 2f;
		}
		for (int l = 3; l < limbs.Length + 3; l++)
		{
			rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[l]);
		}
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < 3; i++)
		{
			Vector2 vector = Vector2.Lerp(drawPositions[i, 1], drawPositions[i, 0], timeStacker);
			sLeaser.sprites[i].x = vector.x - camPos.x;
			sLeaser.sprites[i].y = vector.y - camPos.y;
			sLeaser.sprites[i].scale = base.owner.bodyChunks[i].rad * 2f * base.owner.bodyChunks[i].terrainSqueeze;
		}
		for (int j = 3; j < limbs.Length + 3; j++)
		{
			Vector2 vector2 = Vector2.Lerp(limbs[j - 3].lastPos, limbs[j - 3].pos, timeStacker);
			sLeaser.sprites[j].x = vector2.x - camPos.x;
			sLeaser.sprites[j].y = vector2.y - camPos.y;
			if (limbs[j - 3].currentlyDisabled)
			{
				sLeaser.sprites[j].color = new Color(1f, 1f, 1f);
			}
			else
			{
				sLeaser.sprites[j].color = new Color(1f, 0f, 0f);
			}
		}
		for (int k = limbs.Length + 3; k < limbs.Length + 3 + tail.Length; k++)
		{
			Vector2 vector3 = Vector2.Lerp(tail[k - 3 - limbs.Length].lastPos, tail[k - 3 - limbs.Length].pos, timeStacker);
			sLeaser.sprites[k].x = vector3.x - camPos.x;
			sLeaser.sprites[k].y = vector3.y - camPos.y;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
