namespace IBot
{
    public enum ConnectionState : byte
    {
        Disconnected,
        Connecting,
        ConnectRequested,
        WorldInfoRequested,
        GetSectionRequested,
        SpawnRequested,
        Connected
    }

    public enum ClientVersion : int
    {
        Version1436 = 248,
        Version143 = 242,
        Version1423 = 238,
        Version1422 = 237,
        Version1412 = 234,
        Version1411 = 233,
        Version1405 = 230,
        Version14 = 225,
        Version144 = 269,
        Version1447 = 276,
        //Version135 = 190,
    }

    public enum ReadItemOrder : byte
    {
        IdPrefixStack, StackPrefixId
    }

    public enum Difficulty : byte
    {
        Softcore = 0,
        Mediumcore = 1,
        Hardcore = 2,
        ExtraAccessory = 4,
        Creative = 8
    }

    public enum TorchFlag : byte
    {
        None = 0,
        UsingBiomeTorches = 1,
        HappyFunTorchTime = 2,
        UnlockedBiomeTorches = 4
    }

    public enum Team : byte
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 4,
        Purple = 5
    }

    public enum Control : byte
    {
        None = 0,
        ControlUp = 1,
        ControlDown = 2,
        ControlLeft = 4,
        ControlRight = 8,
        ControlJump = 16,
        ControlUseItem = 32,
        Direction = 64
    }

    public enum Pulley : byte
    {
        None = 0,
        Enabled = 1,
        Direction = 2,
        UpdateVelocity = 4,
        VortexStealthActive = 8,
        GravityDirection = 16,
        ShieldRaised = 32
    }

    public enum Miscs : byte
    {
        None = 0,
        HoveringUp = 1,
        VoidVaultEnabled = 2,
        Sitting = 4,
        DownedDD2Event = 8,
        IsPettingAnimal = 16,
        IsPettingSmallAnimal = 32,
        UsedPotionofReturn = 64,
        HoveringDown = 128
    }
}
