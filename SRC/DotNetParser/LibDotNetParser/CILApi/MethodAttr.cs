using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDotNetParser.CILApi
{
    internal enum MethodAttr
    {
        // member access mask - Use this mask to retrieve 

        // accessibility information.

        mdMemberAccessMask = 0x0007,
        mdPrivateScope = 0x0000,
        // Member not referenceable.

        mdPrivate = 0x0001,
        // Accessible only by the parent type.

        mdFamANDAssem = 0x0002,
        // Accessible by sub-types only in this Assembly.

        mdAssem = 0x0003,
        // Accessibly by anyone in the Assembly.

        mdFamily = 0x0004,
        // Accessible only by type and sub-types.

        mdFamORAssem = 0x0005,
        // Accessibly by sub-types anywhere, plus anyone in assembly.

        mdPublic = 0x0006,
        // Accessibly by anyone who has visibility to this scope.

        // end member access mask


        // method contract attributes.

        mdStatic = 0x0010,
        // Defined on type, else per instance.

        mdFinal = 0x0020,
        // Method may not be overridden.

        mdVirtual = 0x0040,
        // Method virtual.

        mdHideBySig = 0x0080,
        // Method hides by name+sig, else just by name.


        // vtable layout mask - Use this mask to retrieve vtable attributes.

        mdVtableLayoutMask = 0x0100,
        mdReuseSlot = 0x0000,     // The default.

        mdNewSlot = 0x0100,
        // Method always gets a new slot in the vtable.

        // end vtable layout mask


        // method implementation attributes.

        mdCheckAccessOnOverride = 0x0200,
        // Overridability is the same as the visibility.

        mdAbstract = 0x0400,
        // Method does not provide an implementation.

        mdSpecialName = 0x0800,
        // Method is special. Name describes how.


        // interop attributes

        mdPinvokeImpl = 0x2000,
        // Implementation is forwarded through pinvoke.

        mdUnmanagedExport = 0x0008,
        // Managed method exported via thunk to unmanaged code.


        // Reserved flags for runtime use only.

        mdReservedMask = 0xd000,
        mdRTSpecialName = 0x1000,
        // Runtime should check name encoding.

        mdHasSecurity = 0x4000,
        // Method has security associate with it.

        mdRequireSecObject = 0x8000,
        // Method calls another method containing security code.
    }

    public enum MethodImp
    {
        // code impl mask
        miCodeTypeMask = 0x0003,   // Flags about code type.
        miIL = 0x0000,   // Method impl is IL.
        miNative = 0x0001,   // Method impl is native.
        miOPTIL = 0x0002,   // Method impl is OPTIL
        miRuntime = 0x0003,   // Method impl is provided by the runtime.
                              // end code impl mask

        // managed mask
        miManagedMask = 0x0004,   // Flags specifying whether the code is managed
                                  // or unmanaged.
        miUnmanaged = 0x0004,   // Method impl is unmanaged, otherwise managed.
        miManaged = 0x0000,   // Method impl is managed.
                              // end managed mask

        // implementation info and interop
        miForwardRef = 0x0010,   // Indicates method is defined; used primarily
                                 // in merge scenarios.
        miPreserveSig = 0x0080,   // Indicates method sig is not to be mangled to
                                  // do HRESULT conversion.

        miInternalCall = 0x1000,   // Reserved for internal use.

        miSynchronized = 0x0020,   // Method is single threaded through the body.
        miNoInlining = 0x0008,   // Method may not be inlined.
        miMaxMethodImplVal = 0xffff,   // Range check value
    }
}
