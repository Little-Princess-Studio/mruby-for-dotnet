$p0 = proc do
    Fiber.yield 1
    Fiber.yield 2
end

$p1 = proc do
    v = Fiber.yield 3
    Fiber.yield 4 + v
end