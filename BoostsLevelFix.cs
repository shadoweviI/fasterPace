using HarmonyLib;

namespace fasterPace
{
    [HarmonyPatch(typeof(InventoryController), nameof(InventoryController.mergeAll))]
    internal static class Patch_InventoryController_MergeAll_BoostClampLikeNormalItems
    {
        [HarmonyPrefix]
        private static bool Prefix(InventoryController __instance, int id)
        {
            if (id >= __instance.curSpaces() && id < 10000)
                return false;

            if (id < -10)
                return false;

            for (int i = 0; i < __instance.character.inventory.inventory.Count; i++)
            {
                __instance.character.inventory.item1 = id;
                __instance.character.inventory.item2 = i;

                if (id == i)
                    continue;

                if (id >= 0 && id < 10000)
                {
                    var target = __instance.character.inventory.inventory[__instance.character.inventory.item1];
                    var other = __instance.character.inventory.inventory[__instance.character.inventory.item2];

                    if (other.isMacGuffin() &&
                        other.id == target.id &&
                        other.removable &&
                        !other.isBoost())
                    {
                        __instance.character.inventory.item1 = i;
                        __instance.character.inventory.item2 = id;
                        __instance.swapItems();
                    }
                    else if (other.id == target.id &&
                             other.removable &&
                             other.level < 100 &&
                             target.level < 100 &&
                             !other.isBoost())
                    {
                        __instance.character.inventory.item1 = i;
                        __instance.character.inventory.item2 = id;
                        __instance.swapItems();
                    }
                    else if (other.id == target.id &&
                             other.removable &&
                             other.isBoost() &&
                             !__instance.character.inventory.itemList.itemMaxxed[other.id])
                    {
                        // Changed from vanilla:
                        // removed "other.level + target.level <= 100"
                        // so boosts merge like normal items and clamp in mergeItem()
                        __instance.character.inventory.item1 = i;
                        __instance.character.inventory.item2 = id;
                        __instance.swapItems();
                    }
                }
                else if (__instance.isAccessoryID(id))
                {
                    int num = id - 10000;
                    if (num < __instance.character.inventory.accs.Count &&
                        __instance.character.inventory.inventory[__instance.character.inventory.item2].id == __instance.character.inventory.accs[num].id &&
                        __instance.character.inventory.inventory[__instance.character.inventory.item2].removable &&
                        __instance.character.inventory.inventory[__instance.character.inventory.item2].level < 100 &&
                        __instance.character.inventory.accs[num].level < 100 &&
                        __instance.character.inventory.inventory[__instance.character.inventory.item2].isEquipment())
                    {
                        __instance.swapAcc();
                    }
                }
                else if (__instance.toMacGuffinIndex(id) >= 0)
                {
                    int num2 = __instance.toMacGuffinIndex(id);
                    if (num2 < __instance.character.inventory.macguffins.Count &&
                        __instance.character.inventory.inventory[__instance.character.inventory.item2].id == __instance.character.inventory.macguffins[num2].id &&
                        __instance.character.inventory.inventory[__instance.character.inventory.item2].removable &&
                        __instance.character.inventory.inventory[__instance.character.inventory.item2].isMacGuffin())
                    {
                        __instance.swapMacguffin();
                    }
                }
                else
                {
                    __instance.character.inventory.item1 = id;

                    switch (id)
                    {
                        case -6:
                            if (__instance.character.inventory.inventory[__instance.character.inventory.item2].id == __instance.character.inventory.weapon2.id &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].removable &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].level < 100 &&
                                __instance.character.inventory.weapon2.level < 100 &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].isEquipment())
                            {
                                __instance.swapWeapon2();
                            }
                            break;

                        case -5:
                            if (__instance.character.inventory.inventory[__instance.character.inventory.item2].id == __instance.character.inventory.weapon.id &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].removable &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].level < 100 &&
                                __instance.character.inventory.weapon.level < 100 &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].isEquipment())
                            {
                                __instance.swapWeapon();
                            }
                            break;

                        case -4:
                            if (__instance.character.inventory.inventory[__instance.character.inventory.item2].id == __instance.character.inventory.boots.id &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].removable &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].level < 100 &&
                                __instance.character.inventory.boots.level < 100 &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].isEquipment())
                            {
                                __instance.swapBoots();
                            }
                            break;

                        case -3:
                            if (__instance.character.inventory.inventory[__instance.character.inventory.item2].id == __instance.character.inventory.legs.id &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].removable &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].level < 100 &&
                                __instance.character.inventory.legs.level < 100 &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].isEquipment())
                            {
                                __instance.swapLegs();
                            }
                            break;

                        case -2:
                            if (__instance.character.inventory.inventory[__instance.character.inventory.item2].id == __instance.character.inventory.chest.id &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].removable &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].level < 100 &&
                                __instance.character.inventory.chest.level < 100 &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].isEquipment())
                            {
                                __instance.swapChest();
                            }
                            break;

                        case -1:
                            if (__instance.character.inventory.inventory[__instance.character.inventory.item2].id == __instance.character.inventory.head.id &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].removable &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].level != 100 &&
                                __instance.character.inventory.head.level != 100 &&
                                __instance.character.inventory.inventory[__instance.character.inventory.item2].isEquipment())
                            {
                                __instance.swapHead();
                            }
                            break;
                    }
                }
            }

            __instance.character.inventory.item1 = 0;
            __instance.character.inventory.item2 = 0;
            __instance.updateInventory();
            return false;
        }
    }
}