﻿using Sandbox;
using System;
using System.Linq;

partial class DeathmatchPlayer
{
	// TODO - make ragdolls one per entity
	// TODO - make ragdolls dissapear after a load of seconds
	static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 20 };

	[ClientRpc]
	void BecomeRagdollOnClient( Vector3 force, int forceBone )
	{
		// TODO - lets not make everyone write this shit out all the time
		// maybe a CreateRagdoll<T>() on ModelEntity?
		var ent = new ModelEntity();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.SetInteractsAs( CollisionLayer.Debris );
		ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.TakeDecalsFrom( this );
		ent.SetRagdollVelocityFrom( this );
		ent.DeleteAsync( 20.0f );

		ent.PhysicsGroup.AddVelocity( force );

		if ( forceBone >= 0 )
		{
			var body = ent.GetBonePhysicsBody( forceBone );
			if ( body != null )
			{
				body.ApplyForce( force * 1000 );
			}
			else
			{
				ent.PhysicsGroup.AddVelocity( force );
			}
		}


		Corpse = ent;

		RagdollLimit.Watch( ent );
	}
}
