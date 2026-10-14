using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static partial class BattleAnimScriptFuncs
{
    private const int ScriptFuncCount = 0x54;
    private const int SpriteFuncCount = 0x21;

    private static readonly Action<BattleAnimSystem>[] ScriptFuncs =
    {
        BattleAnimScriptFunc_Nop,
        BattleAnimScriptFunc_AnimExample,
        BattleAnimScriptFunc_SoundExample,
        BattleAnimScriptFunc_GenericExample,
        BattleAnimScriptFunc_RotateMon,
        (system) => throw Missing(0x0222B194),
        (system) => throw Missing(0x0222B45C),
        (system) => throw Missing(0x0222B68C),
        (system) => throw Missing(0x0222F770),
        (system) => throw Missing(0x0222F9E4),
        BattleAnimScriptFunc_Submission,
        (system) => throw Missing(0x022300C4),
        (system) => throw Missing(0x022303D0),
        (system) => throw Missing(0x0222B9E0),
        (system) => throw Missing(0x0222BAE4),
        (system) => throw Missing(0x0222BCF4),
        (system) => throw Missing(0x0222BE48),
        (system) => throw Missing(0x02230754),
        (system) => throw Missing(0x02230804),
        (system) => throw Missing(0x0222BFA8),
        (system) => throw Missing(0x02230A8C),
        (system) => throw Missing(0x0222C0C0),
        (system) => throw Missing(0x0222C3C0),
        (system) => throw Missing(0x02231444),
        (system) => throw Missing(0x0222C5FC),
        (system) => throw Missing(0x02231E7C),
        (system) => throw Missing(0x0222C6D4),
        (system) => throw Missing(0x0222C8F8),
        (system) => throw Missing(0x0222CA2C),
        (system) => throw Missing(0x0223249C),
        (system) => throw Missing(0x022326AC),
        (system) => throw Missing(0x02232024),
        (system) => throw Missing(0x02233178),
        BattleAnimScriptFunc_FadeBg,
        BattleAnimScriptFunc_FadeBattlerSprite,
        BattleAnimScriptFunc_ScalePokemonSprite,
        BattleAnimScriptFunc_Shake,
        (system) => throw Missing(0x02233CD4),
        BattleAnimScriptFunc_AlphaFadePokemonSprite,
        (system) => throw Missing(0x0222CB90),
        BattleAnimScriptFunc_HideBattler,
        (system) => throw Missing(0x02233F30),
        BattleAnimScriptFunc_ScaleBattlerSprite,
        (system) => throw Missing(0x02234214),
        BattleAnimScriptFunc_ScrollCustomBg,
        BattleAnimScriptFunc_MuddyWater,
        (system) => throw Missing(0x022342C4),
        (system) => throw Missing(0x022344D0),
        (system) => throw Missing(0x0223464C),
        (system) => throw Missing(0x0222CFA0),
        BattleAnimScriptFunc_BlinkAttacker,
        BattleAnimScriptFunc_MoveBattlerX,
        BattleAnimScriptFunc_MoveBattlerX2,
        BattleAnimScriptFunc_ShakeAndScaleAttacker,
        BattleAnimScriptFunc_ShakeAndScaleAttacker2,
        (system) => throw Missing(0x0222E1A8),
        BattleAnimScriptFunc_Superpower,
        BattleAnimScriptFunc_MoveBattler,
        (system) => throw Missing(0x0222E810),
        (system) => throw Missing(0x0222EAA0),
        BattleAnimScriptFunc_RevolveBattler,
        BattleAnimScriptFunc_MoveBattlerOffScreen,
        BattleAnimScriptFunc_MoveBattlerToDefaultPos,
        BattleAnimScriptFunc_FadePokemonSprite,
        BattleAnimScriptFunc_BattlerPartialDrawTest,
        BattleAnimScriptFunc_MoveEmitterA2BLinear,
        BattleAnimScriptFunc_MoveEmitterA2BParabolic,
        BattleAnimScriptFunc_BattlerPartialDraw,
        BattleAnimScriptFunc_ShakeBg,
        BattleAnimScriptFunc_PixelatePokemonSprite,
        (system) => throw Missing(0x0222EFB0),
        (system) => throw Missing(0x0222F2F8),
        BattleAnimScriptFunc_RevolveEmitter,
        BattleAnimScriptFunc_MoveEmitterViewportTop,
        BattleAnimScriptFunc_SetBgGrayscale,
        BattleAnimScriptFunc_SetPokemonSpritePriority,
        BattleAnimScriptFunc_ScrollSwitchedBg,
        BattleAnimScriptFunc_MoveBattlerOnOrOffScreen,
        BattleAnimScriptFunc_RenderPokemonSprites,
        (system) => throw Missing(0x0222F5EC),
        BattleAnimScriptFunc_StatChangeUp,
        BattleAnimScriptFunc_StatChangeDown,
        BattleAnimScriptFunc_StatChangeHeal,
        BattleAnimScriptFunc_StatChangeMetal,
    };

    private static readonly Action<BattleAnimSystem, SpriteSystem, SpriteManager, ManagedSprite>[] SpriteFuncs =
    {
        BattleAnimSpriteFunc_SpriteExample,
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222AE68),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222FE30),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222D56C),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02230CEC),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02231010),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x022312A4),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02231650),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02231950),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02231C1C),
        BattleAnimSpriteFunc_Swagger,
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02232378),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x022329E0),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02232CA8),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02233454),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02233734),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x022339C4),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222CC54),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02234008),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222D7C0),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222DB60),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222DEFC),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222E2F8),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222E61C),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x0222EC90),
        BattleAnimSpriteFunc_OffsetAndAnimate,
        BattleAnimSpriteFunc_FollowMe,
        BattleAnimSpriteFunc_Fissure,
        BattleAnimSpriteFunc_EscapeItem,
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x022346A4),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02234A10),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x022348C8),
        (system, spriteSystem, spriteManager, sprite) => throw Missing(0x02234D98),
    };

    // OFFSET 0x02226998
    public static Action<BattleAnimSystem>? BattleAnimScript_GetFunc(uint id)
    {
        if (id >= ScriptFuncCount)
        {
            return null;
        }
        return ScriptFuncs[id];
    }

    // OFFSET 0x022269AC
    public static Action<BattleAnimSystem, SpriteSystem, SpriteManager, ManagedSprite>? BattleAnimScript_GetSpriteFunc(uint id)
    {
        if (id >= SpriteFuncCount)
        {
            return null;
        }
        return SpriteFuncs[id];
    }
}

public static partial class BattleParticleUtil
{
    private static readonly SplEmitterCallback?[] EmitterCallbacks =
    {
        BattleAnimEmitterCb_Nop,
        BattleAnimEmitterCb_SetPosToEnemy1,
        BattleAnimEmitterCb_SetPosToPlayer1,
        BattleAnimEmitterCb_SetPosToAttacker,
        BattleAnimEmitterCb_SetPosToDefender,
        BattleAnimEmitterCb_SetAxisAndPos,
        BattleAnimEmitterCb_SetAxisAndPosReverse,
        BattleAnimEmitterCb_SetAxisAndPos_ConvergeDefault,
        BattleAnimEmitterCb_SetAxisAndPos_ConvergeCenter,
        BattleAnimEmitterCb_SetAxisAndPos_ConvergeDefender,
        BattleAnimEmitterCb_SetAxisAndPos_ConvergeAttacker,
        BattleAnimEmitterCb_SetAxisAndPos_ConvergeExplicit,
        BattleAnimEmitterCb_SetAxisAndPos_MagnetDefault,
        BattleAnimEmitterCb_SetAxisAndPos_MagnetCenter,
        BattleAnimEmitterCb_SetAxisAndPos_MagnetDefender,
        BattleAnimEmitterCb_SetAxisAndPos_MagnetAttacker,
        BattleAnimEmitterCb_SetAxisAndPos_MagnetExplicit,
        BattleAnimEmitterCb_Generic,
        BattleAnimEmitterCb_SetPosBasedOnBattlers,
        BattleAnimEmitterCb_SetPosToAttackerSide,
        BattleAnimEmitterCb_SetPosToDefenderSide,
        BattleAnimEmitterCb_SetPosToAttacker2,
        BattleAnimEmitterCb_Nop2,
    };

    // OFFSET 0x02223880
    public static SplEmitter? BattleParticleUtil_CreateEmitter(ParticleSystem particleSystem, int resourceID, int callbackID, BattleAnimSystem system)
    {
        return ParticleSystem.ParticleSystem_CreateEmitterWithCallback(particleSystem, resourceID, EmitterCallbacks[callbackID], system);
    }
}
