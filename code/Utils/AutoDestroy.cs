
/// <summary>
/// Destrói o GameObject após <see cref="Delay"/> segundos.
/// Usado em hit effects para não acumular objetos na cena.
/// </summary>
public sealed class AutoDestroy : Component
{
	[Property] public float Delay { get; set; } = 3f;

	private float _elapsed;

	protected override void OnUpdate()
	{
		_elapsed += Time.Delta;
		if ( _elapsed >= Delay )
			GameObject.Destroy();
	}
}
