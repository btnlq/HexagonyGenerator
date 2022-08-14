namespace HexagonyGenerator.Hexagony.Compiler;

using Bytecode;

partial class Memory
{
    public void Set(Variable variableDest, ISymbol left, BinOp op, ISymbol right)
    {
        var dest = new Register(variableDest);

        bool swap = op == BinOp.Add || op == BinOp.Sub || op == BinOp.Mul;
        swap = PrepareSwitch(dest, left, swap, right);

        _grid.CallBinOp(dest, op.ToCommand());
        if (swap && op == BinOp.Sub)
            CallOp(dest, Command.Negate);
    }

    private bool PrepareSwitch(Register dest, ISymbol left, bool swap, ISymbol right) =>
        left is VariableSymbol leftVar ?
            right is VariableSymbol rightVar ?
                Prepare(dest, leftVar, swap, rightVar) :
                Prepare(dest, leftVar, swap, right) :
            right is VariableSymbol rightVar2 ?
                Prepare(dest, left, swap, rightVar2) :
                Prepare(dest, left, right);

    private bool Prepare(Register dest, VariableSymbol left, bool swap, VariableSymbol right)
    {
        if (left.Variable.Is(right.Variable))
        {
            var variable = left.Variable;
            int sign = dest.ClosestNeighbourTo(variable);
            if (sign > 0)
            {
                Set(dest.Right, new Register(variable));
                Set(dest.Left, dest.Right);
                Modify(dest.Left, left);
                Modify(dest.Right, right);
                return false;
            }
            if (sign < 0)
            {
                Set(dest.Left, new Register(variable));
                Set(dest.Right, dest.Left);
                Modify(dest.Right, right);
                Modify(dest.Left, left);
                return false;
            }
        }

        swap = swap && dest.ClosestNeighbourTo(left.Variable) > dest.ClosestNeighbourTo(right.Variable);

        if (swap)
            (right, left) = (left, right);

        if (dest.ClosestNeighbourTo(left.Variable) > 0)
        {
            if (dest.ClosestNeighbourTo(right.Variable) < 0)
            {
                Set(dest, left);
                Set(dest.Right, right);
                Set(dest.Left, dest);
            }
            else
            {
                Set(dest.Left, left);
                Set(dest.Right, right);
            }
        }
        else
        {
            Set(dest.Right, right);
            Set(dest.Left, left);
        }

        return swap;
    }

    private bool Prepare(Register dest, VariableSymbol left, bool swap, ISymbol rightSymbol)
    {
        swap = swap && dest.ClosestNeighbourTo(left.Variable) > 0;

        if (swap)
        {
            Set(dest.Right, left);
            Set(dest.Left, rightSymbol);
        }
        else
        {
            Set(dest.Left, left);
            Set(dest.Right, rightSymbol);
        }

        return swap;
    }

    private bool Prepare(Register dest, ISymbol leftSymbol, bool swap, VariableSymbol right)
    {
        swap = swap && dest.ClosestNeighbourTo(right.Variable) < 0;

        if (swap)
        {
            Set(dest.Left, right);
            Set(dest.Right, leftSymbol);
        }
        else
        {
            Set(dest.Right, right);
            Set(dest.Left, leftSymbol);
        }

        return swap;
    }

    private bool Prepare(Register dest, ISymbol leftSymbol, ISymbol rightSymbol)
    {
        Set(dest.Left, leftSymbol);
        Set(dest.Right, rightSymbol);
        return false;
    }
}

static class BinOpEx
{
    public static char ToCommand(this BinOp op) => (char)op;
}
