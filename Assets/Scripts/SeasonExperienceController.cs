using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace SeasonDemo
{
    public sealed class SeasonExperienceController : MonoBehaviour
    {
        private const float RayDistance = 14f;

        private readonly Dictionary<Season, SeasonInteractable> interactables = new Dictionary<Season, SeasonInteractable>();
        private readonly Dictionary<Season, ParticleSystem> seasonEffects = new Dictionary<Season, ParticleSystem>();
        private readonly Dictionary<Season, Transform> seasonRoots = new Dictionary<Season, Transform>();
        private readonly List<TextMesh> billboardTexts = new List<TextMesh>();
        private readonly List<InputDevice> xrDevices = new List<InputDevice>();

        private SeasonExperienceModel model = new SeasonExperienceModel();
        private Camera sceneCamera;
        private Light sunLight;
        private TextMesh titleText;
        private TextMesh hintText;
        private TextMesh feedbackText;
        private bool triggerWasDown;
        private bool primaryWasDown;
        private bool secondaryWasDown;
        private bool sceneBuilt;
        private float xrRefreshTimer;

        private void Awake()
        {
            Application.targetFrameRate = 72;
            QualitySettings.vSyncCount = 0;
            BuildScene();
            ApplySeasonState();
            RefreshXrDevices();
        }

        private void Update()
        {
            HandleInput();
            FaceTextsToCamera();
        }

        public void SelectSeason(Season season)
        {
            if (!model.SelectSeason(season))
            {
                return;
            }

            ApplySeasonState();
        }

        public void TriggerInteraction(SeasonInteractable explicitTarget = null)
        {
            var target = explicitTarget;
            if (target == null)
            {
                target = FindAimedInteractable(Input.GetMouseButtonDown(0));
            }

            if (target != null && target.Season != model.CurrentSeason)
            {
                model.SelectSeason(target.Season);
                ApplySeasonState();
            }

            var result = model.Interact();
            PlayFeedback(result);
        }

        private void BuildScene()
        {
            if (sceneBuilt)
            {
                return;
            }

            sceneBuilt = true;
            EnsureCamera();
            CreateLighting();
            CreateGround();
            CreateInstructionPanel();
            CreateSpringScene(new Vector3(-2.7f, 0f, 2.1f));
            CreateSummerScene(new Vector3(2.7f, 0f, 2.1f));
            CreateAutumnScene(new Vector3(2.7f, 0f, -1.8f));
            CreateWinterScene(new Vector3(-2.7f, 0f, -1.8f));
        }

        private void EnsureCamera()
        {
            sceneCamera = Camera.main;
            if (sceneCamera == null)
            {
                var cameraObject = new GameObject("XR Camera");
                cameraObject.tag = "MainCamera";
                sceneCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            sceneCamera.transform.position = new Vector3(0f, 1.6f, -5.25f);
            sceneCamera.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 1.1f, 0f) - sceneCamera.transform.position, Vector3.up);
            sceneCamera.fieldOfView = 70f;
            sceneCamera.nearClipPlane = 0.03f;
            sceneCamera.farClipPlane = 80f;
            sceneCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        private void CreateLighting()
        {
            var lightObject = new GameObject("Season Sun");
            lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            sunLight = lightObject.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = 1.25f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.44f, 0.46f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
        }

        private void CreateGround()
        {
            var groundMaterial = CreateMaterial("Soft Meadow Ground", new Color(0.34f, 0.42f, 0.36f), 0f, 0.25f);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Round Four Seasons Stage";
            ground.transform.position = new Vector3(0f, -0.04f, 0f);
            ground.transform.localScale = new Vector3(4.4f, 0.04f, 4.4f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        }

        private void CreateInstructionPanel()
        {
            var uiRoot = new GameObject("World Instructions").transform;
            uiRoot.position = new Vector3(0f, 2.25f, -1.35f);

            titleText = CreateText("Title", "感受四季", uiRoot, new Vector3(0f, 0.28f, 0f), 0.08f, Color.white);
            hintText = CreateText("Hint", string.Empty, uiRoot, Vector3.zero, 0.035f, new Color(0.92f, 0.96f, 1f));
            feedbackText = CreateText("Feedback", string.Empty, uiRoot, new Vector3(0f, -0.26f, 0f), 0.031f, new Color(1f, 0.96f, 0.76f));
        }

        private void CreateSpringScene(Vector3 position)
        {
            var root = CreateSeasonRoot(Season.Spring, "Spring Grove", position, new Color(0.42f, 0.82f, 0.48f));
            var bark = CreateMaterial("Spring Bark", new Color(0.46f, 0.28f, 0.16f));
            var leaf = CreateMaterial("Spring Leaves", new Color(0.31f, 0.79f, 0.37f), 0f, 0.42f);
            var blossom = CreateMaterial("Spring Blossom", new Color(1f, 0.55f, 0.74f), 0f, 0.48f, true);

            CreatePrimitive(PrimitiveType.Cylinder, "Young Tree Trunk", root, new Vector3(0f, 0.42f, 0f), new Vector3(0.13f, 0.42f, 0.13f), bark);
            CreatePrimitive(PrimitiveType.Sphere, "Fresh Canopy", root, new Vector3(0f, 1.02f, 0f), new Vector3(0.82f, 0.55f, 0.82f), leaf);
            CreatePrimitive(PrimitiveType.Sphere, "Left Blossom Cloud", root, new Vector3(-0.32f, 1.1f, -0.06f), new Vector3(0.3f, 0.22f, 0.3f), blossom);
            CreatePrimitive(PrimitiveType.Sphere, "Right Blossom Cloud", root, new Vector3(0.35f, 1.04f, 0.12f), new Vector3(0.28f, 0.2f, 0.28f), blossom);

            for (var i = 0; i < 14; i++)
            {
                var angle = i * 25.7f * Mathf.Deg2Rad;
                var radius = 0.58f + (i % 3) * 0.11f;
                CreatePrimitive(
                    PrimitiveType.Sphere,
                    "Tiny Flower",
                    root,
                    new Vector3(Mathf.Cos(angle) * radius, 0.12f, Mathf.Sin(angle) * radius),
                    new Vector3(0.055f, 0.055f, 0.055f),
                    i % 2 == 0 ? blossom : leaf);
            }

            CreateInteractable(Season.Spring, "Seed Pod", root, new Vector3(0f, 0.72f, -0.52f), new Vector3(0.22f, 0.22f, 0.22f), blossom);
            CreateSeasonEffect(Season.Spring, root, new Vector3(0f, 0.85f, -0.35f), new Color(1f, 0.58f, 0.76f), new Color(0.64f, 1f, 0.7f));
        }

        private void CreateSummerScene(Vector3 position)
        {
            var root = CreateSeasonRoot(Season.Summer, "Summer Pool", position, new Color(1f, 0.77f, 0.25f));
            var sand = CreateMaterial("Warm Sand", new Color(0.92f, 0.72f, 0.38f), 0f, 0.25f);
            var water = CreateMaterial("Summer Water", new Color(0.1f, 0.62f, 0.95f), 0f, 0.82f, true);
            var sun = CreateMaterial("Summer Sun", new Color(1f, 0.76f, 0.12f), 0f, 0.55f, true);

            CreatePrimitive(PrimitiveType.Cylinder, "Warm Pool Edge", root, new Vector3(0f, 0.05f, 0f), new Vector3(0.95f, 0.06f, 0.95f), sand);
            CreatePrimitive(PrimitiveType.Cylinder, "Water Surface", root, new Vector3(0f, 0.12f, 0f), new Vector3(0.74f, 0.035f, 0.74f), water);
            CreatePrimitive(PrimitiveType.Sphere, "Pocket Sun", root, new Vector3(0f, 1.32f, 0.18f), new Vector3(0.42f, 0.42f, 0.42f), sun);

            for (var i = 0; i < 10; i++)
            {
                var angle = i * 36f * Mathf.Deg2Rad;
                var ray = CreatePrimitive(
                    PrimitiveType.Cube,
                    "Sun Ray",
                    root,
                    new Vector3(Mathf.Cos(angle) * 0.38f, 1.32f + Mathf.Sin(angle) * 0.38f, 0.18f),
                    new Vector3(0.05f, 0.18f, 0.05f),
                    sun);
                ray.transform.localRotation = Quaternion.Euler(0f, 0f, -i * 36f);
            }

            CreateInteractable(Season.Summer, "Ripple Stone", root, new Vector3(0f, 0.32f, -0.18f), new Vector3(0.2f, 0.12f, 0.2f), water);
            CreateSeasonEffect(Season.Summer, root, new Vector3(0f, 0.38f, -0.18f), new Color(0.18f, 0.78f, 1f), new Color(1f, 0.92f, 0.46f));
        }

        private void CreateAutumnScene(Vector3 position)
        {
            var root = CreateSeasonRoot(Season.Autumn, "Autumn Lantern", position, new Color(0.95f, 0.42f, 0.16f));
            var bark = CreateMaterial("Autumn Bark", new Color(0.4f, 0.22f, 0.13f));
            var amber = CreateMaterial("Amber Leaves", new Color(0.9f, 0.36f, 0.08f), 0f, 0.45f, true);
            var gold = CreateMaterial("Golden Leaves", new Color(1f, 0.68f, 0.18f), 0f, 0.38f, true);

            CreatePrimitive(PrimitiveType.Cylinder, "Autumn Trunk", root, new Vector3(0f, 0.45f, 0f), new Vector3(0.14f, 0.45f, 0.14f), bark);
            CreatePrimitive(PrimitiveType.Sphere, "Copper Canopy", root, new Vector3(0f, 1.04f, 0f), new Vector3(0.72f, 0.46f, 0.72f), amber);

            for (var i = 0; i < 18; i++)
            {
                var angle = i * 20f * Mathf.Deg2Rad;
                var radius = 0.36f + (i % 4) * 0.11f;
                var leaf = CreatePrimitive(
                    PrimitiveType.Cube,
                    "Fallen Leaf",
                    root,
                    new Vector3(Mathf.Cos(angle) * radius, 0.08f, Mathf.Sin(angle) * radius - 0.18f),
                    new Vector3(0.14f, 0.018f, 0.06f),
                    i % 2 == 0 ? amber : gold);
                leaf.transform.localRotation = Quaternion.Euler(0f, i * 31f, 12f);
            }

            CreateInteractable(Season.Autumn, "Leaf Lantern", root, new Vector3(0f, 0.68f, -0.48f), new Vector3(0.22f, 0.28f, 0.22f), gold);
            CreateSeasonEffect(Season.Autumn, root, new Vector3(0f, 0.82f, -0.32f), new Color(1f, 0.62f, 0.12f), new Color(0.88f, 0.22f, 0.06f));
        }

        private void CreateWinterScene(Vector3 position)
        {
            var root = CreateSeasonRoot(Season.Winter, "Winter Crystal", position, new Color(0.5f, 0.86f, 1f));
            var snow = CreateMaterial("Fresh Snow", new Color(0.9f, 0.96f, 1f), 0f, 0.6f);
            var ice = CreateMaterial("Blue Ice", new Color(0.48f, 0.84f, 1f), 0f, 0.86f, true);
            var deepIce = CreateMaterial("Deep Ice", new Color(0.18f, 0.48f, 0.78f), 0f, 0.78f, true);

            CreatePrimitive(PrimitiveType.Sphere, "Snow Drift", root, new Vector3(0f, 0.12f, 0f), new Vector3(1.1f, 0.22f, 0.82f), snow);
            for (var i = 0; i < 5; i++)
            {
                var crystal = CreatePrimitive(
                    PrimitiveType.Cube,
                    "Ice Shard",
                    root,
                    new Vector3((i - 2) * 0.16f, 0.52f + i * 0.03f, i % 2 == 0 ? 0.02f : -0.05f),
                    new Vector3(0.1f, 0.62f + i * 0.06f, 0.1f),
                    i % 2 == 0 ? ice : deepIce);
                crystal.transform.localRotation = Quaternion.Euler(0f, i * 28f, i % 2 == 0 ? 11f : -9f);
            }

            CreateInteractable(Season.Winter, "Snow Crystal", root, new Vector3(0f, 0.88f, -0.42f), new Vector3(0.24f, 0.24f, 0.24f), ice);
            CreateSeasonEffect(Season.Winter, root, new Vector3(0f, 1.0f, -0.25f), new Color(0.76f, 0.94f, 1f), new Color(0.36f, 0.66f, 1f));
        }

        private Transform CreateSeasonRoot(Season season, string rootName, Vector3 position, Color patchColor)
        {
            var rootObject = new GameObject(rootName);
            rootObject.transform.position = position;
            seasonRoots[season] = rootObject.transform;

            var patchMaterial = CreateMaterial(rootName + " Patch", patchColor, 0f, 0.36f);
            CreatePrimitive(PrimitiveType.Cylinder, "Season Patch", rootObject.transform, new Vector3(0f, 0.015f, 0f), new Vector3(1.18f, 0.025f, 1.18f), patchMaterial);
            CreateText(rootName + " Label", SeasonExperienceModel.GetChineseName(season), rootObject.transform, new Vector3(0f, 1.85f, 0f), 0.09f, Color.white);

            return rootObject.transform;
        }

        private SeasonInteractable CreateInteractable(Season season, string label, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var item = CreatePrimitive(PrimitiveType.Sphere, label, parent, localPosition, localScale, material);
            var interactable = item.AddComponent<SeasonInteractable>();
            interactable.Initialize(season, this, label);
            interactables[season] = interactable;
            return interactable;
        }

        private ParticleSystem CreateSeasonEffect(Season season, Transform parent, Vector3 localPosition, Color startColor, Color endColor)
        {
            var effectObject = new GameObject(season + " Interaction Particles");
            effectObject.transform.SetParent(parent, false);
            effectObject.transform.localPosition = localPosition;

            var particles = effectObject.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.duration = 1.7f;
            main.loop = false;
            main.startLifetime = 1.05f;
            main.startSpeed = 1.45f;
            main.startSize = 0.085f;
            main.maxParticles = 160;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = startColor;

            var emission = particles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0f, 38),
                new ParticleSystem.Burst(0.28f, 24)
            });

            var shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.32f;

            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(startColor, 0f),
                    new GradientColorKey(endColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;

            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = CreateParticleMaterial(season + " Particle Material");

            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            seasonEffects[season] = particles;
            return particles;
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectSeason(Season.Spring);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectSeason(Season.Summer);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectSeason(Season.Autumn);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SelectSeason(Season.Winter);
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.RightArrow) || WasXrButtonPressed(CommonUsages.primaryButton, ref primaryWasDown))
            {
                model.CycleNext();
                ApplySeasonState();
            }

            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow) || WasXrButtonPressed(CommonUsages.secondaryButton, ref secondaryWasDown))
            {
                model.CyclePrevious();
                ApplySeasonState();
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0) || WasXrButtonPressed(CommonUsages.triggerButton, ref triggerWasDown))
            {
                TriggerInteraction();
            }
        }

        private bool WasXrButtonPressed(InputFeatureUsage<bool> usage, ref bool wasDown)
        {
            xrRefreshTimer -= Time.deltaTime;
            if (xrRefreshTimer <= 0f)
            {
                RefreshXrDevices();
                xrRefreshTimer = 1f;
            }

            var isDown = false;
            for (var i = 0; i < xrDevices.Count; i++)
            {
                if (xrDevices[i].isValid && xrDevices[i].TryGetFeatureValue(usage, out var value) && value)
                {
                    isDown = true;
                    break;
                }
            }

            var pressed = isDown && !wasDown;
            wasDown = isDown;
            return pressed;
        }

        private void RefreshXrDevices()
        {
            xrDevices.Clear();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, xrDevices);
        }

        private SeasonInteractable FindAimedInteractable(bool useMousePosition)
        {
            if (sceneCamera == null)
            {
                return null;
            }

            var ray = useMousePosition
                ? sceneCamera.ScreenPointToRay(Input.mousePosition)
                : new Ray(sceneCamera.transform.position, sceneCamera.transform.forward);

            return Physics.Raycast(ray, out var hit, RayDistance)
                ? hit.collider.GetComponentInParent<SeasonInteractable>()
                : null;
        }

        private void ApplySeasonState()
        {
            var season = model.CurrentSeason;
            sceneCamera.backgroundColor = GetSkyColor(season);
            sunLight.color = GetLightColor(season);
            sunLight.intensity = season == Season.Winter ? 0.92f : 1.25f;
            RenderSettings.ambientLight = Color.Lerp(GetSkyColor(season), Color.white, 0.36f);

            foreach (var pair in interactables)
            {
                pair.Value.SetSelected(pair.Key == season);
            }

            foreach (var pair in seasonRoots)
            {
                pair.Value.localScale = Vector3.one * (pair.Key == season ? 1.08f : 0.92f);
            }

            titleText.text = "感受四季  " + SeasonExperienceModel.GetChineseName(season);
            hintText.text = "PICO: trigger 互动, A/B 切换  |  编辑器: 空格互动, Q/E 切换, 1-4 选季节\n"
                + SeasonExperienceModel.GetInteractionPrompt(season);
        }

        private void PlayFeedback(SeasonInteractionResult result)
        {
            if (seasonEffects.TryGetValue(result.Season, out var particles))
            {
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.Play(true);
            }

            if (interactables.TryGetValue(result.Season, out var interactable))
            {
                interactable.PlayFeedback();
            }

            feedbackText.text = result.Message
                + "\n本季互动 " + result.SeasonInteractionCount
                + " 次 / 总互动 " + result.TotalInteractions + " 次";
        }

        private void FaceTextsToCamera()
        {
            if (sceneCamera == null)
            {
                return;
            }

            for (var i = 0; i < billboardTexts.Count; i++)
            {
                var textTransform = billboardTexts[i].transform;
                var toCamera = sceneCamera.transform.position - textTransform.position;
                if (toCamera.sqrMagnitude > 0.001f)
                {
                    textTransform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
                }
            }
        }

        private TextMesh CreateText(string name, string text, Transform parent, Vector3 localPosition, float characterSize, Color color)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;

            var mesh = textObject.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontSize = 64;
            mesh.characterSize = characterSize;
            mesh.color = color;
            billboardTexts.Add(mesh);
            return mesh;
        }

        private GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localScale = localScale;
            primitive.GetComponent<Renderer>().sharedMaterial = material;
            return primitive;
        }

        private Material CreateMaterial(string materialName, Color color, float metallic = 0f, float smoothness = 0.35f, bool emission = false)
        {
            var shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Diffuse");
            }

            var material = new Material(shader) { name = materialName };
            SetMaterialColor(material, color);

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", smoothness);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.65f);
            }

            return material;
        }

        private Material CreateParticleMaterial(string materialName)
        {
            var shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return new Material(shader) { name = materialName };
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            material.color = color;
        }

        private static Color GetSkyColor(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return new Color(0.58f, 0.78f, 0.68f);
                case Season.Summer:
                    return new Color(0.22f, 0.55f, 0.9f);
                case Season.Autumn:
                    return new Color(0.72f, 0.42f, 0.24f);
                case Season.Winter:
                    return new Color(0.55f, 0.72f, 0.86f);
                default:
                    return Color.gray;
            }
        }

        private static Color GetLightColor(Season season)
        {
            switch (season)
            {
                case Season.Spring:
                    return new Color(1f, 0.88f, 0.82f);
                case Season.Summer:
                    return new Color(1f, 0.94f, 0.7f);
                case Season.Autumn:
                    return new Color(1f, 0.62f, 0.34f);
                case Season.Winter:
                    return new Color(0.76f, 0.9f, 1f);
                default:
                    return Color.white;
            }
        }
    }
}
