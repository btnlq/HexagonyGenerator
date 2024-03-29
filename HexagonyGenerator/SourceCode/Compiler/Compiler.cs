﻿namespace HexagonyGenerator.SourceCode.Compiler;

using Bytecode;

class Compiler
{
    public static Program Compile(SourceCode.Program program) => new Compiler().CompileProgram(program);
    public static Program Compile(string code) => Compile(Parser.Parser.Parse(code));

    private readonly Dictionary<Block, (Procedure Start, Procedure Next)> _pointers = new();

    private Compiler() { }

    private void CompileBlock(Block block, Procedure current, Procedure next)
    {
        foreach (var statement in block)
        {
            switch (statement)
            {
                case ISimpleAction action:
                {
                    current.Actions.Add(action.Action);
                    break;
                }
                case BlockStatement blockStmt:
                {
                    var blockCurrent = new Procedure();
                    var blockNext = new Procedure();

                    _pointers.Add(blockStmt.Block, (blockCurrent, blockNext));
                    CompileBlock(blockStmt.Block, blockCurrent, blockStmt.Block.IsLoop != null ? blockCurrent : blockNext);
                    _pointers.Remove(blockStmt.Block);

                    current.Continuation = new Continuation(blockCurrent);
                    current = blockNext;
                    break;
                }
                case Conditional conditional:
                {
                    Procedure trueBranch;
                    Procedure falseBranch;
                    var conditionalNext = new Procedure();

                    if (conditional.TrueBranch != null)
                        CompileBlock(conditional.TrueBranch, trueBranch = new Procedure(), conditionalNext);
                    else
                        trueBranch = conditionalNext;

                    if (conditional.FalseBranch != null)
                        CompileBlock(conditional.FalseBranch, falseBranch = new Procedure(), conditionalNext);
                    else
                        falseBranch = conditionalNext;

                    current.Continuation = new ConditionalContinuation(
                        conditional.Symbol, conditional.Type, trueBranch, falseBranch);
                    current = conditionalNext;
                    break;
                }
                case Goto @goto:
                {
                    var (Start, Next) = _pointers[@goto.Block];
                    var nextProcedure = @goto.Type switch
                    {
                        GotoType.Break => Next,
                        GotoType.Continue => Start,
                        _ => throw new UnexpectedDefaultException(),
                    };
                    current.Continuation = new Continuation(nextProcedure);
                    return;
                }
                case Exit:
                    current.Continuation = new Continuation(Procedure.Exit);
                    return;
                default:
                    throw new UnexpectedDefaultException();
            }
        }

        current.Continuation = new Continuation(next);
    }

    private Program CompileProgram(SourceCode.Program program)
    {
        VariableAllocator.Allocate(program);
        var start = new Procedure();
        CompileBlock(program.Block, start, Procedure.Exit);
        Program procedureProgram = ContinuationsOptimizer.Optimize(start);
        return procedureProgram;
    }
}
