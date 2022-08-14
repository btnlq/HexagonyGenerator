namespace HexagonyGenerator.Hexagony.Compiler;

using Bytecode;

partial class Memory
{
    public void Set(Variable dest, ModifiableSymbol conditionSymbol, ISymbol trueValue, ISymbol falseValue)
    {
        Register destReg = new(dest);
        bool swap;

        var conditionVar = conditionSymbol as VariableSymbol;

        if (conditionVar == null || conditionVar.Variable.Is(dest))
        {
            swap = PrepareSwitch(destReg, falseValue, true, trueValue);
            Set(destReg, conditionSymbol);
        }
        else
        {
            var trueVar = trueValue as VariableSymbol;
            var falseVar = falseValue as VariableSymbol;

            if (trueVar != null && trueVar.Variable.Is(dest))
            {
                if (falseVar != null && falseVar.Variable.Is(dest))
                {
                    if (destReg.ClosestNeighbourTo(conditionVar.Variable) > 0)
                    {
                        Set(destReg.Left, destReg);
                        Set(destReg, conditionVar);
                        Set(destReg.Right, destReg.Left);
                        Modify(destReg.Right, trueVar);
                        Modify(destReg.Left, falseVar);
                    }
                    else
                    {
                        Set(destReg.Right, destReg);
                        Set(destReg, conditionVar);
                        Set(destReg.Left, destReg.Right);
                        Modify(destReg.Left, falseVar);
                        Modify(destReg.Right, trueVar);
                    }
                    swap = false;
                }
                else
                {
                    swap = Prepare(conditionVar, trueVar, falseValue, 1);
                }
            }
            else
            {
                if (falseVar != null && falseVar.Variable.Is(dest))
                {
                    swap = Prepare(conditionVar, falseVar, trueValue, -1);
                }
                else
                {
                    Set(destReg, conditionVar);

                    if (falseVar != null && falseVar.Variable.Is(conditionVar.Variable))
                        falseValue = ChangeLocation(falseVar, dest);
                    if (trueVar != null && trueVar.Variable.Is(conditionVar.Variable))
                        trueValue = ChangeLocation(trueVar, dest);

                    swap = PrepareSwitch(destReg, falseValue, true, trueValue);
                }
            }
        }

        if (swap)
        {
            CallOp(destReg, Command.Negate);
            CallOp(destReg, Command.Increment);
        }
        _grid.CallBinOp(destReg, Command.Copy);
    }

    private bool Prepare(VariableSymbol conditionVar, VariableSymbol destVar, ISymbol otherValue, int destSign)
    {
        Register destReg = new(destVar.Variable);
        var otherVar = otherValue as VariableSymbol;

        bool swap = otherVar != null && destReg.ClosestNeighbourTo(otherVar.Variable) == destSign;

        if (swap)
            destSign = -destSign;

        if (destReg.ClosestNeighbourTo(conditionVar.Variable) == destSign)
        {
            Set(destReg.Neighbour(-destSign), destVar);
            Set(destReg, conditionVar);
            Set(destReg.Neighbour(destSign), destReg.Neighbour(-destSign));
            Set(destReg.Neighbour(-destSign), otherValue);
        }
        else
        {
            Set(destReg.Neighbour(destSign), destVar);
            Set(destReg, conditionVar);
            if (otherVar != null && otherVar.Variable.Is(conditionVar.Variable))
                Modify(destReg.Neighbour(-destSign), otherVar);
            else
                Set(destReg.Neighbour(-destSign), otherValue);
        }

        return swap;
    }

    private static VariableSymbol ChangeLocation(VariableSymbol source, Variable newLocation)
    {
        var variable = new VariableSymbol(newLocation);
        foreach (var modifier in source.Modifiers)
            variable.Modify(modifier);
        return variable;
    }
}
