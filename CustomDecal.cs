using System.IO;
using RWCustom;
using UnityEngine;

public class CustomDecal : UpdatableAndDeletable, IDrawable
{
	public PlacedObject placedObject;

	public Vector2[] quad;

	public Vector2[] verts;

	public bool meshDirty;

	public bool elementDirty;

	private int gridDiv = 1;

	public CustomDecal(PlacedObject placedObject)
	{
		this.placedObject = placedObject;
		quad = new Vector2[4];
		quad[0] = placedObject.pos;
		quad[1] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0];
		quad[2] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];
		quad[3] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];
		gridDiv = GetIdealGridDiv();
		meshDirty = true;
		LoadFile((placedObject.data as PlacedObject.CustomDecalData).imageName);
	}

	public void LoadFile(string fileName)
	{
		if (Futile.atlasManager.GetAtlasWithName(fileName) == null)
		{
			string text = AssetManager.ResolveFilePath("Decals" + Path.DirectorySeparatorChar + fileName + ".png");
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: true, crispPixels: true);
			HeavyTexturesCache.LoadAndCacheAtlasFromTexture(fileName, texture2D, textureFromAsset: false);
		}
	}

	public void UpdateAsset()
	{
		LoadFile((placedObject.data as PlacedObject.CustomDecalData).imageName);
		elementDirty = true;
	}

	public void UpdateMesh()
	{
		meshDirty = true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (quad[0] != placedObject.pos || quad[1] != placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0] || quad[2] != placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1] || quad[3] != placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2])
		{
			meshDirty = true;
		}
	}

	public int GetIdealGridDiv()
	{
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			if (Vector2.Distance(quad[i], quad[i + 1]) > num)
			{
				num = Vector2.Distance(quad[i], quad[i + 1]);
			}
		}
		if (Vector2.Distance(quad[0], quad[3]) > num)
		{
			num = Vector2.Distance(quad[0], quad[3]);
		}
		return Mathf.Clamp(Mathf.RoundToInt(num / 150f), 1, 20);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh((placedObject.data as PlacedObject.CustomDecalData).imageName, gridDiv);
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["Decal"];
		verts = new Vector2[(sLeaser.sprites[0] as TriangleMesh).vertices.Length];
		AddToContainer(sLeaser, rCam, null);
		meshDirty = true;
	}

	private void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		quad[0] = placedObject.pos;
		quad[1] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0];
		quad[2] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];
		quad[3] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];
		int idealGridDiv = GetIdealGridDiv();
		if (idealGridDiv != gridDiv)
		{
			gridDiv = idealGridDiv;
			sLeaser.sprites[0].RemoveFromContainer();
			InitiateSprites(sLeaser, rCam);
		}
		Random.State state = Random.state;
		Random.InitState((int)(quad[0].x + quad[0].y + quad[1].x + quad[1].y + quad[2].x + quad[2].y + quad[3].x + quad[3].y));
		float[,] vertices = (placedObject.data as PlacedObject.CustomDecalData).vertices;
		for (int i = 0; i <= gridDiv; i++)
		{
			for (int j = 0; j <= gridDiv; j++)
			{
				Vector2 a = Vector2.Lerp(quad[0], quad[1], (float)j / (float)gridDiv);
				Vector2 b = Vector2.Lerp(quad[1], quad[2], (float)i / (float)gridDiv);
				Vector2 b2 = Vector2.Lerp(quad[3], quad[2], (float)j / (float)gridDiv);
				Vector2 a2 = Vector2.Lerp(quad[0], quad[3], (float)i / (float)gridDiv);
				float f = Mathf.Lerp(Mathf.Lerp(vertices[3, 1], vertices[2, 1], (float)i / (float)gridDiv), Mathf.Lerp(vertices[0, 1], vertices[1, 1], (float)i / (float)gridDiv), (float)j / (float)gridDiv);
				float f2 = Mathf.Lerp(Mathf.Lerp(vertices[3, 0], vertices[2, 0], (float)i / (float)gridDiv), Mathf.Lerp(vertices[0, 0], vertices[1, 0], (float)i / (float)gridDiv), (float)j / (float)gridDiv);
				f = Mathf.Pow(f, 1f + Mathf.Lerp(-0.5f, 0.5f, Random.value) * (placedObject.data as PlacedObject.CustomDecalData).noise);
				f2 = Mathf.Pow(f2, 1f + Mathf.Lerp(-0.5f, 0.5f, Random.value) * (placedObject.data as PlacedObject.CustomDecalData).noise);
				f = Mathf.Lerp(f, Random.value, (placedObject.data as PlacedObject.CustomDecalData).noise * Mathf.Pow(1f - 2f * Mathf.Abs(f - 0.5f), 2.5f));
				f2 = Mathf.Lerp(f2, Random.value, (placedObject.data as PlacedObject.CustomDecalData).noise * Mathf.Pow(1f - 2f * Mathf.Abs(f - 0.5f), 2.5f));
				verts[j * (gridDiv + 1) + i] = Custom.LineIntersection(a, b2, a2, b);
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[j * (gridDiv + 1) + i] = new Color((placedObject.data as PlacedObject.CustomDecalData).fromDepth, (placedObject.data as PlacedObject.CustomDecalData).toDepth, f, f2);
			}
		}
		Random.state = state;
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (meshDirty)
		{
			UpdateVerts(sLeaser, rCam);
			meshDirty = false;
		}
		if (elementDirty)
		{
			sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName((placedObject.data as PlacedObject.CustomDecalData).imageName);
			elementDirty = false;
		}
		for (int i = 0; i < verts.Length; i++)
		{
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, verts[i] - camPos);
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
		sLeaser.sprites[0].RemoveFromContainer();
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
	}
}
