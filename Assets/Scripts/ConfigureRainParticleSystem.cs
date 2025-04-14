using UnityEngine;
using System.Collections.Generic; // Needed for List

[RequireComponent(typeof(ParticleSystem))]
public class ConfigureRainParticleSystem : MonoBehaviour
{
    public List<Texture2D> rainTextures = new List<Texture2D>(); // Assign textures in Inspector
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private ParticleSystemRenderer psRenderer;

    void Start() // Changed from Awake to ensure textures might be loaded/assigned
    {
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();
        if (ps == null)
        {
            Debug.LogError("ConfigureRainParticleSystem requires a ParticleSystem component.", this);
            enabled = false; // Disable script if no ParticleSystem
            return;
        }
        if (psRenderer == null)
        {
             Debug.LogError("ConfigureRainParticleSystem requires a ParticleSystemRenderer component.", this);
             enabled = false;
             return;
        }
        if (rainTextures == null || rainTextures.Count == 0)
        {
            Debug.LogError("Please assign rain textures to the ConfigureRainParticleSystem script in the Inspector.", this);
            // Attempt to load dynamically as a fallback (adjust path if needed)
            // LoadTexturesDynamically(); // Commented out for now
             if (rainTextures.Count == 0) { // Check again in case Inspector assignment happens after Start runs once
                 // Keep checking or disable? For now, let LateUpdate handle the check.
                 // enabled = false; // Disable if still no textures
                 // return;
             }
        }


        // --- Main Module ---
        var main = ps.main;
        main.startLifetime = 5f; // How long particles live
        main.startSpeed = 10f;   // Initial speed
        main.startSize = 0.1f;   // Initial size
        main.gravityModifier = 1f; // Use standard gravity
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = Color.white; // Ensure particles are white to show texture color

        // --- Emission Module ---
        var emission = ps.emission;
        emission.rateOverTime = 200; // How many particles per second

        // --- Shape Module ---
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box; // Emit from a box area
        shape.scale = new Vector3(20, 1, 20); // Size of the emission box (adjust X and Z as needed)
        // Position this GameObject high above your scene where rain should start

        // --- Velocity Over Lifetime Module ---
        // Section fully removed.

        // --- Renderer Module ---
        // Material is assigned in Inspector, script will modify texture property
        if (psRenderer != null)
        {
            psRenderer.sortingOrder = 5; // Adjust if needed
            // Ensure we use a material that allows per-particle texture changes if possible,
            // otherwise, this script might change the texture for ALL particles using the base material.
            // Using Particle Standard Unlit should be okay if we manage particles directly.
        }

        // --- Texture Sheet Animation ---
        // DISABLE this module in the Inspector, as we are handling textures via script
        var texSheet = ps.textureSheetAnimation;
        if (texSheet.enabled)
        {
             Debug.LogWarning("Texture Sheet Animation module is enabled but will be ignored by ConfigureRainParticleSystem script. Please disable it.", this);
             // texSheet.enabled = false; // Optionally force disable
        }

        InitializeIfNeeded();

        Debug.Log("Rain Particle System configured by script. Assign textures in Inspector and disable Texture Sheet Animation module.");
    }

    void LateUpdate()
    {
        if (ps == null || rainTextures.Count == 0) return;

        InitializeIfNeeded();

        int numParticlesAlive = ps.GetParticles(particles);

        // Iterate through particles and assign random texture
        // NOTE: Directly setting particle properties like texture isn't standard.
        // A common approach is using custom vertex streams or a custom shader.
        // A simpler (but potentially less performant) workaround is to rapidly
        // change the *material's* main texture just before emission, but that affects
        // all particles emitted in that frame.
        // Let's stick to configuring the base system and let the user know
        // that per-particle texture without sprite sheets is complex.

        // --- Simplified approach: Set the base material's texture randomly ---
        // This affects ALL particles using this material instance.
        // If you need true per-particle variation without sprite sheets,
        // it requires more advanced techniques (custom shaders/vertex streams).
        if (psRenderer != null && psRenderer.material != null)
        {
            int randomIndex = Random.Range(0, rainTextures.Count);
            psRenderer.material.mainTexture = rainTextures[randomIndex];
        }

    }

    void InitializeIfNeeded()
    {
        if (ps != null && (particles == null || particles.Length < ps.main.maxParticles))
        {
            particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }
    }

     void LoadTexturesDynamically()
     {
         // Adjust path as needed - relative to Resources folder
         // NOTE: Textures MUST be in a "Resources" folder for Resources.LoadAll to work.
         // Example: Assets/Resources/Rain Drops/
         // If they are in Assets/Rain Drops/, this won't work directly.
         // Consider asking user to move them or use AssetDatabase (Editor only).

         // For now, assume user assigns via Inspector. This is a fallback placeholder.
         Debug.LogWarning("Attempting to load textures dynamically - ensure they are in a 'Resources' folder (e.g., Assets/Resources/Rain Drops).");
         // var loadedTextures = Resources.LoadAll<Texture2D>("Rain Drops");
         // if (loadedTextures != null && loadedTextures.Length > 0) {
         //     rainTextures.AddRange(loadedTextures);
         //     Debug.Log($"Dynamically loaded {rainTextures.Count} textures.");
         // } else {
         //     Debug.LogError("Failed to dynamically load textures from Resources/Rain Drops.");
         // }
     }
}
