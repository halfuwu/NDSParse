using NDSParse.Conversion.Textures.Pixels;
using NDSParse.Conversion.Textures.Pixels.Indexed;

namespace NDSParse.Conversion.Textures.Images.Types;

public class IndexedImage : ImageTypeBase
{
    public IndexedImage(int width, int height, IPixel[] pixels, string name = "") : base(width, height, pixels, name)
    {
        
    }
}