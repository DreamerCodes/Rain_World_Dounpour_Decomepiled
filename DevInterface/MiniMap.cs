using RWCustom;
using UnityEngine;

namespace DevInterface;

public class MiniMap : RectangularDevUINode
{
	public MapObject.RoomRepresentation roomRep;

	public Vector2[] nodeSquarePositions;

	public bool textureLoaded;

	public World world;

	public MapPage mapPage;

	public FLabel[,] nodeLabels;

	public float Scale
	{
		get
		{
			if (!(parentNode as RoomPanel).CanonView)
			{
				return 2f;
			}
			return 3f;
		}
	}

	public MiniMap(DevUI owner, World world, MapPage mapPage, DevUINode parentNode, Vector2 pos, Vector2 size, MapObject.RoomRepresentation roomRep)
		: base(owner, roomRep.room.name + "_Mini_Map", parentNode, pos, size)
	{
		this.world = world;
		this.mapPage = mapPage;
		this.roomRep = roomRep;
		fSprites.Add(new FSprite("pixel"));
		fSprites[0].anchorX = 0f;
		fSprites[0].anchorY = 0f;
		if (owner != null)
		{
			Futile.stage.AddChild(fSprites[0]);
		}
		for (int i = 0; i < roomRep.room.nodes.Length; i++)
		{
			fSprites.Add(new FSprite("pixel"));
			fSprites[fSprites.Count - 1].scale = 16f;
			fSprites[fSprites.Count - 1].alpha = 0.5f;
			if (roomRep.room.nodes[i].type == AbstractRoomNode.Type.Exit)
			{
				fSprites[fSprites.Count - 1].color = new Color(1f, 1f, 1f);
			}
			else if (roomRep.room.nodes[i].type == AbstractRoomNode.Type.Den)
			{
				fSprites[fSprites.Count - 1].color = new Color(1f, 0f, 1f);
			}
			else if (roomRep.room.nodes[i].type == AbstractRoomNode.Type.RegionTransportation)
			{
				fSprites[fSprites.Count - 1].color = new Color(0.2f, 0.2f, 0.2f);
			}
			else if (roomRep.room.nodes[i].type == AbstractRoomNode.Type.SideExit)
			{
				fSprites[fSprites.Count - 1].color = new Color(0.5f, 0.85f, 0.5f);
			}
			else if (roomRep.room.nodes[i].type == AbstractRoomNode.Type.SkyExit)
			{
				fSprites[fSprites.Count - 1].color = new Color(0.2f, 0.85f, 1f);
			}
			else if (roomRep.room.nodes[i].type == AbstractRoomNode.Type.SeaExit)
			{
				fSprites[fSprites.Count - 1].color = new Color(0f, 0f, 1f);
			}
			else if (roomRep.room.nodes[i].type == AbstractRoomNode.Type.BatHive)
			{
				fSprites[fSprites.Count - 1].color = new Color(0f, 1f, 0.2f);
			}
			else if (roomRep.room.nodes[i].type == AbstractRoomNode.Type.GarbageHoles)
			{
				fSprites[fSprites.Count - 1].color = new Color(1f, 0.5f, 0f);
			}
			if (owner != null)
			{
				Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
			}
		}
		nodeLabels = new FLabel[roomRep.room.exits + roomRep.room.dens, 2];
		for (int j = 0; j < nodeLabels.GetLength(0); j++)
		{
			for (int k = 0; k < nodeLabels.GetLength(1); k++)
			{
				nodeLabels[j, k] = new FLabel(Custom.GetFont(), j.ToString());
				nodeLabels[j, k].color = ((k == 0) ? Color.black : ((j < roomRep.room.exits) ? new Color(0.7f, 0.7f, 0.7f) : new Color(1f, 0.35f, 1f)));
				fLabels.Add(nodeLabels[j, k]);
				if (owner != null)
				{
					Futile.stage.AddChild(nodeLabels[j, k]);
				}
			}
		}
		for (int l = 0; l < roomRep.room.nodes.Length && !(roomRep.room.nodes[l].type != AbstractRoomNode.Type.Exit); l++)
		{
			fSprites.Add(new FSprite("pixel"));
			fSprites[fSprites.Count - 1].anchorY = 0f;
			if (owner != null)
			{
				Futile.stage.AddChild(fSprites[fSprites.Count - 1]);
			}
		}
		nodeSquarePositions = new Vector2[roomRep.room.nodes.Length];
	}

	public override void Update()
	{
		base.Update();
		if (mapPage.attractivenessPanel != null)
		{
			if (base.MouseOver)
			{
				if (owner != null && owner.mouseClick)
				{
					mapPage.attractivenessPanel.RoomClicked((parentNode as RoomPanel).roomRep.room.index);
				}
				else
				{
					mapPage.attractivenessPanel.RoomMouseOver((parentNode as RoomPanel).roomRep.room.index);
				}
			}
		}
		else if (Input.GetKey("n") && owner != null && owner.mouseClick && (parentNode as RoomPanel).CanonView && base.MouseOver)
		{
			(parentNode as RoomPanel).layer++;
			if ((parentNode as RoomPanel).layer > 2)
			{
				(parentNode as RoomPanel).layer = 0;
			}
		}
		else
		{
			if ((parentNode as RoomPanel).CanonView || !mapPage.subRegionsMode || owner == null || !owner.mouseClick || !base.MouseOver)
			{
				return;
			}
			if ((parentNode as RoomPanel).roomRep.room.gate || (parentNode as RoomPanel).roomRep.room.shelter || (parentNode as RoomPanel).roomRep.room.world.region == null)
			{
				(parentNode as RoomPanel).roomRep.room.subregionName = null;
				(parentNode as RoomPanel).roomRep.room.altSubregionName = null;
				return;
			}
			if ((parentNode as RoomPanel).roomRep.room.subregionName == null && (parentNode as RoomPanel).roomRep.room.world.region.subRegions.Count > 1)
			{
				(parentNode as RoomPanel).roomRep.room.subregionName = (parentNode as RoomPanel).roomRep.room.world.region.subRegions[1];
				if ((parentNode as RoomPanel).roomRep.room.world.region.altSubRegions.Count > 1)
				{
					(parentNode as RoomPanel).roomRep.room.altSubregionName = (parentNode as RoomPanel).roomRep.room.world.region.altSubRegions[1];
				}
				else
				{
					(parentNode as RoomPanel).roomRep.room.altSubregionName = null;
				}
				return;
			}
			int num = -1;
			for (int i = 1; i < (parentNode as RoomPanel).roomRep.room.world.region.subRegions.Count; i++)
			{
				if ((parentNode as RoomPanel).roomRep.room.subregionName == (parentNode as RoomPanel).roomRep.room.world.region.subRegions[i])
				{
					num = i;
					break;
				}
			}
			if (num == -1 || num >= (parentNode as RoomPanel).roomRep.room.world.region.subRegions.Count - 1)
			{
				(parentNode as RoomPanel).roomRep.room.subregionName = null;
				(parentNode as RoomPanel).roomRep.room.altSubregionName = null;
				return;
			}
			(parentNode as RoomPanel).roomRep.room.subregionName = (parentNode as RoomPanel).roomRep.room.world.region.subRegions[num + 1];
			if ((parentNode as RoomPanel).roomRep.room.world.region.altSubRegions.Count > num + 1)
			{
				(parentNode as RoomPanel).roomRep.room.altSubregionName = (parentNode as RoomPanel).roomRep.room.world.region.altSubRegions[num + 1];
			}
			else
			{
				(parentNode as RoomPanel).roomRep.room.altSubregionName = null;
			}
		}
	}

	public override void Refresh()
	{
		if (fSprites.Count == 0)
		{
			return;
		}
		fSprites[0].scale = Scale;
		if (roomRep.mapTex != null)
		{
			size = new Vector2(roomRep.mapTex.sourcePixelSize.x, roomRep.mapTex.sourcePixelSize.y) * Scale;
			fSprites[0].element = roomRep.mapTex;
			textureLoaded = true;
		}
		MoveSprite(0, absPos);
		fSprites[0].alpha = ((parentNode as RoomPanel).CanonView ? 0.75f : 1f);
		if (mapPage.viewNodeLabels)
		{
			for (int i = 0; i < nodeLabels.GetLength(0); i++)
			{
				nodeLabels[i, 0].isVisible = true;
				nodeLabels[i, 1].isVisible = true;
				Vector2 exitVisPos = GetExitVisPos(i);
				nodeLabels[i, 0].x = exitVisPos.x - 1f;
				nodeLabels[i, 0].y = exitVisPos.y - 1f;
				nodeLabels[i, 1].x = exitVisPos.x;
				nodeLabels[i, 1].y = exitVisPos.y;
			}
		}
		else
		{
			for (int j = 0; j < nodeLabels.GetLength(0); j++)
			{
				nodeLabels[j, 0].isVisible = false;
				nodeLabels[j, 1].isVisible = false;
			}
		}
		if (mapPage.attractivenessPanel != null)
		{
			fSprites[0].color = mapPage.attractivenessPanel.ColorOfRoom(roomRep.room.index);
		}
		if ((parentNode as RoomPanel).CanonView && mapPage.attractivenessPanel == null)
		{
			for (int k = 0; k < roomRep.room.nodes.Length; k++)
			{
				fSprites[k + 1].isVisible = false;
			}
			switch ((parentNode as RoomPanel).layer)
			{
			case 0:
				fSprites[0].color = new Color(0.5f, 1f, 0.5f);
				break;
			case 2:
				fSprites[0].color = new Color(1f, 0.5f, 0.5f);
				break;
			default:
				fSprites[0].color = new Color(1f, 1f, 1f);
				break;
			}
		}
		else
		{
			if (mapPage.attractivenessPanel == null)
			{
				if (!(parentNode as RoomPanel).CanonView && mapPage.subRegionsMode)
				{
					fSprites[0].color = MapPage.SubregionColor((parentNode as RoomPanel).roomRep.room);
				}
				else
				{
					fSprites[0].color = new Color(1f, 1f, 1f);
				}
			}
			Vector2 vector = new Vector2(10f, -15f);
			for (int l = 0; l < roomRep.room.nodes.Length; l++)
			{
				nodeSquarePositions[l] = vector;
				MoveSprite(l + 1, absPos + vector);
				fSprites[l + 1].isVisible = true;
				vector.x += 18f;
				if (vector.x > (parentNode as RoomPanel).size.x - 18f)
				{
					vector.x = 10f;
					vector.y -= 18f;
				}
			}
		}
		RefreshOnlyConLines();
		for (int m = 0; m < roomRep.room.nodes.Length && !(roomRep.room.nodes[m].type != AbstractRoomNode.Type.Exit); m++)
		{
			mapPage.GetMiniMapOfRoom(roomRep.room.connections[m])?.RefreshOnlyConLines();
		}
	}

	public void RefreshOnlyConLines()
	{
		for (int i = 0; i < roomRep.room.nodes.Length && !(roomRep.room.nodes[i].type != AbstractRoomNode.Type.Exit); i++)
		{
			Vector2 exitVisPos = GetExitVisPos(i);
			Vector2 vector = mapPage.ExitVisPos(roomRep.room.connections[i], world.NodeInALeadingToB(roomRep.room.connections[i], roomRep.room.index).abstractNode);
			if (vector.x == 0f && vector.y == 0f)
			{
				vector = exitVisPos + new Vector2(40f, -40f);
			}
			MoveSprite(i + 1 + roomRep.room.nodes.Length, exitVisPos + Custom.PerpendicularVector((exitVisPos - vector).normalized) * 1.2f);
			fSprites[i + 1 + roomRep.room.nodes.Length].scaleY = Vector2.Distance(exitVisPos, vector);
			fSprites[i + 1 + roomRep.room.nodes.Length].rotation = Custom.AimFromOneVectorToAnother(exitVisPos, vector);
		}
	}

	public Vector2 GetNodeSquarePos(int node)
	{
		if (node < 0 || node >= nodeSquarePositions.Length)
		{
			return absPos + Custom.RNV() * 20f;
		}
		return absPos + nodeSquarePositions[node];
	}

	public Vector2 GetExitVisPos(int node)
	{
		if (node < 0 || node > roomRep.nodePositions.Length - 1)
		{
			return absPos + size * 0.5f;
		}
		if (roomRep.nodePositions[node].x == 0f && roomRep.nodePositions[node].y == 0f)
		{
			if ((parentNode as RoomPanel).CanonView)
			{
				return absPos + size * 0.5f;
			}
			return GetNodeSquarePos(node);
		}
		return absPos + roomRep.nodePositions[node] * Scale;
	}
}
