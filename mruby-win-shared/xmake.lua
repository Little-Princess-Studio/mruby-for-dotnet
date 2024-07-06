add_rules("mode.debug", "mode.releasedbg")

local mruby_dir = "../mruby"

local function common_settings()
    set_kind("shared")
    add_files("src/*.c", "tools/mruby.def")
    add_includedirs("src/", mruby_dir .. "/build/host/include")
    add_defines("MRB_BUILD_AS_DLL", "MRB_CORE", "MRB_LIB")
    set_runtimes("MD")
    add_links("Ws2_32.lib", mruby_dir .. "/build/host/lib/libmruby.lib", mruby_dir .. "/build/host/lib/libmruby_core.lib")
end

local function copy_dll_to_target(target)
    local target_dir = "../mruby-wrapper/MRubyWrapper.Library"
    os.cp(target:targetfile(), target_dir)
    target_dir = "../mruby-wrapper/MRubyWrapper.UnitTest"
    os.cp(target:targetfile(), target_dir)
end

target("mruby_x64")
    set_arch("x64")
    common_settings()
    add_defines("MRB_INT64")
    after_build(copy_dll_to_target)

-- target("mruby_x86")
--     set_arch("x86")
--     common_settings()
--     after_build(copy_dll_to_target)