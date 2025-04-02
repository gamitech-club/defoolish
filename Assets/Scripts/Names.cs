public static class ActionNames
{
    /// <summary>
    /// Usually for pausing the game.
    /// </summary>
    public const string Cancel = "Cancel";
}

public static class NumpadCodes
{
    public const string Hammer = "010406";
    public const string DefuseGuidePaperV1 = "604010";
    public const string FakeDefuseButton1 = "123222";
    public const string FakeDefuseButton2 = "526321";
    public const string Explode1 = "138225";
    public const string Explode2 = "023777";
    public const string RubberDucky = "042676";
    public const string EasterEgg = "200206";
    public const string MorseCodeCluePaper = "324444";
    public const string MorseCodePaper = "143122";
    public const string IncreaseBombTimer = "924112";
    public const string SecondHammer = "772244";
    public const string FasterBombTimer = "718888";
    public const string Rickroll = "990991";
    public const string Police = "911911";

}

public enum EndingID
{
    None = 0,
    BombExploded = 1,
    NormalEnding = 2,
    MorseCommunityEnding = 3,
    PoliceEnding = 4,
    DestroyEverythingEnding = 5
}
