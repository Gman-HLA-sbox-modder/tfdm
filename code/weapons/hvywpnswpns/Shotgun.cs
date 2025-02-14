﻿using Sandbox;


[Library( "tfdm_shotgun", Title = "Shotgun" )]

partial class Shotgun : BaseDmWeapon
{ 
	public override string ViewModelPath => "models/hvwpns/hvywpnswpns/c_shotgun.vmdl";
	public override float PrimaryRate => 1;
	public override float SecondaryRate => 1;
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int ClipSize => 6;
	public override float ReloadTime => 0.5f;
	public override int Bucket => 1;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/weapons/hvywpnswpns/w_shotgun.vmdl" );  

		AmmoClip = 6;

		FinishReload();
	}

	public override void AttackPrimary() 
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		(Owner as AnimEntity).SetAnimBool( "b_attack", true );

		PlaySound("shotgun_shoot");

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();

		if (AmmoClip == 0) 
		{
			Reload();
		}

		//
		// Shoot the bullets
		//
		for ( int i = 0; i < 10; i++ )
		{
			ShootBullet( 0.15f, 0.3f, 9.0f, 3.0f );
		}
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimBool( "fire", true );

		if ( IsLocalPawn )
		{
			new Sandbox.ScreenShake.Perlin(1.0f, 1.5f, 2.0f);
		}

		CrosshairPanel?.CreateEvent( "fire" );
	}

	public override void OnReloadFinish()
	{
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( AmmoClip >= ClipSize )
			return;

		if ( Owner is DeathmatchPlayer player )
		{
			var ammo = player.TakeAmmo( AmmoType, 1 );
			if ( ammo == 0 )
				return;

			AmmoClip += ammo;

			if ( AmmoClip < ClipSize )
			{
				Reload();
			}
			else
			{
				FinishReload();
				(Owner as AnimEntity).SetAnimBool("b_finishreload", true);
			}
		}
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimBool( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetParam( "holdtype", 1 ); // TODO this is shit
		anim.SetParam( "aimat_weight", 1.0f );
	}
}
