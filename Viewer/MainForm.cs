using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using NAudio.Wave;
using PlatinumSharp.Nitro;
using PlatinumSharp.Nitro.Graphics;
using PlatinumSharp.Nitro.Sound;

namespace PlatinumSharp.Viewer;

internal sealed record NodeRef(string Path, int Member, SequenceInfo? Sequence);

internal sealed class SequenceProvider : ISampleProvider
{
    private readonly SequencePlayer player;

    public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    public SequenceProvider(SequencePlayer player)
    {
        this.player = player;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        player.Render(buffer, offset, count / 2);
        return count;
    }
}

internal sealed class MainForm : Form
{
    private const string Placeholder = "...";

    private readonly NitroRom rom;
    private readonly TreeView tree = new() { Dock = DockStyle.Fill, HideSelection = false };
    private readonly PictureBox picture = new() { SizeMode = PictureBoxSizeMode.AutoSize };
    private readonly TextBox info = new() { Dock = DockStyle.Bottom, Multiline = true, ReadOnly = true, Height = 180, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9) };
    private readonly Button stop = new() { Text = "Stop sound", Dock = DockStyle.Top, Height = 28 };
    private readonly Dictionary<string, Narc> narcs = new();
    private readonly Dictionary<string, Sdat> sdats = new();
    private WaveOutEvent? output;

    public MainForm(string romPath)
    {
        rom = new NitroRom(romPath);
        Text = $"PlatinumSharp - {rom.Title} ({rom.GameCode} rev {rom.Revision})";
        Width = 1100;
        Height = 760;

        Panel canvas = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(32, 32, 36) };
        canvas.Controls.Add(picture);
        SplitContainer split = new() { Dock = DockStyle.Fill, SplitterDistance = 340 };
        split.Panel1.Controls.Add(tree);
        split.Panel2.Controls.Add(canvas);
        split.Panel2.Controls.Add(info);
        split.Panel2.Controls.Add(stop);
        Controls.Add(split);

        BuildTree();
        tree.BeforeExpand += (_, e) => Populate(e.Node!);
        tree.AfterSelect += (_, e) => Show(e.Node!);
        stop.Click += (_, _) => StopSound();
        FormClosed += (_, _) =>
        {
            StopSound();
            rom.Dispose();
        };
    }

    private void BuildTree()
    {
        tree.BeginUpdate();
        foreach (string path in rom.Paths)
        {
            string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            TreeNodeCollection nodes = tree.Nodes;
            for (int i = 0; i < parts.Length; i++)
            {
                TreeNode? node = nodes.ContainsKey(parts[i]) ? nodes[parts[i]] : null;
                if (node == null)
                {
                    node = nodes.Add(parts[i], parts[i]);
                    if (i == parts.Length - 1)
                    {
                        node.Tag = new NodeRef(path, -1, null);
                        if (path.EndsWith(".narc", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".sdat", StringComparison.OrdinalIgnoreCase))
                        {
                            node.Nodes.Add(Placeholder);
                        }
                    }
                }
                nodes = node.Nodes;
            }
        }
        tree.EndUpdate();
    }

    private Narc GetNarc(string path)
    {
        if (!narcs.TryGetValue(path, out Narc? narc))
        {
            narc = Narc.Open(rom, path);
            narcs[path] = narc;
        }
        return narc;
    }

    private Sdat GetSdat(string path)
    {
        if (!sdats.TryGetValue(path, out Sdat? sdat))
        {
            sdat = Sdat.Open(rom, path);
            sdats[path] = sdat;
        }
        return sdat;
    }

    private void Populate(TreeNode node)
    {
        if (node.Tag is not NodeRef file || node.Nodes.Count != 1 || node.Nodes[0].Text != Placeholder)
        {
            return;
        }
        node.Nodes.Clear();
        try
        {
            if (file.Path.EndsWith(".sdat", StringComparison.OrdinalIgnoreCase))
            {
                foreach (SequenceInfo sequence in GetSdat(file.Path).Sequences)
                {
                    node.Nodes.Add(new TreeNode($"{sequence.Index:D4}  {sequence.Name}") { Tag = new NodeRef(file.Path, -1, sequence) });
                }
                return;
            }
            Narc narc = GetNarc(file.Path);
            for (int i = 0; i < narc.Count; i++)
            {
                byte[] head = narc.ReadMemberHead(i, 4);
                string magic = Bin.Magic(head, 0);
                if (magic == "" && head.Length > 0 && (head[0] == 0x10 || head[0] == 0x11))
                {
                    magic = "LZ";
                }
                node.Nodes.Add(new TreeNode($"{i:D4}  {magic,-4}  {narc.GetSize(i)}") { Tag = new NodeRef(file.Path, i, null) });
            }
        }
        catch (Exception ex)
        {
            info.Text = ex.Message;
        }
    }

    private void Show(TreeNode node)
    {
        if (node.Tag is not NodeRef target)
        {
            return;
        }
        try
        {
            if (target.Sequence != null)
            {
                Play(GetSdat(target.Path), target.Sequence);
                return;
            }
            if (target.Member < 0 && (target.Path.EndsWith(".narc", StringComparison.OrdinalIgnoreCase) || target.Path.EndsWith(".sdat", StringComparison.OrdinalIgnoreCase)))
            {
                info.Text = $"{target.Path}\r\n{rom.GetSize(target.Path)} bytes";
                return;
            }
            byte[] raw = target.Member >= 0 ? GetNarc(target.Path).ReadMember(target.Member) : rom.ReadFile(target.Path);
            byte[] data = Lz.Unpack(raw);
            Raster? raster = Render(target, data);
            SetImage(raster);
            info.Text = Describe(target, raw, data, raster);
        }
        catch (Exception ex)
        {
            SetImage(null);
            info.Text = ex.Message;
        }
    }

    private Raster? Render(NodeRef target, byte[] data)
    {
        switch (Bin.Magic(data, 0))
        {
            case "RLCN":
                return Renderer.RenderPalette(Palette.Parse(data));
            case "RGCN":
            {
                byte[]? palette = FindNeighbour(target, "RLCN");
                return Renderer.RenderTiles(TileSet.Parse(data), palette != null ? Palette.Parse(palette) : Palette.Grayscale(), 0);
            }
            case "RCSN":
            {
                byte[]? tiles = FindNeighbour(target, "RGCN");
                byte[]? palette = FindNeighbour(target, "RLCN");
                if (tiles == null)
                {
                    return null;
                }
                return Renderer.RenderScreen(TileMap.Parse(data), TileSet.Parse(tiles), palette != null ? Palette.Parse(palette) : Palette.Grayscale());
            }
            default:
                return null;
        }
    }

    private byte[]? FindNeighbour(NodeRef target, string magic)
    {
        if (target.Member < 0)
        {
            return null;
        }
        Narc narc = GetNarc(target.Path);
        for (int distance = 1; distance <= 8; distance++)
        {
            foreach (int member in new[] { target.Member - distance, target.Member + distance })
            {
                if (member < 0 || member >= narc.Count)
                {
                    continue;
                }
                byte[] head = narc.ReadMemberHead(member, 4);
                if (Bin.Magic(head, 0) == magic)
                {
                    return narc.ReadMember(member);
                }
                if (head.Length > 0 && (head[0] == 0x10 || head[0] == 0x11))
                {
                    byte[] data = Lz.Unpack(narc.ReadMember(member));
                    if (Bin.Magic(data, 0) == magic)
                    {
                        return data;
                    }
                }
            }
        }
        return null;
    }

    private static string Describe(NodeRef target, byte[] raw, byte[] data, Raster? raster)
    {
        StringBuilder text = new();
        text.Append(target.Path);
        if (target.Member >= 0)
        {
            text.Append($" [{target.Member}]");
        }
        text.AppendLine();
        text.Append($"{raw.Length} bytes");
        if (!ReferenceEquals(raw, data))
        {
            text.Append($", {data.Length} after LZ decompression");
        }
        string magic = Bin.Magic(data, 0);
        if (magic != "")
        {
            text.Append($", format {magic}");
        }
        if (raster != null)
        {
            text.Append($", {raster.Width}x{raster.Height}");
        }
        text.AppendLine();
        for (int row = 0; row < 8 && row * 16 < data.Length; row++)
        {
            text.Append($"{row * 16:X4}  ");
            for (int i = row * 16; i < Math.Min(data.Length, row * 16 + 16); i++)
            {
                text.Append($"{data[i]:X2} ");
            }
            text.AppendLine();
        }
        return text.ToString();
    }

    private void SetImage(Raster? raster)
    {
        Image? previous = picture.Image;
        if (raster == null || raster.Width == 0 || raster.Height == 0)
        {
            picture.Image = null;
        }
        else
        {
            using Bitmap source = new(raster.Width, raster.Height, PixelFormat.Format32bppArgb);
            BitmapData bits = source.LockBits(new Rectangle(0, 0, raster.Width, raster.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(raster.Pixels, 0, bits.Scan0, raster.Pixels.Length);
            source.UnlockBits(bits);
            int scale = raster.Width <= 512 ? 2 : 1;
            Bitmap scaled = new(raster.Width * scale, raster.Height * scale, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(scaled))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(source, 0, 0, scaled.Width, scaled.Height);
            }
            picture.Image = scaled;
        }
        previous?.Dispose();
    }

    private void Play(Sdat sdat, SequenceInfo sequence)
    {
        StopSound();
        SequencePlayer player = new(sdat, sequence);
        output = new WaveOutEvent { DesiredLatency = 120 };
        output.Init(new SequenceProvider(player));
        output.Play();
        info.Text = $"Playing {sequence.Name} (sequence {sequence.Index}, bank {sequence.Bank}, volume {sequence.Volume})";
    }

    private void StopSound()
    {
        output?.Stop();
        output?.Dispose();
        output = null;
    }
}
