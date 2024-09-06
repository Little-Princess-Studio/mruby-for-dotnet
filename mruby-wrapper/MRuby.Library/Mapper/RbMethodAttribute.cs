using System;

namespace MRuby.Library.Mapper
{
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class RbMethodAttribute : Attribute
    {
        public readonly string MethodName;
        public readonly bool HasBlockArg;

        public RbMethodAttribute(string methodName, bool hasBlockArg)
        {
            this.MethodName = methodName;
            this.HasBlockArg = hasBlockArg;
        }
    }
}