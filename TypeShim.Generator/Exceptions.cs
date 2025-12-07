using System;
using System.Collections.Generic;
using System.Text;

namespace TypeShim.Generator;

public class TypeShimException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}

public class TypeNotSupportedException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}

public class UnsupportedPropertyException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}

public class UnsupportedMethodException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}