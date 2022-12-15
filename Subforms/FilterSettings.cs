﻿using Newtonsoft.Json;
using PKHeX.Core;
using RaidCrawler.Structures;

namespace RaidCrawler.Subforms
{
    public partial class FilterSettings : Form
    {
        private readonly List<RaidFilter> filters;
        private readonly BindingSource bs = new();
        public FilterSettings(ref List<RaidFilter> filters)
        {
            InitializeComponent();
            this.filters = filters;
            var settings = Properties.Settings.Default;
            Species.DataSource = Enum.GetValues(typeof(Species)).Cast<Species>().Where(z => z != PKHeX.Core.Species.MAX_COUNT).ToArray();
            Nature.DataSource = Enum.GetValues(typeof(Nature));
            TeraType.DataSource = Enum.GetValues(typeof(MoveType)).Cast<MoveType>().Where(z => z != MoveType.Any).ToArray();
            ActiveFilters.DisplayMember = "Name";
            Stars.SelectedIndex = 0;
            StarsComp.SelectedIndex = 0;
            HPComp.SelectedIndex = 0;
            AtkComp.SelectedIndex = 0;
            DefComp.SelectedIndex = 0;
            SpaComp.SelectedIndex = 0;
            SpdComp.SelectedIndex = 0;
            SpeComp.SelectedIndex = 0;
            foreach (var filter in filters)
                ActiveFilters.Items.Add(filter);
            if (ActiveFilters.Items.Count > 0)
                ActiveFilters.SelectedIndex = 0;
            if (ActiveFilters.SelectedIndex == -1)
                Remove.Enabled = false;

            bs.DataSource = filters;
            ActiveFilters.DataSource = bs;
        }

        public void SelectFilter(RaidFilter settings)
        {
            FilterName.Text = settings.Name;
            Species.SelectedIndex = settings.Species != null ? (int)settings.Species : 0;
            Nature.SelectedIndex = settings.Nature != null ? (int)settings.Nature : 0;
            Stars.SelectedIndex = settings.Stars != null ? (int)settings.Stars - 1 : 0;
            StarsComp.SelectedIndex = settings.StarsComp;
            TeraType.SelectedIndex = settings.TeraType != null ? (int)settings.TeraType : 0;
            SpeciesCheck.Checked = settings.Species != null;
            NatureCheck.Checked = settings.Nature != null;
            StarCheck.Checked = settings.Stars != null;
            TeraCheck.Checked = settings.TeraType != null;
            ShinyCheck.Checked = settings.Shiny;
            CheckRewards.Checked = settings.RewardItems != null && settings.RewardsCount > 0;
            Rewards.Text = settings.RewardItems != null ? string.Join(",", settings.RewardItems.Select(x => x.ToString()).ToArray())
                                                        : "645,795,1606,1904,1905,1906,1907,1908";
            RewardsComp.SelectedIndex = settings.RewardsComp;
            RewardsCount.Value = settings.RewardsCount;
            DisableFilter.Checked = !settings.Enabled;

            var ivbin = settings.IVBin;
            HP.Checked = (ivbin & 1) == 1;
            Atk.Checked = ((ivbin >> 1) & 1) == 1;
            Def.Checked = ((ivbin >> 2) & 1) == 1;
            SpA.Checked = ((ivbin >> 3) & 1) == 1;
            SpD.Checked = ((ivbin >> 4) & 1) == 1;
            Spe.Checked = ((ivbin >> 5) & 1) == 1;

            var ivvals = settings.IVVals;
            IVHP.Value = ivvals & 31;
            IVATK.Value = (ivvals >> 5) & 31;
            IVDEF.Value = (ivvals >> 10) & 31;
            IVSPA.Value = (ivvals >> 15) & 31;
            IVSPD.Value = (ivvals >> 20) & 31;
            IVSPE.Value = (ivvals >> 25) & 31;

            var ivcomp = settings.IVComps;
            HPComp.SelectedIndex = (ivcomp & 7);
            AtkComp.SelectedIndex = (ivcomp >> 3) & 7;
            DefComp.SelectedIndex = (ivcomp >> 6) & 7;
            SpaComp.SelectedIndex = (ivcomp >> 9) & 7;
            SpdComp.SelectedIndex = (ivcomp >> 12) & 7;
            SpeComp.SelectedIndex = (ivcomp >> 15) & 7;

            IVHP.Enabled = HP.Checked;
            IVATK.Enabled = Atk.Checked;
            IVDEF.Enabled = Def.Checked;
            IVSPA.Enabled = SpA.Checked;
            IVSPD.Enabled = SpD.Checked;
            IVSPE.Enabled = Spe.Checked;

            HPComp.Enabled = HP.Checked;
            AtkComp.Enabled = Atk.Checked;
            DefComp.Enabled = Def.Checked;
            SpaComp.Enabled = SpA.Checked;
            SpdComp.Enabled = SpD.Checked;
            SpeComp.Enabled = Spe.Checked;

            Species.Enabled = SpeciesCheck.Checked;
            Nature.Enabled = NatureCheck.Checked;
            Stars.Enabled = StarCheck.Checked;
            StarsComp.Enabled = StarCheck.Checked;
            Rewards.Enabled = CheckRewards.Checked;
            ButtonOpenRewardsList.Enabled = CheckRewards.Checked;
            RewardsCount.Enabled = CheckRewards.Checked;
            RewardsComp.Enabled = CheckRewards.Checked;
            TeraType.Enabled = TeraCheck.Checked;
        }

        private void Add_Filter_Click(object sender, EventArgs e)
        {
            if (FilterName.Text.Trim() == string.Empty)
            {
                MessageBox.Show("Name is a required field!");
                return;
            }

            RaidFilter filter = new();
            var ivbin = ToInt(HP.Checked) << 0 | ToInt(Atk.Checked) << 1 | ToInt(Def.Checked) << 2 |
                        ToInt(SpA.Checked) << 3 | ToInt(SpD.Checked) << 4 | ToInt(Spe.Checked) << 5;
            var ivcomps = HPComp.SelectedIndex << 0 | AtkComp.SelectedIndex << 3 | DefComp.SelectedIndex << 6 |
                          SpaComp.SelectedIndex << 9 | SpdComp.SelectedIndex << 12 | SpeComp.SelectedIndex << 15;
            var ivvals = (int)IVHP.Value << 0 | (int)IVATK.Value << 5 | (int)IVDEF.Value << 10 |
                         (int)IVSPA.Value << 15 | (int)IVSPD.Value << 20 | (int)IVSPE.Value << 25;

            filter.Name = FilterName.Text.Trim();
            filter.Species = SpeciesCheck.Checked ? Species.SelectedIndex : null;
            filter.Nature = NatureCheck.Checked ? Nature.SelectedIndex : null;
            filter.Stars = StarCheck.Checked ? Stars.SelectedIndex + 1 : null;
            filter.StarsComp = StarsComp.SelectedIndex;
            filter.TeraType = TeraCheck.Checked ? TeraType.SelectedIndex : null;
            filter.Shiny = ShinyCheck.Checked;
            filter.IVBin = ivbin;
            filter.IVVals = ivvals;
            filter.IVComps = ivcomps;
            filter.RewardItems = CheckRewards.Checked ? Rewards.Text.Split(',').Where(z => int.TryParse(z.Trim(), out _) == true).Select(z => int.Parse(z.Trim())).ToArray() : null;
            filter.RewardsCount = (int)RewardsCount.Value;
            filter.RewardsComp = RewardsComp.SelectedIndex;
            filter.Enabled = !DisableFilter.Checked;

            if (filter.IsFilterSet())
            {
                for (int i = 0; i < ActiveFilters.Items.Count; i++)
                {
                    var f = filters.ElementAt(i);
                    if (f.Name == filter.Name)
                    {
                        filters.RemoveAt(i);
                        break;
                    }
                }
                filters.Add(filter);
                bs.ResetBindings(false);
                ActiveFilters.SelectedIndex = ActiveFilters.Items.Count - 1;
            }
            else
            {
                MessageBox.Show("You have not set any stop conditions. No filter will be added.");
            }
        }

        private static int ToInt(bool b) => b ? 1 : 0;

        private void SpeciesCheck_CheckedChanged(object sender, EventArgs e)
        {
            Species.Enabled = SpeciesCheck.Checked;
        }

        private void NatureCheck_CheckedChanged(object sender, EventArgs e)
        {
            Nature.Enabled = NatureCheck.Checked;
        }

        private void StarCheck_CheckedChanged(object sender, EventArgs e)
        {
            Stars.Enabled = StarCheck.Checked;
            StarsComp.Enabled = StarCheck.Checked;
        }

        private void TeraCheck_CheckedChanged(object sender, EventArgs e)
        {
            TeraType.Enabled = TeraCheck.Checked;
        }

        private void HP_CheckedChanged(object sender, EventArgs e)
        {
            IVHP.Enabled = HP.Checked;
            HPComp.Enabled = HP.Checked;
        }

        private void Atk_CheckedChanged(object sender, EventArgs e)
        {
            IVATK.Enabled = Atk.Checked;
            AtkComp.Enabled = Atk.Checked;
        }

        private void Def_CheckedChanged(object sender, EventArgs e)
        {
            IVDEF.Enabled = Def.Checked;
            DefComp.Enabled = Def.Checked;
        }

        private void SpA_CheckedChanged(object sender, EventArgs e)
        {
            IVSPA.Enabled = SpA.Checked;
            SpaComp.Enabled = SpA.Checked;
        }

        private void SpD_CheckedChanged(object sender, EventArgs e)
        {
            IVSPD.Enabled = SpD.Checked;
            SpdComp.Enabled = SpD.Checked;
        }

        private void Spe_CheckedChanged(object sender, EventArgs e)
        {
            IVSPE.Enabled = Spe.Checked;
            SpeComp.Enabled = Spe.Checked;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            string output = JsonConvert.SerializeObject(filters);
            using StreamWriter sw = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "filters.json"));
            sw.Write(output);
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (ActiveFilters.Items.Count == 0)
                return;
            if (ActiveFilters.SelectedIndex == -1)
                return;
            var idx = ActiveFilters.SelectedIndex;
            filters.RemoveAt(idx);
            bs.ResetBindings(false);
        }

        private void ActiveFilters_SelectedIndexChanged(object sender, EventArgs e)
        {
            Remove.Enabled = ActiveFilters.SelectedIndex >= 0;
            if (ActiveFilters.SelectedIndex < 0)
                return;
            SelectFilter(filters[ActiveFilters.SelectedIndex]);
        }

        private void FilterName_TextChanged(object sender, EventArgs e)
        {
            if (ActiveFilters.SelectedIndex > -1 && FilterName.Text == filters[ActiveFilters.SelectedIndex].Name)
            {
                Add.Text = "Update Filter";
            }
            else
            {
                Add.Text = "Add Filter";
            }
        }

        private void CheckRewards_CheckedChanged(object sender, EventArgs e)
        {
            Rewards.Enabled = CheckRewards.Checked;
            ButtonOpenRewardsList.Enabled = CheckRewards.Checked;
            RewardsComp.Enabled = CheckRewards.Checked;
            RewardsCount.Enabled = CheckRewards.Checked;
        }

        private void ButtonOpenRewardsList_Click(object sender, EventArgs e)
        {
            List<int> IDs = Rewards.Text.Split(',').Select(int.Parse).ToList();
            using ItemIDs form = new(IDs);
            if (form.ShowDialog() == DialogResult.OK)
            {
                List<int> s = new();
                if (form.CheckAbilityCapsule.Checked) s.Add(645);
                if (form.CheckBottleCap.Checked) s.Add(795);
                if (form.CheckAbilityPatch.Checked) s.Add(1606);
                if (form.CheckSweet.Checked) s.Add(1904);
                if (form.CheckSalty.Checked) s.Add(1905);
                if (form.CheckSour.Checked) s.Add(1906);
                if (form.CheckBitter.Checked) s.Add(1907);
                if (form.CheckSpicy.Checked) s.Add(1908);

                Rewards.Text = string.Join(",", s);
            }
        }

        private void PerfectIVFilterGroup_Enter(object sender, EventArgs e)
        {

        }
    }
}
