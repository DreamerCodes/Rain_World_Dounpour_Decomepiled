using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

namespace Menu;

public class SlideShowMenuScene : MenuScene
{
	public class CameraMovementEditor
	{
		public class PointVisualizer
		{
			public CameraMovementEditor movEditor;

			public int index;

			public FSprite sprite;

			public FLabel label;

			public FLabel depthLabel;

			public PointVisualizer(CameraMovementEditor movEditor, int index)
			{
				this.movEditor = movEditor;
				this.index = index;
				sprite = new FSprite("pixel");
				sprite.anchorY = 0f;
				sprite.color = new Color(1f, 0f, 0f);
				Futile.stage.AddChild(sprite);
				label = new FLabel(Custom.GetFont(), index.ToString());
				label.color = new Color(1f, 0f, 0f);
				Futile.stage.AddChild(label);
				depthLabel = new FLabel(Custom.GetFont(), movEditor.scene.cameraMovementPoints[index].z.ToString());
				depthLabel.color = new Color(0f, 0f, 1f);
				Futile.stage.AddChild(depthLabel);
				UpdateDepthLabelText();
			}

			public void Update()
			{
				Vector2 vector = (Vector2)movEditor.scene.cameraMovementPoints[index] + movEditor.scene.menu.manager.rainWorld.screenSize * 0.5f;
				Vector2 vector2 = ((index != movEditor.scene.cameraMovementPoints.Count - 1) ? ((Vector2)movEditor.scene.cameraMovementPoints[index + 1] + movEditor.scene.menu.manager.rainWorld.screenSize * 0.5f) : (vector + new Vector2(0f, -10f)));
				sprite.x = vector.x;
				sprite.y = vector.y;
				sprite.scaleY = Vector2.Distance(vector, vector2);
				sprite.rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
				label.x = vector.x + 0.1f;
				label.y = vector.y + 20.1f;
				depthLabel.x = vector.x + 0.1f;
				depthLabel.y = vector.y - 20.1f;
			}

			public void UpdateDepthLabelText()
			{
				if (movEditor.scene.cameraMovementPoints[index].z == -1f)
				{
					depthLabel.text = "na";
					return;
				}
				int num = -1;
				float num2 = float.MaxValue;
				for (int i = 0; i < movEditor.scene.depthIllustrations.Count; i++)
				{
					float num3 = Mathf.Abs(movEditor.scene.cameraMovementPoints[index].z - movEditor.scene.depthIllustrations[i].depth);
					if (num3 < 0.1f && num3 < num2)
					{
						num = i;
						num2 = num3;
					}
				}
				if (num == -1)
				{
					depthLabel.text = movEditor.scene.cameraMovementPoints[index].z.ToString("n3") + "\r\n ";
					return;
				}
				depthLabel.text = movEditor.scene.cameraMovementPoints[index].z.ToString("n3") + "\r\n(" + movEditor.scene.depthIllustrations[num].fileName + "  " + movEditor.scene.depthIllustrations[num].depth + ")";
			}

			public void ClearSprites()
			{
				sprite.RemoveFromContainer();
				label.RemoveFromContainer();
				depthLabel.RemoveFromContainer();
			}
		}

		public SlideShowMenuScene scene;

		public List<PointVisualizer> pointVizs;

		public FSprite mainPosViz;

		public FSprite[] dots;

		public bool lastMouseDown;

		public bool lastAddButton;

		public bool lastRemoveButton;

		public int movePoint = -1;

		public float cycle;

		public Vector2 lastMousePos;

		public CameraMovementEditor(SlideShowMenuScene scene)
		{
			this.scene = scene;
			pointVizs = new List<PointVisualizer>();
			for (int i = 0; i < scene.cameraMovementPoints.Count; i++)
			{
				pointVizs.Add(new PointVisualizer(this, i));
			}
			mainPosViz = new FSprite("mouseEyeB1");
			Futile.stage.AddChild(mainPosViz);
			mainPosViz.color = new Color(0f, 1f, 0.1f);
			dots = new FSprite[10];
			for (int j = 0; j < dots.Length; j++)
			{
				dots[j] = new FSprite("pixel");
				Futile.stage.AddChild(dots[j]);
				dots[j].color = new Color(1f, 0f, 0f);
			}
		}

		public void Update()
		{
			for (int i = 0; i < pointVizs.Count; i++)
			{
				pointVizs[i].Update();
			}
			if (Input.GetMouseButton(0))
			{
				if (!lastMouseDown && movePoint == -1)
				{
					float num = float.MaxValue;
					int num2 = -1;
					for (int j = 0; j < pointVizs.Count; j++)
					{
						if (Vector2.Distance((Vector2)scene.cameraMovementPoints[j] + scene.menu.manager.rainWorld.screenSize * 0.5f, Futile.mousePosition) < num)
						{
							num2 = j;
							num = Vector2.Distance((Vector2)scene.cameraMovementPoints[j] + scene.menu.manager.rainWorld.screenSize * 0.5f, Futile.mousePosition);
						}
					}
					if (num < 20f)
					{
						movePoint = num2;
					}
				}
			}
			else
			{
				movePoint = -1;
			}
			lastMouseDown = Input.GetMouseButton(0);
			if (Input.GetKey("j") && !lastAddButton)
			{
				AddAPoint();
			}
			lastAddButton = Input.GetKey("j");
			if (Input.GetKey("k") && !lastRemoveButton)
			{
				RemoveAPoint();
			}
			lastRemoveButton = Input.GetKey("k");
			if (Input.GetKey("l"))
			{
				for (int k = 0; k < scene.cameraMovementPoints.Count; k++)
				{
					scene.cameraMovementPoints[k] = scene.cameraMovementPoints[k] + (Vector3)((Vector2)Futile.mousePosition - lastMousePos);
				}
			}
			if (movePoint > -1)
			{
				if (Input.GetKey("o"))
				{
					ChangeDepthOfPoint(movePoint);
				}
				else
				{
					scene.cameraMovementPoints[movePoint] = new Vector3(Futile.mousePosition.x - scene.menu.manager.rainWorld.screenSize.x * 0.5f, Futile.mousePosition.y - scene.menu.manager.rainWorld.screenSize.y * 0.5f, scene.cameraMovementPoints[movePoint].z);
					scene.camPos = scene.cameraMovementPoints[movePoint] / 300f;
				}
			}
			else if (!scene.testPlay)
			{
				scene.displayTime = Mathf.InverseLerp(0f, 1000f, Futile.mousePosition.x);
			}
			cycle += 0.0025f;
			for (int l = 0; l < dots.Length; l++)
			{
				Vector2 vector = scene.CameraPosition(Custom.Decimal(cycle + (float)l / (float)dots.Length)) + scene.menu.manager.rainWorld.screenSize * 0.5f;
				dots[l].x = vector.x;
				dots[l].y = vector.y;
			}
			Vector2 vector2 = scene.CameraPosition(scene.displayTime);
			mainPosViz.x = vector2.x + scene.menu.manager.rainWorld.screenSize.x * 0.5f;
			mainPosViz.y = vector2.y + scene.menu.manager.rainWorld.screenSize.y * 0.5f;
			lastMousePos = Futile.mousePosition;
		}

		private void ChangeDepthOfPoint(int point)
		{
			float num = Mathf.InverseLerp(0f, 768f, Futile.mousePosition.y);
			float z = scene.cameraMovementPoints[point].z;
			z = ((num != 1f || point <= 0 || point >= scene.cameraMovementPoints.Count - 1) ? Mathf.Lerp(0f, 10f, num) : (-1f));
			scene.cameraMovementPoints[point] = new Vector3(scene.cameraMovementPoints[point].x, scene.cameraMovementPoints[point].y, z);
			pointVizs[point].UpdateDepthLabelText();
			if (z != -1f)
			{
				scene.focus = z;
			}
		}

		public void AddAPoint()
		{
			scene.cameraMovementPoints.Add(Custom.RNV() * 50f);
			pointVizs.Add(new PointVisualizer(this, scene.cameraMovementPoints.Count - 1));
		}

		public void RemoveAPoint()
		{
			if (scene.cameraMovementPoints.Count > 1 && pointVizs.Count != 0)
			{
				scene.cameraMovementPoints.RemoveAt(scene.cameraMovementPoints.Count - 1);
				pointVizs[pointVizs.Count - 1].ClearSprites();
				pointVizs.RemoveAt(pointVizs.Count - 1);
			}
		}

		public void WriteAndClear()
		{
			for (int i = 0; i < pointVizs.Count; i++)
			{
				pointVizs[i].ClearSprites();
			}
			for (int j = 0; j < dots.Length; j++)
			{
				dots[j].RemoveFromContainer();
			}
			mainPosViz.RemoveFromContainer();
		}
	}

	public List<Vector3> cameraMovementPoints;

	public List<float> depths;

	public bool moveEditorButton;

	public bool testPlayButton;

	public CameraMovementEditor moveEditor;

	public float displayTime;

	public bool testPlay;

	public SlideShowMenuScene(Menu menu, MenuObject owner, SceneID sceneID)
		: base(menu, owner, sceneID)
	{
		cameraMovementPoints = new List<Vector3>();
		depths = new List<float>();
		string path = ((!(cameraFile != "")) ? AssetManager.ResolveFilePath(sceneFolder + Path.DirectorySeparatorChar + "camera.txt") : AssetManager.ResolveFilePath(sceneFolder + Path.DirectorySeparatorChar + cameraFile));
		if (sceneFolder != "" && File.Exists(path))
		{
			string[] array = File.ReadAllLines(path);
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 item = new Vector3
				{
					x = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ","), ", ")[0], NumberStyles.Any, CultureInfo.InvariantCulture),
					y = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ","), ", ")[1], NumberStyles.Any, CultureInfo.InvariantCulture),
					z = float.Parse(Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ","), ", ")[2], NumberStyles.Any, CultureInfo.InvariantCulture)
				};
				cameraMovementPoints.Add(item);
			}
		}
		else
		{
			cameraMovementPoints.Add(default(Vector2));
			depths.Add(0f);
		}
		camPos = CameraPosition(0f);
		lastCamPos = camPos;
		focus = DepthPosition(0f);
		lastFocus = focus;
		ApplySceneSpecificAlphas();
		for (int j = 0; j < depthIllustrations.Count; j++)
		{
			depthIllustrations[j].lastAlpha = depthIllustrations[j].alpha;
		}
	}

	public override void Update()
	{
		if (hidden)
		{
			return;
		}
		UpdateCrossfade();
		for (int i = 0; i < subObjects.Count; i++)
		{
			subObjects[i].Update();
		}
		if (menu.manager.rainWorld.setup.devToolsActive || ModManager.DevTools)
		{
			Vector2 vector = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			if (dragIllustration == null)
			{
				if (Input.GetKey("n"))
				{
					dragIllustration = null;
					float num = float.MaxValue;
					for (int num2 = depthIllustrations.Count - 1; num2 >= 0; num2--)
					{
						if (depthIllustrations[num2].sprite.alpha > 0.2f)
						{
							float num3 = depthIllustrations[num2].DepthAtPosition(menu.mousePosition, devtool: true);
							if (num3 > -1f && num3 < num)
							{
								dragIllustration = depthIllustrations[num2];
								dragOffset = dragIllustration.pos - vector;
								num = num3;
							}
						}
					}
				}
			}
			else if (Input.GetKey("n"))
			{
				dragIllustration.pos = vector + dragOffset;
			}
			else
			{
				dragIllustration = null;
			}
			if (Input.GetKey("b") && !saveButton)
			{
				SaveToFile();
			}
			saveButton = Input.GetKey("b");
		}
		if (testPlay)
		{
			displayTime = Mathf.Min(1f, displayTime + 0.0035714286f);
			if (displayTime == 1f)
			{
				testPlay = false;
			}
		}
		else if (menu is SlideShow)
		{
			displayTime = (menu as SlideShow).inSceneTime;
		}
		lastFocus = focus;
		lastCamPos = camPos;
		Vector2 vector2 = CameraPosition(displayTime);
		camPos = vector2 / 300f;
		focus = DepthPosition(displayTime);
		if (menu.manager.rainWorld.setup.devToolsActive || ModManager.DevTools)
		{
			if (Input.GetKey("m") && !moveEditorButton)
			{
				if (moveEditor == null)
				{
					moveEditor = new CameraMovementEditor(this);
				}
				else
				{
					moveEditor.WriteAndClear();
					moveEditor = null;
				}
			}
			moveEditorButton = Input.GetKey("m");
			if (Input.GetKey("i") && !testPlayButton)
			{
				testPlay = true;
				displayTime = 0f;
			}
			testPlayButton = Input.GetKey("i");
		}
		if (moveEditor != null)
		{
			moveEditor.Update();
		}
		ApplySceneSpecificAlphas();
	}

	private void ApplySceneSpecificAlphas()
	{
		if (sceneID == SceneID.Intro_6_7_Rain_Drop)
		{
			float num = Mathf.InverseLerp(0.52f, 0.58f, displayTime);
			if (flatMode)
			{
				flatIllustrations[1].setAlpha = num;
				return;
			}
			float p = 0.5f;
			depthIllustrations[0].setAlpha = Mathf.Pow(1f - num, p);
			depthIllustrations[1].setAlpha = Mathf.Pow(num, p);
			depthIllustrations[2].setAlpha = Mathf.Pow(1f - num, p);
			depthIllustrations[3].setAlpha = Mathf.Pow(num, p);
			depthIllustrations[4].setAlpha = Mathf.Pow(num, p);
		}
		else if (sceneID == SceneID.Intro_9_Rainy_Climb)
		{
			if (!flatMode)
			{
				depthIllustrations[depthIllustrations.Count - 1].setAlpha = Mathf.InverseLerp(0.96f, 0.99f, displayTime);
			}
		}
		else if (sceneID == SceneID.Intro_10_Fall)
		{
			if (!flatMode)
			{
				depthIllustrations[depthIllustrations.Count - 1].setAlpha = Mathf.InverseLerp(0.04f, 0.01f, displayTime);
			}
		}
		else if (sceneID == SceneID.Outro_4_Tree)
		{
			if (!flatMode)
			{
				depthIllustrations[depthIllustrations.Count - 2].setAlpha = Mathf.InverseLerp(0.22f, 0.44f, displayTime);
				depthIllustrations[depthIllustrations.Count - 3].setAlpha = Mathf.InverseLerp(0.21f, 0.45f, displayTime);
			}
		}
		else if (sceneID == SceneID.Intro_14_Title)
		{
			flatIllustrations[flatIllustrations.Count - 1].setAlpha = Mathf.InverseLerp(1f, 0.7f, displayTime);
		}
		else if (sceneID == SceneID.Yellow_Intro_A)
		{
			float num = Mathf.InverseLerp(0.51f, 0.55f, displayTime);
			if (flatMode)
			{
				flatIllustrations[1].setAlpha = num;
				return;
			}
			depthIllustrations[depthIllustrations.Count - 2].alpha = Mathf.Pow(num, 1.5f) * 0.65f;
			depthIllustrations[depthIllustrations.Count - 1].alpha = Mathf.Pow(num, 0.5f);
		}
		else if (sceneID == SceneID.Yellow_Intro_B)
		{
			if (!flatMode)
			{
				depthIllustrations[1].alpha = Custom.LerpMap(displayTime, 0.1f, 0.9f, 1f, 0.2f, 0.7f);
				depthIllustrations[2].alpha = Custom.LerpMap(displayTime, 0.1f, 0.5f, 0.5f, 1f, 1.2f);
			}
		}
		else if (sceneID == SceneID.Outro_Hunter_3_Embrace && !flatMode)
		{
			depthIllustrations[depthIllustrations.Count - 7].alpha = Mathf.InverseLerp(0.05f, 0.44f, displayTime);
			float num = Mathf.Pow(Custom.SCurve(Mathf.InverseLerp(0.21f, 0.44f, displayTime), 0.65f), 1.5f);
			depthIllustrations[depthIllustrations.Count - 6].alpha = num;
			depthIllustrations[depthIllustrations.Count - 5].alpha = num;
			num = Custom.SCurve(Mathf.InverseLerp(0.21f, 0.45f, displayTime), 0.65f);
			depthIllustrations[depthIllustrations.Count - 4].alpha = num;
			depthIllustrations[depthIllustrations.Count - 2].alpha = num;
			depthIllustrations[depthIllustrations.Count - 1].alpha = 0.2f + 0.8f * Mathf.InverseLerp(0.05f, 0.75f, displayTime);
		}
	}

	public float DisplayTimeToPointCoordinate(float f)
	{
		return f;
	}

	public Vector2 CameraPosition(float f)
	{
		f = DisplayTimeToPointCoordinate(f);
		int num = Math.Min(Mathf.FloorToInt(f * (float)(cameraMovementPoints.Count - 1)), cameraMovementPoints.Count - 1);
		int num2 = Math.Min(num + 1, cameraMovementPoints.Count - 1);
		float f2 = Mathf.InverseLerp(num, num2, f * (float)(cameraMovementPoints.Count - 1));
		return PosBetweenSegments(num, num2, f2);
	}

	private Vector2 PosBetweenSegments(int firstSegment, int nextSegment, float f)
	{
		if (firstSegment == nextSegment)
		{
			return cameraMovementPoints[firstSegment];
		}
		Vector2 vector = cameraMovementPoints[firstSegment];
		Vector2 vector2 = cameraMovementPoints[nextSegment];
		Vector2 cA = vector - DirectionOfSegment(firstSegment) * Vector2.Distance(vector, vector2) / 3f;
		Vector2 cB = vector2 + DirectionOfSegment(nextSegment) * Vector2.Distance(vector, vector2) / 3f;
		return Custom.Bezier(vector, cA, vector2, cB, f);
	}

	private Vector2 DirectionOfSegment(int i)
	{
		if (i == 0 || i == cameraMovementPoints.Count - 1)
		{
			return new Vector2(0f, 0f);
		}
		return ((Vector2)(cameraMovementPoints[i - 1] - cameraMovementPoints[i] + (cameraMovementPoints[i] - cameraMovementPoints[i + 1]))).normalized;
	}

	public float DepthPosition(float f)
	{
		f = DisplayTimeToPointCoordinate(f);
		int num = Math.Min(Mathf.FloorToInt(f * (float)(cameraMovementPoints.Count - 1)), cameraMovementPoints.Count - 1);
		int i = Math.Min(num + 1, cameraMovementPoints.Count - 1);
		while (num > 0 && cameraMovementPoints[num].z == -1f)
		{
			num--;
		}
		for (; i < cameraMovementPoints.Count - 1 && cameraMovementPoints[i].z == -1f; i++)
		{
		}
		return Mathf.Lerp(cameraMovementPoints[num].z, cameraMovementPoints[i].z, Mathf.InverseLerp(num, i, f * (float)(cameraMovementPoints.Count - 1)));
	}

	protected override void SaveToFile()
	{
		if (moveEditor == null)
		{
			base.SaveToFile();
			return;
		}
		string text = "";
		for (int i = 0; i < cameraMovementPoints.Count; i++)
		{
			text = text + cameraMovementPoints[i].x + ", " + cameraMovementPoints[i].y + ", " + cameraMovementPoints[i].z + "\r\n";
		}
		string text2 = AssetManager.ResolveDirectory(sceneFolder);
		if (cameraFile == "")
		{
			using (StreamWriter streamWriter = File.CreateText((text2 + Path.DirectorySeparatorChar + "camera.txt").ToLowerInvariant()))
			{
				streamWriter.Write(text);
				return;
			}
		}
		using StreamWriter streamWriter2 = File.CreateText((text2 + Path.DirectorySeparatorChar + cameraFile).ToLowerInvariant());
		streamWriter2.Write(text);
	}
}
