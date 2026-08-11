using System;
using System.Linq;
using MergeGame.Client.Presentation;
using UnityEditor;
using UnityEngine;

namespace MergeGame.Client.Editor
{
    /// <summary>원본 3×3 PNG를 변경하지 않고 재현 가능한 9개 Unity Sprite sub-asset으로 구성합니다.</summary>
    public static class WorkshopArtImporter
    {
        private const string AtlasPath = "Assets/MergeGame/Art/Items/WorkshopItemsAtlas.png";
        private const string CatalogPath = "Assets/MergeGame/Resources/WorkshopItemArtCatalog.asset";
        private static readonly string[] Names =
        { "01_seed_packet", "02_sprout_pot", "03_leafy_plant", "04_flower_bouquet", "05_toolbox", "06_watering_can", "07_honey_jar", "08_yarn_ball", "09_lantern" };

        [MenuItem("Merge Game/Configure Workshop Item Atlas")]
        public static void Configure()
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath);
            if (texture == null) throw new InvalidOperationException("워크숍 아이템 atlas를 찾을 수 없습니다.");
            var importer = (TextureImporter)AssetImporter.GetAtPath(AtlasPath);
            var cellWidth = texture.width / 3; var cellHeight = texture.height / 3;
            var sprites = new SpriteMetaData[Names.Length];
            for (var index = 0; index < Names.Length; index++)
            {
                var column = index % 3; var rowFromTop = index / 3;
                sprites[index] = new SpriteMetaData
                {
                    name = Names[index], alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    rect = new Rect(column * cellWidth, (2 - rowFromTop) * cellHeight, cellWidth, cellHeight)
                };
            }
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.spritesheet = sprites;
            importer.SaveAndReimport();

            var catalog = AssetDatabase.LoadAssetAtPath<WorkshopItemArtCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<WorkshopItemArtCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            catalog.levelSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(AtlasPath).OfType<Sprite>()
                .OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
            if (catalog.levelSprites.Length != 9) throw new InvalidOperationException("atlas가 정확히 9개 Sprite로 분할되지 않았습니다.");
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }
    }
}
