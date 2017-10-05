/*
* PROJECT:          Aura Operating System Development
* CONTENT:          AuraBasic - Token
* PROGRAMMERS:      John Welsh <djlw78@gmail.com
*/
namespace Aura_OS.AuraBasic
{
    public enum Token
    {
        Unknown,
        Identifer,
        Value,
        //Keywords
        Print,
        If,
        EndIf,
        Then,
        Else,
        For,
        To,
        Next,
        Input,
        Let,
        Gosub,
        Return,
        Rem,
        End,

        NewLine, // \n
        Colon, // :
        Semicolon, // ;
        Comma, // ,

        Plus, // +
        Minus, // -
        Slash, // "/"
        Asterisk, //*
        Caret,
        Equal, //=Dictionary
        Less,
        More,
        NotEqual, //!= 
        LessEqual,
        MoreEqual,
        Or, //| (||) or
        And, //&& (&) and
        Not, //! not

        LParen,
        RParen,

        EOF = -1 //End of file
    }
}
