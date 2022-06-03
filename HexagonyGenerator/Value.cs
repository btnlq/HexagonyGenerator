global using Value = System.Numerics.BigInteger;

namespace HexagonyGenerator;

static class ValueEx
{
    public static Value Div(this Value left, Value right)
    {
        if ((left.Sign ^ right.Sign) >= 0)
            return left / right;
        var div = Value.DivRem(left, right, out var mod);
        if (!mod.IsZero)
            div--;
        return div;
    }

    public static Value Mod(this Value left, Value right)
    {
        var mod = left % right;
        if ((left.Sign ^ right.Sign) < 0 && !mod.IsZero)
            mod += right;
        return mod;
    }
}
