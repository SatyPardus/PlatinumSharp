using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class BattleSubscreens
{
    private const int PartySize = 6;
    private const uint PlayerBallTemplate = 0x02270414;
    private const uint OpponentBallTemplate = 0x02270448;
    private const int SpriteTemplateSize = 0x34;
    private const int ResourcePlayerBallTiles = 0x4E37;
    private const int ResourcePlayerBallAnim = 0x4E2F;
    private const int ResourceOpponentBallTiles = 0x4E38;
    private const int ResourceOpponentBallAnim = 0x4E30;
    private const int MemberPlayerBallTiles = 0xD0;
    private const int MemberPlayerBallCell = 0xCF;
    private const int MemberPlayerBallAnim = 0xD1;
    private const int MemberOpponentBallTiles = 0xCD;
    private const int MemberOpponentBallCell = 0xCC;
    private const int MemberOpponentBallAnim = 0xCE;
    private const int SubscreenYOffset = 0x110000;
    private const int PlayerBallFirstX = 12;
    private const int PlayerBallSpacing = 19;
    private const int PlayerBallY = 13;
    private const int OpponentBallFirstX = 246;
    private const int OpponentBallSpacing = 12;
    private const int OpponentBallY = 9;
    private const uint TaskPriorityPartyBalls = 0x514;
    private const int BallShakeMinPercent = 0x57;
    private const int BallShakeDelay = 10;
    private const int BallShakeRotation = 0x800;
    private const int BallShakeSpeed = 0x180;
    private const int ResourceCursorTiles = 0x4FBB;
    private const int ResourceCursorPalette = 0x4E43;
    private const int ResourceCursorCell = 0x4FB5;
    private const int ResourceCursorAnim = 0x4FAA;

    public static SpriteTemplate RomSpriteTemplate(uint address)
    {
        byte[] raw = RomData.Overlay(16, address, SpriteTemplateSize);
        SpriteTemplate template = new();
        template.X = (short)Nitro.Bin.U16(raw, 0);
        template.Y = (short)Nitro.Bin.U16(raw, 2);
        template.Z = (short)Nitro.Bin.U16(raw, 4);
        template.AnimIdx = (ushort)Nitro.Bin.U16(raw, 6);
        template.Priority = (int)Nitro.Bin.U32(raw, 8);
        template.PlttIdx = (int)Nitro.Bin.U32(raw, 0xC);
        template.VramType = (int)Nitro.Bin.U32(raw, 0x10);
        for (int i = 0; i < template.Resources.Length; i++)
        {
            template.Resources[i] = (int)Nitro.Bin.U32(raw, 0x14 + i * 4);
        }
        template.BgPriority = (int)Nitro.Bin.U32(raw, 0x2C);
        template.VramTransfer = (int)Nitro.Bin.U32(raw, 0x30);
        return template;
    }

    // OFFSET 0x02268D40
    public static void BattleSubscreen_CreatePartyBalls(NARC subscreenNarc, BattleSubscreen subscreen)
    {
        if (subscreen.PlayerPartyBallSprites[0] != null || subscreen.OpponentPartyBallSprites[0] != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        SpriteSystem spriteSys = BattleSystems.BattleSystem_GetSpriteSystem(subscreen.BattleSys!)!;
        SpriteManager spriteMan = BattleSystems.BattleSystem_GetSpriteManager(subscreen.BattleSys!)!;
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, subscreenNarc, MemberPlayerBallTiles, 1, VRamSub, ResourcePlayerBallTiles);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, subscreenNarc, MemberPlayerBallCell, 1, ResourcePlayerBallAnim);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, subscreenNarc, MemberPlayerBallAnim, 1, ResourcePlayerBallAnim);
        SpriteSystem.SpriteSystem_LoadCharResObjFromOpenNarc(spriteSys, spriteMan, subscreenNarc, MemberOpponentBallTiles, 1, VRamSub, ResourceOpponentBallTiles);
        SpriteSystem.SpriteSystem_LoadCellResObjFromOpenNarc(spriteSys, spriteMan, subscreenNarc, MemberOpponentBallCell, 1, ResourceOpponentBallAnim);
        SpriteSystem.SpriteSystem_LoadAnimResObjFromOpenNarc(spriteSys, spriteMan, subscreenNarc, MemberOpponentBallAnim, 1, ResourceOpponentBallAnim);
        for (int i = 0; i < PartySize; i++)
        {
            subscreen.PlayerPartyBallSprites[i] = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, RomSpriteTemplate(PlayerBallTemplate));
            SpriteSystem.Sprite_SetPositionXYWithSubscreenOffset(subscreen.PlayerPartyBallSprites[i]!.Sprite!, (short)(PlayerBallFirstX + PlayerBallSpacing * i), PlayerBallY, SubscreenYOffset);
            SpriteSystem.ManagedSprite_SetAffineOverwriteMode(subscreen.PlayerPartyBallSprites[i]!, 1);
            subscreen.OpponentPartyBallSprites[i] = SpriteSystem.SpriteSystem_NewSprite(spriteSys, spriteMan, RomSpriteTemplate(OpponentBallTemplate));
            SpriteSystem.Sprite_SetPositionXYWithSubscreenOffset(subscreen.OpponentPartyBallSprites[i]!.Sprite!, (short)(OpponentBallFirstX - OpponentBallSpacing * i), OpponentBallY, SubscreenYOffset);
        }
        BattleSubscreen_HidePartyBalls(subscreen);
        if (subscreen.AnimatePartyBallsTask != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        subscreen.AnimatePartyBallsTask = SysTask.SysTask_Start(BattleSubscreen_AnimatePartyBallsTask, subscreen, TaskPriorityPartyBalls);
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(subscreen.BattleSys!)!;
        BattleCursors.BattleCursor_LoadResources(spriteSys, spriteMan, paletteData, HeapIDBattle, ResourceCursorTiles, ResourceCursorPalette, ResourceCursorCell, ResourceCursorAnim);
        subscreen.CursorRenderer = BattleCursors.BattleCursor_New(spriteSys, spriteMan, HeapIDBattle, ResourceCursorTiles, ResourceCursorPalette, ResourceCursorCell, ResourceCursorAnim, 5, 0);
    }

    // OFFSET 0x02268FCC
    private static void BattleSubscreen_AnimatePartyBallsTask(SysTask task, object? param)
    {
        BattleSubscreen subscreen = (BattleSubscreen)param!;
        for (int i = 0; i < PartySize; i++)
        {
            PartyBallAnimState state = subscreen.PartyBallAnimState[i];
            ManagedSprite sprite = subscreen.PlayerPartyBallSprites[i]!;
            int baseX = PlayerBallFirstX + PlayerBallSpacing * i;
            switch (state.Phase)
            {
                case 0:
                    if (SpriteSystem.Sprite_GetDrawFlag2(sprite.Sprite!) != 0 && state.ExpPercent >= BallShakeMinPercent)
                    {
                        state.Unused04 = 0;
                        state.Delay = BallShakeDelay;
                        state.Delay += (byte)(MathUtil.LCRNG_Next() % 8);
                        state.Duration = 2;
                        state.Phase++;
                    }
                    break;
                case 1:
                    if (state.Delay != 0)
                    {
                        state.Delay--;
                    }
                    else
                    {
                        state.Phase++;
                    }
                    break;
                case 2:
                case 4:
                    SpriteSystem.ManagedSprite_OffsetAffineZRotation(sprite, BallShakeRotation);
                    state.XOffset += BallShakeSpeed;
                    SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(sprite, (short)(state.XOffset / 256 + baseX), PlayerBallY, SubscreenYOffset);
                    state.FrameCount++;
                    if (state.FrameCount >= state.Duration)
                    {
                        state.FrameCount = 0;
                        if (state.Phase == 2)
                        {
                            state.Phase++;
                        }
                        else
                        {
                            SpriteSystem.ManagedSprite_SetAffineZRotation(sprite, 0);
                            SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(sprite, (short)baseX, PlayerBallY, SubscreenYOffset);
                            state.XOffset = 0;
                            if (state.BounceCount < 1)
                            {
                                state.BounceCount++;
                                if (state.Duration > 1)
                                {
                                    state.Duration--;
                                }
                                state.Phase = 2;
                            }
                            else
                            {
                                state.BounceCount = 0;
                                state.Phase = 0;
                            }
                        }
                    }
                    break;
                case 3:
                    SpriteSystem.ManagedSprite_OffsetAffineZRotation(sprite, -BallShakeRotation);
                    state.XOffset -= BallShakeSpeed;
                    SpriteSystem.ManagedSprite_SetPositionXYWithSubscreenOffset(sprite, (short)(state.XOffset / 256 + baseX), PlayerBallY, SubscreenYOffset);
                    state.FrameCount++;
                    if (state.FrameCount >= state.Duration * 2)
                    {
                        state.FrameCount = 0;
                        state.Phase++;
                    }
                    break;
            }
        }
    }

    // OFFSET 0x0226914C
    public static void BattleSubscreen_SetPartyBallExpPercents(BattleSubscreen subscreen, byte[] percents)
    {
        for (int i = 0; i < PartySize; i++)
        {
            subscreen.PartyBallAnimState[i].ExpPercent = percents[i];
        }
    }

    // OFFSET 0x02269168
    public static void BattleSubscreen_SetPartyBallStatuses(BattleSubscreen subscreen, byte[] playerStatuses, byte[] opponentStatuses)
    {
        for (int i = 0; i < PartySize; i++)
        {
            Sprite.Sprite_SetAnim(subscreen.PlayerPartyBallSprites[i]!.Sprite!, (uint)PartyBallStatus_GetAnim(playerStatuses[i]));
            SpriteSystem.Sprite_TickFrame(subscreen.PlayerPartyBallSprites[i]!.Sprite!);
            Sprite.Sprite_SetAnim(subscreen.OpponentPartyBallSprites[i]!.Sprite!, (uint)PartyBallStatus_GetAnim(opponentStatuses[i]));
            SpriteSystem.Sprite_TickFrame(subscreen.OpponentPartyBallSprites[i]!.Sprite!);
        }
    }

    // OFFSET 0x022691BC
    public static void BattleSubscreen_ShowPartyBalls(BattleSubscreen subscreen)
    {
        if (subscreen.PlayerPartyBallSprites[0] == null || subscreen.OpponentPartyBallSprites[0] == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < PartySize; i++)
        {
            SpriteSystem.Sprite_SetDrawFlag2(subscreen.PlayerPartyBallSprites[i]!.Sprite!, 1);
        }
        if ((BattleSystems.BattleSystem_GetBattleType(subscreen.BattleSys!) & BattleSystems.BattleTypeTrainer) != 0)
        {
            for (int i = 0; i < PartySize; i++)
            {
                SpriteSystem.Sprite_SetDrawFlag2(subscreen.OpponentPartyBallSprites[i]!.Sprite!, 1);
            }
        }
    }

    // OFFSET 0x02269218
    public static void BattleSubscreen_HidePartyBalls(BattleSubscreen subscreen)
    {
        if (subscreen.PlayerPartyBallSprites[0] == null || subscreen.OpponentPartyBallSprites[0] == null)
        {
            ErrorHandling.GF_AssertFail();
        }
        for (int i = 0; i < PartySize; i++)
        {
            SpriteSystem.Sprite_SetDrawFlag2(subscreen.PlayerPartyBallSprites[i]!.Sprite!, 0);
            SpriteSystem.Sprite_SetDrawFlag2(subscreen.OpponentPartyBallSprites[i]!.Sprite!, 0);
        }
    }

    // OFFSET 0x0226A934
    private static int PartyBallStatus_GetAnim(int status)
    {
        switch (status)
        {
            case 1:
                return 1;
            case 2:
                return 3;
            case 3:
                return 2;
        }
        return 0;
    }
}
