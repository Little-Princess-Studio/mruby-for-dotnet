using System;

namespace MRuby.Library.Mapper
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RbModuleAttribute : Attribute
    {
        public readonly string ParentModuleName;
        public readonly string ModuleName;

        public RbModuleAttribute(string moduleName="", string parentModuleName="")
        {
            this.ParentModuleName = parentModuleName;
            this.ModuleName = moduleName;
        }
    }
}