using XSharp;
using XSharp.Assembler;
using Cosmos.IL2CPU.API.Attribs;

namespace Aura_OS.System.Threading
{
    public delegate void aMethod();
    public delegate void nMethod(uint param);
    public delegate void oMethod(object obj);
    public delegate void ParamMethod1(object arg1);
    public delegate void ParamMethod2(object arg1, object arg2);
    public delegate void ParamMethod3(object arg1, object arg2, object arg3);
    public delegate void ParamMethod4(object arg1, object arg2, object arg3, object arg4);
    public delegate void ParamMethod5(object arg1, object arg2, object arg3, object arg4, object arg5);
    public unsafe delegate void pMethod(void* param);

    public static unsafe class Utils
    {
        public static void memcpy(byte* dst, byte* src, uint len)
        {
            memcpy(dst, src, (int)len);
        }

        [PlugMethod(PlugRequired = true)]
        public static void memcpy(byte* dst, byte* src, int len) { }

        [PlugMethod(PlugRequired = true)]
        public static void memcpy(uint* dst, uint* src, int len) { }

        [PlugMethod(PlugRequired = true)]
        public static void memset(byte* dst, byte value, uint len) { }

        [PlugMethod(PlugRequired = true)]
        public static void memset(uint* dst, uint value, uint len) { }

        public static uint allign4K(uint value)
        {
            value = value >> 12;
            value = value << 12;
            return value;
        }

        public static bool isOdd(int x)
        {
            return (x % 2) == 0;
        }

        public static bool toBool(int intValue)
        {
            return intValue != 0;
        }

        public static int toInt(bool val)
        {
            return val ? 1 : 0;
        }

        public static bool isDigit(byte aChar)
        {
            return (aChar >= '0' && aChar <= '9');
        }

        public static void strDepad(byte* ptr, uint len)
        {
            while (len >= 0)
            {
                if (ptr[len] != ' ' && ptr[len] != 0)
                {
                    break;
                }
                ptr[len] = 0;
                len--;
            }
        }

        public static bool isLetter(byte aChar)
        {
            if ((char)aChar >= 'A' && (char)aChar <= 'Z')
            {
                return true;
            }
            if ((char)aChar >= 'a' && (char)aChar <= 'z')
            {
                return true;
            }
            return false;
        }

        public static bool isChar(byte aChar)
        {
            return ((char)aChar >= ' ' && (char)aChar <= '~');
        }

        public static bool isUpper(byte aChar)
        {
            return ((char)aChar >= 'A' && (char)aChar <= 'Z');
        }

        public static bool IsBitSet(this uint b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static byte SetBit(this uint b, int pos)
        {
            return (byte)(b | (1 << pos));
        }

        public static byte UnsetBit(this uint b, int pos)
        {
            return (byte)(b & ~(1 << pos));
        }

        public static bool IsByteSet(this byte variable, int pos)
        {
            return (variable & (1 << pos)) != 0;
        }

        public static void SetByteBit(this byte variable, int pos)
        {
            variable = (byte)(variable & (1 << pos));
        }

        public static void UnsetByteBit(this byte variable, int pos)
        {
            variable = (byte)(variable & ~(1 << pos));
        }

        public static int strContains(byte* haystack, string needle)
        {
            return strContains(haystack, str2Ptr(needle));
        }

        public static int strContains(byte* haystack, byte* needle)
        {
            uint hLen = strLen(haystack);
            uint nLen = strLen(needle);
            for (int i = 0; i <= hLen - nLen; i++)
            {
                if (match(haystack, hLen, needle, nLen, i))
                {
                    return i;
                }
            }
            return -1;
        }

        static bool match(byte* haystack, uint hLen, byte* needle, uint nLen, int start)
        {
            if (nLen + start > hLen)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < nLen; i++)
                {
                    if (needle[i] != haystack[i + start])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public static byte* strSub(string ptr, uint start, uint size)
        {
            return strSub(str2Ptr(ptr), start, size);
        }

        public static byte* strSub(byte* ptr, uint start, uint size)
        {
            uint length = strLen(ptr);
            if (start + size > length)
            {
                size = length - start;
            }
            byte* data = (byte*)Heap.alloc(size);
            memcpy(data, ptr + start, size);
            return data;
        }

        public static int str2Int(string str)
        {
            int answer = 0, factor = 1;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                answer += (str[i] - '0') * factor;
                factor *= 10;
            }
            return answer;
        }

        public static byte* str2Ptr(string source)
        {
            byte* pointer = (byte*)Heap.alloc((uint)source.Length + 1);
            memset(pointer, 0, (uint)source.Length + 1);
            byte* dest = pointer;
            byte* sour = (byte*)source.GetHashCode();
            for (int i = 0; i < source.Length; i++)
            {
                *dest = *sour;
                dest++;
                sour++;
                sour++;
            }
            return pointer;
        }

        public static uint strLen(byte* source)
        {
            uint i = 0;
            while (isChar(*source))
            {
                source++;
                i++;
            }
            return i;
        }

        public static bool strCmp(byte* str1, byte* str2)
        {
            if (strLen(str1) != strLen(str2))
            {
                return false;
            }
            uint len = strLen(str1);
            for (int i = 0; i < len; i++)
            {
                if (str1[i] != str2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Compare(this string str, byte* pointer, int length)
        {
            if (length != str.Length)
            {
                return false;
            }
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != pointer[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool strCmp(string str1, byte* str2)
        {
            if (str1.Length != strLen(str2))
            {
                return false;
            }
            uint len = (uint)str1.Length;
            for (int i = 0; i < len; i++)
            {
                if (str1[i] != (char)str2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static void str2Low(byte* source)
        {
            for (int i = 0; i < strLen(source); i++)
            {
                source[i] = toLower(source[i]);
            }
        }

        public static void str2Upper(byte* source)
        {
            for (int i = 0; i < strLen(source); i++)
            {
                source[i] = toUpper(source[i]);
            }
        }

        public static byte toLower(byte a)
        {
            if ((char)a >= 'A' && (char)a <= 'Z')
            {
                return (byte)(a + 32);
            }
            return a;
        }

        public static byte toUpper(byte a)
        {
            if ((char)a >= 'a' && (char)a <= 'z')
            {
                return (byte)(a - 32);
            }
            return a;
        }

        public static ushort ToUInt16(byte* n, uint aPos)
        {
            return (ushort)(n[aPos + 1] << 8 | n[aPos]);
        }

        public static uint ToUInt32(byte* n, uint aPos)
        {
            return (uint)(n[aPos + 3] << 24 | n[aPos + 2] << 16 | n[aPos + 1] << 8 | n[aPos]);
        }

        public static uint getMethodHandler(aMethod method)
        {
            return (uint)method.GetHashCode();
        }

        public static uint getMethodHandler(nMethod method)
        {
            return (uint)method.GetHashCode();
        }

        public static uint getMethodHandler(pMethod method)
        {
            return (uint)method.GetHashCode();
        }

        public static byte* experiment(this string s, [FieldAccess(Name = "System.Char System.String.m_firstChar")] char* aFirstChar)
        {
            return (byte*)aFirstChar;
        }

        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * by + secondFloat * (1 - by);
        }

        public static bool Compare(byte* pointer, string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (pointer[i] != str[i])
                {
                    return false;
                }
            }
            if (pointer[str.Length] != 0)
            {
                return false;
            }
            return true;
        }

        public static bool Compare(byte* pointer, byte* str)
        {
            int strLength = 0;
            byte* ptr = str;
            while (*ptr != 0)
            {
                strLength++;
                ptr++;
            }
            int ptrLength = 0;
            ptr = pointer;
            while (*ptr != 0)
            {
                ptrLength++;
                ptr++;
            }
            if (strLength != ptrLength)
            {
                return false;
            }
            for (int i = 0; i < strLength; i++)
            {
                if (pointer[i] != str[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool StartsWith(byte* pointer, string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (pointer[i] != str[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Contains(byte* pointer, string str)
        {
            byte* ptr = pointer;
            int length = 0;
            while (*ptr != 0)
            {
                ptr++;
                length++;
            }
            if (length < str.Length)
            {
                return false;
            }
            ptr = pointer;
            for (int a = 0; a < length; a++)
            {
                if (a + str.Length >= length)
                {
                    return false;
                }
                for (int i = 0; i < str.Length; i++)
                {
                    if (ptr[a + i] != str[i])
                    {
                        break;
                    }
                }
            }
            return true;
        }

        public static bool Contains(byte* pointer, char aChar)
        {
            byte* ptr = pointer;
            int length = 0;
            while (*ptr != 0)
            {
                ptr++;
                length++;
            }
            if (length == 0)
            {
                return false;
            }
            ptr = pointer;
            for (int a = 0; a < length; a++)
            {
                if ((char)ptr[a] == aChar)
                {
                    return true;
                }
            }
            return false;
        }

        [PlugMethod(PlugRequired = true)]
        public static object getHandler(uint address)
        {
            return null;
        }

        [PlugMethod(PlugRequired = true)]
        public static uint getPointer(object obj)
        {
            return 10;
        }

        [Plug(Target = typeof(Utils))]
        public class UtilsPlug
        {
            [PlugMethod(Assembler = typeof(asmConvertType))]
            public static object getHandler(uint address)
            {
                return null;
            }

            [PlugMethod(Assembler = typeof(asmConvertTypeUint))]
            public static uint getPointer(object obj)
            {
                return 10;
            }


            public class asmConvertTypeUint : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceIsIndirect: true, sourceDisplacement: 12);
                    XS.Push(XSRegisters.EAX);
                }
            }

            public class asmConvertType : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceIsIndirect: true, sourceDisplacement: 8);
                    XS.Sub(XSRegisters.EAX, 4);
                    XS.Push(XSRegisters.EAX);
                }
            }

            [PlugMethod(Assembler = typeof(asmCopyBytes))]
            public static void memcpy(byte* dst, byte* src, int len) { }

            public class asmCopyBytes : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    new LiteralAssemblerCode("mov esi, [esp+12]");
                    new LiteralAssemblerCode("mov edi, [esp+16]");
                    new LiteralAssemblerCode("cld");
                    new LiteralAssemblerCode("mov ecx, [esp+8]");
                    new LiteralAssemblerCode("rep movsb");
                }
            }

            [PlugMethod(Assembler = typeof(asmCopyUint))]
            public static void memcpy(uint* dst, uint* src, int len) { }

            public class asmCopyUint : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    new LiteralAssemblerCode("mov esi, [esp+12]");
                    new LiteralAssemblerCode("mov edi, [esp+16]");
                    new LiteralAssemblerCode("cld");
                    new LiteralAssemblerCode("mov ecx, [esp+8]");
                    new LiteralAssemblerCode("rep movsd");
                }
            }

            [PlugMethod(Assembler = typeof(asmSetByte))]
            public static void memset(byte* dst, byte value, uint len) { }

            public class asmSetByte : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    new LiteralAssemblerCode("mov al, [esp+12]");
                    new LiteralAssemblerCode("mov edi, [esp+16]");
                    new LiteralAssemblerCode("cld");
                    new LiteralAssemblerCode("mov ecx, [esp+8]");
                    new LiteralAssemblerCode("rep stosb");
                }
            }

            [PlugMethod(Assembler = typeof(asmSetUint))]
            public static void memset(uint* dst, uint value, uint len) { }

            public class asmSetUint : AssemblerMethod
            {
                public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
                {
                    new LiteralAssemblerCode("mov eax, [esp+12]");
                    new LiteralAssemblerCode("mov edi, [esp+16]");
                    new LiteralAssemblerCode("cld");
                    new LiteralAssemblerCode("mov ecx, [esp+8]");
                    new LiteralAssemblerCode("rep stosd");
                }
            }
        }
    }
}
