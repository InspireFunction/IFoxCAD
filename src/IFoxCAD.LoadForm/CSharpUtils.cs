namespace IFoxCAD.LoadEx;

internal class CSharpUtils
{
    private static unsafe long* addrOfOrgMethodAddr;
    private static unsafe long* orgMethodAddr;

    public static bool ReplaceMethod(Type targetType, string targetMethod, 
                                     Type injectType, string injectMethod, 
                                     BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, 
                                     Binder? binder = null,
                                     CallingConventions callConvention = CallingConventions.Any, 
                                     Type[]? types = null, 
                                     ParameterModifier[]? modifiers = null)
    {
        if (types == null)
            types = Type.EmptyTypes;

        MethodInfo tarMethod = targetType.GetMethod(targetMethod, bindingAttr, binder, callConvention, types, modifiers);
        MethodInfo injMethod = injectType.GetMethod(injectMethod, bindingAttr, binder, callConvention, types, modifiers);

        var tarMethods = injectType.GetMethods(
           BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        var injMethods = injectType.GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        foreach (var item in tarMethods)
            if (item.Name.Equals(targetMethod))
                tarMethod = item;
        foreach (var item2 in injMethods)
            if (item2.Name.Equals(injectMethod))
                injMethod = item2;

        if (tarMethod == null || injMethod == null)
            return false;
        RuntimeHelpers.PrepareMethod(tarMethod.MethodHandle);
        RuntimeHelpers.PrepareMethod(injMethod.MethodHandle);
        unsafe
        {
            if (IntPtr.Size == 4)
            {
                addrOfOrgMethodAddr = (long*)((int*)tarMethod.MethodHandle.Value.ToPointer() + 2);
                orgMethodAddr = (long*)*addrOfOrgMethodAddr;
                int* tar = (int*)tarMethod.MethodHandle.Value.ToPointer() + 2;
                int* inj = (int*)injMethod.MethodHandle.Value.ToPointer() + 2;
                *tar = *inj;
            }
            else
            {
                addrOfOrgMethodAddr = (long*)tarMethod.MethodHandle.Value.ToPointer() + 1;
                orgMethodAddr = (long*)*addrOfOrgMethodAddr;
                long* tar = (long*)tarMethod.MethodHandle.Value.ToPointer() + 1;
                long* inj = (long*)injMethod.MethodHandle.Value.ToPointer() + 1;
                *tar = *inj;
            }
        }
        return true;
    }
    public unsafe static void RestoreMethod()
    {
        if ((long)orgMethodAddr != 0 && (long)addrOfOrgMethodAddr != 0)
        {
            if (IntPtr.Size == 4)
                *(int*)addrOfOrgMethodAddr = (int)orgMethodAddr;
            else
                *addrOfOrgMethodAddr = (long)orgMethodAddr;
        }
    }
}
