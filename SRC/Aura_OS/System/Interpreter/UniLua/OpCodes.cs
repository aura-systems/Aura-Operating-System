 
namespace UniLua
{
	using System.Collections.Generic;

	public enum OpCode
	{
		/*----------------------------------------------------------------------
		name		args	description
		------------------------------------------------------------------------*/
		OP_MOVE,/*	A B	R(A) := R(B)					*/
		OP_LOADK,/*	A Bx	R(A) := Kst(Bx)					*/
		OP_LOADKX,/*	A 	R(A) := Kst(extra arg)				*/
		OP_LOADBOOL,/*	A B C	R(A) := (Bool)B; if (C) pc++			*/
		OP_LOADNIL,/*	A B	R(A), R(A+1), ..., R(A+B) := nil		*/
		OP_GETUPVAL,/*	A B	R(A) := UpValue[B]				*/

		OP_GETTABUP,/*	A B C	R(A) := UpValue[B][RK(C)]			*/
		OP_GETTABLE,/*	A B C	R(A) := R(B)[RK(C)]				*/

		OP_SETTABUP,/*	A B C	UpValue[A][RK(B)] := RK(C)			*/
		OP_SETUPVAL,/*	A B	UpValue[B] := R(A)				*/
		OP_SETTABLE,/*	A B C	R(A)[RK(B)] := RK(C)				*/

		OP_NEWTABLE,/*	A B C	R(A) := {} (size = B,C)				*/

		OP_SELF,/*	A B C	R(A+1) := R(B); R(A) := R(B)[RK(C)]		*/

		OP_ADD,/*	A B C	R(A) := RK(B) + RK(C)				*/
		OP_SUB,/*	A B C	R(A) := RK(B) - RK(C)				*/
		OP_MUL,/*	A B C	R(A) := RK(B) * RK(C)				*/
		OP_DIV,/*	A B C	R(A) := RK(B) / RK(C)				*/
		OP_MOD,/*	A B C	R(A) := RK(B) % RK(C)				*/
		OP_POW,/*	A B C	R(A) := RK(B) ^ RK(C)				*/
		OP_UNM,/*	A B	R(A) := -R(B)					*/
		OP_NOT,/*	A B	R(A) := not R(B)				*/
		OP_LEN,/*	A B	R(A) := length of R(B)				*/

		OP_CONCAT,/*	A B C	R(A) := R(B).. ... ..R(C)			*/

		OP_JMP,/*	A sBx	pc+=sBx; if (A) close all upvalues >= R(A) + 1	*/
		OP_EQ,/*	A B C	if ((RK(B) == RK(C)) ~= A) then pc++		*/
		OP_LT,/*	A B C	if ((RK(B) <  RK(C)) ~= A) then pc++		*/
		OP_LE,/*	A B C	if ((RK(B) <= RK(C)) ~= A) then pc++		*/

		OP_TEST,/*	A C	if not (R(A) <=> C) then pc++			*/
		OP_TESTSET,/*	A B C	if (R(B) <=> C) then R(A) := R(B) else pc++	*/

		OP_CALL,/*	A B C	R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1)) */
		OP_TAILCALL,/*	A B C	return R(A)(R(A+1), ... ,R(A+B-1))		*/
		OP_RETURN,/*	A B	return R(A), ... ,R(A+B-2)	(see note)	*/

		OP_FORLOOP,/*	A sBx	R(A)+=R(A+2);
					if R(A) <?= R(A+1) then { pc+=sBx; R(A+3)=R(A) }*/
		OP_FORPREP,/*	A sBx	R(A)-=R(A+2); pc+=sBx				*/

		OP_TFORCALL,/*	A C	R(A+3), ... ,R(A+2+C) := R(A)(R(A+1), R(A+2));	*/
		OP_TFORLOOP,/*	A sBx	if R(A+1) ~= nil then { R(A)=R(A+1); pc += sBx }*/

		OP_SETLIST,/*	A B C	R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B	*/

		OP_CLOSURE,/*	A Bx	R(A) := closure(KPROTO[Bx])			*/

		OP_VARARG,/*	A B	R(A), R(A+1), ..., R(A+B-2) = vararg		*/

		OP_EXTRAARG/*	Ax	extra (larger) argument for previous opcode	*/
	}

	internal enum OpArgMask
	{
	  OpArgN,  /* argument is not used */
	  OpArgU,  /* argument is used */
	  OpArgR,  /* argument is a register or a jump offset */
	  OpArgK   /* argument is a constant or register/constant */
	}

	/// <summary>
	/// basic instruction format
	/// </summary>
	internal enum OpMode
	{
		iABC,
		iABx,
		iAsBx,
		iAx,
	}

	internal class OpCodeMode
	{
		public bool 		TMode;
		public bool 		AMode;
		public OpArgMask 	BMode;
		public OpArgMask 	CMode;
		public OpMode		OpMode;

        public OpCodeMode(bool tMode, bool aMode, OpArgMask bMode, OpArgMask cMode, OpMode opMode)
        {
            TMode = tMode;
            AMode = aMode;
            BMode = bMode;
            CMode = cMode;
            OpMode = opMode;
        }
    }

	internal static class OpCodeInfo
	{
		public static OpCodeMode GetMode( OpCode op )
		{
			return Info[(int)op];
		}

		private static Dictionary<int, OpCodeMode> Info;

		static OpCodeInfo()
		{
            Info = new Dictionary<int, OpCodeMode>();
            var opCode = OpCode.OP_MOVE;
            var opCodeMode = new OpCodeMode(false, true, OpArgMask.OpArgR, OpArgMask.OpArgN, OpMode.iABC);
            Info.Add((int)opCode, opCodeMode);
            Info.Add( (int)OpCode.OP_LOADK, 		new OpCodeMode(false, true,  OpArgMask.OpArgK, OpArgMask.OpArgN, OpMode.iABx) );
			Info.Add( (int)OpCode.OP_LOADKX, 	new OpCodeMode(false, true,  OpArgMask.OpArgN, OpArgMask.OpArgN, OpMode.iABx) );
			Info.Add( (int)OpCode.OP_LOADBOOL, 	new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgU, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_LOADNIL, 	new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgN, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_GETUPVAL, 	new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgN, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_GETTABUP, 	new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_GETTABLE, 	new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_SETTABUP, 	new OpCodeMode(false, false, OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_SETUPVAL, 	new OpCodeMode(false, false, OpArgMask.OpArgU, OpArgMask.OpArgN, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_SETTABLE, 	new OpCodeMode(false, false, OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_NEWTABLE, 	new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgU, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_SELF, 		new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_ADD, 		new OpCodeMode(false, true,  OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_SUB, 		new OpCodeMode(false, true,  OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_MUL, 		new OpCodeMode(false, true,  OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_DIV, 		new OpCodeMode(false, true,  OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_MOD, 		new OpCodeMode(false, true,  OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_POW, 		new OpCodeMode(false, true,  OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_UNM, 		new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgN, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_NOT, 		new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgN, OpMode.iABC) );
            Info.Add( (int)OpCode.OP_LEN, 		new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgN, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_CONCAT, 	new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgR, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_JMP, 		new OpCodeMode(false, false, OpArgMask.OpArgR, OpArgMask.OpArgN, OpMode.iAsBx) );
			Info.Add( (int)OpCode.OP_EQ, 		new OpCodeMode(true,  false, OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_LT, 		new OpCodeMode(true,  false, OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_LE, 		new OpCodeMode(true,  false, OpArgMask.OpArgK, OpArgMask.OpArgK, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_TEST, 		new OpCodeMode(true,  false, OpArgMask.OpArgN, OpArgMask.OpArgU, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_TESTSET, 	new OpCodeMode(true,  true,  OpArgMask.OpArgR, OpArgMask.OpArgU, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_CALL, 		new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgU, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_TAILCALL, 	new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgU, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_RETURN, 	new OpCodeMode(false, false, OpArgMask.OpArgU, OpArgMask.OpArgN, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_FORLOOP, 	new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgN, OpMode.iAsBx) );
			Info.Add( (int)OpCode.OP_FORPREP, 	new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgN, OpMode.iAsBx) );
			Info.Add( (int)OpCode.OP_TFORCALL, 	new OpCodeMode(false, false, OpArgMask.OpArgN, OpArgMask.OpArgU, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_TFORLOOP, 	new OpCodeMode(false, true,  OpArgMask.OpArgR, OpArgMask.OpArgN, OpMode.iAsBx) );
			Info.Add( (int)OpCode.OP_SETLIST, 	new OpCodeMode(false, false, OpArgMask.OpArgU, OpArgMask.OpArgU, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_CLOSURE, 	new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgN, OpMode.iABx) );
			Info.Add( (int)OpCode.OP_VARARG, 	new OpCodeMode(false, true,  OpArgMask.OpArgU, OpArgMask.OpArgN, OpMode.iABC) );
			Info.Add( (int)OpCode.OP_EXTRAARG, 	new OpCodeMode(false, false, OpArgMask.OpArgU, OpArgMask.OpArgU, OpMode.iAx) );
        }
	}

}

