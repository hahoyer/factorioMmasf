local Constants = require("Constants")
local Helper = require("ingteb.Helper")
local Table = require("core.Table")
local Array = Table.Array
local Dictionary = Table.Dictionary
local Common = require("ingteb.Common")

local FuelCategory = Common:class("FuelCategory")

function FuelCategory:new(name, prototype, database)
    assert(name)

    local self = Common:new(prototype or game.fuel_category_prototypes[name], database)
    self.object_name = FuelCategory.object_name

    assert(self.Prototype.object_name == "LuaFuelCategoryPrototype")

    self.Workers = Array:new()

    self:properties{
        Fuels = {
            cache = true,
            get = function()
                return self.Database.ItemsForFuelCategory[self.Name] --
                :Select(function(itemName) return self.Database:GetItem(itemName) end)
            end,
        },
    }

    function self:SortAll() end

    return self

end

return FuelCategory

