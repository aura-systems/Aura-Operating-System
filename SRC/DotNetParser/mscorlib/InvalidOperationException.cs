// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

/*=============================================================================
**
**
**
** Purpose: Exception class for denoting an object was in a state that
** made calling a method illegal.
**
**
=============================================================================*/


namespace System
{
    public class InvalidOperationException : Exception
    {
        public InvalidOperationException()
            : base("Invaild Operation")
        {
            
        }

        public InvalidOperationException(string? message)
            : base(message)
        {
           
        }

        public InvalidOperationException(string? message, Exception? innerException)
            : base(message, innerException)
        {
            
        }
    }
}