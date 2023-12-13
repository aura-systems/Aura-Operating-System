local function main()
    print("Hello from Lua 5.2!")

    print(table.concat{"hello", ":", "UniLua"})

    local a = 10
    local b = tostring(a)
    print(type(a), type(b))
    print(a)

    for i = 1, 5 do
        print(i)
    end
end

return {
    main = main,
}
