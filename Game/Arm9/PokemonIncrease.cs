namespace PlatinumSharp.Game;

public sealed partial class Pokemon
{
    private const int IncreaseParamCurHP = 0xA3;
    private const int IncreasePartyFirst = 0xA0;
    private const int IncreasePartyLast = 0xAA;
    private const int PartyMaxHPOffset = 0x90;
    private const int IncreaseExpMaxLevel = 100;
    private const int MaxPPUpsValue = 3;
    private const int MaxIVValue = 0x1F;

    // OFFSET 0x0207536C
    public static void Pokemon_IncreaseValue(Pokemon mon, int param, int value)
    {
        if ((mon.U16(FlagsOffset) & FlagPartyDecrypted) == 0)
        {
            Pokemon_DecryptData(mon, PartyOffset, PartySize, mon.U32(0));
            Pokemon_DecryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
            ushort checksum = Pokemon_GetDataChecksum(mon, BlocksOffset, BlocksSize);
            if (checksum != mon.U16(ChecksumOffset))
            {
                if (checksum != mon.U16(ChecksumOffset))
                {
                    ErrorHandling.GF_AssertFail();
                }
                Pokemon_EncryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
                return;
            }
        }
        Pokemon_IncreaseValueInternal(mon, param, value);
        if ((mon.U16(FlagsOffset) & FlagPartyDecrypted) == 0)
        {
            Pokemon_EncryptData(mon, PartyOffset, PartySize, mon.U32(0));
            mon.W16(ChecksumOffset, Pokemon_GetDataChecksum(mon, BlocksOffset, BlocksSize));
            Pokemon_EncryptData(mon, BlocksOffset, BlocksSize, mon.U16(ChecksumOffset));
        }
    }

    // OFFSET 0x020753F4
    private static void Pokemon_IncreaseValueInternal(Pokemon mon, int param, int value)
    {
        if (param >= IncreasePartyFirst && param <= IncreasePartyLast)
        {
            if (param == IncreaseParamCurHP)
            {
                int maxHP = mon.U16(PartyMaxHPOffset);
                int hp = mon.U16(PartyHPOffset) + value;
                mon.W16(PartyHPOffset, hp > maxHP ? maxHP : hp);
                return;
            }
            ErrorHandling.GF_AssertFail();
            return;
        }
        BoxPokemon_IncreaseValueInternal(mon.Box, param, value);
    }

    // OFFSET 0x02075454
    private static void BoxPokemon_IncreaseValueInternal(BoxPokemon boxMon, int param, int value)
    {
        uint personality = boxMon.U32(0);
        int a = BoxPokemon_GetDataBlock(boxMon, personality, BlockA);
        int b = BoxPokemon_GetDataBlock(boxMon, personality, BlockB);
        BoxPokemon_GetDataBlock(boxMon, personality, 2);
        BoxPokemon_GetDataBlock(boxMon, personality, 3);
        if (param == 8)
        {
            uint exp = boxMon.U32(a + 8) + (uint)value;
            uint maxExp = Species_GetExpForLevel(boxMon.U16(a), IncreaseExpMaxLevel);
            boxMon.W32(a + 8, exp > maxExp ? Species_GetExpForLevel(boxMon.U16(a), IncreaseExpMaxLevel) : exp);
            return;
        }
        if (param == 9)
        {
            int friendship = boxMon.U8(a + 0xC);
            if (friendship + value > 0xFF)
            {
                friendship = 0xFF;
            }
            friendship += value;
            if (friendship < 0)
            {
                friendship = 0;
            }
            boxMon.W8(a + 0xC, (byte)friendship);
            return;
        }
        if (param >= 0xD && param <= 0x12)
        {
            int offset = a + 0x10 + (param - 0xD);
            boxMon.W8(offset, (byte)(boxMon.U8(offset) + value));
            return;
        }
        if (param >= 0x13 && param <= 0x18)
        {
            int offset = a + 0x16 + (param - 0x13);
            int stat = boxMon.U8(offset) + value;
            boxMon.W8(offset, (byte)(stat > 0xFF ? 0xFF : stat));
            return;
        }
        if (param >= 0x3A && param <= 0x3D)
        {
            int slot = param - 0x3A;
            int pp = boxMon.U8(b + 8 + slot) + value;
            int maxPP = (int)MoveTable.MoveTable_CalcMaxPP(boxMon.U16(b + slot * 2), boxMon.U8(b + 0xC + slot));
            if (pp > maxPP)
            {
                boxMon.W8(b + 8 + slot, (byte)MoveTable.MoveTable_CalcMaxPP(boxMon.U16(b + slot * 2), boxMon.U8(b + 0xC + slot)));
            }
            else
            {
                boxMon.W8(b + 8 + slot, (byte)pp);
            }
            return;
        }
        if (param >= 0x3E && param <= 0x41)
        {
            int slot = param - 0x3E;
            int ups = boxMon.U8(b + 0xC + slot) + value;
            boxMon.W8(b + 0xC + slot, (byte)(ups > MaxPPUpsValue ? MaxPPUpsValue : ups));
            return;
        }
        if (param >= 0x42 && param <= 0x45)
        {
            return;
        }
        if (param >= 0x46 && param <= 0x4B)
        {
            int shift = (param - 0x46) * 5;
            uint ivs = boxMon.U32(b + 0x10);
            uint iv = ((ivs >> shift) & 0x1F) + (uint)value;
            if (iv > MaxIVValue)
            {
                iv = MaxIVValue;
            }
            boxMon.W32(b + 0x10, (ivs & ~(0x1Fu << shift)) | ((iv & 0x1F) << shift));
            return;
        }
        ErrorHandling.GF_AssertFail();
    }
}
