
public class AardvarkEffectPlugin : PropertyBasedEffect
{
    private Pair<double, double> centreOfTile = Pair.Create<double, double>(0.0, 0.0);
    private double xZoomFactor = 1.0;
    private double yZoomFactor = 1.0;
    private int maximumNumberOfSamples = 4;
    private byte tilingOption;
    private double tiltAngle; //tiltangle
    private double tileSurfaceAngle;  //tsangle
    private double imageAngle; //imgangle
    private float Mxw;
    private float Mxx;
    private float Mxy;
    private float Myw;
    private float Myx;
    private float Myy;
    private float Mww;
    private float Mwx;
    private float Mwy;
    private float xStepSize;
    private float yStepSize;
    private int xSampleCount;
    private int ySampleCount;
    private float xStepStart;
    private float yStepStart;

    private float width;
    private float height;

    protected override PropertyCollection OnCreatePropertyCollection()
    {
        return new PropertyCollection((IEnumerable<Property>)new List<Property>()
      {
        (Property) new DoubleVectorProperty((object) AardvarkEffectPlugin.PropertyNames.CentreOfTile, Pair.Create<double, double>(0.0, 0.0), Pair.Create<double, double>(-3.0, -3.0), Pair.Create<double, double>(3.0, 3.0)),
        (Property) StaticListChoiceProperty.CreateForEnum<AardvarkEffectPlugin.TilingChoiceOptions>((object) AardvarkEffectPlugin.PropertyNames.TilingChoice, AardvarkEffectPlugin.TilingChoiceOptions.Reflect, false),
        (Property) new BooleanProperty((object) AardvarkEffectPlugin.PropertyNames.Limit2Ints, true),
        (Property) new DoubleProperty((object) AardvarkEffectPlugin.PropertyNames.Xzoom, 1.0, 0.25, 11.0),
        (Property) new DoubleProperty((object) AardvarkEffectPlugin.PropertyNames.Yzoom, 1.0, 0.25, 11.0),
        (Property) new BooleanProperty((object) AardvarkEffectPlugin.PropertyNames.LinkXY, true),
        (Property) new DoubleProperty((object) AardvarkEffectPlugin.PropertyNames.TiltAngle, 0.0, -90.0, 90.0),
        (Property) new DoubleProperty((object) AardvarkEffectPlugin.PropertyNames.TSAngle, 0.0, -180.0, 180.0),
        (Property) new BooleanProperty((object) AardvarkEffectPlugin.PropertyNames.LimitImgRotAng, false),
        (Property) new DoubleProperty((object) AardvarkEffectPlugin.PropertyNames.ImgAngle, 0.0, -180.0, 180.0),
        (Property) new Int32Property((object) AardvarkEffectPlugin.PropertyNames.MaxSample, 11, 1, 21)
      }, (IEnumerable<PropertyCollectionRule>)new List<PropertyCollectionRule>()
      {
        (PropertyCollectionRule) new LinkValuesBasedOnBooleanRule<double, DoubleProperty>(new object[2]
        {
          (object) AardvarkEffectPlugin.PropertyNames.Xzoom,
          (object) AardvarkEffectPlugin.PropertyNames.Yzoom
        }, (object) AardvarkEffectPlugin.PropertyNames.LinkXY, false),
        (PropertyCollectionRule) new ReadOnlyBoundToValueRule<double, DoubleProperty>((object) AardvarkEffectPlugin.PropertyNames.TSAngle, (object) AardvarkEffectPlugin.PropertyNames.TiltAngle, 0.0, false)
      });
    }

    protected override void OnSetRenderInfo(
      PropertyBasedEffectConfigToken newToken,
      RenderArgs dstArgs,
      RenderArgs srcArgs)
    {
        centreOfTile = newToken.GetProperty<DoubleVectorProperty>((object)AardvarkEffectPlugin.PropertyNames.CentreOfTile).Value;
        tilingOption = (byte)(int)newToken.GetProperty<StaticListChoiceProperty>((object)AardvarkEffectPlugin.PropertyNames.TilingChoice).Value;
        lim2ints = newToken.GetProperty<BooleanProperty>((object)AardvarkEffectPlugin.PropertyNames.Limit2Ints).Value;
        xzoomout = newToken.GetProperty<DoubleProperty>((object)AardvarkEffectPlugin.PropertyNames.Xzoom).Value;
        yzoomout = newToken.GetProperty<DoubleProperty>((object)AardvarkEffectPlugin.PropertyNames.Yzoom).Value;
        linkxy = newToken.GetProperty<BooleanProperty>((object)AardvarkEffectPlugin.PropertyNames.LinkXY).Value;
        tiltAngle = newToken.GetProperty<DoubleProperty>((object)AardvarkEffectPlugin.PropertyNames.TiltAngle).Value;
        tileSurfaceAngle = newToken.GetProperty<DoubleProperty>((object)AardvarkEffectPlugin.PropertyNames.TSAngle).Value;
        limImgAng = newToken.GetProperty<BooleanProperty>((object)AardvarkEffectPlugin.PropertyNames.LimitImgRotAng).Value;
        imgangle = newToken.GetProperty<DoubleProperty>((object)AardvarkEffectPlugin.PropertyNames.ImgAngle).Value;
        maxsample = newToken.GetProperty<Int32Property>((object)AardvarkEffectPlugin.PropertyNames.MaxSample).Value;
        base.OnSetRenderInfo(newToken, dstArgs, srcArgs);

        width = (float)selection.Width;
        height = (float)selection.Height;
        float midpointX = width / 2f;  //num1
        float midpointY = height / 2f;  // num2
        float tileCenterX = (float)centreOfTile.First * midpointX;  //num3
        float tileCenterY = (float)centreOfTile.Second * midpointY;  //num4

        double tiltAngleRadians = Math.PI * tiltAngle / 180.0; // num7
        double tiltAngleTangent = Math.Tan(tiltAngleRadians); // num8
        float tiltAngleSecant = (float)(1.0 / Math.Cos(tiltAngleRadians)); // num9
        float newWidth = xZoomFactor * width; //num10
        float newHeight = yZoomFactor * height; //num11
        float rotationFactor = 1f / (float)Math.Sqrt((double)newHeight * (double)newHeight + (double)newWidth * (double)newWidth) * (float)tiltAngleTangent; //num12
        double tileSurfaceRadians = Math.PI * tileSurfaceAngle / 180.0; //num13

        if (tiltangle == 0.0)
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

        xSampleCount = Math.Min(xZoomFactorInt, maximumNumberOfSamples);
        ySampleCount = Math.Min(yZoomFactorInt, maximumNumberOfSamples);

        xStepSize = 1f / (float)xSampleCount;
        yStepSize = 1f / (float)ySampleCount;

        xStepStart = (float)(-0.5 * (1.0 - (double)xStepSize));
        yStepStart = (float)(-0.5 * (1.0 - (double)yStepSize));

        Mxw = (float)-((double)tileCenterX + (double)midpointX);
        Myw = (float)-((double)tileCenterY + (double)midpointY);
        Mxx = oppositeCosImageSurface;
        Mxy = -oppositeSineImageSurface;
        float xRotation = (float)((double)oppositeCosImageSurface * (double)Mxw - (double)oppositeSineImageSurface * (double)Myw); //num19
        Myx = oppositeSineImageSurface;
        Myy = oppositeCosImageSurface;
        Myw = (float)((double)oppositeSineImageSurface * (double)Mxw + (double)oppositeCosImageSurface * (double)Myw);
        Mxw = xRotation;
        Mxx *= xZoomFactor;
        Mxy *= xZoomFactor;
        Mxw *= xZoomFactor;
        Myx *= yZoomFactor;
        Myy *= yZoomFactor;
        Myw *= yZoomFactor;
        Mwx = rotationFactor * Myx;
        Mwy = rotationFactor * Myy;
        Mww = (float)((double)rotationFactor * (double)Myw + 1.0);
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
    }

    private ColorBgra Move(Surface src, float x, float y)
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

    private ColorBgra SSpix(Surface src, int x, int y)
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
                colorBgra = Move(src, sampleX, sampleY);
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

    private void Render(Surface dst, Surface src, Rectangle rect)
    {
        for (int y = rect.Top; y < rect.Bottom; y++)
        {
            for (int x = rect.Left; x < rect.Right; x++)
            {
                ColorBgra colorBgra = SSpix(src, x, y);
                dst[x, y] = colorBgra;
            }
        }
    }

}