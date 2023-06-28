using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectDMG {
    public static class Cycles {

        //This covers the base cpu cycle values
        // 0 values are special cases for branchs that are specially dealed below
        // 00 values are unused opcodes
        public static readonly int[] Value = {
              //0   1   2   3   4   5   6   7   8   9   A   B   C   D   E   F
                4, 12,  8,  8,  4,  4,  8,  4, 20,  8,  8,  8,  4,  4,  8,  4, //0
	            4, 12,  8,  8,  4,  4,  8,  4,  0,  8,  8,  8,  4,  4,  8,  4, //1
                0, 12,  8,  8,  4,  4,  8,  4,  0,  8,  8,  8,  4,  4,  8,  4, //2
                0, 12,  8,  8, 12, 12, 12,  4,  0,  8,  8,  8,  4,  4,  8,  4, //3 

                4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4,  4,  4,  8,  4, //4
	            4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4,  4,  4,  8,  4, //5
                4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4,  4,  4,  8,  4, //6
                8,  8,  8,  8,  8,  8,  4,  8,  4,  4,  4,  4,  4,  4,  8,  4, //7

                4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4,  4,  4,  8,  4, //8
	            4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4,  4,  4,  8,  4, //9
                4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4,  4,  4,  8,  4, //A
                4,  4,  4,  4,  4,  4,  8,  4,  4,  4,  4,  4,  4,  4,  8,  4, //B

                0,  12,  0,  0,  0, 16, 8, 16,  0, -4,  0,  0,  0,  0,  8, 16, //C
	            0,  12,  0, 00,  0, 16, 8, 16,  0, -4,  0, 00,  0, 00,  8, 16, //D
                12, 12,  8, 00, 00, 16, 8, 16, 16,  4, 16, 00, 00, 00,  8, 16, //E
                12, 12,  8,  4, 00, 16, 8, 16, 12,  8, 16,  4, 00, 00,  8, 16  //F
                };

        public static readonly int[] CBValue = {
              //0 1 2 3 4 5  6 7 8 9 A B C D  E  F
	            8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //0
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //1
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //2
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //3
                                                   
                8,8,8,8,8,8,12,8,8,8,8,8,8,8,12,8, //4
                8,8,8,8,8,8,12,8,8,8,8,8,8,8,12,8, //5
                8,8,8,8,8,8,12,8,8,8,8,8,8,8,12,8, //6
                8,8,8,8,8,8,12,8,8,8,8,8,8,8,12,8, //7
                                                   
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //8
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //9
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //A
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //B
                                                   
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //C
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //D
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8, //E
                8,8,8,8,8,8,16,8,8,8,8,8,8,8,16,8  //F
                };


        //This covers cycle values for Return, Jump and Call opcodes as
        //they have dual values when true or false
        public static readonly int RETURN_TRUE = 20;
        public static readonly int RETURN_FALSE = 8;
        public static readonly int JUMP_TRUE = 16;
        public static readonly int JUMP_FALSE = 12;
        public static readonly int CALL_TRUE = 24;
        public static readonly int CALL_FALSE = 12;
        public static readonly int JUMP_RELATIVE_TRUE = 12;
        public static readonly int JUMP_RELATIVE_FALSE = 8;

    }
}
