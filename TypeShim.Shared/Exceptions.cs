using Microsoft.CodeAnalysis;
using System;

namespace TypeShim.Shared;

public class TypeShimException(string message, Exception? innerException = null) : Exception(message, innerException)
{
}

public class NotSupportedTypeException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}

public class NotSupportedPropertyException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}

public class NotSupportedFieldException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}

public class NotSupportedMethodException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}

public class NotSupportedMethodOverloadException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}

public class NotSupportedConstructorOverloadException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}

public class NotFoundClassInfoException(string message, Exception? innerException = null) : TypeShimException(message, innerException)
{
}