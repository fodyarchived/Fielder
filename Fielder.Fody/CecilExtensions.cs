using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public static class CecilExtensions
{
    public static bool IsRefOrOut(this Instruction next)
    {
        if (next.OpCode != OpCodes.Call && next.OpCode != OpCodes.Calli)
        {
            return false;
        }

        if (!(next.Operand is MethodReference methodReference))
        {
            return false;
        }
        return methodReference.Parameters.Any(x => x.IsOut || x.ParameterType.Name.EndsWith("&"));
    }
}