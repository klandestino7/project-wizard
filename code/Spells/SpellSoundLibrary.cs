namespace Warlocks;

public static class SpellSoundLibrary
{
	public const string BasicCast = "sounds/spells/warlocks/basic_cast.sound";
	public const string StupefyCast = "sounds/spells/warlocks/stupefy_cast.sound";
	public const string IncendioCast = "sounds/spells/warlocks/incendio_cast.sound";
	public const string ImpedimentaCast = "sounds/spells/warlocks/impedimenta_cast.sound";
	public const string ProtegoCast = "sounds/spells/warlocks/protego_cast.sound";
	public const string ProtegoBlock = "sounds/spells/warlocks/protego_block.sound";
	public const string ApparitionCast = "sounds/spells/warlocks/apparition_cast.sound";
	public const string SectumsempraCast = "sounds/spells/warlocks/sectumsempra_cast.sound";
	public const string EpiskeyCast = "sounds/spells/warlocks/episkey_cast.sound";
	public const string GenericProjectileImpact = "sounds/spells/warlocks/projectile_impact.sound";
	public const string GenericProjectilePassBy = "sounds/spells/warlocks/projectile_passby.sound";

	public static string GetCastSound( BaseSpell spell )
	{
		if ( spell is null )
			return null;

		return spell switch
		{
			BasicCastSpell => BasicCast,
			StupefySpell => StupefyCast,
			IncendioSpell => IncendioCast,
			ImpedimentaSpell => ImpedimentaCast,
			ProtegoSpell => ProtegoCast,
			DashSpell => ApparitionCast,
			SectumsempraSpell => SectumsempraCast,
			EpiskeySpell => EpiskeyCast,
			_ => null
		};
	}

	public static void PlayAtPosition( string soundPath, Vector3 worldPosition )
	{
		if ( string.IsNullOrWhiteSpace( soundPath ) )
			return;

		var handle = Sound.Play( soundPath, worldPosition );
		if ( !handle.IsValid() )
			Log.Warning( $"[SpellSoundLibrary] Failed to play sound '{soundPath}'." );
	}

	public static void PlayLocal( string soundPath )
	{
		if ( string.IsNullOrWhiteSpace( soundPath ) )
			return;

		var handle = Sound.Play( soundPath );
		if ( !handle.IsValid() )
			Log.Warning( $"[SpellSoundLibrary] Failed to play local sound '{soundPath}'." );
	}
}
