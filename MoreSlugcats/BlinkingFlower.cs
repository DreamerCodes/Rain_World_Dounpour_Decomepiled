using System;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class BlinkingFlower : PhysicalObject, IDrawable, Explosion.IReactToExplosions
{
	private class Petal : CosmeticSprite
	{
		private float size;

		private float length;

		private float width;

		private Color color1;

		private Color color2;

		private float rotation;

		private float life;

		private float lifeDecrease;

		private float sine;

		private bool leaf;

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			if (!leaf)
			{
				sLeaser.sprites = new FSprite[2];
				for (int i = 0; i < 2; i++)
				{
					sLeaser.sprites[i] = new FSprite("SnailShell" + ((i == 0) ? "A" : "B"));
					sLeaser.sprites[i].scaleX = size * 0.35f * width;
					sLeaser.sprites[i].scaleY = 0.4f * size * length;
				}
				sLeaser.sprites[0].color = color1;
				sLeaser.sprites[1].color = color2;
			}
			else
			{
				sLeaser.sprites = new FSprite[1];
				sLeaser.sprites[0] = new FSprite("Cicada0body");
				sLeaser.sprites[0].scaleX = size * 0.7f * width;
				sLeaser.sprites[0].scaleY = 0.55f * size * length;
				sLeaser.sprites[0].anchorY = 0.5f;
				sLeaser.sprites[0].color = color1;
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			life -= lifeDecrease;
			lifeDecrease *= 1.02f;
			if (life <= 0f)
			{
				Destroy();
			}
			sine += 0.005f;
			vel.y = Mathf.Lerp(vel.y, leaf ? (-1f) : (-0.5f), 0.05f);
			vel.x += Mathf.Sin(sine * (float)Math.PI) * (leaf ? 0.025f : 0.05f);
			vel.x *= 0.96f;
			vel.y *= 0.96f;
			rotation += 1.5f + Mathf.Sin(sine * (float)Math.PI);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			for (int i = 0; i < (leaf ? 1 : 2); i++)
			{
				sLeaser.sprites[i].SetPosition(pos - camPos);
				sLeaser.sprites[i].rotation = rotation;
				sLeaser.sprites[i].scaleX = size * (leaf ? 0.7f : 0.35f) * width * life;
			}
		}

		public Petal(Vector2 pos, Vector2 vel, Color color1, Color color2, float size, float length, float width, bool leaf)
		{
			this.leaf = leaf;
			base.pos = pos;
			lastPos = pos;
			base.vel = vel;
			this.size = size;
			this.length = length;
			this.width = width;
			this.color1 = color1;
			this.color2 = color2;
			rotation = UnityEngine.Random.Range(45f, 135f);
			life = 1f;
			sine = UnityEngine.Random.value;
			lifeDecrease = UnityEngine.Random.Range(0.00025f, 0.001f) * (this.leaf ? 0.25f : 1f);
		}
	}

	private float leafOffset;

	private float leafLength;

	private float leafWidth;

	private float flinch;

	private float petalOffset;

	private float petalLength;

	private float petalWidth;

	private float[] leaves;

	private float size;

	private int totalSprites;

	private Vector2 lookPoint;

	private Vector2 lookDir;

	private float[] petals;

	private Color dropColor1;

	private Color dropColor2;

	private int closeCounter;

	private int hideCounter;

	private Color dropColor3;

	private int firstPetalSprite => 2;

	private int firstLeafSprite => 2 + petals.Length * 2;

	private float flowerVisible => Mathf.InverseLerp(120f, 0f, leaves[0]);

	public BlinkingFlower(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.gravity = 0f;
		collisionLayer = 0;
		totalSprites += 2;
		canBeHitByWeapons = true;
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		size = UnityEngine.Random.Range(0.85f, 1.25f);
		leafOffset = UnityEngine.Random.Range(-90f, 90f);
		leafLength = UnityEngine.Random.Range(0.9f, 1.1f);
		leafWidth = UnityEngine.Random.Range(0.9f, 1.1f);
		petalOffset = UnityEngine.Random.Range(-90f, 90f);
		petals = new float[(!(UnityEngine.Random.value < 0.15f)) ? 6 : ((UnityEngine.Random.value < 0.5f) ? 5 : 7)];
		totalSprites += petals.Length * 2;
		leaves = new float[(!(UnityEngine.Random.value < 0.3f)) ? 3 : ((UnityEngine.Random.value < 0.5f) ? 2 : 4)];
		totalSprites += leaves.Length;
		petalLength = UnityEngine.Random.Range(0.9f, 1.1f);
		petalWidth = UnityEngine.Random.Range(0.9f, 1.1f);
		UnityEngine.Random.state = state;
	}

	public void Explosion(Explosion explosion)
	{
		float num = Mathf.Lerp(explosion.rad, Mathf.Min(explosion.rad, 90f), 0.5f);
		if (Custom.DistLess(base.firstChunk.pos, explosion.pos, num * 3f))
		{
			float num2 = Vector2.Distance(base.firstChunk.pos, explosion.pos);
			if (!room.VisualContact(base.firstChunk.pos, explosion.pos))
			{
				num2 *= 4f;
			}
			if (num2 < explosion.rad * 0.5f)
			{
				Explode(Custom.DirVec(explosion.pos, base.firstChunk.pos) * explosion.force);
				return;
			}
			Ruffle(Mathf.Pow(Mathf.InverseLerp(num * 2f, num * 0.2f, num2), 2f) * explosion.force * 22.2f);
			flinch = UnityEngine.Random.Range(30, 60);
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		lookPoint = base.firstChunk.pos;
	}

	private int PetalSprite(int petal, int side)
	{
		return firstPetalSprite + petal * 2 + side;
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (weapon is Spear)
		{
			Explode(weapon.firstChunk.vel * 0.1f);
		}
		else
		{
			Ruffle(weapon.firstChunk.vel.magnitude * 3f);
		}
	}

	private void Explode(Vector2 vel)
	{
		for (int i = 0; i < petals.Length; i++)
		{
			room.AddObject(new Petal(base.firstChunk.pos, Custom.RNV() * UnityEngine.Random.value * 5f + vel, dropColor1, dropColor2, size, petalLength, petalWidth, leaf: false));
		}
		for (int j = 0; j < leaves.Length; j++)
		{
			room.AddObject(new Petal(base.firstChunk.pos, Custom.RNV() * UnityEngine.Random.value * 5f + vel * 0.75f, dropColor3, dropColor3, size, leafLength, leafWidth, leaf: true));
		}
		Destroy();
	}

	private void Ruffle(float power)
	{
		room.PlaySound(SoundID.Fly_Lure_Ruffled_By_Fly, base.firstChunk.pos, 1f, 1.5f);
		for (int i = 0; i < petals.Length; i++)
		{
			petals[i] += UnityEngine.Random.Range(-1f, 1f) * power;
		}
		for (int j = 0; j < leaves.Length; j++)
		{
			leaves[j] += UnityEngine.Random.Range(-1f, 1f) * power;
		}
	}

	public override void Update(bool eu)
	{
		base.firstChunk.pos = room.MiddleOfTile(abstractPhysicalObject.pos.Tile);
		base.firstChunk.vel *= 0f;
		base.Update(eu);
		PhysicalObject physicalObject = null;
		float num = 300f;
		closeCounter = Mathf.Max(closeCounter - 1, 0);
		hideCounter = Mathf.Max(hideCounter - 1, 0);
		flinch = Mathf.Max(flinch - 1f, 0f);
		if (room.abstractRoom.entities != null && room.abstractRoom.entities.Count > 0)
		{
			for (int i = 0; i < room.abstractRoom.entities.Count; i++)
			{
				if ((room.abstractRoom.entities[i] is AbstractCreature && (room.abstractRoom.entities[i] as AbstractCreature).realizedCreature != null && room.VisualContact(base.firstChunk.pos, (room.abstractRoom.entities[i] as AbstractCreature).realizedCreature.firstChunk.pos) && !(room.abstractRoom.entities[i] as AbstractCreature).realizedCreature.inShortcut && (base.firstChunk.pos - (room.abstractRoom.entities[i] as AbstractCreature).realizedCreature.firstChunk.pos).magnitude < num) || (room.abstractRoom.entities[i] is AbstractPhysicalObject && (room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject != null && (room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject is Weapon && ((room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject as Weapon).mode == Weapon.Mode.Thrown && (base.firstChunk.pos - (room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject.firstChunk.pos).magnitude < num / 2f))
				{
					physicalObject = (room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject;
					num = (base.firstChunk.pos - (room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject.firstChunk.pos).magnitude * (((room.abstractRoom.entities[i] as AbstractPhysicalObject).realizedObject is Weapon) ? 0.5f : 1f);
				}
			}
		}
		bool flag = false;
		if (physicalObject != null)
		{
			lookPoint = physicalObject.firstChunk.pos;
			flag = (base.firstChunk.pos - physicalObject.firstChunk.pos).magnitude < 80f;
			closeCounter = UnityEngine.Random.Range(15, 30);
		}
		if (flag && physicalObject is Weapon)
		{
			flinch = UnityEngine.Random.Range(30, 60);
		}
		if (flinch > 0f)
		{
			for (int j = 0; j < petals.Length; j++)
			{
				petals[j] = Mathf.Lerp(petals[j], 175f, 0.25f);
			}
			for (int k = 0; k < leaves.Length; k++)
			{
				leaves[k] = Mathf.Lerp(leaves[k], 160f, 0.2f);
			}
		}
		else
		{
			for (int l = 0; l < petals.Length; l++)
			{
				if (closeCounter > 0)
				{
					if (flag && hideCounter == 0)
					{
						hideCounter = UnityEngine.Random.Range(30, 60);
					}
					petals[l] = Mathf.Lerp(petals[l], (flag || hideCounter != 0) ? 160 : 0, 0.15f);
				}
				else
				{
					petals[l] = Mathf.Lerp(petals[l], 160f, 0.1f);
				}
			}
			for (int m = 0; m < leaves.Length; m++)
			{
				if (closeCounter > 0)
				{
					leaves[m] = Mathf.Lerp(leaves[m], 0f, 0.1f);
				}
				else
				{
					leaves[m] = Mathf.Lerp(leaves[m], 135f, 0.075f);
				}
			}
		}
		lookDir = Vector2.Lerp(lookDir, Custom.DirVec(base.firstChunk.pos, lookPoint), 0.1f);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[totalSprites];
		sLeaser.sprites[0] = new FSprite("Circle20");
		sLeaser.sprites[1] = new FSprite("Circle20");
		sLeaser.sprites[0].scale = 0.5f * size;
		sLeaser.sprites[1].scale = 0.3f * size;
		sLeaser.sprites[1].color = new Color(0.5f, 0.5f, 0.5f);
		for (int i = 0; i < petals.Length; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[PetalSprite(i, j)] = new FSprite("SnailShell" + ((j == 0) ? "A" : "B"));
				sLeaser.sprites[PetalSprite(i, j)].scaleX = size * 0.35f * petalWidth;
				sLeaser.sprites[PetalSprite(i, j)].scaleY = 0.4f * size * petalLength;
				sLeaser.sprites[PetalSprite(i, j)].anchorY = 0.25f;
			}
		}
		for (int k = 0; k < leaves.Length; k++)
		{
			sLeaser.sprites[LeafSprite(k)] = new FSprite("Cicada0body");
			sLeaser.sprites[LeafSprite(k)].scaleX = size * 0.7f * leafWidth;
			sLeaser.sprites[LeafSprite(k)].scaleY = 0.55f * size * leafLength;
			sLeaser.sprites[LeafSprite(k)].anchorY = 0.5f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.RemoveAllSpritesFromContainer();
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Background");
		}
		for (int i = 0; i < leaves.Length; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[LeafSprite(i)]);
		}
		for (int j = 0; j < 2; j++)
		{
			newContatiner.AddChild(sLeaser.sprites[j]);
		}
		for (int k = 0; k < petals.Length * 2; k++)
		{
			newContatiner.AddChild(sLeaser.sprites[firstPetalSprite + k]);
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (base.slatedForDeletetion)
		{
			sLeaser.CleanSpritesAndRemove();
			return;
		}
		RoomPalette currentPalette = rCam.currentPalette;
		dropColor1 = Color.Lerp(Color.Lerp(Color.Lerp((new Color(1f, 1f, 1f) - currentPalette.texture.GetPixel(30, 2)).CloneWithNewAlpha(1f), currentPalette.blackColor, 0.3f), rCam.PixelColorAtCoordinate(base.firstChunk.pos), 0.1f), currentPalette.blackColor, room.Darkness(base.firstChunk.pos));
		dropColor2 = Color.Lerp(Color.Lerp(Color.Lerp(currentPalette.texture.GetPixel(30, 2), currentPalette.blackColor, 0.3f), rCam.PixelColorAtCoordinate(base.firstChunk.pos), 0.1f), currentPalette.blackColor, room.Darkness(base.firstChunk.pos));
		dropColor3 = Color.Lerp(Color.Lerp(rCam.PixelColorAtCoordinate(base.firstChunk.pos), currentPalette.blackColor, 0.25f + flowerVisible * 0.35f), currentPalette.blackColor, room.Darkness(base.firstChunk.pos));
		for (int i = 0; i < petals.Length; i++)
		{
			sLeaser.sprites[PetalSprite(i, 0)].color = dropColor1;
			sLeaser.sprites[PetalSprite(i, 1)].color = dropColor2;
		}
		sLeaser.sprites[0].color = Color.Lerp(rCam.PixelColorAtCoordinate(base.firstChunk.pos), currentPalette.blackColor, room.Darkness(base.firstChunk.pos));
		sLeaser.sprites[1].color = Color.Lerp(Color.Lerp(rCam.PixelColorAtCoordinate(base.firstChunk.pos), Color.Lerp(Color.white, currentPalette.fogColor, 0.5f), 0.4f), currentPalette.blackColor, room.Darkness(base.firstChunk.pos));
		for (int j = 0; j < leaves.Length; j++)
		{
			sLeaser.sprites[LeafSprite(j)].color = dropColor3;
		}
		sLeaser.sprites[0].SetPosition(base.firstChunk.pos - camPos);
		sLeaser.sprites[1].SetPosition(base.firstChunk.pos + lookDir * size * flowerVisible - camPos);
		sLeaser.sprites[0].scale = 0.5f * size * flowerVisible;
		sLeaser.sprites[1].scale = 0.3f * size * flowerVisible;
		sLeaser.sprites[1].color = new Color(0.5f, 0.5f, 0.5f);
		for (int k = 0; k < petals.Length; k++)
		{
			for (int l = 0; l < 2; l++)
			{
				sLeaser.sprites[PetalSprite(k, l)].SetPosition(base.firstChunk.pos + Custom.DegToVec(360f * ((float)(k + 1) / (float)petals.Length) + petalOffset) * 6f * size * flowerVisible - camPos);
				sLeaser.sprites[PetalSprite(k, l)].rotation = 360f * ((float)(k + 1) / (float)petals.Length) + petalOffset + petals[k];
				sLeaser.sprites[PetalSprite(k, l)].scaleX = size * 0.35f * petalWidth * flowerVisible;
				sLeaser.sprites[PetalSprite(k, l)].scaleY = 0.4f * size * petalLength * flowerVisible;
			}
		}
		for (int m = 0; m < leaves.Length; m++)
		{
			sLeaser.sprites[LeafSprite(m)].SetPosition(base.firstChunk.pos + Custom.DegToVec(360f * ((float)(m + 1) / (float)leaves.Length) + leafOffset) * 3f * size - camPos);
			sLeaser.sprites[LeafSprite(m)].rotation = 360f * ((float)(m + 1) / (float)leaves.Length) + leafOffset + leaves[m];
		}
	}

	private int LeafSprite(int leaf)
	{
		return firstLeafSprite + leaf;
	}
}
