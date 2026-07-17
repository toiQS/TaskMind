using System;

namespace TaskMind.WPFs.Modules.Companies.Models
{
    public enum ActivityPriority
    {
        Low,
        Medium,
        High
    }

    public class StatCardModel
    {
        public string Title { get; set; }
        public string Value { get; set; }
        public string Icon { get; set; }        // Tên SymbolRegular của WPF-UI, vd: "People24"
        public string TrendText { get; set; }    // vd: "+12% so với tháng trước"
        public bool IsTrendPositive { get; set; }
    }

    public class ChartPointModel
    {
        public string Label { get; set; }  // vd: "T1", "T2"...
        public double Value { get; set; }
    }

    public class ActivityItemModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Time { get; set; }
        public ActivityPriority Priority { get; set; }
    }
}