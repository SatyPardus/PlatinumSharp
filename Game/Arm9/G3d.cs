namespace PlatinumSharp.Game;

public struct MtxFx33
{
    public int M00;
    public int M01;
    public int M02;
    public int M10;
    public int M11;
    public int M12;
    public int M20;
    public int M21;
    public int M22;
}

public sealed class G3dAllocator
{
}

public sealed class G3dModelSet
{
    public byte[] File = Array.Empty<byte>();
    public int Offset;
}

public sealed class G3dModel
{
    public byte[] File = Array.Empty<byte>();
    public int Offset;
}

public sealed class G3dTexture
{
    public byte[] File = Array.Empty<byte>();
    public int Offset;
}

public sealed class G3dAnim
{
    public byte[] File = Array.Empty<byte>();
    public int Offset;
    public ushort NumFrames;
}

public sealed class G3dAnimObj
{
    public int Frame;
    public G3dAnim? ResAnm;
    public int Kind;
    public object? Binding;
}

public sealed class G3dRenderObj
{
    public G3dModel? Model;
    public readonly List<G3dAnimObj> AnimObjs = new();
}
