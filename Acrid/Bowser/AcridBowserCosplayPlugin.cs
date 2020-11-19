using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace AcridBowserCosplay
{
    [R2APISubmoduleDependency(nameof(LoadoutAPI), nameof(LanguageAPI))]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.muppp.AcridBowserCosplay","AcridBowserCosplay","1.0.0")]
    public partial class AcridBowserCosplayPlugin : BaseUnityPlugin
    {
        private static AssetBundle assetBundle;
        private static readonly List<Material> materialsWithRoRShader = new List<Material>();
        private void Awake()
        {
            BeforeAwake();
            using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AcridBowserCosplay.mupppacridbowsercosplay"))
            {
                assetBundle = AssetBundle.LoadFromStream(assetStream);
            }

            On.RoR2.BodyCatalog.Init += BodyCatalogInit;

            ReplaceShaders();
            AddLanguageTokens();

            AfterAwake();
        }

        partial void BeforeAwake();
        partial void AfterAwake();
        static partial void BeforeBodyCatalogInit();
        static partial void AfterBodyCatalogInit();

        private static void ReplaceShaders()
        {
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/matCroco.mat", @"Hopoo Games/Deferred/Standard"));
            materialsWithRoRShader.Add(LoadMaterialWithReplacedShader(@"Assets/Resources/BodyMT.mat", @"Hopoo Games/Deferred/Standard"));
        }

        private static Material LoadMaterialWithReplacedShader(string materialPath, string shaderName)
        {
            var material = assetBundle.LoadAsset<Material>(materialPath);
            material.shader = Shader.Find(shaderName);

            return material;
        }

        private static void AddLanguageTokens()
        {
            LanguageAPI.Add("MUPPP_SKIN_ACRIDBOWSERCOSPLAYSKIN_NAME", "Bowser");
            LanguageAPI.Add("MUPPP_SKIN_ACRIDBOWSERCOSPLAYSKIN_NAME", "Bowser", "en");
            LanguageAPI.Add("MUPPP_SKIN_ACRIDBOWSERCOSPLAYSKIN_NAME", "クッパ", "ja");
        }

        private static void BodyCatalogInit(On.RoR2.BodyCatalog.orig_Init orig)
        {
            orig();

            BeforeBodyCatalogInit();

            AddCrocoBodyAcridBowserCosplaySkinSkin();

            AfterBodyCatalogInit();
        }

        private static void AddCrocoBodyAcridBowserCosplaySkinSkin()
        {
            var bodyName = "CrocoBody";
            var skinName = "AcridBowserCosplaySkin";
            try
            {
                var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);

                var renderers = bodyPrefab.GetComponentsInChildren<Renderer>(true);
                var skinController = bodyPrefab.GetComponentInChildren<ModelSkinController>();
                var mdl = skinController.gameObject;

                var skin = new LoadoutAPI.SkinDefInfo
                {
                    Icon = assetBundle.LoadAsset<Sprite>(@"Assets\SkinMods\AcridBowserCosplay\Icons\AcridBowserCosplaySkinIcon.png"),
                    Name = skinName,
                    NameToken = "MUPPP_SKIN_ACRIDBOWSERCOSPLAYSKIN_NAME",
                    RootObject = mdl,
                    BaseSkins = new SkinDef[] 
                    { 
                        skinController.skins[0],
                    },
                    UnlockableName = "",
                    GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>(),
                    RendererInfos = new CharacterModel.RendererInfo[]
                    {
                        new CharacterModel.RendererInfo
                        {
                            defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/matCroco.mat"),
                            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                            ignoreOverlays = false,
                            renderer = renderers[2]
                        },
                        new CharacterModel.RendererInfo
                        {
                            defaultMaterial = assetBundle.LoadAsset<Material>(@"Assets/Resources/BodyMT.mat"),
                            defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                            ignoreOverlays = false,
                            renderer = renderers[3]
                        },
                    },
                    MeshReplacements = new SkinDef.MeshReplacement[]
                    {
                        new SkinDef.MeshReplacement
                        {
                            mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\AcridBowserCosplay\Meshes\CrocoMesh.mesh"),
                            renderer = renderers[2]
                        },
                        new SkinDef.MeshReplacement
                        {
                            mesh = assetBundle.LoadAsset<Mesh>(@"Assets\SkinMods\AcridBowserCosplay\Meshes\BowserMesh.mesh"),
                            renderer = renderers[3]
                        },
                    },
                    MinionSkinReplacements = Array.Empty<SkinDef.MinionSkinReplacement>(),
                    ProjectileGhostReplacements = Array.Empty<SkinDef.ProjectileGhostReplacement>()
                };

                Array.Resize(ref skinController.skins, skinController.skins.Length + 1);
                skinController.skins[skinController.skins.Length - 1] = LoadoutAPI.CreateNewSkinDef(skin);

                var skinsField = typeof(BodyCatalog).GetFieldValue<SkinDef[][]>("skins");
                skinsField[BodyCatalog.FindBodyIndex(bodyPrefab)] = skinController.skins;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to add \"{skinName}\" skin to \"{bodyName}\"");
                Debug.LogError(e);
            }
        }
    }
}
