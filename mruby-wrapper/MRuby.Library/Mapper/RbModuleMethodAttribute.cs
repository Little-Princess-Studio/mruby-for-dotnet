using System;

namespace MRuby.Library.Mapper
{
    public class RbModuleMethodAttribute : RbMethodAttribute
    {
        public RbModuleMethodAttribute(string methodName, bool hasBlockArg = false) : base(methodName, hasBlockArg)
        {
        }
    }
}