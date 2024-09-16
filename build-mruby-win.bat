@echo off

cd ./mruby
rake MRUBY_CONFIG=../costumized-build-conf-win.rb all test