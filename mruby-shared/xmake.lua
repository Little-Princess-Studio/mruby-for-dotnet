add_rules("mode.debug", "mode.releasedbg")

local os_name = os.host()
local mruby_dir = "../mruby"

local function common_settings()
    set_kind("shared")
    add_files("src/*.c")

    if os_name == "windows" then
        add_files("tools/mruby.def")
    end

    add_includedirs("src/", mruby_dir .. "/build/host/include")
    add_defines("MRB_CORE", "MRB_LIB")
    add_defines("MRB_INT64")

    if os_name == "windows" then
        add_defines("MRB_BUILD_AS_DLL")
        set_runtimes("MD")
        add_links("Ws2_32.lib", mruby_dir .. "/build/host/lib/libmruby.lib", mruby_dir .. "/build/host/lib/libmruby_core.lib")
    elseif os_name == "linux" then
        add_links(mruby_dir .. "/build/host/lib/libmruby.a", mruby_dir .. "/build/host/lib/libmruby_core.a")
    else
        -- error: not support platform
        print("unsupported platform")
    end
end

local function copy_dll_to_target(target)
    local target_dir = "../mruby-wrapper/MRuby.Library"
    os.cp(target:targetfile(), target_dir)
    target_dir = "../mruby-wrapper/MRuby.UnitTest"
    os.cp(target:targetfile(), target_dir)
end

target("mruby_x64")
    set_arch("x64")
    common_settings()
    after_build(copy_dll_to_target)

-- target("mruby_x86")
--     set_arch("x86")
--     common_settings()
--     after_build(copy_dll_to_target)