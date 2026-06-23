using UnityEngine;

namespace SeasonDemo
{
    [RequireComponent(typeof(Collider))]
    public sealed class SeasonInteractable : MonoBehaviour
    {
        private const float SelectedScale = 1.12f;
        private const float PulseScale = 1.28f;

        private Vector3 baseScale = Vector3.one;
        private SeasonExperienceController controller;
        private bool isSelected;
        private float pulseTime;

        public Season Season { get; private set; }
        public string Label { get; private set; }

        public void Initialize(Season season, SeasonExperienceController owner, string label)
        {
            Season = season;
            controller = owner;
            Label = label;
            baseScale = transform.localScale;
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }

        public void PlayFeedback()
        {
            pulseTime = 0.55f;
        }

        private void Update()
        {
            var selectedMultiplier = isSelected ? SelectedScale : 1f;
            var pulseMultiplier = 1f;

            if (pulseTime > 0f)
            {
                pulseTime -= Time.deltaTime;
                var normalizedPulse = 1f - Mathf.Clamp01(pulseTime / 0.55f);
                pulseMultiplier = Mathf.Lerp(1f, PulseScale, Mathf.Sin(normalizedPulse * Mathf.PI));
            }

            transform.localScale = baseScale * selectedMultiplier * pulseMultiplier;

            if (isSelected)
            {
                transform.Rotate(Vector3.up, 36f * Time.deltaTime, Space.World);
            }
        }

        public SeasonExperienceController Controller => controller;
    }
}
