namespace powerplant_coding_challenge.Models;

public class Powerplant
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Efficiency { get; set; }
    public double Pmin { get; set; }
    public double Pmax { get; set; }
}