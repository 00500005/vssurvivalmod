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
using Vintagestory.ServerMods;

namespace Vintagestory.GameContent
{
    public class BlockMicroBlock : Block
    {
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
        }


        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            //return forPlayer?.InventoryManager.ActiveHotbarSlot?.Itemstack?.Item is ItemChisel;
            return true;
        }



        public override byte[] GetLightHsv(IBlockAccessor blockAccessor, BlockPos pos, ItemStack stack = null)
        {
            // Causes relighting issues
            /*int[] matids;
            float sizerel=1;

            if (pos != null)
            {
                BlockEntityChisel bec = blockAccessor.GetBlockEntity(pos) as BlockEntityChisel;
                if (bec?.MaterialIds == null)
                {
                    return base.GetLightHsv(blockAccessor, pos, stack);
                }

                matids = bec.MaterialIds;
                sizerel = bec.sizeRel;

            } else
            {
                matids = (stack.Attributes?["materials"] as IntArrayAttribute)?.value;
            }

            for (int i = 0; i < matids.Length; i++)
            {
                Block block = blockAccessor.GetBlock(matids[i]);
                if (block.LightHsv[2] > 0)
                {
                    return new byte[] { block.LightHsv[0], block.LightHsv[1], (byte)(block.LightHsv[2] * sizerel) };
                }
            }*/

            return base.GetLightHsv(blockAccessor, pos, stack);
        }
        
        public override bool MatchesForCrafting(ItemStack inputStack, GridRecipe gridRecipe, CraftingRecipeIngredient ingredient)
        {
            int len = (inputStack.Attributes["materials"] as StringArrayAttribute)?.value?.Length ?? 0;

            if (len > 2) return false;

            return base.MatchesForCrafting(inputStack, gridRecipe, ingredient);
        }

        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            List<int> matids = new List<int>();
            bool first = false;
            foreach (var val in allInputslots)
            {
                if (val.Empty) continue;
                if (!first)
                {
                    first = true;
                    outputSlot.Itemstack.Attributes = val.Itemstack.Attributes.Clone();
                }

                int[] mats = (val.Itemstack.Attributes?["materials"] as IntArrayAttribute)?.value;
                if (mats != null) matids.AddRange(mats);

                string[] smats = (val.Itemstack.Attributes?["materials"] as StringArrayAttribute)?.value;
                if (smats != null)
                {
                    foreach (var code in smats)
                    {
                        Block block = api.World.GetBlock(new AssetLocation(code));
                        if (block != null) matids.Add(block.Id);
                    }
                }
            }

            IntArrayAttribute attr = new IntArrayAttribute(matids.ToArray());
            outputSlot.Itemstack.Attributes["materials"] = attr;

            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);
        }

        public override int GetLightAbsorption(IBlockAccessor blockAccessor, BlockPos pos)
        {
            return GetLightAbsorption(blockAccessor.GetChunkAtBlockPos(pos), pos);
        }

        public override int GetLightAbsorption(IWorldChunk chunk, BlockPos pos)
        {
            BlockEntityMicroBlock bec = chunk?.GetLocalBlockEntityAtBlockPos(pos) as BlockEntityMicroBlock;
            return bec?.GetLightAbsorption() ?? 0;
        }


        public override bool DoEmitSideAo(IBlockAccessor blockAccessor, BlockPos pos, int facing)
        {
            BlockEntityMicroBlock bec = blockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;
            if (bec == null)
            {
                return base.DoEmitSideAo(blockAccessor, pos, facing);
            }

            return bec.DoEmitSideAo(facing);
        }

        public override bool DoEmitSideAoByFlag(IBlockAccessor blockAccessor, BlockPos pos, int flag)
        {
            BlockEntityMicroBlock bec = blockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;
            if (bec == null)
            {
                return base.DoEmitSideAoByFlag(blockAccessor, pos, flag);
            }

            return bec.DoEmitSideAoByFlag(flag);
        }

        public override ItemStack OnPickBlock(IWorldAccessor world, BlockPos pos)
        {
            BlockEntityMicroBlock bec = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;
            if (bec == null)
            {
                return null;
            }

            TreeAttribute tree = new TreeAttribute();
            bec.ToTreeAttributes(tree);
            tree.RemoveAttribute("posx");
            tree.RemoveAttribute("posy");
            tree.RemoveAttribute("posz");
            
            return new ItemStack(Id, EnumItemClass.Block, 1, tree, world);
        }

        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (Attributes?.IsTrue("dropSelf") == true)
            {
                return new ItemStack[] { OnPickBlock(world, pos) };
            }

            return new ItemStack[0];
        }

        public override bool CanAttachBlockAt(IBlockAccessor world, Block block, BlockPos pos, BlockFacing blockFace, Cuboidi attachmentArea = null)
        {
            BlockEntityMicroBlock be = world.GetBlockEntity(pos) as BlockEntityMicroBlock;
            if (be != null)
            {
                return be.CanAttachBlockAt(blockFace, attachmentArea);
            }

            return base.CanAttachBlockAt(world, block, pos, blockFace);
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);

            BlockEntityMicroBlock be = world.BlockAccessor.GetBlockEntity(blockPos) as BlockEntityMicroBlock;
            if (be != null && byItemStack != null)
            {
                byItemStack.Attributes.SetInt("posx", blockPos.X);
                byItemStack.Attributes.SetInt("posy", blockPos.Y);
                byItemStack.Attributes.SetInt("posz", blockPos.Z);

                be.FromTreeAttributes(byItemStack.Attributes, world);
                be.MarkDirty(true);

                if (world.Side == EnumAppSide.Client)
                {
                    be.RegenMesh();
                }

                be.RegenSelectionBoxes(null);
            }
        }

        public override float OnGettingBroken(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
        {
            return base.OnGettingBroken(player, blockSel, itemslot, remainingResistance, dt, counter);
        }


        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            ChiselBlockModelCache cache = capi.ModLoader.GetModSystem<ChiselBlockModelCache>();
            renderinfo.ModelRef = cache.GetOrCreateMeshRef(itemstack);
        }

        public override void GetDecal(IWorldAccessor world, BlockPos pos, ITexPositionSource decalTexSource, ref MeshData decalModelData, ref MeshData blockModelData)
        {
            BlockEntityMicroBlock be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;
            if (be != null)
            {
                blockModelData = be.Mesh;
                decalModelData = be.CreateDecalMesh(decalTexSource);
                return;
            }

            base.GetDecal(world, pos, decalTexSource, ref decalModelData, ref blockModelData);
        }


        public override Cuboidf[] GetSelectionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BlockEntityMicroBlock bec = blockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;

            if (bec != null)
            {
                Cuboidf[] selectionBoxes = bec.GetSelectionBoxes(blockAccessor, pos);

                return selectionBoxes;
            }

            return base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override Cuboidf[] GetCollisionBoxes(IBlockAccessor blockAccessor, BlockPos pos)
        {
            BlockEntityMicroBlock bec = blockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;

            if (bec != null)
            {
                Cuboidf[] selectionBoxes = bec.GetCollisionBoxes(blockAccessor, pos);

                return selectionBoxes;
            }

            return base.GetSelectionBoxes(blockAccessor, pos);
        }

        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing)
        {
            BlockEntityMicroBlock be = capi.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;
            if (be?.MaterialIds != null && be.MaterialIds.Length > 0)
            {
                Block block = capi.World.GetBlock(be.MaterialIds[0]);
                if (block is BlockMicroBlock) return 0; // Prevent-chisel-ception. Happened to WQP, not sure why

                return block.GetRandomColor(capi, pos, facing);
            }

            return base.GetRandomColor(capi, pos, facing);
        }


        public override bool Equals(ItemStack thisStack, ItemStack otherStack, params string[] ignoreAttributeSubTrees)
        {
            List<string> ign = new List<string>(ignoreAttributeSubTrees);
            ign.Add("meshid");
            return base.Equals(thisStack, otherStack, ign.ToArray());
        }

        public override int GetColorWithoutTint(ICoreClientAPI capi, BlockPos pos)
        {
            BlockEntityMicroBlock be = capi.World.BlockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;
            if (be?.MaterialIds != null && be.MaterialIds.Length > 0)
            {
                Block block = capi.World.GetBlock(be.MaterialIds[0]);
                if (block is BlockMicroBlock) return 0; // Prevent-chisel-ception. Happened to WQP, not sure why

                return block.GetColor(capi, pos);
            }

            return base.GetColorWithoutTint(capi, pos);
        }


        public override string GetPlacedBlockName(IWorldAccessor world, BlockPos pos)
        {
            BlockEntityMicroBlock be = world.BlockAccessor.GetBlockEntity(pos) as BlockEntityMicroBlock;
            if (be != null) return be.BlockName;

            return base.GetPlacedBlockName(world, pos);
        }


        public override Block GetSnowCoveredVariant(BlockPos pos, float snowLevel)
        {
            return base.GetSnowCoveredVariant(pos, snowLevel);
        }

        public override float GetSnowLevel(BlockPos pos)
        {
            return base.GetSnowLevel(pos);
        }

        public override void PerformSnowLevelUpdate(IBulkBlockAccessor ba, BlockPos pos, Block newBlock, float snowLevel)
        {
            base.PerformSnowLevelUpdate(ba, pos, newBlock, snowLevel);
        }

    }
}