namespace PCTRTools
{
  internal class Generation
  {
    public enum Gen
    {
      Gen4,
      Gen5,
      Unknown
    }

    public static Gen IdentifyGeneration(NARC msg)
    {
      switch (msg.Files.Count)
      {
        case 610: // DP
        case 624: // DP_USA
        case 709: // Pt
        case 723: // Pt_USA
        case 814: // HGSS
        case 828: // HGSS_USA
          return Gen.Gen4;
        case 273:// BW a002
        case 472:// BW a003
        case 480:// BW2 a002
        case 676:// BW2 a003
          return Gen.Gen5;
        default:
          return Gen.Unknown;
      }
    }
  }
}
