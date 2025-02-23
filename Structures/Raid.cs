﻿using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace RaidCrawler.Structures
{
    public partial class Raid
    // See also https://github.com/kwsch/PKHeX/blob/master/PKHeX.Core/Saves/Substructures/Gen9/RaidSpawnList9.cs
    {
        public const byte SIZE = 0x20;
        const int IV_MAX = 31;

        public static string Game = "Scarlet";
        public static readonly GameStrings strings = GameInfo.GetStrings(1);

        public static ITeraRaid[]? GemTeraRaids;
        public static ITeraRaid[]? DistTeraRaids;
        public static List<RaidFixedRewards>? BaseFixedRewards = new();
        public static List<RaidLotteryRewards>? BaseLotteryRewards = new();
        public static List<DeliveryRaidFixedRewardItem>? DeliveryRaidFixedRewards = new();
        public static List<DeliveryRaidLotteryRewardItem>? DeliveryRaidLotteryRewards = new();

        public readonly byte[] Data; // Raw data

        public Raid(byte[] data) => Data = data;
        public Raid(int size) => Data = new byte[size];

        public virtual bool IsValid => Validate(); // Does this look like an actual raid

        public virtual bool IsActive => ReadUInt32LittleEndian(Data.AsSpan(0x00)) == 1;
        public virtual uint Area => ReadUInt32LittleEndian(Data.AsSpan(0x04));
        public virtual uint DisplayType => ReadUInt32LittleEndian(Data.AsSpan(0x08));
        public virtual uint Den => ReadUInt32LittleEndian(Data.AsSpan(0x0C));
        public virtual uint Seed => ReadUInt32LittleEndian(Data.AsSpan(0x10));
        public virtual uint Flags => ReadUInt32LittleEndian(Data.AsSpan(0x18));
        public virtual bool IsBlack => Flags == 1;
        public virtual bool IsEvent => Flags >= 2;

        // Derived Values
        public virtual int TeraType => GetTeraType(Seed);
        public virtual uint Difficulty => GetDifficulty(Seed);
        public ITeraRaid? Encounter(int Stage) => IsEvent ? TeraDistribution.GetEncounter(Seed, Stage, Flags == 3) : TeraEncounter.GetEncounter(Seed, Stage, IsBlack);

        public virtual uint EC => GenericRaidData[0];
        /* public virtual uint TIDSID => GenericRaidData[1]; */ // Unneeded
        public virtual uint PID => GenericRaidData[2];
        public virtual bool IsShiny => GenericRaidData[3] == 1;


        uint[] GenericRaidData => GenerateGenericRaidData(Seed);

        // Methods
        private bool Validate()
        {
            if (Seed == 0) return false;
            if (!IsActive) return false;
            if (Area > 22) return false;
            GenerateGenericRaidData(Seed);
            return true;
        }

        private static int GetTeraType(uint Seed)
        {
            var rng = new Xoroshiro128Plus(Seed);
            return (int)rng.NextInt(18);
        }

        private static uint[] GenerateGenericRaidData(uint Seed)
        {
            var rng = new Xoroshiro128Plus(Seed);
            uint EC = (uint)rng.NextInt();
            uint TIDSID = (uint)rng.NextInt();
            uint PID = (uint)rng.NextInt();
            var Shiny = (((PID >> 16) ^ (PID & 0xFFFF)) >> 4) == (((TIDSID >> 16) ^ (TIDSID & 0xFFFF)) >> 4) ? 1 : 0;
            return new uint[] { EC, TIDSID, PID, (uint)Shiny };
        }

        public int[] GetIVs(uint Seed, int FlawlessIVs)
        {
            var rng = new Xoroshiro128Plus(Seed);
            // Dummy calls
            rng.NextInt(); // EC
            rng.NextInt(); // TIDSID
            rng.NextInt(); // PID

            Span<int> ivs = stackalloc[] { -1, -1, -1, -1, -1, -1 };
            // Flawless IVs
            for (int i = 0; i < FlawlessIVs; i++)
            {
                int index;
                do { index = (int)rng.NextInt(6); }
                while (ivs[index] != -1);

                ivs[index] = IV_MAX;
            }
            // Other IVs
            for (int i = 0; i < ivs.Length; i++)
            {
                if (ivs[i] == -1)
                    ivs[i] = (int)rng.NextInt(32);
            }

            return ivs.ToArray();
        }

        public static int GetStarCount(uint Difficulty, int Progress, bool IsBlack)
        {
            if (IsBlack) return 6;
            return Progress switch
            {
                0 => Difficulty switch
                {
                    > 80 => 2,
                    _ => 1,
                },
                1 => Difficulty switch
                {
                    > 70 => 3,
                    > 30 => 2,
                    _ => 1,
                },
                2 => Difficulty switch
                {
                    > 70 => 4,
                    > 40 => 3,
                    > 20 => 2,
                    _ => 1,
                },
                3 => Difficulty switch
                {
                    > 75 => 5,
                    > 40 => 4,
                    _ => 3,
                },
                4 => Difficulty switch
                {
                    > 70 => 5,
                    > 30 => 4,
                    _ => 3,
                },
                _ => 1,
            };
        }

        private static uint GetDifficulty(uint Seed)
        {
            var rng = new Xoroshiro128Plus(Seed);
            return (uint)rng.NextInt(100);
        }
    }
}
