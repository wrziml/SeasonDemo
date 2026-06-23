namespace SeasonDemo
{
    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }

    public readonly struct SeasonInteractionResult
    {
        public SeasonInteractionResult(
            Season season,
            string visualCue,
            string message,
            int seasonInteractionCount,
            int totalInteractions)
        {
            Season = season;
            VisualCue = visualCue;
            Message = message;
            SeasonInteractionCount = seasonInteractionCount;
            TotalInteractions = totalInteractions;
        }

        public Season Season { get; }
        public string VisualCue { get; }
        public string Message { get; }
        public int SeasonInteractionCount { get; }
        public int TotalInteractions { get; }
    }

    public sealed class SeasonExperienceModel
    {
        public const int SeasonCount = 4;

        private readonly int[] interactionCounts = new int[SeasonCount];

        public Season CurrentSeason { get; private set; } = Season.Spring;
        public int TotalInteractions { get; private set; }

        public bool SelectSeason(Season season)
        {
            if (!IsDefined(season))
            {
                return false;
            }

            CurrentSeason = season;
            return true;
        }

        public Season CycleNext()
        {
            CurrentSeason = (Season)(((int)CurrentSeason + 1) % SeasonCount);
            return CurrentSeason;
        }

        public Season CyclePrevious()
        {
            CurrentSeason = (Season)(((int)CurrentSeason + SeasonCount - 1) % SeasonCount);
            return CurrentSeason;
        }

        public SeasonInteractionResult Interact()
        {
            var index = (int)CurrentSeason;
            interactionCounts[index]++;
            TotalInteractions++;

            return new SeasonInteractionResult(
                CurrentSeason,
                GetVisualCue(CurrentSeason),
                GetFeedbackMessage(CurrentSeason),
                interactionCounts[index],
                TotalInteractions);
        }

        public int GetInteractionCount(Season season)
        {
            return IsDefined(season) ? interactionCounts[(int)season] : 0;
        }

        public static bool IsDefined(Season season)
        {
            return season >= Season.Spring && season <= Season.Winter;
        }

        public static string GetChineseName(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return "春";
                case Season.Summer:
                    return "夏";
                case Season.Autumn:
                    return "秋";
                case Season.Winter:
                    return "冬";
                default:
                    return "?";
            }
        }

        public static string GetInteractionPrompt(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return "触碰种子，让花朵生长";
                case Season.Summer:
                    return "触碰水池，激起清凉涟漪";
                case Season.Autumn:
                    return "触碰叶灯，卷起落叶旋风";
                case Season.Winter:
                    return "触碰冰晶，唤醒雪光铃声";
                default:
                    return string.Empty;
            }
        }

        public static string GetVisualCue(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return "BloomPulse";
                case Season.Summer:
                    return "WaterRipple";
                case Season.Autumn:
                    return "LeafGust";
                case Season.Winter:
                    return "SnowChime";
                default:
                    return string.Empty;
            }
        }

        private static string GetFeedbackMessage(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return "Spring 春天：种子绽放，花瓣回应了你的触碰。";
                case Season.Summer:
                    return "Summer 夏天：水面扩散清凉涟漪。";
                case Season.Autumn:
                    return "Autumn 秋天：落叶被暖风卷成旋涡。";
                case Season.Winter:
                    return "Winter 冬天：冰晶亮起，雪光轻轻回响。";
                default:
                    return string.Empty;
            }
        }
    }
}
