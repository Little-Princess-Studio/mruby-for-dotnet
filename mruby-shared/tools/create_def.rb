ctags_exe = 'ctags.exe'
mruby_include_dir = '../../mruby/include'
files = Dir["#{mruby_include_dir}/*.h", "#{mruby_include_dir}/mruby/*.h", "../src/main.h"]
if files.empty?
  $stderr.puts 'Header files not found!'
  exit 1
end

exclude_names = []
File.open('exclude_export_names.txt', "r") do |f|
  f.each_line do |line|
    line.chomp!
    exclude_names << line
  end
end

File.open('mruby.def', 'w') do |f|
  f.puts 'LIBRARY mruby.dll'
  f.puts 'EXPORTS'
  IO.popen("#{ctags_exe} -u -x --c-kinds=p #{files.join(' ')}") do |io|
    io.each_line do |line|
      if line.include? 'MRB_API'
        name_to_write = line[/^([A-Za-z_][\w]*)\b/]

        if not exclude_names.include? name_to_write
          puts line
          f.putc "\t"
          f.puts line[/^([A-Za-z_][\w]*)\b/]
        end
      end
    end
  end
end