using NUnit.Framework;

namespace SeasonDemo.Tests
{
    public sealed class SeasonExperienceModelTests
    {
        [Test]
        public void NewModelStartsInSpringWithNoInteractions()
        {
            var model = new SeasonExperienceModel();

            Assert.That(model.CurrentSeason, Is.EqualTo(Season.Spring));
            Assert.That(model.GetInteractionCount(Season.Spring), Is.EqualTo(0));
            Assert.That(model.TotalInteractions, Is.EqualTo(0));
        }

        [Test]
        public void CycleNextMovesThroughFourSeasonsAndWraps()
        {
            var model = new SeasonExperienceModel();

            Assert.That(model.CycleNext(), Is.EqualTo(Season.Summer));
            Assert.That(model.CycleNext(), Is.EqualTo(Season.Autumn));
            Assert.That(model.CycleNext(), Is.EqualTo(Season.Winter));
            Assert.That(model.CycleNext(), Is.EqualTo(Season.Spring));
        }

        [Test]
        public void CyclePreviousMovesThroughFourSeasonsAndWraps()
        {
            var model = new SeasonExperienceModel();

            Assert.That(model.CyclePrevious(), Is.EqualTo(Season.Winter));
            Assert.That(model.CyclePrevious(), Is.EqualTo(Season.Autumn));
            Assert.That(model.CyclePrevious(), Is.EqualTo(Season.Summer));
            Assert.That(model.CyclePrevious(), Is.EqualTo(Season.Spring));
        }

        [Test]
        public void SelectSeasonRejectsValuesOutsideDefinedRange()
        {
            var model = new SeasonExperienceModel();

            Assert.That(model.SelectSeason((Season)99), Is.False);
            Assert.That(model.CurrentSeason, Is.EqualTo(Season.Spring));
        }

        [TestCase(Season.Spring, "BloomPulse")]
        [TestCase(Season.Summer, "WaterRipple")]
        [TestCase(Season.Autumn, "LeafGust")]
        [TestCase(Season.Winter, "SnowChime")]
        public void InteractReturnsSeasonSpecificFeedback(Season season, string expectedCue)
        {
            var model = new SeasonExperienceModel();
            model.SelectSeason(season);

            var result = model.Interact();

            Assert.That(result.Season, Is.EqualTo(season));
            Assert.That(result.VisualCue, Is.EqualTo(expectedCue));
            Assert.That(result.Message, Does.Contain(season.ToString()));
            Assert.That(result.SeasonInteractionCount, Is.EqualTo(1));
            Assert.That(result.TotalInteractions, Is.EqualTo(1));
            Assert.That(model.GetInteractionCount(season), Is.EqualTo(1));
            Assert.That(model.TotalInteractions, Is.EqualTo(1));
        }

        [TestCase(Season.Spring, "春", "种子")]
        [TestCase(Season.Summer, "夏", "水池")]
        [TestCase(Season.Autumn, "秋", "叶灯")]
        [TestCase(Season.Winter, "冬", "冰晶")]
        public void ProvidesDisplayTextForEachSeason(Season season, string expectedName, string promptKeyword)
        {
            Assert.That(SeasonExperienceModel.GetChineseName(season), Is.EqualTo(expectedName));
            Assert.That(SeasonExperienceModel.GetInteractionPrompt(season), Does.Contain(promptKeyword));
        }

        [Test]
        public void EachSeasonKeepsItsOwnInteractionCount()
        {
            var model = new SeasonExperienceModel();

            model.SelectSeason(Season.Spring);
            model.Interact();
            model.Interact();
            model.SelectSeason(Season.Winter);
            model.Interact();

            Assert.That(model.GetInteractionCount(Season.Spring), Is.EqualTo(2));
            Assert.That(model.GetInteractionCount(Season.Winter), Is.EqualTo(1));
            Assert.That(model.GetInteractionCount(Season.Summer), Is.EqualTo(0));
            Assert.That(model.TotalInteractions, Is.EqualTo(3));
        }
    }
}
