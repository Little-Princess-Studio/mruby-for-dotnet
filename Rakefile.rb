task :default => [:build]

task :build do
  sh 'cd ./mruby && rake MRUBY_CONFIG=../costumized-build-conf-mac.rb all test'
end
