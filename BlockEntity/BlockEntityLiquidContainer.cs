﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace Vintagestory.GameContent
{
    public abstract class BlockEntityLiquidContainer : BlockEntityContainer
    {
        protected InventoryGeneric inventory;
        public override InventoryBase Inventory => inventory;

        

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            inventory.OnGetAutoPushIntoSlot = GetAutoPushIntoSlot;
        }

        // Cannot receive items
        protected virtual ItemSlot GetAutoPushIntoSlot(BlockFacing atBlockFace, ItemSlot fromSlot)
        {
            return null;
        }


        public ItemStack GetContent()
        {
            return inventory[0].Itemstack;
        }


        public void SetContent(ItemStack stack)
        {
            inventory[0].Itemstack = stack;
            MarkDirty(true);
        }
        



    }
}
