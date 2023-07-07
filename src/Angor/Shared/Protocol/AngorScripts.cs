﻿using System.Text;
using NBitcoin;
using NBitcoin.Crypto;
using Script = Blockcore.Consensus.ScriptInfo.Script;


namespace Angor.Shared.Protocol
{
    public class AngorScripts
    {
        public static Script CreateStage(Blockcore.Networks.Network network, ProjectScripts scripts)
        {
            var treeInfo = AngorScripts.BuildTaprootSpendInfo(scripts);

            var address = treeInfo.OutputPubKey.GetAddress(NetworkMapper.Map(network));

            return new Script(address.ScriptPubKey.ToBytes());
        }

        public static Script CreateControlBlockFounder(ProjectScripts scripts)
        {
            var treeInfo = AngorScripts.BuildTaprootSpendInfo(scripts);

            ControlBlock controlBlock = treeInfo.GetControlBlock(new NBitcoin.Script(scripts.Founder.ToBytes()), 
                (byte)TaprootConstants.TAPROOT_LEAF_TAPSCRIPT);

            return new Script(controlBlock.ToBytes());
        }

        public static Script CreateControlBlockExpiry(ProjectScripts scripts)
        {
            var treeInfo = AngorScripts.BuildTaprootSpendInfo(scripts);

            ControlBlock controlBlock = treeInfo.GetControlBlock(new NBitcoin.Script(scripts.EndOfProject.ToBytes()),
                (byte)TaprootConstants.TAPROOT_LEAF_TAPSCRIPT);

            return new Script(controlBlock.ToBytes());
        }

        private static TaprootSpendInfo BuildTaprootSpendInfo(ProjectScripts scripts)
        {
            var taprootKey = CreateUnspendableInternalKey();

            var scriptWeights = new List<(uint, NBitcoin.Script)>()
            {
                (70u, new NBitcoin.Script (scripts.Founder.ToBytes())),
                (40u, new NBitcoin.Script (scripts.Recover.ToBytes())),
                (1u, new NBitcoin.Script (scripts.EndOfProject.ToBytes()))
            };

            foreach (var scriptsSeeder in scripts.Seeders)
            {
                scriptWeights.Add((10u, new NBitcoin.Script(scriptsSeeder.ToBytes())));
            }

            var treeInfo = TaprootSpendInfo.WithHuffmanTree(taprootKey, scriptWeights.ToArray());

            return treeInfo;
        }

        public static TaprootInternalPubKey CreateUnspendableInternalKey()
        {
            // 1. Calculate the SHA256 of a known constant
            var sha256 = Hashes.SHA256(Encoding.UTF8.GetBytes("Angor Unspendable Taproot Key"));

            if (!TaprootPubKey.TryCreate(sha256, out TaprootPubKey? taprootPubKey))
            {
                throw new Exception();
            }

            var taprootInternalPubKey = new TaprootInternalPubKey(taprootPubKey.ToBytes());

            //// todo: double check this key is unspendable
            //https://github.com/bitcoin/bips/blob/master/bip-0341.mediawiki#constructing-and-spending-taproot-outputs
            //// this is a key that can not be spent, we will always spend a tapscript using scripts
            //var taprootKey = TaprootInternalPubKey.Parse("0x50929b74c1a04954b78b4b6035e97a5e078a5a0f28ec96d547bfee9ace803ac0");


            return taprootInternalPubKey;
        }
    }
}