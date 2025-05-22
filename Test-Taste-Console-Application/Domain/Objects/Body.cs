using System.Collections.Generic;

public class Body
{
    public string Id { get; set; }
    public string EnglishName { get; set; }
    public bool IsPlanet { get; set; }
    public List<MoonReference> Moons { get; set; }
    public PlanetReference AroundPlanet { get; set; }
    public double? AvgTemp { get; set; }
    public Mass Mass { get; set; }
}

public class MoonReference
{
    public string Moon { get; set; }
    public string Rel { get; set; }
}

public class PlanetReference
{
    public string Planet { get; set; }
}

public class Mass
{
    public double? MassValue { get; set; }
    public int? MassExponent { get; set; }
}
