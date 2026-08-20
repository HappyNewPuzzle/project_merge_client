using System;
using MergeGame.Client.Presentation;
using UnityEditor;
using UnityEngine;

namespace MergeGame.Client.Editor
{
    /// <summary>사용자가 제공한 Cat Merge PNG를 기존 표시 구조에 재현 가능하게 연결합니다.</summary>
    public static class CatMergeToyArtImporter
    {
        private const string ArtRoot = "Assets/Art/CatMerge";
        private const string ItemCatalogPath = "Assets/MergeGame/Resources/WorkshopItemArtCatalog.asset";
        private const string HudCatalogPath = "Assets/MergeGame/Resources/WorkshopHudArtCatalog.asset";

        [MenuItem("Merge Game/Configure Cat Merge Art")]
        public static void Configure()
        {
            var toySprites = ImportLine("Toy");
            var foodSprites = ImportLine("Food");
            var restSprites = ImportLine("Rest");

            var catalog = AssetDatabase.LoadAssetAtPath<WorkshopItemArtCatalog>(ItemCatalogPath);
            if (catalog == null) throw new InvalidOperationException("기존 WorkshopItemArtCatalog를 찾을 수 없습니다.");
            catalog.toySprites = toySprites;
            catalog.foodSprites = foodSprites;
            catalog.restSprites = restSprites;
            // 아트의 투명 여백과 체감 면적만 보정하는 Presentation 데이터이며 서버 콘텐츠 규칙과 무관합니다.
            catalog.toyVisualScales = new[] { 1.69f, 1.18f, 1.45f, 1.25f, 1.06f, 0.82f, 1.03f, 0.90f };
            catalog.foodVisualScales = new[] { 1.35f, 0.97f, 0.97f, 0.97f, 0.97f, 0.97f, 0.97f, 0.97f };
            catalog.restVisualScales = new[] { 0.95f, 0.97f, 0.97f, 0.98f, 0.95f, 0.97f, 0.95f, 0.97f };
            EditorUtility.SetDirty(catalog);

            var hudCatalog = AssetDatabase.LoadAssetAtPath<WorkshopHudArtCatalog>(HudCatalogPath);
            if (hudCatalog == null)
            {
                hudCatalog = ScriptableObject.CreateInstance<WorkshopHudArtCatalog>();
                AssetDatabase.CreateAsset(hudCatalog, HudCatalogPath);
            }
            hudCatalog.toyGenerator = ImportSingle($"{ArtRoot}/Generators/Generator_Toy.png");
            hudCatalog.foodGenerator = ImportSingle($"{ArtRoot}/Generators/Generator_Food.png");
            hudCatalog.restGenerator = ImportSingle($"{ArtRoot}/Generators/Generator_Rest.png");
            hudCatalog.coin = ImportSingle($"{ArtRoot}/Currency/Currency_Coin.png");
            hudCatalog.energy = ImportSingle($"{ArtRoot}/Currency/Currency_Energy.png");
            hudCatalog.gem = ImportSingle($"{ArtRoot}/Currency/Currency_Gem.png");
            hudCatalog.defaultMascot = ImportSingle($"{ArtRoot}/Characters/Mascot/Cat_Mascot_Default.png");
            hudCatalog.roomBackground = ImportSingle($"{ArtRoot}/Backgrounds/CatMerge_Room_Background.png");
            hudCatalog.navHome = ImportSingle($"{ArtRoot}/UI/Navigation/UI_Nav_Home.png");
            hudCatalog.navCollection = ImportSingle($"{ArtRoot}/UI/Navigation/UI_Nav_Collection.png");
            hudCatalog.navShop = ImportSingle($"{ArtRoot}/UI/Navigation/UI_Nav_Shop.png");
            hudCatalog.navQuest = ImportSingle($"{ArtRoot}/UI/Navigation/UI_Nav_Quest.png");
            EditorUtility.SetDirty(hudCatalog);
            AssetDatabase.SaveAssets();
        }

        private static Sprite[] ImportLine(string line)
        {
            var sprites = new Sprite[8];
            for (var level = 1; level <= sprites.Length; level++)
                sprites[level - 1] = ImportSingle($"{ArtRoot}/Items/{line}/{line}_Lv{level:00}.png");
            return sprites;
        }

        /// <summary>Toy에서 검증된 모바일용 Single Sprite 정책을 모든 개별 PNG에 동일하게 적용합니다.</summary>
        private static Sprite ImportSingle(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"Sprite 원본을 찾을 수 없습니다: {path}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = 100f;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidOperationException($"Sprite import가 실패했습니다: {path}");
            return sprite;
        }
    }
}
