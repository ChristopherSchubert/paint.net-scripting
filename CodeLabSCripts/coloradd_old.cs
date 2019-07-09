// Name:
// Submenu:
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
IntSliderControl Amount1 = 66; // [1,255] Color 1 Tone Band Start
IntSliderControl Amount2 = 99; // [1,255] Color 1 Tone Band End
ColorWheelControl Amount3 = ColorBgra.FromBgra(34,34,178,100); // [Firebrick?!] Color Add 1
BinaryPixelOp Amount4 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // Color 1 Blend Operation
IntSliderControl Amount5 = 105; // [0,255] Color 2 Tone Band Start
IntSliderControl Amount6 = 125; // [0,255] Color 2 Tone Band End
ColorWheelControl Amount7 = ColorBgra.FromBgra(0,69,255,100); // [OrangeRed?!] Color Add 2
BinaryPixelOp Amount8 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // Color 2 Blend Operation
IntSliderControl Amount9 = 130; // [0,255] Color 3 Tone Band Start
IntSliderControl Amount10 = 175; // [0,255] Color 3 Tone Band End
ColorWheelControl Amount11 = ColorBgra.FromBgra(32,165,218,100); // [Goldenrod?!] Color Add 3
BinaryPixelOp Amount12 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // Color 3 Blend Operation
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{    
    var color1ToneBandStart = Amount1;
    var color1ToneBandEnd = Amount2;
    var colorAdd1 = Amount3;
    var color1BlendOp = Amount4;

    var color2ToneBandStart = Amount5;
    var color2ToneBandEnd = Amount6;
    var colorAdd2 = Amount7;
    var color2BlendOp = Amount8;
    
    var color3ToneBandStart = Amount9;
    var color3ToneBandEnd = Amount10;
    var colorAdd3 = Amount11;
    var color3BlendOp = Amount12;

    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            
            var band = (CurrentPixel.R + CurrentPixel.G + CurrentPixel.B) / 3;
            
            if (band >= color1ToneBandStart && band <= color1ToneBandEnd)
            {
                CurrentPixel = color1BlendOp.Apply(CurrentPixel, colorAdd1);
            }
            else if (band >= color2ToneBandStart && band <= color2ToneBandEnd)
            {
                CurrentPixel = color2BlendOp.Apply(CurrentPixel, colorAdd2);
            }
            else if (band >= color3ToneBandStart && band <= color3ToneBandEnd)
            {
                CurrentPixel = color3BlendOp.Apply(CurrentPixel, colorAdd3);
            }
            
            dst[x,y] = CurrentPixel;
        }
    }
}
