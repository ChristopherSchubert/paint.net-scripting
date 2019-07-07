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
PanSliderControl Amount1 = Pair.Create(0.000,0.000); // Center Of Tile
DoubleSliderControl Amount2 = 1.25; // [0,10,4] X Zoom Out
DoubleSliderControl Amount3 = 2; // [0,10,4] Y Zoom Out
ListBoxControl Amount4 = 2; // Tiling Options|Reflect|Repeat|Reflect Brick|Repeat Brick
DoubleSliderControl Amount5 = 40; // [-90,90] Tilt Angle
DoubleSliderControl Amount6 = -10; // [-180,180] Surface Angle
DoubleSliderControl Amount7 = 15; // [-180,180] Image Angle
IntSliderControl Amount8 = 4; // [1,21,10] Samples
#endregion

    
Rectangle selection;
private Pair<double, double> centerOfTile;
private double xZoomFactor;
private double yZoomFactor;
private byte tilingOption;
private double tiltAngle; //tiltangle
private double tileSurfaceAngle;  //tsangle
private double imageAngle; //imgangle
private int maximumNumberOfSamples;

private float width;
private float height;
    
void PreRender(Surface dst, Surface src)
{
    selection = EnvironmentParameters.GetSelection(src.Bounds).GetBoundsInt();
    centerOfTile = Amount1;
    xZoomFactor = Amount2;
    yZoomFactor = Amount3;
    tilingOption = Amount4;
    tiltAngle = Amount5;
    tileSurfaceAngle = Amount6;
    imageAngle = Amount7;
    maximumNumberOfSamples = Amount8;
    
    PreRender_Tiling(dst, src);
}

unsafe void Render(Surface dst, Surface src, Rectangle rect)
{
   
}



private unsafe void PreRender_Tiling(Surface dst, Surface src)
{
    width = (float)selection.Width;
    height = (float)selection.Height;
    float midpointX = width / 2f;  //num1
    float midpointY = height / 2f;  // num2
    float tileCenterX = (float)centerOfTile.First * midpointX;  //num3
    float tileCenterY = (float)centerOfTile.Second * midpointY;  //num4

    double tiltAngleRadians = Math.PI * tiltAngle / 180.0; // num7
    double tiltAngleTangent = Math.Tan(tiltAngleRadians); // num8
    float tiltAngleSecant = (float)(1.0 / Math.Cos(tiltAngleRadians)); // num9
    double newWidth = xZoomFactor * width; //num10
    double newHeight = yZoomFactor * height; //num11
    float rotationFactor = 1f / (float)Math.Sqrt((double)newHeight * (double)newHeight + (double)newWidth * (double)newWidth) * (float)tiltAngleTangent; //num12
    double tileSurfaceRadians = Math.PI * tileSurfaceAngle / 180.0; //num13

    if (tiltAngle == 0.0)
    {
        tileSurfaceRadians = 0.0;
    }
    
    float oppositeCosTileSurface = (float)Math.Cos(-tileSurfaceRadians); //num14
    float oppositeSineTileSurface = (float)Math.Sin(-tileSurfaceRadians); //num15
    double imageSurfaceRadians = Math.PI * imageAngle / 180.0; //num16
    float oppositeCosImageSurface = (float)Math.Cos(-imageSurfaceRadians); //num17
    float oppositeSineImageSurface = (float)Math.Sin(-imageSurfaceRadians); //num18

    int xZoomFactorInt = (int)xZoomFactor;
    int yZoomFactorInt = (int)yZoomFactor;

    if ((double)xZoomFactor < 1.0)
    {
        xZoomFactorInt = (int)(1.0 / (double)xZoomFactor);
    }
    if ((double)yZoomFactor < 1.0)
    {
        yZoomFactorInt = (int)(1.0 / (double)yZoomFactor);
    }

    var xSampleCount = Math.Min(xZoomFactorInt, maximumNumberOfSamples);
    var ySampleCount = Math.Min(yZoomFactorInt, maximumNumberOfSamples);

    var xStepSize = 1f / (float)xSampleCount;
    var yStepSize = 1f / (float)ySampleCount;

    var xStepStart = (float)(-0.5 * (1.0 - (double)xStepSize));
    var yStepStart = (float)(-0.5 * (1.0 - (double)yStepSize));

    var Mxw = (float)-((double)tileCenterX + (double)midpointX);
    var Myw = (float)-((double)tileCenterY + (double)midpointY);
    var Mxx = oppositeCosImageSurface;
    var Mxy = -oppositeSineImageSurface;
    float xRotation = (float)((double)oppositeCosImageSurface * (double)Mxw - (double)oppositeSineImageSurface * (double)Myw); //num19
    var Myx = oppositeSineImageSurface;
    var Myy = oppositeCosImageSurface;
    Myw = (float)((double)oppositeSineImageSurface * (double)Mxw + (double)oppositeCosImageSurface * (double)Myw);
    Mxw = xRotation;
    Mxx *= (float)xZoomFactor;
    Mxy *= (float)xZoomFactor;
    Mxw *= (float)xZoomFactor;
    Myx *= (float)yZoomFactor;
    Myy *= (float)yZoomFactor;
    Myw *= (float)yZoomFactor;
    var Mwx = rotationFactor * Myx;
    var Mwy = rotationFactor * Myy;
    var Mww = (float)((double)rotationFactor * (double)Myw + 1.0);
    Myx = tiltAngleSecant * Myx;
    Myy = tiltAngleSecant * Myy;
    Myw = tiltAngleSecant * Myw;
    float xShift = (float)((double)oppositeCosTileSurface * (double)Mxx - (double)oppositeSineTileSurface * (double)Myx); //num20
    float yShift = (float)((double)oppositeCosTileSurface * (double)Mxy - (double)oppositeSineTileSurface * (double)Myy); //num21
    float wShift = (float)((double)oppositeCosTileSurface * (double)Mxw - (double)oppositeSineTileSurface * (double)Myw); //num22
    Myx = (float)((double)oppositeSineTileSurface * (double)Mxx + (double)oppositeCosTileSurface * (double)Myx);
    Myy = (float)((double)oppositeSineTileSurface * (double)Mxy + (double)oppositeCosTileSurface * (double)Myy);
    Myw = (float)((double)oppositeSineTileSurface * (double)Mxw + (double)oppositeCosTileSurface * (double)Myw);
    Mxx = xShift;
    Mxy = yShift;
    Mxw = wShift;
    Mxx += midpointX * Mwx;
    Mxy += midpointX * Mwy;
    Mxw += midpointX * Mww;
    Myx += midpointY * Mwx;
    Myy += midpointY * Mwy;
    Myw += midpointY * Mww;
    
    for (int y = 0; y < src.Height; y++)
    {
        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(0, y);
        
        for (int x = 0; x < src.Width; x++)
        {
            if (IsCancelRequested) return;
        
             *dstPtr = SSpix(src, x, y, Mxx, Mxy, Mxw, Myx, Myy, Myw, Mwx, Mwy, Mww, xSampleCount, ySampleCount, xStepSize, yStepSize, xStepStart, yStepStart);
             
             dstPtr++;
        }
    }
}

private ColorBgra SSpix(Surface src, int x, int y, float Mxx, float Mxy, float Mxw, float Myx, float Myy, float Myw, float Mwx, float Mwy, float Mww,
    int xSampleCount, int ySampleCount, float xStepSize, float yStepSize, float xStepStart, float yStepStart)
{
    ColorBgra colorBgra = ColorBgra.FromBgra(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0);
    int sampleCount = xSampleCount * ySampleCount;
    float sumBlue = 0.0f;
    float sumGreen = 0.0f;
    float sumRed = 0.0f;
    float sumAlpha = 0.0f;

    for (int ySampleIndex = 0; ySampleIndex < ySampleCount; ++ySampleIndex)
    {
        float sampleY = (float)((double)(y - selection.Top) + (double)yStepStart + (double)ySampleIndex * (double)yStepSize);

        for (int xSampleIndex = 0; xSampleIndex < xSampleCount; ++xSampleIndex)
        {
            float sampleX = (float)((double)(x - selection.Left) + (double)xStepStart + (double)xSampleIndex * (double)xStepSize);
            colorBgra = Move(src, sampleX, sampleY, Mxx, Mxy, Mxw, Myx, Myy, Myw, Mwx,  Mwy, Mww);
            sumBlue += (float)((int)colorBgra.B * (int)colorBgra.A);
            sumGreen += (float)((int)colorBgra.G * (int)colorBgra.A);
            sumRed += (float)((int)colorBgra.R * (int)colorBgra.A);
            sumAlpha += (float)colorBgra.A;
        }
    }
    if ((double)sumAlpha == 0.0)
    {
        return colorBgra;
    }

    return ColorBgra.FromBgra(
          (byte)(int)((double)sumBlue / (double)sumAlpha),
          (byte)(int)((double)sumGreen / (double)sumAlpha),
          (byte)(int)((double)sumRed / (double)sumAlpha),
          (byte)(int)((double)sumAlpha / (double)sampleCount));
}

private ColorBgra Move(Surface src, float x, float y, float Mxx, float Mxy, float Mxw, float Myx, float Myy, float Myw, float Mwx, float Mwy, float Mww)
{
    float shiftXAmount = (float)((double)Mxx * (double)x + (double)Mxy * (double)y) + Mxw; //num1
    float shiftYAmount = (float)((double)Myx * (double)x + (double)Myy * (double)y) + Myw; //num2
    float rotationAmount = (float)((double)Mwx * (double)x + (double)Mwy * (double)y) + Mww; //num3

    ColorBgra colorBgra;
    if ((double)rotationAmount <= 0.0)
    {
        colorBgra = ColorBgra.Transparent;
    }
    else
    {
        float inverseRotationAmount = 1f / rotationAmount; //num4
        float xRotation = inverseRotationAmount * shiftXAmount; //num5
        float yRotation = inverseRotationAmount * shiftYAmount; //num6

        bool flag = false;

        float absoluteXShift = (float)(int)Math.Abs(xRotation / width); // num7
        float absoluteYShift = (float)(int)Math.Abs(yRotation / height); // num8
        float halfRotation = width / 2f; //num9

        switch (tilingOption)
        {
            case 0: // reflect
                if ((double)xRotation < 0.0)
                    xRotation = -xRotation;
                if ((double)xRotation >= (double)width)
                    xRotation = (double)absoluteXShift % 2.0 >= 1.0 ? width - xRotation % width : xRotation % width;
                if ((double)yRotation < 0.0)
                    yRotation = -yRotation;
                if ((double)yRotation >= (double)height)
                {
                    yRotation = (double)absoluteYShift % 2.0 >= 1.0 ? height - yRotation % height : yRotation % height;
                    break;
                }
                break;
            case 1: // repeat
                if ((double)xRotation < 0.0)
                    xRotation = width - Math.Abs(xRotation % width);
                if ((double)xRotation >= (double)width)
                    xRotation %= width;
                if ((double)yRotation < 0.0)
                    yRotation = height - Math.Abs(yRotation % height);
                if ((double)yRotation >= (double)height)
                {
                    yRotation %= height;
                    break;
                }
                break;
            case 2: // reflect brick
                if ((double)yRotation >= 0.0)
                {
                    ++absoluteYShift;
                }
                if ((double)absoluteYShift % 2.0 == 0.0)
                {
                    xRotation = halfRotation + xRotation;
                }
                float yBrick = (float)(int)Math.Abs(yRotation / height); // num10
                float xBrick = (float)(int)Math.Abs(xRotation / width); // num11
                if ((double)xRotation < 0.0)
                {
                    xRotation = -xRotation;
                }
                if ((double)xRotation >= (double)width)
                {
                    xRotation = (double)xBrick % 2.0 >= 1.0 ? width - xRotation % width : xRotation % width;
                }
                if ((double)yRotation < 0.0)
                {
                    yRotation = -yRotation;
                }
                if ((double)yRotation >= (double)height)
                {
                    yRotation = (double)yBrick % 2.0 >= 1.0 ? height - yRotation % height : yRotation % height;
                    break;
                }
                break;
            case 3: // repeat brick
                if ((double)yRotation >= 0.0)
                {
                    ++absoluteYShift;
                }
                if ((double)absoluteYShift % 2.0 == 0.0)
                {
                    xRotation = halfRotation + xRotation;
                }
                if ((double)xRotation < 0.0)
                {
                    xRotation = width - Math.Abs(xRotation % width);
                }
                xRotation %= width;
                if ((double)yRotation < 0.0)
                {
                    yRotation = height - Math.Abs(yRotation % height);
                }
                yRotation %= height;
                break;
        }

        colorBgra = src.GetBilinearSampleClamped(xRotation + (float)selection.Left, yRotation + (float)selection.Top);

        if (flag)
        {
            colorBgra = ColorBgra.Transparent;
        }
    }
    return colorBgra;
}