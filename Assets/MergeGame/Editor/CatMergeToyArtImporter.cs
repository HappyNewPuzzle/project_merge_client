using System;
using System.Linq;
using MergeGame.Client.Presentation;
using UnityEditor;
using UnityEngine;

namespace MergeGame.Client.Editor
{
    /// <summary>사용자가 제공한 Toy Lv01~08 PNG를 기존 공용 카탈로그에 재현 가능하게 연결합니다.</summary>
    public static class CatMergeToyArtImporter
    {
        private const string ToyDirectory = "Assets/Art/CatMerge/Items/Toy";
        private const string CatalogPath = "Assets/MergeGame/Resources/WorkshopItemArtCatalog.asset";

        [MenuItem("Merge Game/Configure Cat Merge Toy Art")]
        public static void Configure()
        {
            var sprites = new Sprite[8];
            for (var level = 1; level <= sprites.Length; level++)
            {
                var path = $"{ToyDirectory}/Toy_Lv{level:00}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException($"Toy Sprite 원본을 찾을 수 없습니다: {path}");
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
                sprites[level - 1] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprites[level - 1] == null) throw new InvalidOperationException($"Sprite import가 실패했습니다: {path}");
            }

            var catalog = AssetDatabase.LoadAssetAtPath<WorkshopItemArtCatalog>(CatalogPath);
            if (catalog == null) throw new InvalidOperationException("기존 WorkshopItemArtCatalog를 찾을 수 없습니다.");
            catalog.toySprites = sprites.OrderBy(value => value.name, StringComparer.Ordinal).ToArray();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }
    }
}
