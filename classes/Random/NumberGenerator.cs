namespace GodotEGP.Random;

using Godot;

public partial class NumberGenerator : RandomNumberGenerator
{
	private ulong _initialSeed { get; set; }
	private ulong _initialState { get; set; }

	public NumberGenerator(ulong seed = 0, ulong state = 0)
	{
		_initialSeed = seed;
		_initialState = state;

		// set the seed we provided
		Seed = seed;

		// either set restored state, or randomize the instance
		if (state != 0)
		{
			State = state;
		}
		else
		{
			Randomize();
		}
	}

	// override to return double instead of float
	public double Randf()
	{
		return base.Randf();
	}

	// override to return double instead of float
	public double RandfRange(double from, double to)
	{
		return base.RandfRange((float) from, (float) to);
	}
}
