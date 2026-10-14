using System.Numerics;

namespace PlatinumSharp.Game;

public static class Render3D
{
    public const int Width = 256;
    public const int Height = 192;
    public const int ModeProjection = 0;
    public const int ModePosition = 1;
    public const int ModePositionVector = 2;
    public const int ModeTexture = 3;

    private const float FixedOne = 4096f;
    private const int StackSize = 32;
    private const int MaxClipVertices = 16;
    private const int FormatNone = 0;
    private const int FormatA3I5 = 1;
    private const int Format4Color = 2;
    private const int Format16Color = 3;
    private const int Format256Color = 4;
    private const int FormatCompressed = 5;
    private const int FormatA5I3 = 6;
    private const int FormatDirect = 7;

    private struct GeomVertex
    {
        public float X;
        public float Y;
        public float Z;
        public float W;
        public float R;
        public float G;
        public float B;
        public float S;
        public float T;
    }

    private sealed class Polygon
    {
        public GeomVertex[] Vertices = Array.Empty<GeomVertex>();
        public uint Attr;
        public uint TexParam;
        public int PaletteBase;
        public bool Translucent;
    }

    private static readonly byte[] parameterCounts = BuildParameterCounts();

    public static readonly int[] Frame = new int[Width * Height];

    private static readonly float[] depthBuffer = new float[Width * Height];
    private static readonly byte[] translucentId = new byte[Width * Height];
    private static readonly bool[] fogFlags = new bool[Width * Height];
    private static bool clearFog;
    private static readonly List<Polygon> polygons = new();
    private static readonly Matrix4x4[] positionStack = new Matrix4x4[StackSize];
    private static readonly Matrix4x4[] vectorStack = new Matrix4x4[StackSize];
    private static readonly Stack<Matrix4x4> projectionStack = new();
    private static readonly Vector3[] lightVectors = new Vector3[4];
    private static readonly Vector3[] lightColors = new Vector3[4];
    private static readonly GeomVertex[] primitive = new GeomVertex[4];
    private static readonly GeomVertex[] clipA = new GeomVertex[MaxClipVertices];
    private static readonly GeomVertex[] clipB = new GeomVertex[MaxClipVertices];
    private static readonly ushort[] toonTable = new ushort[32];
    private static readonly uint[] parameters = new uint[32];

    private static short[]? sinCosTable;
    private static Matrix4x4 projection = Matrix4x4.Identity;
    private static Matrix4x4 position = Matrix4x4.Identity;
    private static Matrix4x4 vector = Matrix4x4.Identity;
    private static Matrix4x4 texture = Matrix4x4.Identity;
    private static int stackPointer;
    private static int matrixMode = ModePositionVector;
    private static uint texImageParam;
    private static uint paletteBase;
    private static uint polygonAttr;
    private static uint activePolygonAttr;
    private static Vector3 diffuse = new(31, 31, 31);
    private static Vector3 ambient;
    private static Vector3 specular;
    private static Vector3 emission;
    private static Vector3 color = new(31, 31, 31);
    private static float texS;
    private static float texT;
    private static float rawS;
    private static float rawT;
    private static float lastX;
    private static float lastY;
    private static float lastZ;
    private static int primitiveType = -1;
    private static int primitiveCount;
    private static bool stripOdd;
    private static int alphaReference;

    private static byte[] BuildParameterCounts()
    {
        byte[] counts = new byte[256];
        counts[0x10] = 1;
        counts[0x12] = 1;
        counts[0x13] = 1;
        counts[0x14] = 1;
        counts[0x16] = 16;
        counts[0x17] = 12;
        counts[0x18] = 16;
        counts[0x19] = 12;
        counts[0x1A] = 9;
        counts[0x1B] = 3;
        counts[0x1C] = 3;
        counts[0x20] = 1;
        counts[0x21] = 1;
        counts[0x22] = 1;
        counts[0x23] = 2;
        counts[0x24] = 1;
        counts[0x25] = 1;
        counts[0x26] = 1;
        counts[0x27] = 1;
        counts[0x28] = 1;
        counts[0x29] = 1;
        counts[0x2A] = 1;
        counts[0x2B] = 1;
        counts[0x30] = 1;
        counts[0x31] = 1;
        counts[0x32] = 1;
        counts[0x33] = 1;
        counts[0x34] = 32;
        counts[0x40] = 1;
        counts[0x50] = 1;
        counts[0x60] = 1;
        counts[0x70] = 3;
        counts[0x71] = 2;
        counts[0x72] = 1;
        return counts;
    }

    public static void BeginFrame()
    {
    }

    public static void FlushBuffer()
    {
    }

    public static int SortMode { get; private set; }

    public static int BufferMode { get; private set; }

    public static ushort Display3DControl { get; set; } = 1;

    public static bool FogEnabled { get; private set; }

    public static uint ClearColor { get; private set; }

    public static int ClearAlpha { get; private set; }

    public static int ClearDepth { get; private set; } = 0x7FFF;

    public static int ClearPolygonID { get; private set; }

    public static (int Left, int Top, int Right, int Bottom) Viewport { get; private set; } = (0, 0, 255, 191);

    public static readonly ushort[] EdgeColors = new ushort[8];

    public static void SetEdgeColorTable(ushort[] colors)
    {
        Array.Copy(colors, EdgeColors, EdgeColors.Length);
    }

    public static void SetToonTable(ushort[] colors)
    {
        Array.Copy(colors, toonTable, Math.Min(colors.Length, toonTable.Length));
    }

    public static void SetAlphaTest(bool enable, int reference)
    {
        Display3DControl = (ushort)(enable ? Display3DControl | 4 : Display3DControl & ~4);
        alphaReference = reference;
    }

    public static int FogBlendMode { get; private set; }

    public static int FogSlope { get; private set; }

    public static int FogOffset { get; private set; }

    public static uint FogColor { get; private set; }

    public static readonly byte[] FogTable = new byte[32];

    public static void SetFog(bool enable, int blendMode, int slope, int offset)
    {
        FogEnabled = enable;
        FogBlendMode = blendMode;
        FogSlope = slope;
        FogOffset = offset;
    }

    public static void SetFogColor(uint value)
    {
        FogColor = value;
    }

    public static void SetFogTable(uint[] table)
    {
        for (int i = 0; i < FogTable.Length; i++)
        {
            FogTable[i] = (byte)(table[i >> 2] >> ((i & 3) * 8));
        }
    }

    public static void SetClearColor(uint color, int alpha, int depth, int polygonID, bool fog)
    {
        ClearColor = color;
        ClearAlpha = alpha;
        ClearDepth = depth;
        ClearPolygonID = polygonID;
        clearFog = fog;
    }

    public static void SetViewport(int left, int top, int right, int bottom)
    {
        Viewport = (left, top, right, bottom);
    }

    public static void Reset()
    {
        stackPointer = 0;
        projectionStack.Clear();
        position = Matrix4x4.Identity;
        vector = Matrix4x4.Identity;
        texture = Matrix4x4.Identity;
        matrixMode = ModePositionVector;
        primitiveType = -1;
    }

    public static void SetupSoftwareSpriteCamera()
    {
        SetViewport(0, 0, 255, 191);
        projection = OrthoMatrix(0, 192, 0, 256, -64, 1024, 1024);
        texture = Matrix4x4.Identity;
        position = Matrix4x4.Identity;
        vector = Matrix4x4.Identity;
        matrixMode = ModePositionVector;
    }

    public static MtxFx33 BaseRotation { get; private set; } = new() { M00 = 4096, M11 = 4096, M22 = 4096 };

    public static VecFx32 BaseTranslation { get; private set; }

    public static VecFx32 BaseScale { get; private set; } = new() { X = 4096, Y = 4096, Z = 4096 };

    public static void SetBaseRotation(in MtxFx33 rotation)
    {
        BaseRotation = rotation;
    }

    public static void SetBaseTranslation(in VecFx32 translation)
    {
        BaseTranslation = translation;
    }

    public static void SetBaseScale(in VecFx32 scale)
    {
        BaseScale = scale;
    }

    public static VecFx32 CameraPosition { get; private set; }

    public static VecFx32 CameraUp { get; private set; }

    public static VecFx32 CameraTarget { get; private set; }

    public static Matrix4x4 View { get; private set; } = Matrix4x4.Identity;

    public static Matrix4x4 Projection { get; private set; } = Matrix4x4.Identity;

    public static int ProjectionCache { get; set; }

    public static void SetGlobalProjection(in Matrix4x4 matrix)
    {
        Projection = matrix;
    }

    private static readonly Vector3[] globalLightVectors = new Vector3[4];
    private static readonly ushort[] globalLightColors = new ushort[4];
    private static uint globalDiffuseAmbient = 0x7FFF;
    private static uint globalSpecularEmission;
    private static uint globalPolygonAttr = 0x001F008F;

    public static uint GlobalDiffuseAmbient => globalDiffuseAmbient;

    public static uint GlobalSpecularEmission => globalSpecularEmission;

    public static uint GlobalPolygonAttr => globalPolygonAttr;

    public static void SetGlobalLightVector(int light, int x, int y, int z)
    {
        globalLightVectors[light] = new Vector3(x / FixedOne, y / FixedOne, z / FixedOne);
    }

    public static void SetGlobalLightColor(int light, ushort value)
    {
        globalLightColors[light] = value;
    }

    public static void SetGlobalDiffuseAmbient(ushort diffuseColor, ushort ambientColor, bool vertexColor)
    {
        globalDiffuseAmbient = (uint)(diffuseColor & 0x7FFF) | (vertexColor ? 0x8000u : 0) | ((uint)(ambientColor & 0x7FFF) << 16);
    }

    public static void SetGlobalSpecularEmission(ushort specularColor, ushort emissionColor, bool shininess)
    {
        globalSpecularEmission = (uint)(specularColor & 0x7FFF) | (shininess ? 0x8000u : 0) | ((uint)(emissionColor & 0x7FFF) << 16);
    }

    public static void SetGlobalPolygonAttr(int lights, int polygonMode, int cullMode, int polygonID, int alpha, int misc)
    {
        globalPolygonAttr = (uint)(lights | (polygonMode << 4) | (cullMode << 6) | misc | (polygonID << 24) | (alpha << 16));
    }

    private static Vector3 ToVector(in VecFx32 value)
    {
        return new Vector3(value.X / FixedOne, value.Y / FixedOne, value.Z / FixedOne);
    }

    private static Matrix4x4 ToMatrix(in MtxFx33 m)
    {
        return new Matrix4x4(
            m.M00 / FixedOne, m.M01 / FixedOne, m.M02 / FixedOne, 0,
            m.M10 / FixedOne, m.M11 / FixedOne, m.M12 / FixedOne, 0,
            m.M20 / FixedOne, m.M21 / FixedOne, m.M22 / FixedOne, 0,
            0, 0, 0, 1);
    }

    public static void SetLookAt(in VecFx32 cameraPosition, in VecFx32 up, in VecFx32 target)
    {
        CameraPosition = cameraPosition;
        CameraUp = up;
        CameraTarget = target;
        Vector3 eye = ToVector(cameraPosition);
        Vector3 at = ToVector(target);
        View = eye == at ? Matrix4x4.Identity : Matrix4x4.CreateLookAt(eye, at, ToVector(up));
    }

    public static void SetPerspective(int sinFovY, int cosFovY, int aspect, int near, int far, int scaleW)
    {
        float cot = sinFovY == 0 ? 0 : (float)cosFovY / sinFovY;
        float n = near / FixedOne;
        float f = far / FixedOne;
        float w = scaleW / FixedOne;
        float a = aspect / FixedOne;
        Matrix4x4 matrix = default;
        matrix.M11 = a == 0 ? 0 : cot / a * w;
        matrix.M22 = cot * w;
        matrix.M33 = n == f ? 0 : (f + n) / (n - f) * w;
        matrix.M34 = -w;
        matrix.M43 = n == f ? 0 : 2 * f * n / (n - f) * w;
        Projection = matrix;
    }

    private static Matrix4x4 OrthoMatrix(float t, float b, float l, float r, float n, float f, float w)
    {
        Matrix4x4 matrix = default;
        matrix.M11 = r == l ? 0 : 2 / (r - l) * w;
        matrix.M22 = t == b ? 0 : 2 / (t - b) * w;
        matrix.M33 = n == f ? 0 : 2 / (n - f) * w;
        matrix.M41 = r == l ? 0 : (r + l) / (l - r) * w;
        matrix.M42 = t == b ? 0 : (t + b) / (b - t) * w;
        matrix.M43 = n == f ? 0 : (n + f) / (n - f) * w;
        matrix.M44 = w;
        return matrix;
    }

    public static void SetOrtho(int top, int bottom, int left, int right, int near, int far, int scaleW)
    {
        Projection = OrthoMatrix(top / FixedOne, bottom / FixedOne, left / FixedOne, right / FixedOne, near / FixedOne, far / FixedOne, scaleW / FixedOne);
    }

    public static void FlushGlobalState()
    {
        projection = Projection;
        position = View;
        vector = View;
        vector.M41 = 0;
        vector.M42 = 0;
        vector.M43 = 0;
        for (int i = 0; i < 4; i++)
        {
            lightVectors[i] = Vector3.TransformNormal(globalLightVectors[i], vector);
            lightColors[i] = ColorVector(globalLightColors[i]);
        }
        SetDiffuseAmbient(globalDiffuseAmbient);
        SetSpecularEmission(globalSpecularEmission);
        polygonAttr = globalPolygonAttr;
        Matrix4x4 rotation = ToMatrix(BaseRotation);
        Vector3 scale = ToVector(BaseScale);
        Matrix4x4 local = Matrix4x4.CreateScale(scale) * rotation;
        Matrix4x4 translated = local;
        translated.M41 = BaseTranslation.X / FixedOne;
        translated.M42 = BaseTranslation.Y / FixedOne;
        translated.M43 = BaseTranslation.Z / FixedOne;
        position = translated * position;
        vector = rotation * vector;
        matrixMode = ModePositionVector;
    }

    public static void PushProjection()
    {
        projectionStack.Push(projection);
    }

    public static void TranslateProjection(int x, int y, int z)
    {
        projection = Matrix4x4.CreateTranslation(x / FixedOne, y / FixedOne, z / FixedOne) * projection;
    }

    public static void PopProjection()
    {
        if (projectionStack.Count > 0)
        {
            projection = projectionStack.Pop();
        }
    }

    public static void PushMatrix()
    {
        if (stackPointer < StackSize)
        {
            positionStack[stackPointer] = position;
            vectorStack[stackPointer] = vector;
        }
        stackPointer++;
    }

    public static void PopMatrix(int count)
    {
        stackPointer = Math.Clamp(stackPointer - count, 0, StackSize - 1);
        position = positionStack[stackPointer];
        vector = vectorStack[stackPointer];
    }

    public static void StoreMatrix(int index)
    {
        index &= StackSize - 1;
        positionStack[index] = position;
        vectorStack[index] = vector;
    }

    public static void RestoreMatrix(int index)
    {
        index &= StackSize - 1;
        position = positionStack[index];
        vector = vectorStack[index];
    }

    public static Matrix4x4 PositionMatrix
    {
        get => position;
        set => position = value;
    }

    public static Matrix4x4 VectorMatrix
    {
        get => vector;
        set => vector = value;
    }

    public static Matrix4x4 TextureMatrix
    {
        get => texture;
        set => texture = value;
    }

    public static Matrix4x4 ProjectionMatrix
    {
        get => projection;
        set => projection = value;
    }

    public static Matrix4x4 StackPosition(int index)
    {
        return positionStack[index & (StackSize - 1)];
    }

    public static Matrix4x4 StackVector(int index)
    {
        return vectorStack[index & (StackSize - 1)];
    }

    public static void Identity()
    {
        position = Matrix4x4.Identity;
        vector = Matrix4x4.Identity;
    }

    public static void Multiply(in Matrix4x4 matrix)
    {
        position = matrix * position;
        Vector3 x = new(matrix.M11, matrix.M12, matrix.M13);
        Vector3 y = new(matrix.M21, matrix.M22, matrix.M23);
        Vector3 z = new(matrix.M31, matrix.M32, matrix.M33);
        float lengthX = x.Length();
        float lengthY = y.Length();
        float lengthZ = z.Length();
        if (lengthX > 0)
        {
            x /= lengthX;
        }
        if (lengthY > 0)
        {
            y /= lengthY;
        }
        if (lengthZ > 0)
        {
            z /= lengthZ;
        }
        Matrix4x4 rotation = new(x.X, x.Y, x.Z, 0, y.X, y.Y, y.Z, 0, z.X, z.Y, z.Z, 0, 0, 0, 0, 1);
        vector = rotation * vector;
    }

    public static void Translate(int x, int y, int z)
    {
        position = Matrix4x4.CreateTranslation(x / FixedOne, y / FixedOne, z / FixedOne) * position;
    }

    public static void Scale(int x, int y, int z)
    {
        position = Matrix4x4.CreateScale(x / FixedOne, y / FixedOne, z / FixedOne) * position;
    }

    public static short Sin(int index)
    {
        sinCosTable ??= LoadSinCosTable();
        return sinCosTable[(index & 0xFFF) * 2];
    }

    public static short Cos(int index)
    {
        sinCosTable ??= LoadSinCosTable();
        return sinCosTable[(index & 0xFFF) * 2 + 1];
    }

    private static short[] LoadSinCosTable()
    {
        ushort[] raw = RomData.Arm9U16(0x020F983C, 4096 * 2);
        short[] table = new short[raw.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            table[i] = (short)raw[i];
        }
        return table;
    }

    public static void RotateX(int sin, int cos)
    {
        float s = sin / FixedOne;
        float c = cos / FixedOne;
        Multiply(new Matrix4x4(1, 0, 0, 0, 0, c, s, 0, 0, -s, c, 0, 0, 0, 0, 1));
    }

    public static void RotateY(int sin, int cos)
    {
        float s = sin / FixedOne;
        float c = cos / FixedOne;
        Multiply(new Matrix4x4(c, 0, -s, 0, 0, 1, 0, 0, s, 0, c, 0, 0, 0, 0, 1));
    }

    public static void RotateZ(int sin, int cos)
    {
        float s = sin / FixedOne;
        float c = cos / FixedOne;
        Multiply(new Matrix4x4(c, s, 0, 0, -s, c, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1));
    }

    private static Vector3 ColorVector(uint value)
    {
        return new Vector3(value & 0x1F, (value >> 5) & 0x1F, (value >> 10) & 0x1F);
    }

    public static void SetTexImageParam(uint value)
    {
        texImageParam = value;
    }

    public static void SetPaletteBase(uint value)
    {
        paletteBase = value & 0x1FFF;
    }

    public static void SetDiffuseAmbient(uint value)
    {
        diffuse = ColorVector(value);
        ambient = ColorVector(value >> 16);
        if ((value & 0x8000) != 0)
        {
            color = diffuse;
        }
    }

    public static void SetSpecularEmission(uint value)
    {
        specular = ColorVector(value);
        emission = ColorVector(value >> 16);
    }

    public static void SetPolygonAttr(uint value)
    {
        polygonAttr = value;
        if (primitiveType < 0)
        {
            activePolygonAttr = value;
        }
    }

    public static void SetLightVector(int light, float x, float y, float z)
    {
        lightVectors[light] = Vector3.TransformNormal(new Vector3(x, y, z), vector);
    }

    public static void SetLightColor(int light, uint value)
    {
        lightColors[light] = ColorVector(value);
    }

    public static void SetColor(uint value)
    {
        color = ColorVector(value);
    }

    public static bool BoxTest(short x, short y, short z, short width, short height, short depth)
    {
        Matrix4x4 clip = position * projection;
        Span<Vector4> corners = stackalloc Vector4[8];
        for (int i = 0; i < 8; i++)
        {
            float cornerX = (x + ((i & 1) != 0 ? width : 0)) / FixedOne;
            float cornerY = (y + ((i & 2) != 0 ? height : 0)) / FixedOne;
            float cornerZ = (z + ((i & 4) != 0 ? depth : 0)) / FixedOne;
            corners[i] = Vector4.Transform(new Vector4(cornerX, cornerY, cornerZ, 1), clip);
        }
        for (int plane = 0; plane < 6; plane++)
        {
            bool allOutside = true;
            for (int i = 0; i < 8; i++)
            {
                Vector4 corner = corners[i];
                float distance = plane switch
                {
                    0 => corner.W + corner.X,
                    1 => corner.W - corner.X,
                    2 => corner.W + corner.Y,
                    3 => corner.W - corner.Y,
                    4 => corner.W + corner.Z,
                    _ => corner.W - corner.Z,
                };
                if (distance >= 0)
                {
                    allOutside = false;
                    break;
                }
            }
            if (allOutside)
            {
                return false;
            }
        }
        return true;
    }

    public static void Begin(int type)
    {
        primitiveType = type & 3;
        primitiveCount = 0;
        stripOdd = false;
        activePolygonAttr = polygonAttr;
    }

    public static void End()
    {
        primitiveType = -1;
    }

    public static void TexCoord(float s, float t)
    {
        rawS = s;
        rawT = t;
        if ((texImageParam >> 30) == 1)
        {
            texS = s * texture.M11 + t * texture.M21 + (texture.M31 + texture.M41) / 16f;
            texT = s * texture.M12 + t * texture.M22 + (texture.M32 + texture.M42) / 16f;
        }
        else
        {
            texS = s;
            texT = t;
        }
    }

    public static void Normal(float x, float y, float z)
    {
        if ((texImageParam >> 30) == 2)
        {
            texS = rawS + (x * texture.M11 + y * texture.M21 + z * texture.M31) / 16f;
            texT = rawT + (x * texture.M12 + y * texture.M22 + z * texture.M32) / 16f;
        }
        Vector3 normal = Vector3.TransformNormal(new Vector3(x, y, z), vector);
        Vector3 result = emission;
        for (int i = 0; i < 4; i++)
        {
            if ((activePolygonAttr & (1u << i)) == 0)
            {
                continue;
            }
            Vector3 light = lightVectors[i];
            float diffuseLevel = Math.Max(0, -Vector3.Dot(light, normal));
            Vector3 half = (light + new Vector3(0, 0, -1)) * 0.5f;
            float shine = Math.Max(0, -Vector3.Dot(half, normal));
            shine = Math.Max(0, 2 * shine * shine - 1);
            Vector3 lightColor = lightColors[i] / 31f;
            result += specular * lightColor * shine + diffuse * lightColor * Math.Min(1, diffuseLevel) + ambient * lightColor;
        }
        color = Vector3.Min(result, new Vector3(31, 31, 31));
    }

    public static void Vertex(float x, float y, float z)
    {
        lastX = x;
        lastY = y;
        lastZ = z;
        if (primitiveType < 0)
        {
            return;
        }
        Vector4 clip = Vector4.Transform(new Vector4(x, y, z, 1), position * projection);
        float s = texS;
        float t = texT;
        if ((texImageParam >> 30) == 3)
        {
            s = rawS + (x * texture.M11 + y * texture.M21 + z * texture.M31) / 16f;
            t = rawT + (x * texture.M12 + y * texture.M22 + z * texture.M32) / 16f;
        }
        GeomVertex vertex = new() { X = clip.X, Y = clip.Y, Z = clip.Z, W = clip.W, R = color.X, G = color.Y, B = color.Z, S = s, T = t };
        switch (primitiveType)
        {
            case 0:
                primitive[primitiveCount++] = vertex;
                if (primitiveCount == 3)
                {
                    Submit(primitive[0], primitive[1], primitive[2]);
                    primitiveCount = 0;
                }
                break;
            case 1:
                primitive[primitiveCount++] = vertex;
                if (primitiveCount == 4)
                {
                    Submit(primitive[0], primitive[1], primitive[2], primitive[3]);
                    primitiveCount = 0;
                }
                break;
            case 2:
                primitive[primitiveCount++] = vertex;
                if (primitiveCount == 3)
                {
                    if (stripOdd)
                    {
                        Submit(primitive[1], primitive[0], primitive[2]);
                    }
                    else
                    {
                        Submit(primitive[0], primitive[1], primitive[2]);
                    }
                    stripOdd = !stripOdd;
                    primitive[0] = primitive[1];
                    primitive[1] = primitive[2];
                    primitiveCount = 2;
                }
                break;
            default:
                primitive[primitiveCount++] = vertex;
                if (primitiveCount == 4)
                {
                    Submit(primitive[0], primitive[1], primitive[3], primitive[2]);
                    primitive[0] = primitive[2];
                    primitive[1] = primitive[3];
                    primitiveCount = 2;
                }
                break;
        }
    }

    private static void Submit(in GeomVertex a, in GeomVertex b, in GeomVertex c)
    {
        clipA[0] = a;
        clipA[1] = b;
        clipA[2] = c;
        SubmitPolygon(3);
    }

    private static void Submit(in GeomVertex a, in GeomVertex b, in GeomVertex c, in GeomVertex d)
    {
        clipA[0] = a;
        clipA[1] = b;
        clipA[2] = c;
        clipA[3] = d;
        SubmitPolygon(4);
    }

    private static float PlaneDistance(in GeomVertex v, int plane)
    {
        return plane switch
        {
            0 => v.W + v.X,
            1 => v.W - v.X,
            2 => v.W + v.Y,
            3 => v.W - v.Y,
            4 => v.W + v.Z,
            _ => v.W - v.Z,
        };
    }

    private static GeomVertex Lerp(in GeomVertex a, in GeomVertex b, float t)
    {
        return new GeomVertex
        {
            X = a.X + (b.X - a.X) * t,
            Y = a.Y + (b.Y - a.Y) * t,
            Z = a.Z + (b.Z - a.Z) * t,
            W = a.W + (b.W - a.W) * t,
            R = a.R + (b.R - a.R) * t,
            G = a.G + (b.G - a.G) * t,
            B = a.B + (b.B - a.B) * t,
            S = a.S + (b.S - a.S) * t,
            T = a.T + (b.T - a.T) * t,
        };
    }

    private static void SubmitPolygon(int count)
    {
        uint attr = activePolygonAttr;
        GeomVertex v0 = clipA[0];
        GeomVertex v1 = clipA[1];
        GeomVertex v2 = clipA[2];
        float facing = v0.X * (v1.Y * v2.W - v2.Y * v1.W) + v1.X * (v2.Y * v0.W - v0.Y * v2.W) + v2.X * (v0.Y * v1.W - v1.Y * v0.W);
        if (count == 4 && facing == 0)
        {
            GeomVertex v3 = clipA[3];
            facing = v0.X * (v2.Y * v3.W - v3.Y * v2.W) + v2.X * (v3.Y * v0.W - v0.Y * v3.W) + v3.X * (v0.Y * v2.W - v2.Y * v0.W);
        }
        if (facing > 0 && (attr & 0x80) == 0)
        {
            return;
        }
        if (facing < 0 && (attr & 0x40) == 0)
        {
            return;
        }
        GeomVertex[] source = clipA;
        GeomVertex[] target = clipB;
        for (int plane = 0; plane < 6 && count >= 3; plane++)
        {
            int produced = 0;
            for (int i = 0; i < count; i++)
            {
                GeomVertex current = source[i];
                GeomVertex next = source[(i + 1) % count];
                float currentDistance = PlaneDistance(current, plane);
                float nextDistance = PlaneDistance(next, plane);
                if (currentDistance >= 0)
                {
                    if (produced < MaxClipVertices)
                    {
                        target[produced++] = current;
                    }
                }
                if ((currentDistance >= 0) != (nextDistance >= 0))
                {
                    float t = currentDistance / (currentDistance - nextDistance);
                    if (produced < MaxClipVertices)
                    {
                        target[produced++] = Lerp(current, next, t);
                    }
                }
            }
            count = produced;
            (source, target) = (target, source);
        }
        if (count < 3)
        {
            return;
        }
        (int left, int top, int right, int bottom) = Viewport;
        float viewWidth = right - left + 1;
        float viewHeight = bottom - top + 1;
        GeomVertex[] vertices = new GeomVertex[count];
        for (int i = 0; i < count; i++)
        {
            GeomVertex v = source[i];
            float inverse = v.W != 0 ? 1f / v.W : 0;
            GeomVertex result = v;
            result.X = left + (v.X * inverse + 1) * 0.5f * viewWidth;
            result.Y = (Height - 1 - bottom) + (1 - v.Y * inverse) * 0.5f * viewHeight;
            result.Z = v.Z * inverse * 0.5f + 0.5f;
            result.W = v.W;
            vertices[i] = result;
        }
        int format = (int)((texImageParam >> 26) & 7);
        int alpha = (int)((attr >> 16) & 0x1F);
        polygons.Add(new Polygon
        {
            Vertices = vertices,
            Attr = attr,
            TexParam = texImageParam,
            PaletteBase = (int)paletteBase,
            Translucent = (alpha > 0 && alpha < 31) || format == FormatA3I5 || format == FormatA5I3,
        });
    }

    public static void DrawSpriteFast(short x, short y, int z, int width, int height, int u0, int v0, int u1, int v1)
    {
        uint savedAttr = activePolygonAttr;
        int savedType = primitiveType;
        Begin(1);
        TexCoord(u0, v0);
        Vertex(x, y, z);
        TexCoord(u0, v1);
        Vertex(x, y + height, z);
        TexCoord(u1, v1);
        Vertex(x + width, y + height, z);
        TexCoord(u1, v0);
        Vertex(x + width, y, z);
        End();
        primitiveType = savedType;
        activePolygonAttr = savedAttr;
    }

    private static Matrix4x4 ReadMatrix(int count, int columns)
    {
        Matrix4x4 m = Matrix4x4.Identity;
        if (columns == 4)
        {
            m = new Matrix4x4(
                (int)parameters[0] / FixedOne, (int)parameters[1] / FixedOne, (int)parameters[2] / FixedOne, (int)parameters[3] / FixedOne,
                (int)parameters[4] / FixedOne, (int)parameters[5] / FixedOne, (int)parameters[6] / FixedOne, (int)parameters[7] / FixedOne,
                (int)parameters[8] / FixedOne, (int)parameters[9] / FixedOne, (int)parameters[10] / FixedOne, (int)parameters[11] / FixedOne,
                (int)parameters[12] / FixedOne, (int)parameters[13] / FixedOne, (int)parameters[14] / FixedOne, (int)parameters[15] / FixedOne);
        }
        else if (count == 12)
        {
            m = new Matrix4x4(
                (int)parameters[0] / FixedOne, (int)parameters[1] / FixedOne, (int)parameters[2] / FixedOne, 0,
                (int)parameters[3] / FixedOne, (int)parameters[4] / FixedOne, (int)parameters[5] / FixedOne, 0,
                (int)parameters[6] / FixedOne, (int)parameters[7] / FixedOne, (int)parameters[8] / FixedOne, 0,
                (int)parameters[9] / FixedOne, (int)parameters[10] / FixedOne, (int)parameters[11] / FixedOne, 1);
        }
        else
        {
            m = new Matrix4x4(
                (int)parameters[0] / FixedOne, (int)parameters[1] / FixedOne, (int)parameters[2] / FixedOne, 0,
                (int)parameters[3] / FixedOne, (int)parameters[4] / FixedOne, (int)parameters[5] / FixedOne, 0,
                (int)parameters[6] / FixedOne, (int)parameters[7] / FixedOne, (int)parameters[8] / FixedOne, 0,
                0, 0, 0, 1);
        }
        return m;
    }

    private static void LoadCurrent(in Matrix4x4 m)
    {
        switch (matrixMode)
        {
            case ModeProjection:
                projection = m;
                break;
            case ModePosition:
                position = m;
                break;
            case ModePositionVector:
                position = m;
                vector = m;
                break;
            default:
                texture = m;
                break;
        }
    }

    private static void MultiplyCurrent(in Matrix4x4 m, bool includeVector)
    {
        switch (matrixMode)
        {
            case ModeProjection:
                projection = m * projection;
                break;
            case ModePosition:
                position = m * position;
                break;
            case ModePositionVector:
                position = m * position;
                if (includeVector)
                {
                    vector = m * vector;
                }
                break;
            default:
                texture = m * texture;
                break;
        }
    }

    private static float Signed10(uint value)
    {
        int v = (int)(value & 0x3FF);
        if (v >= 0x200)
        {
            v -= 0x400;
        }
        return v;
    }

    public static void Execute(byte[] data, int offset, int size)
    {
        int end = offset + size;
        int cursor = offset;
        while (cursor + 4 <= end)
        {
            uint packed = (uint)(data[cursor] | (data[cursor + 1] << 8) | (data[cursor + 2] << 16) | (data[cursor + 3] << 24));
            cursor += 4;
            for (int k = 0; k < 4; k++)
            {
                int command = (int)((packed >> (k * 8)) & 0xFF);
                int count = parameterCounts[command];
                if (cursor + count * 4 > end)
                {
                    return;
                }
                for (int i = 0; i < count; i++)
                {
                    parameters[i] = (uint)(data[cursor] | (data[cursor + 1] << 8) | (data[cursor + 2] << 16) | (data[cursor + 3] << 24));
                    cursor += 4;
                }
                Command(command, count);
            }
        }
    }

    private static void Command(int command, int count)
    {
        uint p = parameters[0];
        switch (command)
        {
            case 0x10:
                matrixMode = (int)(p & 3);
                break;
            case 0x11:
                if (matrixMode == ModeProjection)
                {
                    PushProjection();
                }
                else if (matrixMode != ModeTexture)
                {
                    PushMatrix();
                }
                break;
            case 0x12:
                if (matrixMode == ModeProjection)
                {
                    PopProjection();
                }
                else if (matrixMode != ModeTexture)
                {
                    int amount = (int)(p & 0x3F);
                    if (amount >= 32)
                    {
                        amount -= 64;
                    }
                    PopMatrix(amount);
                }
                break;
            case 0x13:
                StoreMatrix((int)(p & 0x1F));
                break;
            case 0x14:
                RestoreMatrix((int)(p & 0x1F));
                break;
            case 0x15:
                LoadCurrent(Matrix4x4.Identity);
                break;
            case 0x16:
                LoadCurrent(ReadMatrix(16, 4));
                break;
            case 0x17:
                LoadCurrent(ReadMatrix(12, 3));
                break;
            case 0x18:
                MultiplyCurrent(ReadMatrix(16, 4), true);
                break;
            case 0x19:
                MultiplyCurrent(ReadMatrix(12, 3), true);
                break;
            case 0x1A:
                MultiplyCurrent(ReadMatrix(9, 3), true);
                break;
            case 0x1B:
                MultiplyCurrent(Matrix4x4.CreateScale((int)parameters[0] / FixedOne, (int)parameters[1] / FixedOne, (int)parameters[2] / FixedOne), false);
                break;
            case 0x1C:
                MultiplyCurrent(Matrix4x4.CreateTranslation((int)parameters[0] / FixedOne, (int)parameters[1] / FixedOne, (int)parameters[2] / FixedOne), true);
                break;
            case 0x20:
                color = ColorVector(p);
                break;
            case 0x21:
                Normal(Signed10(p) / 512f, Signed10(p >> 10) / 512f, Signed10(p >> 20) / 512f);
                break;
            case 0x22:
                TexCoord((short)(p & 0xFFFF) / 16f, (short)(p >> 16) / 16f);
                break;
            case 0x23:
                Vertex((short)(p & 0xFFFF) / FixedOne, (short)(p >> 16) / FixedOne, (short)(parameters[1] & 0xFFFF) / FixedOne);
                break;
            case 0x24:
                Vertex(Signed10(p) / 64f, Signed10(p >> 10) / 64f, Signed10(p >> 20) / 64f);
                break;
            case 0x25:
                Vertex((short)(p & 0xFFFF) / FixedOne, (short)(p >> 16) / FixedOne, lastZ);
                break;
            case 0x26:
                Vertex((short)(p & 0xFFFF) / FixedOne, lastY, (short)(p >> 16) / FixedOne);
                break;
            case 0x27:
                Vertex(lastX, (short)(p & 0xFFFF) / FixedOne, (short)(p >> 16) / FixedOne);
                break;
            case 0x28:
                Vertex(lastX + Signed10(p) / FixedOne, lastY + Signed10(p >> 10) / FixedOne, lastZ + Signed10(p >> 20) / FixedOne);
                break;
            case 0x29:
                SetPolygonAttr(p);
                break;
            case 0x2A:
                texImageParam = p;
                break;
            case 0x2B:
                paletteBase = p & 0x1FFF;
                break;
            case 0x30:
                SetDiffuseAmbient(p);
                break;
            case 0x31:
                SetSpecularEmission(p);
                break;
            case 0x32:
                SetLightVector((int)(p >> 30), Signed10(p) / 512f, Signed10(p >> 10) / 512f, Signed10(p >> 20) / 512f);
                break;
            case 0x33:
                SetLightColor((int)(p >> 30), p);
                break;
            case 0x40:
                Begin((int)(p & 3));
                break;
            case 0x41:
                End();
                break;
            case 0x50:
                SwapBuffers((int)(p & 1), (int)((p >> 1) & 1));
                break;
            case 0x60:
                SetViewport((int)(p & 0xFF), (int)((p >> 8) & 0xFF), (int)((p >> 16) & 0xFF), (int)(p >> 24));
                break;
        }
    }

    public static void SwapBuffers(int sortMode, int bufferMode)
    {
        SortMode = sortMode;
        BufferMode = bufferMode;
        int clear = ClearAlpha == 0 ? 0 : (int)(ClearColor & 0x7FFF) | (ClearAlpha << 15);
        Array.Fill(Frame, clear);
        Array.Fill(depthBuffer, float.MaxValue);
        Array.Fill(translucentId, (byte)0xFF);
        Array.Fill(fogFlags, clearFog);
        foreach (Polygon polygon in polygons)
        {
            if (!polygon.Translucent)
            {
                Rasterize(polygon);
            }
        }
        foreach (Polygon polygon in polygons)
        {
            if (polygon.Translucent)
            {
                Rasterize(polygon);
            }
        }
        polygons.Clear();
        if (FogEnabled)
        {
            ApplyFog();
        }
    }

    private static void ApplyFog()
    {
        bool wBuffer = BufferMode != 0;
        bool alphaOnly = FogBlendMode != 0;
        int step = 0x400 >> Math.Clamp(FogSlope, 0, 10);
        int fogRed = (int)(FogColor & 0x1F);
        int fogGreen = (int)((FogColor >> 5) & 0x1F);
        int fogBlue = (int)((FogColor >> 10) & 0x1F);
        int fogAlpha = (int)((FogColor >> 16) & 0x1F);
        for (int pixel = 0; pixel < Frame.Length; pixel++)
        {
            if (!fogFlags[pixel])
            {
                continue;
            }
            int value = Frame[pixel];
            int alpha = (value >> 15) & 0x1F;
            float depth = depthBuffer[pixel];
            int fogDepth;
            if (depth == float.MaxValue)
            {
                fogDepth = 0x7FFF;
            }
            else
            {
                fogDepth = Math.Clamp((int)(wBuffer ? depth * 8 : depth * 0x7FFF), 0, 0x7FFF);
            }
            int distance = fogDepth - FogOffset;
            int density;
            if (distance < step)
            {
                density = FogTable[0];
            }
            else
            {
                int index = distance / step - 1;
                if (index >= 31)
                {
                    density = FogTable[31];
                }
                else
                {
                    int fraction = distance % step;
                    density = (FogTable[index] * (step - fraction) + FogTable[index + 1] * fraction) / step;
                }
            }
            density &= 0x7F;
            if (density == 127)
            {
                density = 128;
            }
            int red = value & 0x1F;
            int green = (value >> 5) & 0x1F;
            int blue = (value >> 10) & 0x1F;
            if (!alphaOnly)
            {
                red = (fogRed * density + red * (128 - density)) >> 7;
                green = (fogGreen * density + green * (128 - density)) >> 7;
                blue = (fogBlue * density + blue * (128 - density)) >> 7;
            }
            alpha = (fogAlpha * density + alpha * (128 - density)) >> 7;
            Frame[pixel] = red | (green << 5) | (blue << 10) | (alpha << 15);
        }
    }

    private static int SampleTexture(uint param, int paletteIndex, float s, float t)
    {
        int format = (int)((param >> 26) & 7);
        int width = 8 << (int)((param >> 20) & 7);
        int height = 8 << (int)((param >> 23) & 7);
        int x = (int)MathF.Floor(s);
        int y = (int)MathF.Floor(t);
        if ((param & 0x10000) != 0)
        {
            if ((param & 0x40000) != 0 && ((x >> BitOperations.Log2((uint)width)) & 1) != 0)
            {
                x = width - 1 - (x & (width - 1));
            }
            else
            {
                x &= width - 1;
            }
        }
        else
        {
            x = Math.Clamp(x, 0, width - 1);
        }
        if ((param & 0x20000) != 0)
        {
            if ((param & 0x80000) != 0 && ((y >> BitOperations.Log2((uint)height)) & 1) != 0)
            {
                y = height - 1 - (y & (height - 1));
            }
            else
            {
                y &= height - 1;
            }
        }
        else
        {
            y = Math.Clamp(y, 0, height - 1);
        }
        byte[] memory = Video.Textures;
        ushort[] palette = Video.TexturePalettes;
        int address = (int)(param & 0xFFFF) << 3;
        int mask = memory.Length - 1;
        int paletteMask = palette.Length - 1;
        bool color0Transparent = (param & 0x20000000) != 0;
        int index;
        switch (format)
        {
            case FormatA3I5:
            {
                int value = memory[(address + y * width + x) & mask];
                int alpha = value >> 5;
                return (palette[(paletteIndex * 8 + (value & 0x1F)) & paletteMask] & 0x7FFF) | (((alpha << 2) + (alpha >> 1)) << 15);
            }
            case Format4Color:
                index = (memory[(address + ((y * width + x) >> 2)) & mask] >> ((x & 3) * 2)) & 3;
                if (index == 0 && color0Transparent)
                {
                    return 0;
                }
                return (palette[(paletteIndex * 4 + index) & paletteMask] & 0x7FFF) | (31 << 15);
            case Format16Color:
                index = (memory[(address + ((y * width + x) >> 1)) & mask] >> ((x & 1) * 4)) & 0xF;
                if (index == 0 && color0Transparent)
                {
                    return 0;
                }
                return (palette[(paletteIndex * 8 + index) & paletteMask] & 0x7FFF) | (31 << 15);
            case Format256Color:
                index = memory[(address + y * width + x) & mask];
                if (index == 0 && color0Transparent)
                {
                    return 0;
                }
                return (palette[(paletteIndex * 8 + index) & paletteMask] & 0x7FFF) | (31 << 15);
            case FormatCompressed:
            {
                int block = (y >> 2) * (width >> 2) + (x >> 2);
                int texels = memory[(address + block * 4 + (y & 3)) & mask];
                int code = (texels >> ((x & 3) * 2)) & 3;
                int infoAddress = 0x20000 + ((address & 0x1FFFF) >> 1) + (address >= 0x40000 ? 0x10000 : 0) + block * 2;
                int info = memory[infoAddress & mask] | (memory[(infoAddress + 1) & mask] << 8);
                int basePalette = paletteIndex * 8 + (info & 0x3FFF) * 2;
                int mode = info >> 14;
                int c0 = palette[basePalette & paletteMask] & 0x7FFF;
                int c1 = palette[(basePalette + 1) & paletteMask] & 0x7FFF;
                if (code == 0)
                {
                    return c0 | (31 << 15);
                }
                if (code == 1)
                {
                    return c1 | (31 << 15);
                }
                if (code == 2)
                {
                    if (mode == 0 || mode == 2)
                    {
                        return (palette[(basePalette + 2) & paletteMask] & 0x7FFF) | (31 << 15);
                    }
                    if (mode == 1)
                    {
                        return Mix(c0, c1, 1, 1, 1) | (31 << 15);
                    }
                    return Mix(c0, c1, 5, 3, 3) | (31 << 15);
                }
                if (mode == 2)
                {
                    return (palette[(basePalette + 3) & paletteMask] & 0x7FFF) | (31 << 15);
                }
                if (mode == 3)
                {
                    return Mix(c0, c1, 3, 5, 3) | (31 << 15);
                }
                return 0;
            }
            case FormatA5I3:
            {
                int value = memory[(address + y * width + x) & mask];
                return (palette[(paletteIndex * 8 + (value & 7)) & paletteMask] & 0x7FFF) | ((value >> 3) << 15);
            }
            case FormatDirect:
            {
                int texelAddress = address + (y * width + x) * 2;
                int value = memory[texelAddress & mask] | (memory[(texelAddress + 1) & mask] << 8);
                return (value & 0x7FFF) | ((value & 0x8000) != 0 ? 31 << 15 : 0);
            }
        }
        return 0x7FFF | (31 << 15);
    }

    private static int Mix(int c0, int c1, int w0, int w1, int shift)
    {
        int r = ((c0 & 0x1F) * w0 + (c1 & 0x1F) * w1) >> shift;
        int g = (((c0 >> 5) & 0x1F) * w0 + ((c1 >> 5) & 0x1F) * w1) >> shift;
        int b = (((c0 >> 10) & 0x1F) * w0 + ((c1 >> 10) & 0x1F) * w1) >> shift;
        return r | (g << 5) | (b << 10);
    }

    private static void Rasterize(Polygon polygon)
    {
        GeomVertex[] v = polygon.Vertices;
        int count = v.Length;
        uint attr = polygon.Attr;
        int polygonMode = (int)((attr >> 4) & 3);
        if (polygonMode == 3)
        {
            return;
        }
        int polygonAlpha = (int)((attr >> 16) & 0x1F);
        if (polygonAlpha == 0)
        {
            return;
        }
        int polygonId = (int)((attr >> 24) & 0x3F);
        bool depthEqual = (attr & 0x4000) != 0;
        bool translucentDepth = (attr & 0x800) != 0;
        bool fogPolygon = (attr & 0x8000) != 0;
        bool textured = ((polygon.TexParam >> 26) & 7) != FormatNone && (Display3DControl & 1) != 0;
        bool alphaTest = (Display3DControl & 4) != 0;
        bool alphaBlend = (Display3DControl & 8) != 0;
        bool wBuffer = BufferMode != 0;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        for (int i = 0; i < count; i++)
        {
            minY = Math.Min(minY, v[i].Y);
            maxY = Math.Max(maxY, v[i].Y);
        }
        int startY = Math.Max(0, (int)MathF.Ceiling(minY - 0.5f));
        int endY = Math.Min(Height - 1, (int)MathF.Ceiling(maxY - 0.5f) - 1);
        Span<float> leftValues = stackalloc float[8];
        Span<float> rightValues = stackalloc float[8];
        Span<float> candidate = stackalloc float[8];
        for (int y = startY; y <= endY; y++)
        {
            float centerY = y + 0.5f;
            float leftX = float.MaxValue;
            float rightX = float.MinValue;
            for (int i = 0; i < count; i++)
            {
                ref GeomVertex a = ref v[i];
                ref GeomVertex b = ref v[(i + 1) % count];
                if (!((a.Y <= centerY && b.Y > centerY) || (b.Y <= centerY && a.Y > centerY)))
                {
                    continue;
                }
                float t = (centerY - a.Y) / (b.Y - a.Y);
                float x = a.X + (b.X - a.X) * t;
                float inverseA = a.W != 0 ? 1f / a.W : 0;
                float inverseB = b.W != 0 ? 1f / b.W : 0;
                candidate[0] = inverseA + (inverseB - inverseA) * t;
                candidate[1] = a.R * inverseA + (b.R * inverseB - a.R * inverseA) * t;
                candidate[2] = a.G * inverseA + (b.G * inverseB - a.G * inverseA) * t;
                candidate[3] = a.B * inverseA + (b.B * inverseB - a.B * inverseA) * t;
                candidate[4] = a.S * inverseA + (b.S * inverseB - a.S * inverseA) * t;
                candidate[5] = a.T * inverseA + (b.T * inverseB - a.T * inverseA) * t;
                candidate[6] = a.Z + (b.Z - a.Z) * t;
                if (x < leftX)
                {
                    leftX = x;
                    candidate.CopyTo(leftValues);
                }
                if (x > rightX)
                {
                    rightX = x;
                    candidate.CopyTo(rightValues);
                }
            }
            if (leftX > rightX)
            {
                continue;
            }
            int startX = Math.Max(0, (int)MathF.Ceiling(leftX - 0.5f));
            int endX = Math.Min(Width - 1, (int)MathF.Ceiling(rightX - 0.5f) - 1);
            float span = rightX - leftX;
            int row = y * Width;
            for (int x = startX; x <= endX; x++)
            {
                float u = span > 0 ? (x + 0.5f - leftX) / span : 0;
                float inverseW = leftValues[0] + (rightValues[0] - leftValues[0]) * u;
                float w = inverseW != 0 ? 1f / inverseW : 0;
                float depth = wBuffer ? w : leftValues[6] + (rightValues[6] - leftValues[6]) * u;
                int pixel = row + x;
                float existing = depthBuffer[pixel];
                if (depthEqual)
                {
                    float tolerance = wBuffer ? Math.Abs(existing) * 0.02f + 0.01f : 0.002f;
                    if (Math.Abs(depth - existing) > tolerance)
                    {
                        continue;
                    }
                }
                else if (depth >= existing)
                {
                    continue;
                }
                float r = (leftValues[1] + (rightValues[1] - leftValues[1]) * u) * w;
                float g = (leftValues[2] + (rightValues[2] - leftValues[2]) * u) * w;
                float b = (leftValues[3] + (rightValues[3] - leftValues[3]) * u) * w;
                int red = Math.Clamp((int)(r + 0.5f), 0, 31);
                int green = Math.Clamp((int)(g + 0.5f), 0, 31);
                int blue = Math.Clamp((int)(b + 0.5f), 0, 31);
                int alpha = polygonAlpha;
                if (polygonMode == 2)
                {
                    int toon = toonTable[red];
                    if ((Display3DControl & 2) == 0)
                    {
                        red = toon & 0x1F;
                        green = (toon >> 5) & 0x1F;
                        blue = (toon >> 10) & 0x1F;
                    }
                }
                if (textured)
                {
                    float s = (leftValues[4] + (rightValues[4] - leftValues[4]) * u) * w;
                    float t = (leftValues[5] + (rightValues[5] - leftValues[5]) * u) * w;
                    int texel = SampleTexture(polygon.TexParam, polygon.PaletteBase, s, t);
                    int texelAlpha = (texel >> 15) & 0x1F;
                    int texelRed = texel & 0x1F;
                    int texelGreen = (texel >> 5) & 0x1F;
                    int texelBlue = (texel >> 10) & 0x1F;
                    if (polygonMode == 1)
                    {
                        red = (texelRed * texelAlpha + red * (31 - texelAlpha)) / 31;
                        green = (texelGreen * texelAlpha + green * (31 - texelAlpha)) / 31;
                        blue = (texelBlue * texelAlpha + blue * (31 - texelAlpha)) / 31;
                    }
                    else
                    {
                        red = ((texelRed + 1) * (red + 1) - 1) >> 5;
                        green = ((texelGreen + 1) * (green + 1) - 1) >> 5;
                        blue = ((texelBlue + 1) * (blue + 1) - 1) >> 5;
                        alpha = ((texelAlpha + 1) * (alpha + 1) - 1) >> 5;
                    }
                }
                if (alpha == 0 || (alphaTest && alpha <= alphaReference))
                {
                    continue;
                }
                int destination = Frame[pixel];
                int destinationAlpha = (destination >> 15) & 0x1F;
                if (alpha < 31)
                {
                    if (translucentId[pixel] == polygonId)
                    {
                        continue;
                    }
                    if (alphaBlend && destinationAlpha != 0)
                    {
                        red = (red * (alpha + 1) + (destination & 0x1F) * (31 - alpha)) >> 5;
                        green = (green * (alpha + 1) + ((destination >> 5) & 0x1F) * (31 - alpha)) >> 5;
                        blue = (blue * (alpha + 1) + ((destination >> 10) & 0x1F) * (31 - alpha)) >> 5;
                        alpha = Math.Max(alpha, destinationAlpha);
                    }
                    translucentId[pixel] = (byte)polygonId;
                    if (translucentDepth)
                    {
                        depthBuffer[pixel] = depth;
                    }
                }
                else
                {
                    depthBuffer[pixel] = depth;
                    translucentId[pixel] = 0xFF;
                    fogFlags[pixel] = fogPolygon;
                }
                Frame[pixel] = red | (green << 5) | (blue << 10) | (alpha << 15);
            }
        }
    }
}
