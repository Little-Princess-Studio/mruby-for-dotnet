using System;

namespace MRuby.Library.Mapper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RbClassAttribute : Attribute
    {
        public readonly string ClassName;
        public readonly string ParentClassName;
        public readonly string ParentModuleName;

        public RbClassAttribute(string className, string parentClassName, string parentModuleName)
        {
            this.ClassName = className;
            this.ParentModuleName = parentModuleName;
            this.ParentClassName = parentClassName;
        }
    }
}