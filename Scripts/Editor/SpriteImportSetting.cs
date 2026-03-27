using UnityEditor;

public class SpriteImportSetting : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // 처음 임포트할 때만 적용 (재임포트 시 덮어쓰지 않음)
        if (assetImporter.importSettingsMissing)
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
        }
    }
}