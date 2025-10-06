namespace BlazorLoginDemo.Shared.Models.Static;

public static class AcrissType
{
    public static readonly (char code, string label)[] CategoryOptions =
    [
        ('M', "Mini"),
        ('M', "Mini Elite"),
        ('E', "Economy"),
        ('H', "Economy Elite"),
        ('C', "Compact"),
        ('D', "Compact Elite"),
        ('I', "Intermediate"),
        ('J', "Intermediate Elite"),
        ('S', "Standard"),
        ('R', "Standard Elite"),
        ('F', "Full Size"),
        ('G', "Full Size Elite"),
        ('P', "Premium"),
        ('U', "Premium Elite"),
        ('L', "Luxury"),
        ('W', "Luxury Elite"),
        ('O', "Oversize"),
        ('X',"Special")
    ];

    public static readonly (char code, string label)[] BodyOptions =
    [
        ('B', "2-3 Door"),
        ('C', "2/4 Door"),
        ('D', "4-5 Door"),
        ('W', "Wagon/Estate"),
        ('V', "Passenger Van"),
        ('L', "Limousine/Sedan"),
        ('S', "Sport"),
        ('T', "Convertible"),
        ('F', "SUV"),
        ('J', "Open Air All Terrain"),
        ('X', "Special"),
        ('P', "Pick up Regular Cab"),
        ('Q', "Pick up Extended Cab"),
        ('Z', "Special Offer Car"),
        ('E', "Coup"),
        ('M', "MPV/Monospace"),
        ('R', "Recreational Vehicle"),
        ('H', "Motor Home"),
        ('Y', "2 Wheel Vehicle"),
        ('N', "Roadster"),
        ('G', "Crossover"),
        ('K', "Commercial Van/Truck")
    ];

    public static readonly (char code, string label)[] TransmissionOptions =
    [
        ('M', "Manual"),
        ('A', "Automatic"),
        ('N', "Manual 4WD"),
        ('C', "Manual AWD"),
        ('B', "Automatic 4WD"),
        ('D', "Automatic AWD")
    ];

    public static readonly (char code, string label)[] FuelOptions =
    [
        ('R', "Unspecified Power with Air"),
        ('U', "Unspecified Power no Air"),
        ('D', "Diesel Air"),
        ('Q', "Diesel no Air"),
        ('H', "Hybrid Air"),
        ('I', "Hybrid Plug in Air"),
        ('E', "Electric"),
        ('C', "Electric"),
        ('L', "LPG Power with Air"),
        ('S', "LPG Power no Air"),
        ('A', "Hydrogen Air"),
        ('B', "Hydrogen no Air"),
        ('M', "Flex Fuel Power Air"),
        ('F', "Flex Fuel no Air"),
        ('V', "Petrol Air"),
        ('Z', "Petrol no Air"),
        ('U', "Ethanol Air"),
        ('X', "Ethanol no Air"),
        ('N', "Petrol/Gasoline"),
        ('D', "Diesel"),
        ('H', "Hybrid"),
        ('E', "Electric"),
        ('L', "LPG"),
        ('A', "Hydrogen"),
        ('C', "Flex Fuel")
    ];

    public static class AcrissCodec
    {
        public static string Encode(string cat, string body, string trans, string fuel)
            => new string(new[] {
                GetChar(cat,'M'), GetChar(body,'C'), GetChar(trans,'A'), GetChar(fuel,'R')
            });

        public static (string cat, string body, string trans, string fuel) Decode(
            string? code, (char cat,char body,char trans,char fuel) fallbacks)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 4)
                return (fallbacks.cat.ToString(), fallbacks.body.ToString(),
                        fallbacks.trans.ToString(), fallbacks.fuel.ToString());

            return (code[0].ToString(), code[1].ToString(), code[2].ToString(), code[3].ToString());
        }

        static char GetChar(string? s, char fallback)
            => string.IsNullOrEmpty(s) ? fallback : s![0];
    }
}
