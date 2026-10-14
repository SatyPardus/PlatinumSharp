using System.Collections;
using System.Reflection;
using System.Text;
using static PlatinumSharp.Game.Globals;

namespace PlatinumSharp.Game;

public static class DebugInfo
{
    private const int MaxFieldLines = 400;

    public static string DescribeApplication()
    {
        StringBuilder text = new();
        ApplicationManager? manager = sApplication.CurrentManager;
        if (manager == null)
        {
            text.AppendLine("application: none");
            return text.ToString();
        }
        string name = manager.Main?.Method.DeclaringType?.Name ?? "?";
        text.AppendLine($"application: {name}");
        text.AppendLine($"template: 0x{manager.TemplateAddress:X8} overlay {sApplication.CurrentOverlay}");
        text.AppendLine($"phase {manager.State} state {manager.SubState}");
        if (manager.Data is FieldSystem fieldSystem)
        {
            AppendFieldTasks(text, fieldSystem);
        }
        return text.ToString();
    }

    private static void AppendFieldTasks(StringBuilder text, FieldSystem fieldSystem)
    {
        text.AppendLine($"field map running {fieldSystem.RunningFieldMap} map {fieldSystem.Location?.U32(0)}");
        if (fieldSystem.PlayerAvatar != null && fieldSystem.PlayerAvatar.MapObject != null)
        {
            text.AppendLine($"player x {PlayerAvatar.PlayerAvatar_GetXPos(fieldSystem.PlayerAvatar)} z {PlayerAvatar.PlayerAvatar_GetZPos(fieldSystem.PlayerAvatar)} dir {PlayerAvatar.PlayerAvatar_GetFacingDir(fieldSystem.PlayerAvatar)}");
        }
        if (fieldSystem.MapHeaderData is MapHeaderData headerData && headerData.WarpEvents != null)
        {
            text.AppendLine("warps " + string.Join(" ", headerData.WarpEvents.Select(w => $"({w.X},{w.Z})>{w.DestHeaderID}.{w.DestWarpID}")));
        }
        text.Append(TestWorld.Describe(fieldSystem));
        for (FieldTask? task = fieldSystem.Task; task != null; task = task.Previous)
        {
            text.AppendLine($"field task {task.Function?.Method.DeclaringType?.Name}.{task.Function?.Method.Name} state {task.State}");
            if (task.Environment is ScriptManager scriptManager)
            {
                text.AppendLine($"  script {scriptManager.ScriptID} state {scriptManager.State} contexts {scriptManager.NumActiveContexts}");
                for (int i = 0; i < scriptManager.Contexts.Length; i++)
                {
                    ScriptContext? ctx = scriptManager.Contexts[i];
                    if (ctx == null)
                    {
                        continue;
                    }
                    string bytes = "";
                    if (ctx.Scripts != null && ctx.ScriptPtr >= 0)
                    {
                        bytes = string.Join(" ", ctx.Scripts.Skip(Math.Max(0, ctx.ScriptPtr - 8)).Take(24).Select(v => v.ToString("X2")));
                    }
                    text.AppendLine($"  ctx {i} state {ctx.State} ptr 0x{ctx.ScriptPtr:X} wait {ctx.ShouldResume?.Method.Name}");
                    text.AppendLine($"  bytes(-8) {bytes}");
                }
            }
        }
    }

    public static string DescribeVideo()
    {
        StringBuilder text = new();
        for (int display = 0; display < 2; display++)
        {
            uint control = Video.GetDisplayControl(display);
            text.AppendLine($"{(display == 0 ? "main" : "sub ")} DISPCNT {control:X8} mode {control & 7}{(display == 0 && (control & 8) != 0 ? " 3D" : "")}");
            text.Append("  layers ");
            for (int i = 0; i < 4; i++)
            {
                text.Append((control & (0x100u << i)) != 0 ? $"BG{i}(p{Video.GetBgControl(display * 4 + i) & 3}) " : "");
            }
            text.AppendLine((control & 0x1000) != 0 ? "OBJ" : "");
            text.AppendLine($"  blend {Video.GetBlendControl(display):X4} alpha {Video.GetBlendAlpha(display):X4} bright {Video.GetBlendBrightness(display)} master {Video.GetMasterBrightness(display)}");
            int windows = (int)((control >> 13) & 7) | Video.GetVisibleWindows(display);
            if (windows != 0)
            {
                (byte left, byte right, byte top, byte bottom) = Video.GetWindow(display, 0);
                text.AppendLine($"  win {windows} w0 {left},{top}-{right},{bottom} in {Video.GetWindowInside(display, 0):X2} out {Video.GetWindowOutside(display):X2}");
            }
        }
        text.AppendLine($"3D ctrl {Render3D.Display3DControl:X4} fog {(Render3D.FogEnabled ? "on" : "off")} top={(gSystem.WhichScreenIs3D == 0 ? "main" : "sub")}");
        return text.ToString();
    }

    public static string DescribeVideoFull()
    {
        StringBuilder text = new();
        text.Append(DescribeVideo());
        for (int layer = 0; layer < 8; layer++)
        {
            (MtxFx22 matrix, int x, int y) = Video.GetBgAffine(layer);
            text.AppendLine($"bg{layer}: control {Video.GetBgControl(layer):X4} offset {Video.GetBgOffset(layer):X8} screenBase {Video.GetBgScreenBase(layer):X} charBase {Video.GetBgCharBase(layer):X} affine {matrix.M00:X},{matrix.M01:X},{matrix.M10:X},{matrix.M11:X} @ {x:X},{y:X}");
        }
        text.AppendLine("banks " + string.Join(" ", Video.Banks.Select(b => b.ToString("X"))));
        text.AppendLine($"fog mode {Render3D.FogBlendMode} slope {Render3D.FogSlope} offset {Render3D.FogOffset:X} color {Render3D.FogColor:X8}");
        text.AppendLine($"clear color {Render3D.ClearColor:X4} alpha {Render3D.ClearAlpha} viewport {Render3D.Viewport}");
        for (int display = 0; display < 2; display++)
        {
            OamAttributes[] oam = OamManager.GetScreenOam(display);
            for (int i = 0; i < oam.Length; i++)
            {
                if ((oam[i].Attr0 & 0x300) != 0x200)
                {
                    text.AppendLine($"obj {display}:{i} {oam[i].Attr0:X4} {oam[i].Attr1:X4} {oam[i].Attr2:X4}");
                }
            }
        }
        string[] names = { "main bg", "sub bg", "main obj", "sub obj" };
        for (int target = 0; target < 4; target++)
        {
            ushort[] palette = Video.GetPalette(target);
            for (int row = 0; row < 16; row++)
            {
                bool any = false;
                for (int i = 0; i < 16; i++)
                {
                    any |= palette[row * 16 + i] != 0;
                }
                if (any)
                {
                    text.AppendLine($"{names[target]} pal {row:X}: " + string.Join(" ", palette.Skip(row * 16).Take(16).Select(c => c.ToString("X4"))));
                }
            }
        }
        return text.ToString();
    }

    public static string DescribeData()
    {
        StringBuilder text = new();
        object? data = sApplication.CurrentManager?.Data;
        if (data == null)
        {
            return "";
        }
        int lines = 0;
        AppendFields(text, data, data.GetType().Name, 0, new HashSet<object>(ReferenceEqualityComparer.Instance), ref lines);
        return text.ToString();
    }

    private static void AppendFields(StringBuilder text, object value, string label, int depth, HashSet<object> seen, ref int lines)
    {
        if (depth > 3 || lines > MaxFieldLines || !seen.Add(value))
        {
            return;
        }
        text.AppendLine($"{new string(' ', depth * 2)}{label}:");
        lines++;
        foreach (FieldInfo field in value.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (lines > MaxFieldLines)
            {
                return;
            }
            object? item = field.GetValue(value);
            Type type = field.FieldType;
            string indent = new(' ', depth * 2 + 2);
            if (item == null)
            {
                continue;
            }
            if (type.IsPrimitive || type.IsEnum)
            {
                text.AppendLine($"{indent}{field.Name} = {item}");
                lines++;
            }
            else if (type == typeof(VecFx32))
            {
                VecFx32 vector = (VecFx32)item;
                text.AppendLine($"{indent}{field.Name} = ({vector.X:X}, {vector.Y:X}, {vector.Z:X})");
                lines++;
            }
            else if (item is Array || item is IList || item is Delegate || item is string)
            {
                continue;
            }
            else if (type.IsClass && type.Namespace == typeof(DebugInfo).Namespace && depth < 3)
            {
                AppendFields(text, item, field.Name, depth + 1, seen, ref lines);
            }
        }
    }
}
