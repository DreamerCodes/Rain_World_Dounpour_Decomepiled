using System;
using RWCustom;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace CoralBrain;

public class Mycelium : IDisposable
{
	public struct MyceliaConnection : IEquatable<MyceliaConnection>
	{
		public Mycelium A;

		public Mycelium B;

		public bool Equals(MyceliaConnection other)
		{
			if (object.Equals(A, other.A))
			{
				return object.Equals(B, other.B);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is MyceliaConnection other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (((A != null) ? A.GetHashCode() : 0) * 397) ^ ((B != null) ? B.GetHashCode() : 0);
		}

		public static bool operator ==(MyceliaConnection left, MyceliaConnection right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(MyceliaConnection left, MyceliaConnection right)
		{
			return !left.Equals(right);
		}

		public MyceliaConnection(Mycelium A, Mycelium B)
		{
			this.A = A;
			this.B = B;
		}

		public Mycelium Other(Mycelium me)
		{
			if (me == A)
			{
				return B;
			}
			return A;
		}
	}

	public Vector2[,] points;

	public float conRad;

	public Color color;

	public CoralNeuronSystem system;

	public IOwnMycelia owner;

	public int index;

	public float length;

	public MyceliaConnection connection;

	public int lastCameraCullTick;

	public bool viewedByCamera;

	public bool useStaticCulling;

	public bool culled;

	public bool lastCulled;

	public bool moveAwayFromWalls = true;

	public int rest;

	private JobData _jobDataStep1;

	private JobHandle handleJob01;

	public Vector2 Tip => points[points.GetLength(0) - 1, 0];

	public Vector2 Base => points[0, 0];

	public void Dispose()
	{
		_jobDataStep1.naFourDirections.Dispose();
		_jobDataStep1.nativePoints.Dispose();
		_jobDataStep1.outPoints.Dispose();
	}

	public Mycelium(CoralNeuronSystem system, IOwnMycelia owner, int index, float length, Vector2 initPoint)
	{
		lastCameraCullTick = -1;
		useStaticCulling = true;
		this.system = system;
		system?.mycelia.Add(this);
		this.owner = owner;
		this.index = index;
		this.length = length;
		float num = Mathf.Max(length, Mathf.Lerp(length, 40f, 0.5f));
		points = new Vector2[Custom.IntClamp((int)(num / 15f), 2, 20), 3];
		conRad = length / (float)points.GetLength(0);
		Reset(initPoint);
		_jobDataStep1 = default(JobData);
		_jobDataStep1.naFourDirections = new NativeArray<int2>(4, Allocator.Persistent);
		_jobDataStep1.naFourDirections[0] = new int2(-1, 0);
		_jobDataStep1.naFourDirections[1] = new int2(0, -1);
		_jobDataStep1.naFourDirections[2] = new int2(1, 0);
		_jobDataStep1.naFourDirections[3] = new int2(0, 1);
		_jobDataStep1.nativePoints = new NativeArray<float2>(points.GetLength(0) * 3, Allocator.Persistent);
		_jobDataStep1.outPoints = new NativeArray<Vector2>(points.GetLength(0) * 3, Allocator.Persistent);
	}

	public void ConnectSystem(CoralNeuronSystem newSystem)
	{
		system = newSystem;
		newSystem?.mycelia.Add(this);
	}

	public void Reset(Vector2 resetPos)
	{
		Vector2 vector = resetPos + length * 0.6f * owner.ResetDir(index);
		Vector2 cA = resetPos + (Custom.DirVec(resetPos, vector) + Custom.RNV()).normalized * Vector2.Distance(resetPos, vector) * 0.5f;
		Vector2 cB = vector + (Custom.DirVec(vector, resetPos) + Custom.RNV()).normalized * Vector2.Distance(resetPos, vector) * 0.5f;
		for (int i = 0; i < points.GetLength(0); i++)
		{
			points[i, 0] = Custom.Bezier(resetPos, cA, vector, cB, (float)i / (float)(points.GetLength(0) - 1)) + Custom.RNV();
			points[i, 1] = points[i, 0];
			points[i, 2] = Custom.RNV();
		}
	}

	public void Update()
	{
		if (useStaticCulling)
		{
			culled = (system != null && system.Frozen) || !viewedByCamera;
		}
		else
		{
			culled = system != null && system.Frozen;
		}
		if (lastCameraCullTick != owner.OwnerRoom.camerasChangedTick)
		{
			viewedByCamera = owner.OwnerRoom.ViewedByAnyCamera(Base, length + 50f);
			lastCameraCullTick = owner.OwnerRoom.camerasChangedTick;
		}
		if (lastCulled && !culled)
		{
			Reset(owner.ConnectionPos(index, 1f));
		}
		lastCulled = culled;
		if (culled || owner.OwnerRoom.aimap == null)
		{
			return;
		}
		int num = points.GetLength(0);
		ExtraExtentions.Indexer indexer = default(ExtraExtentions.Indexer);
		indexer.width = 3;
		ExtraExtentions.Indexer indexer2 = indexer;
		for (int i = 0; i < num && indexer2.ind(i, 2) < points.Length; i++)
		{
			float2 @float = new float2(points[i, 0].x, points[i, 0].y);
			float2 float2 = new float2(points[i, 2].x, points[i, 2].y);
			_jobDataStep1.nativePoints[indexer2.ind(i, 0)] = @float + float2;
			_jobDataStep1.nativePoints[indexer2.ind(i, 1)] = @float;
			_jobDataStep1.nativePoints[indexer2.ind(i, 2)] = float2 * 0.999f;
		}
		Phase1(num);
		Phase4();
		if (owner != null)
		{
			points[0, 0] = owner.ConnectionPos(index, 1f);
			points[0, 2] *= 0f;
		}
		if (rest > 0)
		{
			rest--;
		}
		if (connection != default(MyceliaConnection))
		{
			if (connection.Other(this).connection != connection || !Custom.DistLess(connection.A.Base, connection.B.Base, connection.A.length + connection.B.length) || UnityEngine.Random.value < 0.005f)
			{
				connection = default(MyceliaConnection);
				rest = UnityEngine.Random.Range(20, 200);
				return;
			}
			Mycelium mycelium = connection.Other(this);
			if (Custom.DistLess(mycelium.Tip, Tip, 10f))
			{
				Vector2 vector = Custom.DirVec(Tip, mycelium.Tip);
				float num2 = Vector2.Distance(Tip, mycelium.Tip);
				points[points.GetLength(0) - 1, 0] += vector * (num2 - 1f) * 0.5f;
				points[points.GetLength(0) - 1, 2] += vector * (num2 - 1f) * 0.5f;
				mycelium.points[mycelium.points.GetLength(0) - 1, 0] -= vector * (num2 - 1f) * 0.5f;
				mycelium.points[mycelium.points.GetLength(0) - 1, 2] -= vector * (num2 - 1f) * 0.5f;
				if (UnityEngine.Random.value < 0.05f)
				{
					owner.OwnerRoom.AddObject(new NeuronSpark((mycelium.Tip + Tip) / 2f));
				}
			}
			else
			{
				points[points.GetLength(0) - 1, 2] = Vector2.Lerp(points[points.GetLength(0) - 1, 2], Vector2.ClampMagnitude(mycelium.Tip - Tip, 5f), 0.5f);
			}
		}
		else if (system != null && rest < 1 && system.mycelia.Count > 0)
		{
			Mycelium mycelium2 = system.mycelia[UnityEngine.Random.Range(0, system.mycelia.Count)];
			if (mycelium2 != this && mycelium2.owner != owner && mycelium2.connection == default(MyceliaConnection) && Custom.DistLess(Base, mycelium2.Base, (length + mycelium2.length) * 0.75f))
			{
				connection = new MyceliaConnection(this, mycelium2);
				mycelium2.connection = connection;
			}
		}
	}

	private void Phase4()
	{
		_jobDataStep1.outPoints.CopyToAlt(points);
	}

	private void Phase1(int pointsDim0Length)
	{
		MyceliumJob01 jobData = default(MyceliumJob01);
		jobData.pointslen = pointsDim0Length;
		jobData.conRad = conRad;
		jobData.aiMapHeight = owner.OwnerRoom.aimap.height;
		jobData.aiMapWidth = owner.OwnerRoom.aimap.width;
		jobData.moveAwayFromWalls = moveAwayFromWalls;
		jobData.terrainProximity = owner.OwnerRoom.aimap.terrainProximity;
		jobData.points = _jobDataStep1.nativePoints;
		jobData.naFourDirections = _jobDataStep1.naFourDirections;
		jobData.outPoints = _jobDataStep1.outPoints;
		jobData.Run();
	}

	public void InitiateSprites(int spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites[spr] = TriangleMesh.MakeLongMesh(points.GetLength(0), pointyTip: false, customColor: true);
		UpdateColor(color, 0f, spr, sLeaser);
	}

	public void DrawSprites(int spr, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[spr].isVisible = !culled;
		if (!culled)
		{
			Vector2 vector = Vector2.Lerp(points[0, 1], points[0, 0], timeStacker);
			if (owner != null)
			{
				vector = owner.ConnectionPos(index, timeStacker);
			}
			float num = 0.5f;
			for (int i = 0; i < points.GetLength(0); i++)
			{
				Vector2 vector2 = Vector2.Lerp(points[i, 1], points[i, 0], timeStacker);
				Vector2 normalized = (vector - vector2).normalized;
				Vector2 vector3 = Custom.PerpendicularVector(normalized);
				float num2 = Vector2.Distance(vector, vector2) / 5f;
				(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4, vector - normalized * num2 - vector3 * num - camPos);
				(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 1, vector - normalized * num2 + vector3 * num - camPos);
				(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 + normalized * num2 - vector3 * num - camPos);
				(sLeaser.sprites[spr] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + normalized * num2 + vector3 * num - camPos);
				vector = vector2;
			}
		}
	}

	public void UpdateColor(Color newColor, float gradientStart, int spr, RoomCamera.SpriteLeaser sLeaser)
	{
		color = newColor;
		for (int i = 0; i < (sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length; i++)
		{
			float value = (float)i / (float)((sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length - 1);
			(sLeaser.sprites[spr] as TriangleMesh).verticeColors[i] = Color.Lerp(color, Custom.HSL2RGB(22f / 45f, 0.5f, 0.2f), Mathf.InverseLerp(gradientStart, 1f, value));
		}
		for (int j = 1; j < 3; j++)
		{
			(sLeaser.sprites[spr] as TriangleMesh).verticeColors[(sLeaser.sprites[spr] as TriangleMesh).verticeColors.Length - j] = new Color(0f, 0f, 1f);
		}
	}
}
