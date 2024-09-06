using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using MRuby.Library.Language;

namespace MRuby.Library.Mapper
{

    [ExcludeFromCodeCoverage]
    public static class RbTypeRegisterHelper
    {
        public static void Init(RbState stat, Assembly[]? assemblies)
        {
            // scan the assembly all the type attributed by RbClassAttribute
            // and register them to the RubyScriptManager
            
            var classTypeList = GetAttributedTypes<RbClassAttribute>(assemblies);
            var clsPairList = classTypeList.Select(t => (t, DefineClass(stat, t))).ToArray();
            Array.ForEach(clsPairList, pair =>
            {
                var (t, cls) = pair;

                // try get entry method
                var entryMethod = t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(m => m.GetCustomAttribute<RbInitEntryPointAttribute>() != null);
                if (entryMethod != null)
                {
                    entryMethod.Invoke(null, new object[] { cls });
                }
            });

            var modTypeList = GetAttributedTypes<RbModuleAttribute>(assemblies);
            var modPairList = modTypeList.Select(t => (t, DefineModule(stat, t))).ToArray();
            Array.ForEach(modPairList, pair =>
            {
                var (t, mod) = pair;

                // try get entry method
                var entryMethod = t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(m => m.GetCustomAttribute<RbInitEntryPointAttribute>() != null);
                if (entryMethod != null)
                {
                    entryMethod.Invoke(null, new object[] { mod });
                }
            });
        }

        private static Type[] GetAttributedTypes<TAttr>(Assembly[]? assemblies) where TAttr : Attribute
        {
            var allTypes =
                assemblies?.Select(assembly => assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<TAttr>() != null))
                    .SelectMany(type => type)
                ?? Enumerable.Empty<Type>();

            var callingAssemblyTypes = Assembly.GetCallingAssembly().GetTypes()
                .Where(t => t.GetCustomAttribute<TAttr>() != null);
            allTypes = allTypes.Concat(callingAssemblyTypes);

            return allTypes.ToArray();
        }

        private static RbClass DefineClass(RbState stat, Type t)
        {
            var attr = t.GetCustomAttribute<RbClassAttribute>()!;
            var parModName = attr.ParentModuleName;
            var parClsName = attr.ParentClassName;
            var clsName = attr.ClassName;

            RbClass? parClass = null;
            if (!string.IsNullOrEmpty(parClsName))
            {
                parClass = stat.GetClass(parClsName);
            }

            RbClass cls;
            if (!string.IsNullOrEmpty(parModName))
            {
                var parMod = stat.GetModule(parModName);
                cls = stat.DefineClassUnder(parMod, clsName, parClass);
            }
            else
            {
                cls = stat.DefineClass(clsName, parClass);
            }

            // scan methods attributed by RbClassMethodAttribute
            var methods = t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            methods.Where(m => m.GetCustomAttribute<RbClassMethodAttribute>() != null)
                .ToList().ForEach(
                    m => DefineMethod<RbClassMethodAttribute>(
                        (name, fn, aspect) => cls.DefineClassMethod(name, fn, aspect),
                        m));

            // scan methods attributed by RbInstanceMethodAttribute
            methods.Where(m => m.GetCustomAttribute<RbInstanceMethodAttribute>() != null)
                .ToList().ForEach(
                    m => DefineMethod<RbInstanceMethodAttribute>(
                        (name, fn, aspect) => cls.DefineMethod(name, fn, aspect),
                        m));

            return cls;
        }

        private static RbClass DefineModule(RbState stat, Type t)
        {
            var attr = t.GetCustomAttribute<RbModuleAttribute>()!;
            var parModName = attr.ParentModuleName;
            var moduleName = string.IsNullOrEmpty(attr.ModuleName) ? t.Name : attr.ModuleName;

            RbClass mod;
            if (!string.IsNullOrEmpty(parModName))
            {
                var par = stat.GetModule(parModName);
                mod = stat.DefineModuleUnder(par, moduleName);
            }
            else
            {
                mod = stat.DefineModule(moduleName);
            }

            // scan methods attributed by RbClassMethodAttribute
            var methods = t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            methods.Where(m => m.GetCustomAttribute<RbClassMethodAttribute>() != null)
                .ToList().ForEach(
                    m => DefineMethod<RbClassMethodAttribute>(
                        (name, fn, aspect) => mod.DefineClassMethod(name, fn, aspect),
                        m));

            methods.Where(m => m.GetCustomAttribute<RbModuleMethodAttribute>() != null)
                .ToList().ForEach(
                    m => DefineMethod<RbModuleMethodAttribute>(
                        (name, fn, aspect) => mod.DefineModuleMethod(name, fn, aspect),
                        m));

            methods.Where(m => m.GetCustomAttribute<RbInstanceMethodAttribute>() != null)
                .ToList().ForEach(
                    m => DefineMethod<RbInstanceMethodAttribute>(
                        (name, fn, aspect) => mod.DefineMethod(name, fn, aspect),
                        m));

            return mod;
        }

        private static void DefineMethod<TAttrType>(Action<string, CSharpMethodFunc, uint> defineFn, MethodInfo methodInfo)
            where TAttrType : RbMethodAttribute
        {
            var attr = methodInfo.GetCustomAttribute<TAttrType>()!;
            var name = attr.MethodName;

            uint reqArgCnt = 0;
            bool hasVarArgs = false;

            var paramInfos = methodInfo.GetParameters();
            if (paramInfos.Length < 2)
            {
                throw new Exception("Method must have at least 2 parameters");
            }

            var returnType = methodInfo.ReturnType;
            if (returnType != typeof(RbValue))
            {
                throw new Exception("Return type must be RbValue");
            }

            var firstParam = paramInfos[0];

            if (firstParam.ParameterType != typeof(RbState))
            {
                throw new Exception("First parameter must be RbState");
            }

            var secondParam = paramInfos[1];
            if (secondParam.ParameterType != typeof(RbValue))
            {
                throw new Exception("Second parameter must be RbValue");
            }

            var argNumToScan = paramInfos.Length;
            var lastParam = paramInfos.Last();

            uint aspect = 0;

            // variable args
            if (lastParam.ParameterType == typeof(RbValue[]))
            {
                hasVarArgs = true;
                --argNumToScan;
            }

            for (var i = 2; i < argNumToScan; i++)
            {
                var param = paramInfos[i];

                if (param.ParameterType != typeof(RbValue))
                {
                    throw new Exception("Parameter must be RbValue");
                }

                ++reqArgCnt;
            }

            // valid method signatures:
            // RbValue Foo(RbState state, RbValue self)
            // RbValue Foo(RbState state, RbValue self, RbValue arg0) 
            // RbValue Foo(RbState state, RbValue self, RbValue arg0, RbValue[] varArgs)
            // RbValue Foo(RbState state, RbValue self, RbValue[] varArgs)

            if (reqArgCnt == 0)
            {
                aspect |= RbHelper.MRB_ARGS_NONE();
            }
            else
            {
                aspect |= RbHelper.MRB_ARGS_ARG(reqArgCnt, 0);
            }

            if (attr.HasBlockArg)
            {
                aspect |= RbHelper.MRB_ARGS_BLOCK();
            }

            if (hasVarArgs)
            {
                aspect |= RbHelper.MRB_ARGS_REST();
            }

            // todo: use emit instead of reflection
            var invokeFn = new CSharpMethodFunc((state, self, args) =>
            {
                var callArgs = new object[2 + reqArgCnt + (hasVarArgs ? 1 : 0)];
                callArgs[0] = state;
                callArgs[1] = self;
                for (var i = 2; i < 2 + reqArgCnt; i++)
                {
                    callArgs[i] = args[i - 2];
                }

                if (hasVarArgs)
                {
                    callArgs[reqArgCnt + 2] = args.Skip((int)reqArgCnt).ToArray();
                }
                return methodInfo.Invoke(null, callArgs) as RbValue;
            });

            defineFn(name, invokeFn, aspect);
        }
    }
}