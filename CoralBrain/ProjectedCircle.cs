using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace CoralBrain;

public class ProjectedCircle : CosmeticSprite
{
	public IOwnProjectedCircles owner;

	private int index;

	private float baseRad;

	private float rad;

	private float lastRad;

	private float getToRad;

	private float lastDepthOffset;

	private float depthOffset;

	private bool updateDepth;

	private float blink;

	private float lastBlink;

	private float blinkGetTo;

	private int blinkCounter;

	private float rotation;

	private float lastRotation;

	private float rotationSpeed;

	public int[] connectionsBlink;

	public List<ProjectedCircle> connectedCircles;

	public Vector2[] offScreenConnections;

	public int spokes;

	public float lastBaseRad;

	public ProjectedCircle(Room room, IOwnProjectedCircles owner, int index, float size)
	{
		this.owner = owner;
		this.index = index;
		baseRad = Mathf.Lerp(60f, 180f, size);
		spokes = (int)((float)Math.PI * 2f * baseRad / 50f);
		rad = baseRad;
		lastRad = rad;
		getToRad = rad;
		connectedCircles = new List<ProjectedCircle>();
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (connectedCircles.Count >= 5)
			{
				break;
			}
			if (room.updateList[i] is ProjectedCircle && UnityEngine.Random.value < 0.5f && !(room.updateList[i] as ProjectedCircle).connectedCircles.Contains(this))
			{
				connectedCircles.Add(room.updateList[i] as ProjectedCircle);
			}
		}
		if (UnityEngine.Random.value < 0.5f)
		{
			offScreenConnections = new Vector2[0];
		}
		else
		{
			offScreenConnections = new Vector2[UnityEngine.Random.Range(1, 4)];
			for (int j = 0; j < offScreenConnections.Length; j++)
			{
				bool num = UnityEngine.Random.value < room.PixelHeight / (room.PixelHeight + room.PixelWidth);
				bool flag = UnityEngine.Random.value < 0.5f;
				if (num)
				{
					offScreenConnections[j].y = room.PixelHeight * UnityEngine.Random.value;
					if (flag)
					{
						offScreenConnections[j].x = -10f;
					}
					else
					{
						offScreenConnections[j].x = room.PixelWidth + 10f;
					}
				}
				else
				{
					offScreenConnections[j].x = room.PixelWidth * UnityEngine.Random.value;
					if (flag)
					{
						offScreenConnections[j].y = -10f;
					}
					else
					{
						offScreenConnections[j].y = room.PixelHeight + 10f;
					}
				}
			}
		}
		connectionsBlink = new int[connectedCircles.Count + offScreenConnections.Length];
		rotationSpeed = Mathf.Lerp(0.2f, 1f, UnityEngine.Random.value) * ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastPos = pos;
		lastRad = rad;
		if (rad < getToRad)
		{
			rad = Mathf.Min(rad + 0.8f, getToRad);
		}
		else if (rad > getToRad)
		{
			rad = Mathf.Max(rad - 0.8f, getToRad);
		}
		rad = Mathf.Lerp(rad, getToRad, 0.02f);
		if (UnityEngine.Random.value < 1f / 120f)
		{
			getToRad = Mathf.Lerp(baseRad * 0.8f, baseRad * 1.2f, UnityEngine.Random.value);
		}
		if (ModManager.MSC)
		{
			if (baseRad < lastBaseRad)
			{
				getToRad = baseRad * 0.8f;
			}
			if (baseRad > lastBaseRad)
			{
				getToRad = baseRad * 1.2f;
			}
			lastBaseRad = baseRad;
		}
		updateDepth = true;
		lastDepthOffset = depthOffset;
		if (UnityEngine.Random.value < 0.0009803922f)
		{
			blinkCounter = UnityEngine.Random.Range(7, 230);
			blinkGetTo = 1f;
		}
		if (blinkCounter > 0)
		{
			blinkCounter--;
		}
		else
		{
			blinkGetTo = 0f;
		}
		lastBlink = blink;
		if (blink < blinkGetTo)
		{
			blink = Mathf.Min(blink + 1f / 30f, blinkGetTo);
		}
		else if (blink > blinkGetTo)
		{
			blink = Mathf.Max(blink - 1f / 30f, blinkGetTo);
		}
		blink = Mathf.Lerp(blink, blinkGetTo, 0.02f);
		for (int i = 0; i < connectionsBlink.Length; i++)
		{
			if (connectionsBlink[i] > 0)
			{
				connectionsBlink[i]--;
			}
			else if (UnityEngine.Random.value < 0.0033333334f)
			{
				connectionsBlink[i] = UnityEngine.Random.Range(10, 50);
			}
		}
		lastRotation = rotation;
		rotation += rotationSpeed * 140f * rotationSpeed / rad;
		if ((ModManager.MSC && room != owner.HostingCircleFromRoom()) || !owner.CanHostCircle())
		{
			base.slatedForDeletetion = true;
		}
	}

	public void UpdateDepth(RoomCamera rCam)
	{
		Vector2 coord = owner.CircleCenter(index, 1f);
		float b = rCam.DepthAtCoordinate(coord);
		depthOffset = Mathf.Lerp(depthOffset, b, 0.5f);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2 + spokes + connectedCircles.Count + offScreenConnections.Length];
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
		}
		sLeaser.sprites[0].color = new Color(0.003921569f, 0f, 0f);
		sLeaser.sprites[1].color = new Color(0f, 0f, 0f);
		for (int j = 0; j < spokes; j++)
		{
			sLeaser.sprites[j + 2] = new FSprite("pixel");
			sLeaser.sprites[j + 2].anchorY = 0f;
			sLeaser.sprites[j + 2].color = new Color(0f, 0f, 0f);
		}
		for (int k = 0; k < connectedCircles.Count + offScreenConnections.Length; k++)
		{
			sLeaser.sprites[k + spokes + 2] = new FSprite("pixel");
			sLeaser.sprites[k + spokes + 2].anchorY = 0f;
			sLeaser.sprites[k + spokes + 2].color = new Color(0f, 0f, 0f);
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
	}

	public Vector2 Position(float timeStacker)
	{
		if (ModManager.MMF && room == null)
		{
			return owner.CircleCenter(index, timeStacker);
		}
		return owner.CircleCenter(index, timeStacker) - new Vector2(room.lightAngle.x, 0f - room.lightAngle.y) * Mathf.Lerp(lastDepthOffset, depthOffset, timeStacker) * 15f;
	}

	public float Rad(float timeStacker)
	{
		return Mathf.Lerp(lastRad, rad, timeStacker) * Mathf.Lerp(1f, 0.6f, Mathf.Lerp(lastBlink, blink, timeStacker));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (updateDepth)
		{
			UpdateDepth(rCam);
			updateDepth = false;
		}
		float num = Rad(timeStacker);
		Vector2 vector = Position(timeStacker);
		float num2 = Mathf.Lerp(lastBlink, blink, timeStacker);
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].x = vector.x - camPos.x;
			sLeaser.sprites[i].y = vector.y - camPos.y;
			sLeaser.sprites[i].scale = num / 8f;
		}
		sLeaser.sprites[1].alpha = 4f / Mathf.Lerp(rad, 4f, Mathf.Pow(num2, 0.5f));
		float num3 = Mathf.Lerp(lastRotation, rotation, timeStacker);
		for (int j = 0; j < spokes; j++)
		{
			Vector2 vector2 = vector + Custom.DegToVec((float)j / (float)spokes * 360f + num3) * (num - Mathf.Lerp(1f, 5f, num2));
			sLeaser.sprites[j + 2].x = vector2.x - camPos.x;
			sLeaser.sprites[j + 2].y = vector2.y - camPos.y;
			sLeaser.sprites[j + 2].rotation = (float)j / (float)spokes * 360f + num3;
			sLeaser.sprites[j + 2].scaleY = 10f;
			sLeaser.sprites[j + 2].scaleX = Mathf.Lerp(4f, num * (float)Math.PI / (float)spokes, Mathf.Pow(num2, 1.5f));
		}
		if (ModManager.MMF)
		{
			for (int num4 = connectedCircles.Count - 1; num4 >= 0; num4--)
			{
				if (connectedCircles[num4].slatedForDeletetion)
				{
					connectedCircles.RemoveAt(num4);
				}
			}
		}
		for (int k = 0; k < connectedCircles.Count + offScreenConnections.Length; k++)
		{
			Vector2 vector3;
			float num5;
			if (k < connectedCircles.Count)
			{
				vector3 = connectedCircles[k].Position(timeStacker);
				num5 = connectedCircles[k].Rad(timeStacker);
			}
			else
			{
				vector3 = offScreenConnections[k - connectedCircles.Count];
				num5 = 0f;
			}
			sLeaser.sprites[k + spokes + 2].rotation = Custom.AimFromOneVectorToAnother(vector, vector3);
			sLeaser.sprites[k + spokes + 2].scaleY = Mathf.Max(0f, Vector2.Distance(vector, vector3) - num - num5);
			sLeaser.sprites[k + spokes + 2].x = vector.x + Custom.DirVec(vector, vector3).x * num - camPos.x;
			sLeaser.sprites[k + spokes + 2].y = vector.y + Custom.DirVec(vector, vector3).y * num - camPos.y;
			sLeaser.sprites[k + spokes + 2].scaleX = ((connectionsBlink[k] > 0 && UnityEngine.Random.value < 0.7f) ? 4f : 2f);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void SetBaseRad(float rad)
	{
		baseRad = rad;
	}

	public float GetBaseRad()
	{
		return baseRad;
	}
}
