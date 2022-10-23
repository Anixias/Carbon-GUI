using Godot;

public class NumberFieldInspector : FieldInspector
{
	private double minimum = 0.0;
	private double maximum = 100.0;
	private bool allowLower = true;
	private bool allowHigher = true;
	private double step = 1.0;
	private string prefix = "";
	private string suffix = "";

	public double Minimum { get => minimum; set => minimum = value; }
	public double Maximum { get => maximum; set => maximum = value; }
	public bool AllowLower { get => allowLower; set => allowLower = value; }
	public bool AllowHigher { get => allowHigher; set => allowHigher = value; }
	public double Step { get => step; set => step = value; }
	public string Prefix { get => prefix; set => prefix = value; }
	public string Suffix { get => suffix; set => suffix = value; }

	public override void UpdateState()
	{

	}

	public override void _Ready()
	{

	}

	protected override void FieldChanged()
	{

	}
}
