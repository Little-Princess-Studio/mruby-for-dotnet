using System;

namespace MRuby.Library.Mapper
{
    public class RbClassMethodAttribute : RbMethodAttribute
    {
        public RbClassMethodAttribute(string methodName, bool hasBlockArg=false) : base(methodName, hasBlockArg)
        {
        }
    }
}