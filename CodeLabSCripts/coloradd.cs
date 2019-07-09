// Name:Color Add
// Submenu:Chris
// Author:
// Title:
// Version:
// Desc:
// Keywords:
// URL:
// Help:
#region UICode
CheckboxControl Amount1 = true; // [0,1] Use Color 1
ColorWheelControl Amount2 = ColorBgra.FromBgra(0,0,0,255); // [PrimaryColor?] {Amount1} Color  1
IntSliderControl Amount3 = 66; // [1,255] {Amount1} Tone Start
IntSliderControl Amount4 = 99; // [1,255] {Amount1} Tone End
BinaryPixelOp Amount5 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // {Amount1} Blend Operation
CheckboxControl Amount6 = true; // [0,1] Use Color 2
ColorWheelControl Amount7 = ColorBgra.FromBgra(255,255,255,255); // [SecondaryColor?] {Amount6} Color 2
IntSliderControl Amount8 = 105; // [0,255] {Amount6} Tone Start
IntSliderControl Amount9 = 125; // [0,255] {Amount6} Tone End
BinaryPixelOp Amount10 = LayerBlendModeUtil.CreateCompositionOp(LayerBlendMode.Normal); // {Amount6} Blend Operation
#endregion

unsafe void Render(Surface dst, Surface src, Rectangle rect)
{    
    var useColor1 = Amount1;
    var colorAdd1 = Amount2;
    var color1ToneBandStart = Amount3;
    var color1ToneBandEnd = Amount4;
    var color1BlendOp = Amount5;
    
    var useColor2 = Amount6;
    var colorAdd2 = Amount7;
    var color2ToneBandStart = Amount8;
    var color2ToneBandEnd = Amount9;
    var color2BlendOp = Amount10;

    ColorBgra CurrentPixel;
    ColorBgra* srcPtr;
    ColorBgra* dstPtr;
    
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        if (IsCancelRequested) return;
        srcPtr = src.GetPointAddressUnchecked(rect.Left, y);
        dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = *srcPtr;
            
            var band = (CurrentPixel.R + CurrentPixel.G + CurrentPixel.B) / 3;
            
            if (useColor1 && band >= color1ToneBandStart && band <= color1ToneBandEnd)
            {
                CurrentPixel = color1BlendOp.Apply(CurrentPixel, colorAdd1);
            }
            else if (useColor2 && band >= color2ToneBandStart && band <= color2ToneBandEnd)
            {
                CurrentPixel = color2BlendOp.Apply(CurrentPixel, colorAdd2);
            }

            *dstPtr = CurrentPixel;
            srcPtr++;
            dstPtr++;
        }
    }
}
