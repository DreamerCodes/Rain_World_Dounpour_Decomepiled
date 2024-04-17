using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class OracleCutsceneBehavior : OracleBehavior
{
	public override Vector2 GetToDir => Custom.DegToVec(90f);

	public override Vector2 OracleGetToPos => oracle.room.MiddleOfTile(oracle.abstractPhysicalObject.pos);

	public OracleCutsceneBehavior(Oracle oracle)
		: base(oracle)
	{
		base.oracle.setGravity(0.9f);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		oracle.bodyChunks[1].vel *= 0.4f;
		oracle.bodyChunks[0].vel *= 0.4f;
		oracle.bodyChunks[0].vel += Vector2.ClampMagnitude(OracleGetToPos - oracle.bodyChunks[0].pos, 100f) / 100f * 6.2f;
		oracle.bodyChunks[1].vel += Vector2.ClampMagnitude(OracleGetToPos - GetToDir * oracle.bodyChunkConnections[0].distance - oracle.bodyChunks[0].pos, 100f) / 100f * 3.2f;
	}
}
