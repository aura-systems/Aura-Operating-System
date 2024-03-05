using Cosmos.HAL.BlockDevice;
using IL2CPU.API.Attribs;
using System.Reflection;
using System;
using XSharp.Assembler;
using static XSharp.XSRegisters;
using XSharp;
using Aura_OS.System.Graphics.UI.GUI;
using Cosmos.System.Graphics;
using XSharp.Assembler.x86;
using System.Security.Cryptography;
using XSharp.Assembler.x86.SSE;
using static System.Net.Mime.MediaTypeNames;

namespace Cosmos.System_Plugs.System.Drawing
{
    [Plug(Target = typeof(DirectBitmap))]
    public unsafe static class DrawImageAlphaImpl
    {
        [PlugMethod(Assembler = typeof(AlphaBltSSE2ASM))]
        public static void AlphaBltSSE2(uint* dest, int dbpl, uint* src, int sbpl, int width, int height) => throw new NotImplementedException();
        [PlugMethod(Assembler = typeof(BrightnessASM))]
        public static void BrightnessSSE(byte* image, int len, byte alpha) => throw new NotImplementedException();
    }

    public class AlphaBltSSE2ASM : AssemblerMethod
    {
        // public static void AlphaBltSSE2(uint* dest, int dbpl, uint* src, int sbpl, int width, int height)
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.LiteralCode("	push ebp");
            XS.LiteralCode("	push ebx");
            XS.LiteralCode("	push esi");
            XS.LiteralCode("	push edi");
            XS.LiteralCode("mov eax,dword [ebp+12]"); // width
            XS.LiteralCode("	shl eax,2");
            XS.LiteralCode("sub dword [ebp+16],eax"); // sbpl
            XS.LiteralCode("sub dword [ebp+24],eax"); // dbpl
            XS.LiteralCode("	test eax,15");
            XS.LiteralCode("	jnz iadbldstart");
            XS.LiteralCode("test dword [ebp+28],15"); // dest
            XS.LiteralCode("	jnz iadbld4start");
            XS.LiteralCode("test dword [ebp+24],15"); // dbpl
            XS.LiteralCode("	jnz iadbld4start");
            XS.LiteralCode("	jmp iadbld4astart");
            XS.LiteralCode("	align 16");
            XS.LiteralCode("iadbldstart:");
            XS.LiteralCode("	mov ebx,0x10101010");
            XS.LiteralCode("	pxor xmm2,xmm2");
            XS.LiteralCode("	movd xmm4,ebx");
            XS.LiteralCode("	punpcklbw xmm4, xmm2");
            XS.LiteralCode("mov edi,dword [ebp+28]"); // dest
            XS.LiteralCode("mov esi,dword [ebp+20]"); // src
            XS.LiteralCode("mov edx,dword [ebp+8]"); // height
            XS.LiteralCode("	ALIGN 16");
            XS.LiteralCode("iadbldlop:");
            XS.LiteralCode("mov ecx,dword [ebp+12]"); // width
            XS.LiteralCode("	align 16");
            XS.LiteralCode("iadbldlop2:");
            XS.LiteralCode("	mov bl,byte [esi+3]");
            XS.LiteralCode("	mov bh,bl");
            XS.LiteralCode("	movzx eax,bx");
            XS.LiteralCode("	shl ebx,16");
            XS.LiteralCode("	or eax,ebx");
            XS.LiteralCode("	movd xmm0,eax");
            XS.LiteralCode("	mov ebx,0xffffffff");
            XS.LiteralCode("	sub ebx,eax");
            XS.LiteralCode("	or ebx,0xff000000");
            XS.LiteralCode("	movd xmm3,ebx");
            XS.LiteralCode("	movd xmm1,dword [esi]");
            XS.LiteralCode("	punpcklbw xmm0, xmm2");
            XS.LiteralCode("	punpcklbw xmm1, xmm2");
            XS.LiteralCode("	pmullw xmm0,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm0");
            XS.LiteralCode("	movd xmm0,dword [edi]");
            XS.LiteralCode("	punpcklbw xmm3, xmm2");
            XS.LiteralCode("	punpcklbw xmm0, xmm2");
            XS.LiteralCode("	pmullw xmm0,xmm3");
            XS.LiteralCode("	paddusw xmm0,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm0");
            XS.LiteralCode("	psrlw xmm1,8");
            XS.LiteralCode("	paddusw xmm0,xmm1");
            XS.LiteralCode("	paddusw xmm0,xmm4");
            XS.LiteralCode("	psrlw xmm0,8");
            XS.LiteralCode("	packuswb xmm0,xmm2");
            XS.LiteralCode("	movd dword [edi],xmm0");
            XS.LiteralCode("	lea esi,[esi+4]");
            XS.LiteralCode("	lea edi,[edi+4]");
            XS.LiteralCode("	dec ecx");
            XS.LiteralCode("	jnz iadbldlop2");
            XS.LiteralCode("add esi,dword [ebp+16]"); // sAdd
            XS.LiteralCode("add edi,dword [ebp+24]"); // dAdd
            XS.LiteralCode("	dec edx");
            XS.LiteralCode("	jnz iadbldlop");
            XS.LiteralCode("	jmp iadbldexit");
            XS.LiteralCode("	align 16");
            XS.LiteralCode("iadbld4start:");
            XS.LiteralCode("shr dword [ebp+12],2"); // width
            XS.LiteralCode("	mov ebx,0x10101010");
            XS.LiteralCode("	pxor xmm2,xmm2");
            XS.LiteralCode("	movd xmm4,ebx");
            XS.LiteralCode("	punpckldq xmm4, xmm4");
            XS.LiteralCode("	punpcklbw xmm4, xmm2");
            XS.LiteralCode("	mov ebx,0xffffffff");
            XS.LiteralCode("	movd xmm6,ebx");
            XS.LiteralCode("	mov ebx,0xff000000");
            XS.LiteralCode("	movd xmm7,ebx");
            XS.LiteralCode("	punpckldq xmm6,xmm6");
            XS.LiteralCode("	punpckldq xmm7,xmm7");
            XS.LiteralCode("mov edi,dword [ebp+28]"); // dest
            XS.LiteralCode("mov esi,dword [ebp+20]"); // src
            XS.LiteralCode("mov edx,dword [ebp+8]"); // height
            XS.LiteralCode("	ALIGN 16");
            XS.LiteralCode("iadbld4lop:");
            XS.LiteralCode("mov ecx,dword [ebp+12]"); // width
            XS.LiteralCode("	align 16");
            XS.LiteralCode("iadbld4lop2:");
            XS.LiteralCode("	movzx eax,byte [esi+3]");
            XS.LiteralCode("	mov ah,byte [esi+7]");
            XS.LiteralCode("	movd xmm0,eax");
            XS.LiteralCode("	punpcklbw xmm0,xmm0");
            XS.LiteralCode("	punpcklwd xmm0,xmm0");
            XS.LiteralCode("	movdqa xmm3,xmm6");
            XS.LiteralCode("	psubd xmm3,xmm0");
            XS.LiteralCode("	por xmm3,xmm7");
            XS.LiteralCode("	movq xmm1,[esi]");
            XS.LiteralCode("	punpcklbw xmm0, xmm2");
            XS.LiteralCode("	punpcklbw xmm1, xmm2");
            XS.LiteralCode("	pmullw xmm0,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm0");
            XS.LiteralCode("	movq xmm0,[edi]");
            XS.LiteralCode("	punpcklbw xmm3, xmm2");
            XS.LiteralCode("	punpcklbw xmm0, xmm2");
            XS.LiteralCode("	pmullw xmm0,xmm3");
            XS.LiteralCode("	paddusw xmm0,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm0");
            XS.LiteralCode("	psrlw xmm1,8");
            XS.LiteralCode("	paddusw xmm0,xmm1");
            XS.LiteralCode("	paddusw xmm0,xmm4");
            XS.LiteralCode("	movzx eax,byte [esi+11]");
            XS.LiteralCode("	mov ah,byte [esi+15]");
            XS.LiteralCode("	movd xmm5,eax");
            XS.LiteralCode("	punpcklbw xmm5,xmm5");
            XS.LiteralCode("	punpcklwd xmm5,xmm5");
            XS.LiteralCode("	movdqa xmm3,xmm6");
            XS.LiteralCode("	psubd xmm3,xmm5");
            XS.LiteralCode("	por xmm3,xmm7");
            XS.LiteralCode("	movq xmm1,[esi+8]");
            XS.LiteralCode("	punpcklbw xmm5,xmm2");
            XS.LiteralCode("	punpcklbw xmm1,xmm2");
            XS.LiteralCode("	pmullw xmm5,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm5");
            XS.LiteralCode("	movq xmm5,[edi+8]");
            XS.LiteralCode("	punpcklbw xmm3,xmm2");
            XS.LiteralCode("	punpcklbw xmm5,xmm2");
            XS.LiteralCode("	pmullw xmm5,xmm3");
            XS.LiteralCode("	paddusw xmm5,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm5");
            XS.LiteralCode("	psrlw xmm1,8");
            XS.LiteralCode("	paddusw xmm5,xmm1");
            XS.LiteralCode("	paddusw xmm5,xmm4");
            XS.LiteralCode("	psrlw xmm0,8");
            XS.LiteralCode("	psrlw xmm5,8");
            XS.LiteralCode("	packuswb xmm0,xmm5");
            XS.LiteralCode("	movdqu [edi],xmm0");
            XS.LiteralCode("	lea esi,[esi+16]");
            XS.LiteralCode("	lea edi,[edi+16]");
            XS.LiteralCode("	dec ecx");
            XS.LiteralCode("	jnz iadbld4lop2");
            XS.LiteralCode("add esi,dword [ebp+16]"); // sAdd
            XS.LiteralCode("add edi,dword [ebp+24]"); // dAdd
            XS.LiteralCode("	dec edx");
            XS.LiteralCode("	jnz iadbld4lop");
            XS.LiteralCode("	jmp iadbldexit");
            XS.LiteralCode("	align 16");
            XS.LiteralCode("iadbld4astart:");
            XS.LiteralCode("shr dword [ebp+12],2"); // width
            XS.LiteralCode("	mov ebx,0x10101010");
            XS.LiteralCode("	pxor xmm2,xmm2");
            XS.LiteralCode("	movd xmm4,ebx");
            XS.LiteralCode("	punpckldq xmm4, xmm4");
            XS.LiteralCode("	punpcklbw xmm4, xmm2");
            XS.LiteralCode("	mov ebx,0xffffffff");
            XS.LiteralCode("	movd xmm6,ebx");
            XS.LiteralCode("	mov ebx,0xff000000");
            XS.LiteralCode("	movd xmm7,ebx");
            XS.LiteralCode("	punpckldq xmm6,xmm6");
            XS.LiteralCode("	punpckldq xmm7,xmm7");
            XS.LiteralCode("mov edi,dword [ebp+28]"); // dest
            XS.LiteralCode("mov esi,dword [ebp+20]"); // src
            XS.LiteralCode("mov edx,dword [ebp+8]"); // height
            XS.LiteralCode("	ALIGN 16");
            XS.LiteralCode("iadbld4alop:");
            XS.LiteralCode("mov ecx,dword [ebp+12]"); // width
            XS.LiteralCode("	align 16");
            XS.LiteralCode("iadbld4alop2:");
            XS.LiteralCode("	movzx eax,byte [esi+3]");
            XS.LiteralCode("	mov ah,byte [esi+7]");
            XS.LiteralCode("	movd xmm0,eax");
            XS.LiteralCode("	punpcklbw xmm0,xmm0");
            XS.LiteralCode("	punpcklwd xmm0,xmm0");
            XS.LiteralCode("	movdqa xmm3,xmm6");
            XS.LiteralCode("	psubd xmm3,xmm0");
            XS.LiteralCode("	por xmm3,xmm7");
            XS.LiteralCode("	movq xmm1,[esi]");
            XS.LiteralCode("	punpcklbw xmm0, xmm2");
            XS.LiteralCode("	punpcklbw xmm1, xmm2");
            XS.LiteralCode("	pmullw xmm0,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm0");
            XS.LiteralCode("	movq xmm0,[edi]");
            XS.LiteralCode("	punpcklbw xmm3, xmm2");
            XS.LiteralCode("	punpcklbw xmm0, xmm2");
            XS.LiteralCode("	pmullw xmm0,xmm3");
            XS.LiteralCode("	paddusw xmm0,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm0");
            XS.LiteralCode("	psrlw xmm1,8");
            XS.LiteralCode("	paddusw xmm0,xmm1");
            XS.LiteralCode("	paddusw xmm0,xmm4");
            XS.LiteralCode("	movzx eax,byte [esi+11]");
            XS.LiteralCode("	mov ah,byte [esi+15]");
            XS.LiteralCode("	movd xmm5,eax");
            XS.LiteralCode("	punpcklbw xmm5,xmm5");
            XS.LiteralCode("	punpcklwd xmm5,xmm5");
            XS.LiteralCode("	movdqa xmm3,xmm6");
            XS.LiteralCode("	psubd xmm3,xmm5");
            XS.LiteralCode("	por xmm3,xmm7");
            XS.LiteralCode("	movq xmm1,[esi+8]");
            XS.LiteralCode("	punpcklbw xmm5,xmm2");
            XS.LiteralCode("	punpcklbw xmm1,xmm2");
            XS.LiteralCode("	pmullw xmm5,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm5");
            XS.LiteralCode("	movq xmm5,[edi+8]");
            XS.LiteralCode("	punpcklbw xmm3,xmm2");
            XS.LiteralCode("	punpcklbw xmm5,xmm2");
            XS.LiteralCode("	pmullw xmm5,xmm3");
            XS.LiteralCode("	paddusw xmm5,xmm1");
            XS.LiteralCode("	movdqa xmm1,xmm5");
            XS.LiteralCode("	psrlw xmm1,8");
            XS.LiteralCode("	paddusw xmm5,xmm1");
            XS.LiteralCode("	paddusw xmm5,xmm4");
            XS.LiteralCode("	psrlw xmm0,8");
            XS.LiteralCode("	psrlw xmm5,8");
            XS.LiteralCode("	packuswb xmm0,xmm5");
            XS.LiteralCode("	movntdq [edi],xmm0");
            XS.LiteralCode("	lea esi,[esi+16]");
            XS.LiteralCode("	lea edi,[edi+16]");
            XS.LiteralCode("	dec ecx");
            XS.LiteralCode("	jnz iadbld4alop2");
            XS.LiteralCode("add esi,dword [ebp+16]"); // sAdd
            XS.LiteralCode("add edi,dword [ebp+24]"); // dAdd
            XS.LiteralCode("	dec edx");
            XS.LiteralCode("	jnz iadbld4alop");
            XS.LiteralCode("	align 16");
            XS.LiteralCode("iadbldexit:");
            XS.LiteralCode("	pop edi");
            XS.LiteralCode("	pop esi");
            XS.LiteralCode("	pop ebx");
            XS.LiteralCode("	pop ebp");
        }
    }

    public class BrightnessASM : AssemblerMethod
    {
        private const int AlphaDisplacement = 8; // Déplacement pour l'argument alpha
        private const int LenDisplacement = 12;
        private const int ImgDisplacement = 16;

        //public static void BrightnessSSE(byte* image, int len, byte alpha)
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Set(ECX, EBP, sourceIsIndirect: true, sourceDisplacement: LenDisplacement); // Charge la longueur dans ECX
            XS.Set(EDI, EBP, sourceIsIndirect: true, sourceDisplacement: ImgDisplacement); // Charge le pointeur d'image dans EDI
            XS.Set(EAX, EBP, sourceIsIndirect: true, sourceDisplacement: AlphaDisplacement); // Charge la valeur alpha dans EAX

            XS.Label("start_loop");

            XS.LiteralCode("test ecx, ecx"); // Teste si on a fini de traiter tous les pixels
            XS.LiteralCode("jz end_loop"); // Si oui, saute à la fin

            XS.LiteralCode("movzx ebx, byte [edi + 3]"); // Charge la valeur alpha actuelle du pixel dans EBX
            XS.LiteralCode("test ebx, ebx"); // Teste si l'alpha actuel est 0x00
            XS.LiteralCode("jz skip_alpha_update"); // Si c'est le cas, saute la mise à jour de cet alpha
            XS.LiteralCode("mov dl, al"); // Sinon, charge la valeur alpha dynamique de EAX (AL) dans DL
            XS.LiteralCode("mov [edi + 3], dl"); // Et écrit cette valeur alpha dans le canal alpha du pixel

            XS.Label("skip_alpha_update");

            XS.Add(EDI, 4); // Passe au pixel suivant
            XS.Decrement(ECX); // Décrémente le compteur
            XS.Jump("start_loop"); // Boucle

            XS.Label("end_loop");
        }
    }
}
