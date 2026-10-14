using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public static class FieldTransition
{
    private const int SceneBattle = 5;
    private const int HeapIDFieldTransition = 4;
    private const int FadeModeBothScreens = 0;
    private const int FadeTypeBrightnessOut = 0;
    private const int FadeTypeBrightnessIn = 1;
    private const int FadeColorBlack = 0;
    private const int FadeDuration = 6;
    private const int FadeStepsPerFrame = 1;

    private sealed class EncounterEffectTaskData
    {
        public const int StateOffset = 0x00;
        public const int DoneOffset = 0x04;
        public const int EncounterEffectIDOffset = 0x08;
        public const int BattleBgmOffset = 0x0C;
        public const int Size = 0x10;

        public int State;
        public readonly int[] Done = new int[1];
        public int EncounterEffectID;
        public int BattleBgm;
    }

    // OFFSET 0x0205578C
    private static bool FieldTask_RunEncounterEffect(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        EncounterEffectTaskData data = (EncounterEffectTaskData)FieldTask.FieldTask_GetEnv(task)!;
        switch (data.State)
        {
            case 0:
                EncounterEffects.EncounterEffect_Start(data.EncounterEffectID, fieldSystem, data.Done);
                Sound.Sound_SetSceneAndPlayBGM(SceneBattle, (ushort)data.BattleBgm, 1);
                data.State++;
                break;
            case 1:
                if (data.Done[0] == 1)
                {
                    return true;
                }
                break;
        }
        return false;
    }

    // OFFSET 0x020557DC
    public static void FieldTransition_StartEncounterEffect(FieldTask task, int encounterEffectID, int battleBgm)
    {
        EncounterEffectTaskData data = new();
        data.State = 0;
        data.Done[0] = 0;
        data.EncounterEffectID = encounterEffectID;
        data.BattleBgm = battleBgm;
        FieldTask.FieldTask_InitCall(task, FieldTask_RunEncounterEffect, data);
    }

    // OFFSET 0x02055808
    private static bool FieldTask_WaitUntilMapFinished(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        if (FieldSystemCore.FieldSystem_HasParentProcess(fieldSystem) == 0)
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02055820
    public static void FieldTransition_FinishMap(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        if (FieldSystemCore.FieldSystem_HasParentProcess(fieldSystem) == 0)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        FieldSystemCore.FieldSystem_FlagNotRunningFieldMap(fieldSystem);
        FieldTask.FieldTask_InitCall(task, FieldTask_WaitUntilMapFinished, null);
    }

    // OFFSET 0x02055850
    private static bool FieldTask_WaitUntilMapStarted(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        if (FieldTask.FieldSystem_IsRunningFieldMap(fieldSystem))
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x02055868
    public static void FieldTransition_StartMap(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        if (FieldSystemCore.FieldSystem_HasParentProcess(fieldSystem) != 0)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        FieldTask.FieldSystem_StartFieldMap(fieldSystem);
        FieldTask.FieldTask_InitCall(task, FieldTask_WaitUntilMapStarted, null);
    }

    // OFFSET 0x02055898
    private static bool FieldTask_WaitUntilScreenTransitionDone(FieldTask task)
    {
        if (ScreenFadeControl.IsScreenFadeDone())
        {
            return true;
        }
        return false;
    }

    // OFFSET 0x020558AC
    public static void FieldTransition_FadeOut(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        if (FieldSystemCore.FieldSystem_HasParentProcess(fieldSystem) == 0)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        ScreenFadeControl.StartScreenFade(FadeModeBothScreens, FadeTypeBrightnessOut, FadeTypeBrightnessOut, FadeColorBlack, FadeDuration, FadeStepsPerFrame, HeapIDFieldTransition);
        FieldTask.FieldTask_InitCall(task, FieldTask_WaitUntilScreenTransitionDone, null);
    }

    // OFFSET 0x020558F0
    public static void FieldTransition_FadeIn(FieldTask task)
    {
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        if (FieldSystemCore.FieldSystem_HasParentProcess(fieldSystem) == 0)
        {
            ErrorHandling.GF_AssertFail();
            return;
        }
        ScreenFadeControl.StartScreenFade(FadeModeBothScreens, FadeTypeBrightnessIn, FadeTypeBrightnessIn, FadeColorBlack, FadeDuration, FadeStepsPerFrame, HeapIDFieldTransition);
        FieldTask.FieldTask_InitCall(task, FieldTask_WaitUntilScreenTransitionDone, null);
    }

    // OFFSET 0x02055934
    private static bool FieldTask_FadeOutAndFinishMap(FieldTask task)
    {
        ref int state = ref FieldTask.FieldTask_GetState(task);
        switch (state)
        {
            case 0:
                FieldTransition_FadeOut(task);
                state++;
                break;
            case 1:
                FieldTransition_FinishMap(task);
                state++;
                break;
            case 2:
                return true;
        }
        return false;
    }

    // OFFSET 0x02055974
    public static void FieldTransition_FadeOutAndFinishMap(FieldTask task)
    {
        FieldTask.FieldTask_InitCall(task, FieldTask_FadeOutAndFinishMap, null);
    }

    // OFFSET 0x02055984
    private static bool FieldTask_StartMapAndFadeIn(FieldTask task)
    {
        ref int state = ref FieldTask.FieldTask_GetState(task);
        FieldSystem fieldSystem = FieldTask.FieldTask_GetFieldSystem(task);
        switch (state)
        {
            case 0:
                FieldTransition_StartMap(task);
                state++;
                break;
            case 1:
                MapNamePopUps.FieldSystem_RequestLocationName(fieldSystem);
                FieldTransition_FadeIn(task);
                state++;
                break;
            case 2:
                return true;
        }
        return false;
    }

    // OFFSET 0x020559CC
    public static void FieldTransition_StartMapAndFadeIn(FieldTask task)
    {
        FieldTask.FieldTask_InitCall(task, FieldTask_StartMapAndFadeIn, null);
    }
}
