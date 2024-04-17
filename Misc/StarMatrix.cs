using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class StarMatrix : UpdatableAndDeletable, IDrawable
{
	public class Star
	{
		public StarMatrix owner;

		public int index;

		public int fistSprite;

		public int totalSprites;

		public Vector3 position;

		public List<int> connections;

		private int circles;

		public bool lit = true;

		public bool[] connectionsOn;

		public Star(StarMatrix owner, int index, int firstSprite)
		{
			this.owner = owner;
			this.index = index;
			fistSprite = firstSprite;
			position = UnityEngine.Random.insideUnitSphere;
			circles = ((!(UnityEngine.Random.value < 0.5f)) ? UnityEngine.Random.Range(0, UnityEngine.Random.Range(1, 4)) : 0);
			int num = ((index > 10) ? UnityEngine.Random.Range(1, 3) : 0);
			connections = new List<int>();
			for (int i = 0; i < num * 100; i++)
			{
				int num2 = UnityEngine.Random.Range(0, index);
				if (Vector3.Distance(owner.stars[num2].position, position) < 0.4f && !connections.Contains(num2))
				{
					connections.Add(num2);
				}
				if (connections.Count >= num)
				{
					break;
				}
			}
			connectionsOn = new bool[connections.Count];
			for (int j = 0; j < connectionsOn.Length; j++)
			{
				connectionsOn[j] = UnityEngine.Random.value < 0.5f;
			}
			totalSprites = 1 + connections.Count + circles;
		}

		public void Update()
		{
			if (UnityEngine.Random.value < 0.05f && connections.Count > 0)
			{
				connectionsOn[UnityEngine.Random.Range(0, connections.Count)] = UnityEngine.Random.value < 0.5f;
			}
		}

		public Vector2 OnPlanePos(float timeStacker)
		{
			float num = Mathf.Lerp(owner.lastXRotat, owner.xRotat, timeStacker);
			float num2 = Mathf.Lerp(owner.lastYRotat, owner.yRotat, timeStacker);
			float num3 = Mathf.Lerp(owner.lastZRotat, owner.zRotat, timeStacker);
			float num4 = position.x - Mathf.Lerp(owner.lastFocus.x, owner.focus.x, timeStacker);
			float num5 = position.y - Mathf.Lerp(owner.lastFocus.y, owner.focus.y, timeStacker);
			float num6 = position.z - Mathf.Lerp(owner.lastFocus.z, owner.focus.z, timeStacker);
			float num7 = num5 * Mathf.Cos(num * (float)Math.PI * 2f) - num6 * Mathf.Sin(num * (float)Math.PI * 2f);
			num6 = num5 * Mathf.Sin(num * (float)Math.PI * 2f) + num6 * Mathf.Cos(num * (float)Math.PI * 2f);
			num5 = num7;
			float num8 = num6 * Mathf.Cos(num2 * (float)Math.PI * 2f) - num4 * Mathf.Sin(num2 * (float)Math.PI * 2f);
			num4 = num6 * Mathf.Sin(num2 * (float)Math.PI * 2f) + num4 * Mathf.Cos(num2 * (float)Math.PI * 2f);
			num6 = num8;
			float num9 = num4 * Mathf.Cos(num3 * (float)Math.PI * 2f) - num5 * Mathf.Sin(num3 * (float)Math.PI * 2f);
			num5 = num4 * Mathf.Sin(num3 * (float)Math.PI * 2f) + num5 * Mathf.Cos(num3 * (float)Math.PI * 2f);
			num4 = num9;
			return new Vector2(num4, num5);
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[fistSprite] = new FSprite("pixel");
			sLeaser.sprites[fistSprite].scale = 5f;
			sLeaser.sprites[fistSprite].color = new Color(0f, 0f, 0f);
			for (int i = 0; i < connections.Count; i++)
			{
				sLeaser.sprites[fistSprite + 1 + i] = new FSprite("pixel");
				sLeaser.sprites[fistSprite + 1 + i].color = new Color(0f, 0f, 0f);
				sLeaser.sprites[fistSprite + 1 + i].scaleX = 1.5f;
				sLeaser.sprites[fistSprite + 1 + i].anchorY = 0f;
			}
			for (int j = 0; j < circles; j++)
			{
				sLeaser.sprites[fistSprite + 1 + connections.Count + j] = new FSprite("Futile_White");
				sLeaser.sprites[fistSprite + 1 + connections.Count + j].color = new Color(0f, 0f, 0f);
				sLeaser.sprites[fistSprite + 1 + connections.Count + j].scale = (8f + 5f * (float)j) / 8f;
				sLeaser.sprites[fistSprite + 1 + connections.Count + j].shader = rCam.game.rainWorld.Shaders["VectorCircle"];
				sLeaser.sprites[fistSprite + 1 + connections.Count + j].alpha = 2f / (8f + 5f * (float)j);
			}
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < totalSprites; i++)
			{
				sLeaser.sprites[fistSprite + i].isVisible = lit;
			}
			if (!lit)
			{
				return;
			}
			Vector2 vector = OnPlanePos(timeStacker) * owner.rad + owner.pos - camPos;
			sLeaser.sprites[fistSprite].x = vector.x;
			sLeaser.sprites[fistSprite].y = vector.y;
			for (int j = 0; j < connections.Count; j++)
			{
				if (connectionsOn[j])
				{
					sLeaser.sprites[fistSprite + 1 + j].isVisible = true;
					sLeaser.sprites[fistSprite + 1 + j].x = vector.x;
					sLeaser.sprites[fistSprite + 1 + j].y = vector.y;
					Vector2 vector2 = owner.stars[connections[j]].OnPlanePos(timeStacker) * owner.rad + owner.pos - camPos;
					sLeaser.sprites[fistSprite + 1 + j].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
					sLeaser.sprites[fistSprite + 1 + j].scaleY = Vector2.Distance(vector, vector2);
				}
				else
				{
					sLeaser.sprites[fistSprite + 1 + j].isVisible = false;
				}
			}
			for (int k = 0; k < circles; k++)
			{
				sLeaser.sprites[fistSprite + 1 + connections.Count + k].x = vector.x;
				sLeaser.sprites[fistSprite + 1 + connections.Count + k].y = vector.y;
			}
		}
	}

	public PlacedObject placedObject;

	public Star[] stars;

	public int totalSprites;

	private float lastXRotat;

	private float xRotat;

	private float xRotatVel;

	private float xRotatVelVel;

	private float lastYRotat;

	private float yRotat;

	private float yRotatVel;

	private float yRotatVelVel;

	private float lastZRotat;

	private float zRotat;

	private float zRotatVel;

	private float zRotatVelVel;

	public Vector2 pos;

	public Vector3 focus;

	public Vector3 lastFocus;

	public Vector3 getToFocus;

	public float rad;

	public StarMatrix(PlacedObject placedObject)
	{
		this.placedObject = placedObject;
		pos = placedObject.pos;
		rad = (placedObject.data as PlacedObject.ResizableObjectData).Rad + 50f;
		float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(rad, 500f, 0.3f)), 2f) * (float)Math.PI;
		stars = new Star[(int)(num / 4000f)];
		totalSprites = 0;
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i] = new Star(this, i, totalSprites);
			totalSprites += stars[i].totalSprites;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastFocus = focus;
		focus = Vector3.Lerp(Vector3.MoveTowards(focus, getToFocus, 0.002f), getToFocus, 0.007f);
		if (UnityEngine.Random.value < 0.025f && stars.Length != 0)
		{
			int num = UnityEngine.Random.Range(0, stars.Length);
			if (stars[num].position.magnitude < 0.6f)
			{
				getToFocus = stars[num].position + UnityEngine.Random.insideUnitSphere * 0.05f;
			}
		}
		lastXRotat = xRotat;
		lastYRotat = yRotat;
		lastZRotat = zRotat;
		xRotat += xRotatVel;
		yRotat += yRotatVel;
		zRotat += zRotatVel;
		xRotatVel = Mathf.Clamp(xRotatVel, -0.1f, 0.1f) * 0.9f;
		yRotatVel = Mathf.Clamp(yRotatVel, -0.1f, 0.1f) * 0.9f;
		zRotatVel = Mathf.Clamp(zRotatVel, -0.1f, 0.1f) * 0.9f;
		xRotatVel += xRotatVelVel;
		yRotatVel += yRotatVelVel;
		zRotatVelVel += zRotatVelVel;
		xRotatVelVel = Mathf.Clamp(xRotatVelVel, -0.01f, 0.01f) * 0.9f;
		yRotatVelVel = Mathf.Clamp(yRotatVelVel, -0.01f, 0.01f) * 0.9f;
		zRotatVelVel = Mathf.Clamp(zRotatVelVel, -0.01f, 0.01f) * 0.9f;
		if (UnityEngine.Random.value < 0.05f)
		{
			xRotatVelVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Pow(UnityEngine.Random.value, 6f) * 0.005f;
		}
		if (UnityEngine.Random.value < 0.05f)
		{
			yRotatVelVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Pow(UnityEngine.Random.value, 6f) * 0.005f;
		}
		if (UnityEngine.Random.value < 0.05f)
		{
			zRotatVelVel += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * Mathf.Pow(UnityEngine.Random.value, 6f) * 0.005f;
		}
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i].Update();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i].InitiateSprites(sLeaser, rCam);
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < stars.Length; i++)
		{
			stars[i].DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
