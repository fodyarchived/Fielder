using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public static class CecilExtensions
{

    public static bool IsRefOrOut(this Instruction next)
    {
        if (next.OpCode == OpCodes.Call || next.OpCode == OpCodes.Calli)
        {
            var methodReference = next.Operand as MethodReference;
            if (methodReference != null)
            {
                return methodReference.Parameters.Any(x => x.IsOut || x.ParameterType.Name.EndsWith("&"));
            }
        }
        return false;
    }

}