using System;

namespace MRuby.Library.Mapper
{
    public class RbInstanceMethodAttribute : RbMethodAttribute
    {
        public RbInstanceMethodAttribute(string methodName, bool hasBlockArg=false) : base(methodName, hasBlockArg)
        {
        }
    }
}