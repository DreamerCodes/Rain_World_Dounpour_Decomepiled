using System.Collections.Generic;
using System.IO;
using RWCustom;
using UnityEngine;

namespace DevInterface;

public class CustomDecalRepresentation : QuadObjectRepresentation
{
	public class CustomDecalControlPanel : Panel, IDevUISignals
	{
		public class CustomDecalControlSlider : Slider
		{
			public CustomDecalControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				switch (IDstring)
				{
				case "From_Depth_Slider":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).fromDepth;
					base.NumberText = ((int)(num * 30f)).ToString();
					break;
				case "To_Depth_Slider":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).toDepth;
					base.NumberText = ((int)(num * 30f)).ToString();
					break;
				case "Noise_Slider":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).noise;
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "Intensity_Slider":
				{
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[0, 0];
					for (int j = 1; j < 4; j++)
					{
						if (num != ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[j, 0])
						{
							num = -1f;
							break;
						}
					}
					if (num < 0f)
					{
						base.NumberText = "N/A";
					}
					else
					{
						base.NumberText = (int)(num * 100f) + "%";
					}
					break;
				}
				case "Erosion_Slider":
				{
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[0, 1];
					for (int i = 1; i < 4; i++)
					{
						if (num != ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[i, 1])
						{
							num = -1f;
							break;
						}
					}
					if (num < 0f)
					{
						base.NumberText = "N/A";
					}
					else
					{
						base.NumberText = (int)(num * 100f) + "%";
					}
					break;
				}
				case "Intensity_Slider_0":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[0, 0];
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "Erosion_Slider_0":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[0, 1];
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "Intensity_Slider_1":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[1, 0];
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "Erosion_Slider_1":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[1, 1];
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "Intensity_Slider_2":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[2, 0];
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "Erosion_Slider_2":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[2, 1];
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "Intensity_Slider_3":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[3, 0];
					base.NumberText = (int)(num * 100f) + "%";
					break;
				case "Erosion_Slider_3":
					num = ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[3, 1];
					base.NumberText = (int)(num * 100f) + "%";
					break;
				}
				RefreshNubPos(num);
			}

			public override void NubDragged(float nubPos)
			{
				switch (IDstring)
				{
				case "From_Depth_Slider":
					nubPos = Mathf.Min(nubPos, ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).toDepth);
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).fromDepth = nubPos;
					break;
				case "To_Depth_Slider":
					nubPos = Mathf.Max(nubPos, ((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).fromDepth);
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).toDepth = nubPos;
					break;
				case "Noise_Slider":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).noise = nubPos;
					break;
				case "Intensity_Slider":
				{
					for (int j = 0; j < 4; j++)
					{
						((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[j, 0] = nubPos;
					}
					break;
				}
				case "Erosion_Slider":
				{
					for (int i = 0; i < 4; i++)
					{
						((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[i, 1] = nubPos;
					}
					break;
				}
				case "Intensity_Slider_0":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[0, 0] = nubPos;
					break;
				case "Erosion_Slider_0":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[0, 1] = nubPos;
					break;
				case "Intensity_Slider_1":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[1, 0] = nubPos;
					break;
				case "Erosion_Slider_1":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[1, 1] = nubPos;
					break;
				case "Intensity_Slider_2":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[2, 0] = nubPos;
					break;
				case "Erosion_Slider_2":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[2, 1] = nubPos;
					break;
				case "Intensity_Slider_3":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[3, 0] = nubPos;
					break;
				case "Erosion_Slider_3":
					((parentNode.parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).vertices[3, 1] = nubPos;
					break;
				}
				parentNode.parentNode.Refresh();
				(parentNode.parentNode as CustomDecalRepresentation).CD.UpdateMesh();
				Refresh();
			}
		}

		public SelectDecalPanel decalsSelectPanel;

		public CustomDecalControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(250f, 285f), "Custom Decal")
		{
			subNodes.Add(new CustomDecalControlSlider(owner, "From_Depth_Slider", this, new Vector2(5f, 265f), "From Depth: "));
			subNodes.Add(new CustomDecalControlSlider(owner, "To_Depth_Slider", this, new Vector2(5f, 245f), "To Depth: "));
			subNodes.Add(new CustomDecalControlSlider(owner, "Noise_Slider", this, new Vector2(5f, 225f), "Noise: "));
			subNodes.Add(new CustomDecalControlSlider(owner, "Intensity_Slider", this, new Vector2(5f, 205f), "Alpha: "));
			subNodes.Add(new CustomDecalControlSlider(owner, "Erosion_Slider", this, new Vector2(5f, 105f), "Erosion: "));
			for (int i = 0; i < 4; i++)
			{
				subNodes.Add(new CustomDecalControlSlider(owner, "Intensity_Slider_" + i, this, new Vector2(5f, 125f + (float)(3 - i) * 20f), "Alpha " + i + ": "));
				subNodes.Add(new CustomDecalControlSlider(owner, "Erosion_Slider_" + i, this, new Vector2(5f, 25f + (float)(3 - i) * 20f), "Erosion " + i + ": "));
			}
			subNodes.Add(new Button(owner, "Select_Decal_Panel_Button", this, new Vector2(5f, 5f), 240f, "Decal : " + ((parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).imageName));
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			if (sender.IDstring == "Select_Decal_Panel_Button")
			{
				if (decalsSelectPanel != null)
				{
					subNodes.Remove(decalsSelectPanel);
					decalsSelectPanel.ClearSprites();
					decalsSelectPanel = null;
				}
				else
				{
					decalsSelectPanel = new SelectDecalPanel(owner, this, new Vector2(200f, 15f) - absPos, (parentNode as CustomDecalRepresentation).decalFiles);
					subNodes.Add(decalsSelectPanel);
				}
				return;
			}
			if (sender.IDstring == "BackPage99289..?/~")
			{
				decalsSelectPanel.PrevPage();
				return;
			}
			if (sender.IDstring == "NextPage99289..?/~")
			{
				decalsSelectPanel.NextPage();
				return;
			}
			((parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).imageName = sender.IDstring;
			(parentNode as CustomDecalRepresentation).CD.UpdateAsset();
			for (int i = 0; i < subNodes.Count; i++)
			{
				if (subNodes[i].IDstring == "Select_Decal_Panel_Button")
				{
					(subNodes[i] as Button).Text = "Decal : " + ((parentNode as CustomDecalRepresentation).pObj.data as PlacedObject.CustomDecalData).imageName;
				}
			}
			if (decalsSelectPanel != null)
			{
				subNodes.Remove(decalsSelectPanel);
				decalsSelectPanel.ClearSprites();
				decalsSelectPanel = null;
			}
		}
	}

	public class SelectDecalPanel : Panel
	{
		private string[] decalNames;

		private int perpage;

		private int currentOffset;

		public SelectDecalPanel(DevUI owner, DevUINode parentNode, Vector2 pos, string[] decalNames)
			: base(owner, "Select_Decal_Panel", parentNode, pos, new Vector2(305f, 420f), "Select decal")
		{
			this.decalNames = decalNames;
			currentOffset = 0;
			perpage = 36;
			PopulateDecals(currentOffset);
		}

		public void PopulateDecals(int offset)
		{
			currentOffset = offset;
			foreach (DevUINode subNode in subNodes)
			{
				subNode.ClearSprites();
			}
			subNodes.Clear();
			IntVector2 intVector = new IntVector2(0, 0);
			for (int i = currentOffset; i < decalNames.Length && i < currentOffset + perpage; i++)
			{
				subNodes.Add(new Button(owner, decalNames[i], this, new Vector2(5f + (float)intVector.x * 150f, size.y - 25f - 20f * (float)intVector.y), 145f, decalNames[i]));
				intVector.y++;
				if (intVector.y >= (int)Mathf.Floor((float)perpage / 2f))
				{
					intVector.x++;
					intVector.y = 0;
				}
			}
			subNodes.Add(new Button(owner, "BackPage99289..?/~", this, new Vector2(5f, size.y - 25f - 20f * ((float)(perpage / 2) + 1f)), 145f, "Previous"));
			subNodes.Add(new Button(owner, "NextPage99289..?/~", this, new Vector2(155f, size.y - 25f - 20f * ((float)(perpage / 2) + 1f)), 145f, "Next"));
		}

		public void PrevPage()
		{
			currentOffset -= perpage;
			if (currentOffset < 0)
			{
				currentOffset = 0;
			}
			PopulateDecals(currentOffset);
		}

		public void NextPage()
		{
			currentOffset += perpage;
			if (currentOffset > decalNames.Length)
			{
				currentOffset = perpage * (int)Mathf.Floor((float)decalNames.Length / (float)perpage);
			}
			PopulateDecals(currentOffset);
		}
	}

	public CustomDecal CD;

	private CustomDecalControlPanel controlPanel;

	private int lineSprite;

	public string[] decalFiles;

	public Vector2 savLastPos;

	public CustomDecalRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, "Custom Decal")
	{
		controlPanel = new CustomDecalControlPanel(owner, "Custom_Decal_Panel", this, new Vector2(0f, 100f));
		subNodes.Add(controlPanel);
		controlPanel.pos = (pObj.data as PlacedObject.CustomDecalData).panelPos;
		fSprites.Add(new FSprite("pixel"));
		lineSprite = fSprites.Count - 1;
		owner.placedObjectsContainer.AddChild(fSprites[lineSprite]);
		fSprites[lineSprite].anchorY = 0f;
		if (CD == null)
		{
			for (int i = 0; i < owner.room.updateList.Count; i++)
			{
				if (owner.room.updateList[i] is CustomDecal && (owner.room.updateList[i] as CustomDecal).placedObject == pObj)
				{
					CD = owner.room.updateList[i] as CustomDecal;
					break;
				}
			}
			if (CD == null)
			{
				CD = new CustomDecal(pObj);
				owner.room.AddObject(CD);
			}
		}
		string[] array = AssetManager.ListDirectory("decals");
		List<string> list = new List<string>();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].ToLowerInvariant().EndsWith(".png"))
			{
				list.Add(Path.GetFileNameWithoutExtension(array[j]));
			}
		}
		decalFiles = list.ToArray();
	}

	public override void Update()
	{
		base.Update();
		if (!(pObj.pos != savLastPos))
		{
			return;
		}
		savLastPos = pObj.pos;
		if (Input.GetKey("l") && Futile.atlasManager.GetAtlasWithName((pObj.data as PlacedObject.CustomDecalData).imageName) != null)
		{
			float x = Futile.atlasManager.GetAtlasWithName((pObj.data as PlacedObject.CustomDecalData).imageName).textureSize.x;
			float y = Futile.atlasManager.GetAtlasWithName((pObj.data as PlacedObject.CustomDecalData).imageName).textureSize.y;
			(pObj.data as PlacedObject.CustomDecalData).handles[0] = new Vector2(0f, y);
			(pObj.data as PlacedObject.CustomDecalData).handles[1] = new Vector2(x, y);
			(pObj.data as PlacedObject.CustomDecalData).handles[2] = new Vector2(x, 0f);
			MoveAllHandles();
		}
		else if (Input.GetKey("k"))
		{
			for (int i = 0; i < 3; i++)
			{
				(pObj.data as PlacedObject.CustomDecalData).handles[i] *= 1.025f;
			}
			MoveAllHandles();
		}
		else if (Input.GetKey("j"))
		{
			for (int j = 0; j < 3; j++)
			{
				(pObj.data as PlacedObject.CustomDecalData).handles[j] *= 0.975f;
			}
			MoveAllHandles();
		}
	}

	private void MoveAllHandles()
	{
		for (int i = 0; i < 3; i++)
		{
			(subNodes[i] as Handle).pos = (pObj.data as PlacedObject.QuadObjectData).handles[i];
		}
		CD.UpdateMesh();
		Refresh();
	}

	public override void Refresh()
	{
		base.Refresh();
		MoveSprite(lineSprite, absPos);
		fSprites[lineSprite].scaleY = controlPanel.pos.magnitude;
		fSprites[lineSprite].rotation = Custom.AimFromOneVectorToAnother(absPos, controlPanel.absPos);
		(pObj.data as PlacedObject.CustomDecalData).panelPos = controlPanel.pos;
	}
}
