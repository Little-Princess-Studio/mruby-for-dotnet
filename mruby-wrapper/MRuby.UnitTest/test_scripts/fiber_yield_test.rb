fib = Fiber.new do
    test_yield 1 # = yield 1+1
    test_yield 2 # = yield 2+1
end

fib