using System;

namespace ZealEducation.Domain.Exceptions;

public class SystemAssetDecommissionedException : Exception
{
    public SystemAssetDecommissionedException(string message) : base(message)
    {
    }
}
