namespace PlatinumSharp.Game;

public sealed class ForgetMoveData
{
    public BattleSystem? BattleSys;
    public BattlePartyContext? BattlePartyCtx;
    public byte Command;
    public byte Battler;
    public byte State;
    public ushort Move;
    public byte Slot;
}

public static partial class Battlers
{
    // OFFSET 0x0225C65C
    private static void Battler_CommandForgetMove(BattleSystem battleSys, BattlerData battler)
    {
        BattleDisplay.BattleDisplay_InitTaskForgetMove(battleSys, battler, battler.Data);
        Battler_ClearCommand(battler);
    }
}

public static partial class BattleDisplay
{
    private const int ForgetMoveHeapIDBattle = 5;
    private const int ForgetMovePartyModeLearnMove = 3;
    private const int ForgetMoveCancelSlot = 4;

    // OFFSET 0x0225E0BC
    public static void BattleDisplay_InitTaskForgetMove(BattleSystem battleSys, BattlerData battler, byte[] message)
    {
        ForgetMoveData data = new();
        data.State = 0;
        data.BattleSys = battleSys;
        data.Command = message[0];
        data.Battler = battler.Battler;
        data.Move = (ushort)BattleIO.R16(message, 2);
        data.Slot = message[1];
        SysTask.SysTask_Start(Task_ForgetMove, data, 0);
    }

    // OFFSET 0x022633A4
    private static void Task_ForgetMove(SysTask task, object? param)
    {
        ForgetMoveData data = (ForgetMoveData)param!;
        PaletteData paletteData = BattleSystems.BattleSystem_GetPaletteData(data.BattleSys!)!;
        switch (data.State)
        {
            case 0:
                PaletteData.PaletteData_StartFade(paletteData, 0xA, 0xFFFF, -8, 0, 0x10, 0);
                data.State++;
                break;
            case 1:
            {
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    break;
                }
                BattleSystems.BattleSystem_EnterSubMenu(data.BattleSys!);
                data.BattlePartyCtx = new BattlePartyContext();
                BattlePartyContext partyCtx = data.BattlePartyCtx;
                partyCtx.Party = BattleSystems.BattleSystem_GetParty(data.BattleSys!, data.Battler);
                partyCtx.BattleSys = data.BattleSys;
                partyCtx.HeapID = ForgetMoveHeapIDBattle;
                partyCtx.SelectedPartyIndex = data.Slot;
                partyCtx.MoveToLearn = data.Move;
                partyCtx.BattlePartyExited = 0;
                partyCtx.DoubleBattleFirstSelectedPartySlot = 0;
                partyCtx.BattlePartyMode = ForgetMovePartyModeLearnMove;
                partyCtx.SelectedBattleBagItem = 0;
                partyCtx.Battler = data.Battler;
                partyCtx.IsCursorEnabled = 0;
                BattleParties.BattlePartyTask_Start(partyCtx);
                data.State++;
                break;
            }
            case 2:
                if (data.BattlePartyCtx!.BattlePartyExited == 0)
                {
                    break;
                }
                BattleSystems.BattleSystem_ExitSubMenu(data.BattleSys!);
                PaletteData.PaletteData_StartFade(paletteData, 0xA, 0xFFFF, -8, 0x10, 0, 0);
                data.State++;
                break;
            case 3:
                if (PaletteData.PaletteData_GetSelectedBuffersMask(paletteData) != 0)
                {
                    break;
                }
                if (data.BattlePartyCtx!.SelectedMoveSlot == ForgetMoveCancelSlot)
                {
                    BattleIO.BattleIO_PartyMenuResult(data.BattleSys!, data.Battler, 0xFF);
                }
                else
                {
                    BattleIO.BattleIO_PartyMenuResult(data.BattleSys!, data.Battler, (uint)(data.BattlePartyCtx.SelectedMoveSlot + 1));
                }
                BattleIO.BattleIO_ClearMessage(data.BattleSys!, data.Battler, data.Command);
                SysTask.SysTask_Done(task);
                break;
        }
    }
}
