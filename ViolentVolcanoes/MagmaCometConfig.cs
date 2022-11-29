using UnityEngine;

namespace ViolentVolcanoes
{
    public class MagmaCometConfig : IEntityConfig
    {
        public static string ID = "MagmaComet";

        public float mass = 10f;

        public string[] GetDlcIds()
        {
            return DlcManager.AVAILABLE_ALL_VERSIONS;
        }

        public GameObject CreatePrefab()
        {
            GameObject gameObject = EntityTemplates.CreateEntity(ID, STRINGS.UI.SPACEDESTINATIONS.COMETS.MAGMACOMET.NAME);
            gameObject.AddOrGet<SaveLoadRoot>();
            gameObject.AddOrGet<LoopingSounds>();
            Comet comet = gameObject.AddOrGet<Comet>();
            comet.massRange = new Vector2(mass - 5f, mass + 5f);
            comet.EXHAUST_ELEMENT = SimHashes.Magma;
            comet.EXHAUST_RATE = mass * 0.8f;
            comet.temperatureRange = new Vector2(1650f, 1750f);
            comet.entityDamage = 2;
            comet.totalTileDamage = 0.8f;
            comet.splashRadius = 0;
            comet.impactSound = "Meteor_Medium_Impact";
            comet.flyingSoundID = 1;
            comet.explosionEffectHash = SpawnFXHashes.MeteorImpactDust;
            comet.addTiles = 1;

            comet.spawnVelocity = new Vector2(5f, 8f);

            PrimaryElement primaryElement = gameObject.AddOrGet<PrimaryElement>();
            primaryElement.SetElement(SimHashes.Diamond);
            primaryElement.Temperature = (comet.temperatureRange.x + comet.temperatureRange.y) / 2f;
            KBatchedAnimController kBatchedAnimController = gameObject.AddOrGet<KBatchedAnimController>();
            kBatchedAnimController.AnimFiles = new KAnimFile[1] { Assets.GetAnim("meteor_rock_kanim") };
            kBatchedAnimController.isMovable = true;
            kBatchedAnimController.initialAnim = "fall_loop";
            kBatchedAnimController.initialMode = KAnim.PlayMode.Loop;
            kBatchedAnimController.visibilityType = KAnimControllerBase.VisibilityType.OffscreenUpdate;
            gameObject.AddOrGet<KCircleCollider2D>().radius = 0.5f;
            gameObject.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
            gameObject.AddTag(GameTags.Comet);
            return gameObject;
        }

        public void OnPrefabInit(GameObject go)
        {
        }

        public void OnSpawn(GameObject go)
        {
        }
    }
}