public interface ITerrainGenerationStrategy
{
    TerrainMask Generate(int width, int height, int seed);
}