$p0 = proc do
    Fiber.yield 1
    Fiber.yield 2
end

$p1 = proc do
    Fiber.yield 3
    Fiber.yield 4
end