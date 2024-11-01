MRuby::Build.new do |conf|
    # load specific toolchain settings
    conf.toolchain :clang
  
    # Define build settings for x86_64
    MRuby::CrossBuild.new('x86_64') do |conf|
      conf.cc do cc
        cc.defines = %w(MRB_CORE MRB_INT64 MRB_UTF8_STRING MRB_LIB)
      end
  
      toolchain :clang
      conf.cc.command = 'clang'
      conf.cc.flags << '-arch x86_64'
      conf.linker.command = 'clang'
      conf.linker.flags << '-arch x86_64'
      conf.archiver.command = 'ar'
      conf.archiver.archive_options = 'rs %{outfile} %{objs}'
      conf.gembox 'default'

      conf.enable_bintest
      conf.enable_test  
    end

    # Define build settings for arm64
    MRuby::CrossBuild.new('arm64') do |conf|
      conf.cc do cc
        cc.defines = %w(MRB_CORE MRB_INT64 MRB_UTF8_STRING MRB_LIB)
      end
  
      toolchain :clang
      conf.cc.command = 'clang'
      conf.cc.flags << '-arch arm64'
      conf.linker.command = 'clang'
      conf.linker.flags << '-arch arm64'
      conf.archiver.command = 'ar'
      conf.archiver.archive_options = 'rs %{outfile} %{objs}'
      conf.gembox 'default'

      conf.enable_bintest
      conf.enable_test  
    end

    # Use mrbgems
    # conf.gem 'examplesmrbgemsruby_extension_example'
    # conf.gem 'examplesmrbgemsc_extension_example' do g
    #   g.cc.flags  '-g' # append cflags in this gem
    # end
    # conf.gem 'examplesmrbgemsc_and_ruby_extension_example'
    # conf.gem core = 'mruby-eval'
    # conf.gem mgem = 'mruby-onig-regexp'
    # conf.gem github = 'mattnmruby-onig-regexp'
    # conf.gem git = 'git@github.commattnmruby-onig-regexp.git', branch = 'master', options = '-v'
  
    # include the GEM box
    # conf.gembox 'default'
  
    # C compiler settings
    # conf.cc do cc
    #   cc.command = ENV['CC']  'gcc'
    #   cc.flags = [ENV['CFLAGS']  %w()]
    #   cc.include_paths = [#{root}include]
    #   cc.defines = %w()
    #   cc.option_include_path = %q[-I%s]
    #   cc.option_define = '-D%s'
    #   cc.compile_options = %Q[%{flags} -MMD -o %{outfile} -c %{infile}]
    # end
    conf.cc do cc
      cc.defines = %w(MRB_CORE MRB_INT64 MRB_LIB)
    end

    # mrbc settings
    # conf.mrbc do mrbc
    #   mrbc.compile_options = -g -B%{funcname} -o- # The -g option is required for line numbers
    # end
  
    # Linker settings
    # conf.linker do linker
    #   linker.command = ENV['LD']  'gcc'
    #   linker.flags = [ENV['LDFLAGS']  []]
    #   linker.flags_before_libraries = []
    #   linker.libraries = %w()
    #   linker.flags_after_libraries = []
    #   linker.library_paths = []
    #   linker.option_library = '-l%s'
    #   linker.option_library_path = '-L%s'
    #   linker.link_options = %Q[%{flags} -o %{outfile} %{objs} %{libs}]
    # end
  
    # Archiver settings
    # conf.archiver do archiver
    #   archiver.command = ENV['AR']  'ar'
    #   archiver.archive_options = 'rs %{outfile} %{objs}'
    # end
  
    # Parser generator settings
    # conf.yacc do yacc
    #   yacc.command = ENV['YACC']  'bison'
    #   yacc.compile_options = %q[-o %{outfile} %{infile}]
    # end
  
    # gperf settings
    # conf.gperf do gperf
    #   gperf.command = 'gperf'
    #   gperf.compile_options = %q[-L ANSI-C -C -p -j1 -i 1 -g -o -t -N mrb_reserved_word -k1,3,$ %{infile}  %{outfile}]
    # end
  
    # file extensions
    # conf.exts do exts
    #   exts.object = '.o'
    #   exts.executable = '' # '.exe' if Windows
    #   exts.library = '.a'
    # end
  
    # file separator
    # conf.file_separator = ''
  
    # change library directory name from the default lib if necessary
    # conf.libdir_name = 'lib64'
  
    # Turn on `enable_debug` for better debugging
    # conf.enable_debug
    conf.enable_bintest
    conf.enable_test
  end
  
