add_rules("mode.debug", "mode.releasedbg")

local os_name = os.host()
local mruby_dir = "../mruby"

function common_settings()
    if is_arch("x86_64") then
        set_arch("x64")
    else
	set_arch("arm64")
    end
    set_kind("shared")
    add_files("src/*.c")

    add_includedirs("src/", mruby_dir .. "/build/host/include")
    add_defines("MRB_CORE", "MRB_LIB")
    add_defines("MRB_INT64", "MRB_UTF8_STRING")
end

function copy_dll_to_target(target)
    local target_dir = "../mruby-wrapper/MRuby.Library"
    os.cp(target:targetfile(), target_dir)
    target_dir = "../mruby-wrapper/MRuby.UnitTest"
    os.cp(target:targetfile(), target_dir)
end

local function copy_dylib_to_target(dylib_path, os)
    local target_dir = "../mruby-wrapper/MRuby.Library"
    os.cp(dylib_path, target_dir)
    target_dir = "../mruby-wrapper/MRuby.UnitTest"
    os.cp(dylib_path, target_dir)
end

function after_build_macos(target)
    local mode = is_mode("debug") and "debug" or "release"
    local output_dir = path.join(os.projectdir(), string.format("build/macosx/universal/%s/", mode))
    os.exec("mkdir -p %s", output_dir)
    os.exec("lipo -create -output %s %s %s", 
            path.join(output_dir, "libmruby_x64.dylib"), 
            path.join(os.projectdir(), string.format("build/macosx/arm64/%s/libmruby_arm64.dylib", mode)), 
            path.join(os.projectdir(), string.format("build/macosx/x86_64/%s/libmruby_x86_64.dylib", mode)))

    copy_dylib_to_target(path.join(output_dir, "libmruby_x64.dylib"), os)
end

target("libmruby_x64")
if os_name == "windows" then
    common_settings()

    add_defines("MRB_BUILD_AS_DLL")
    set_basename("libmruby_x64")
    add_files("tools/mruby_x64.def")
    set_runtimes("MD")
    add_links("Ws2_32.lib",
        mruby_dir .. "/build/host/lib/libmruby.lib",
        mruby_dir .. "/build/host/lib/libmruby_core.lib")
    after_build(copy_dll_to_target)
elseif os_name == "linux" then
    common_settings()

    set_basename("mruby_x64")
    add_links(
        mruby_dir .. "/build/host/lib/libmruby.a",
        mruby_dir .. "/build/host/lib/libmruby_core.a")
    after_build(copy_dll_to_target)
elseif os_name == "macosx" then
    -- Build for x86_64
    target("mruby_mac_x86_64")
        common_settings()

        set_basename("mruby_x86_64")
        set_arch("x86_64")
        add_links(
            mruby_dir .. "/build/x86_64/lib/libmruby.a",
            mruby_dir .. "/build/x86_64/lib/libmruby_core.a")

    target("mruby_mac_arm64")
        common_settings()

        set_basename("mruby_arm64")
        set_arch("arm64")
        add_links(
            mruby_dir .. "/build/arm64/lib/libmruby.a",
            mruby_dir .. "/build/arm64/lib/libmruby_core.a")
    
    -- Combine into a universal binary
    target("libmruby_x64")
        set_kind("phony")
        add_deps("mruby_mac_x86_64", "mruby_mac_arm64")
        after_build(after_build_macos)
        on_clean(function(target)
            local mode = is_mode("debug") and "debug" or "release"
            local output_dir = path.join(os.projectdir(), string.format("build/macosx/universal/%s/", mode))
            local bin_file = path.join(output_dir, "libmruby_x64.dylib")
            if os.isfile(bin_file) then
                os.rm(bin_file)
            end
        end)
else
    -- error: not support platform
    print("unsupported platform")
end

-- target("mruby_x86")
--     set_arch("x86")
--     common_settings()
--     after_build(copy_dll_to_target)
