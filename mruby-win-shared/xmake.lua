add_rules("mode.debug", "mode.release")

local mruby_dir = "../mruby"

target("mruby")
    set_kind("shared")
    add_files("src/*.cpp", "tools/mruby.def")
    add_includedirs(mruby_dir .. "/build/host/include")
    add_defines("MRB_BUILD_AS_DLL", "MRB_CORE", "MRB_LIB")
    set_runtimes("MD")
    add_links("Ws2_32.lib", mruby_dir .. "/build/host/lib/libmruby.lib", mruby_dir .. "/build/host/lib/libmruby_core.lib")
